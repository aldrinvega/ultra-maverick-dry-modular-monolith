param(
    [string] $LegacyRoot = 'C:\Users\Aldrin\Ultramaverick-Dry',
    [string] $RepoRoot = '',
    [string] $RulingsPath = '',
    [switch] $FailOnOpen
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}
if ([string]::IsNullOrWhiteSpace($RulingsPath)) {
    $RulingsPath = Join-Path $PSScriptRoot 'rulings.json'
}

function Get-ClassAndRoute {
    param([string] $Text)
    $class = [regex]::Match($Text, 'class\s+(\w+)').Groups[1].Value
    $route = [regex]::Match($Text, '\[Route\("([^"]+)"\)\]').Groups[1].Value
    return @{ Class = $class; Route = $route }
}

function Get-Actions {
    param([string] $Root, [switch] $Legacy)

    $rows = @()
    $files = Get-ChildItem $Root -Recurse -Filter '*Controller.cs' |
        Where-Object { $_.Name -notin @('BaseApiController.cs', 'ErrorController.cs') }

    foreach ($file in $files) {
        $text = Get-Content $file.FullName -Raw
        $meta = Get-ClassAndRoute $text
        $lines = Get-Content $file.FullName

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -notmatch '^\s*\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]*)"\))?\]') { continue }

            $verb = $Matches[1]
            $route = if ($Matches[2]) { $Matches[2] } else { '' }

            for ($j = $i + 1; $j -lt [Math]::Min($i + 5, $lines.Count); $j++) {
                if ($lines[$j] -match '^\s*\[Route\("([^"]+)"\)\]') { $route = $Matches[1] }

                if ($lines[$j] -match '^\s*(?:public|internal)\s+') {
                    $signature = $lines[$j]
                    if ($signature -notmatch '\(' -and ($j + 1) -lt $lines.Count) {
                        $signature = $signature + ' ' + $lines[$j + 1]
                    }

                    if ($signature -match '\b(\w+)\s*\(') {
                        $method = $Matches[1]
                        if ($route) {
                            $full = if ($Legacy -or [string]::IsNullOrWhiteSpace($meta.Route)) {
                                $route
                            } else {
                                $meta.Route.TrimEnd('/') + '/' + $route.TrimStart('/')
                            }

                            $rows += [pscustomobject]@{
                                Controller = $meta.Class
                                Verb       = $verb
                                Route      = $route
                                FullRoute  = $full
                                Method     = $method
                                File       = $file.FullName
                                Line       = $j + 1
                            }
                        }
                        break
                    }
                }
            }
        }
    }

    return $rows
}

function Get-Shape {
    param([string] $Route)
    $shape = $Route.ToLowerInvariant()
    $shape = $shape -replace '^/?api/', ''
    $shape = $shape -replace '\{[^}]+\}', '{}'
    $shape = $shape -replace '/+', '/'
    return $shape.Trim('/')
}

$legacy = Get-Actions -Root (Join-Path $LegacyRoot 'ELIXIR.API\Controllers') -Legacy
$new = Get-Actions -Root (Join-Path $RepoRoot 'src\Ultramaverick.Api\Controllers')

$newShapes = @{}
foreach ($action in $new) {
    $shape = Get-Shape $action.FullRoute
    $newShapes[$shape] = $true
}

$rulings = Get-Content $RulingsPath -Raw | ConvertFrom-Json

$resolved = @()
foreach ($action in $legacy) {
    $controllerRule = $null
    $property = $rulings.controllers.PSObject.Properties[$action.Controller]
    if ($property) { $controllerRule = $property.Value }

    $rule = $null
    if ($controllerRule) {
        $actionKey = "$($action.Verb)|$($action.Route)"
        if ($controllerRule.actions) {
            $actionProperty = $controllerRule.actions.PSObject.Properties[$actionKey]
            if ($actionProperty) { $rule = $actionProperty.Value }
        }

        if (-not $rule -and $controllerRule.patterns) {
            foreach ($pattern in $controllerRule.patterns) {
                if ($action.Route -match $pattern.match) { $rule = $pattern; break }
            }
        }

        if (-not $rule -and $controllerRule.default) { $rule = $controllerRule.default }
    }

    $disposition = if ($rule) { $rule.disposition } else { 'Unruled' }
    $target = if ($rule) { $rule.target } else { '' }
    $reason = if ($rule) { $rule.reason } else { 'No controller default or action rule.' }
    $targetShape = if ($target) { Get-Shape $target } else { '' }
    $targetExists = if ($targetShape) { $newShapes.ContainsKey($targetShape) } else { $false }

    $resolved += [pscustomobject]@{
        Controller  = $action.Controller
        Verb        = $action.Verb
        Route       = $action.Route
        Method      = $action.Method
        Disposition = $disposition
        Target      = $target
        Reason      = $reason
        TargetExists = $targetExists
    }
}

$summary = $resolved |
    Group-Object Controller |
    ForEach-Object {
        $items = $_.Group
        [pscustomobject]@{
            Controller = $_.Name
            Legacy     = $items.Count
            Port       = @($items | Where-Object { $_.Disposition -eq 'Port' }).Count
            Replace    = @($items | Where-Object { $_.Disposition -eq 'Replace' }).Count
            Retire     = @($items | Where-Object { $_.Disposition -eq 'Retire' }).Count
            Unruled    = @($items | Where-Object { $_.Disposition -eq 'Unruled' }).Count
            Missing    = @($items | Where-Object { $_.Disposition -eq 'Port' -and -not $_.TargetExists }).Count
        }
    } |
    Sort-Object Controller

$totals = [pscustomobject]@{
    Legacy  = $resolved.Count
    Port    = @($resolved | Where-Object { $_.Disposition -eq 'Port' }).Count
    Replace = @($resolved | Where-Object { $_.Disposition -eq 'Replace' }).Count
    Retire  = @($resolved | Where-Object { $_.Disposition -eq 'Retire' }).Count
    Unruled = @($resolved | Where-Object { $_.Disposition -eq 'Unruled' }).Count
    Missing = @($resolved | Where-Object { $_.Disposition -eq 'Port' -and -not $_.TargetExists }).Count
}

$summaryPath = Join-Path $RepoRoot 'docs\action-parity.md'
$rulingsOutPath = Join-Path $RepoRoot 'docs\action-rulings.md'
$machinePath = Join-Path $PSScriptRoot 'last-run.json'

$summaryLines = @(
    '# Action parity',
    '',
    "Generated: $(Get-Date -Format o)",
    '',
    "Legacy actions: $($totals.Legacy) | Port: $($totals.Port) | Replace: $($totals.Replace) | Retire: $($totals.Retire) | Unruled: $($totals.Unruled) | Port targets missing: $($totals.Missing)",
    '',
    'Scope: business controller actions. The legacy Excel export handlers in ELIXIR.DATA are ruled separately in docs/retirement-decisions.md.',
    '',
    '| Controller | Legacy | Port | Replace | Retire | Unruled | Port target missing |',
    '|---|---:|---:|---:|---:|---:|---:|'
)
foreach ($row in $summary) {
    $summaryLines += "| $($row.Controller) | $($row.Legacy) | $($row.Port) | $($row.Replace) | $($row.Retire) | $($row.Unruled) | $($row.Missing) |"
}

$rulingLines = @(
    '# Action rulings',
    '',
    "Generated: $(Get-Date -Format o)",
    '',
    'Every legacy action must have exactly one disposition: Port, Replace or Retire.',
    '',
    '| Controller | Verb | Route | Disposition | Target | Reason |',
    '|---|---|---|---|---|---|'
)
foreach ($row in $resolved | Sort-Object Controller, Route) {
    $rulingLines += "| $($row.Controller) | $($row.Verb) | $($row.Route) | $($row.Disposition) | $($row.Target) | $($row.Reason) |"
}

Set-Content -Path $summaryPath -Value ($summaryLines -join [Environment]::NewLine) -Encoding UTF8
Set-Content -Path $rulingsOutPath -Value ($rulingLines -join [Environment]::NewLine) -Encoding UTF8
$resolved | ConvertTo-Json -Depth 5 | Set-Content -Path $machinePath -Encoding UTF8

Write-Host ("Legacy {0} | Port {1} | Replace {2} | Retire {3} | Unruled {4} | Missing port targets {5}" -f `
    $totals.Legacy, $totals.Port, $totals.Replace, $totals.Retire, $totals.Unruled, $totals.Missing)
Write-Host "Wrote $summaryPath and $rulingsOutPath."

if ($totals.Unruled -gt 0 -or $totals.Missing -gt 0) {
    $open = @($resolved | Where-Object { $_.Disposition -eq 'Unruled' -or ($_.Disposition -eq 'Port' -and -not $_.TargetExists) })
    foreach ($item in $open | Select-Object -First 20) {
        Write-Host ("OPEN {0} {1} {2} -> {3}" -f $item.Controller, $item.Verb, $item.Route, $(if ($item.Target) { $item.Target } else { $item.Disposition }))
    }
    if ($FailOnOpen) { exit 1 }
}
