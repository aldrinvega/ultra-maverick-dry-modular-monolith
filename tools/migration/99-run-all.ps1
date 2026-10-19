param(
    [Parameter(Mandatory = $true)][string] $TargetDb,
    [string] $LegacyDb = 'ElixirDepotDry',
    [string] $Server = '.\SQLEXPRESS',
    [switch] $SkipProvision,
    [switch] $SkipWarm,
    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

$sqlcmd = (Get-Command sqlcmd -ErrorAction Stop).Source

if (-not $SkipProvision) {
    $provision = Join-Path $PSScriptRoot 'provision-target.ps1'
    & $provision -TargetDb $TargetDb -Server $Server -RepoRoot $RepoRoot
    if ($LASTEXITCODE -ne 0) { throw "Provisioning failed for $TargetDb." }
}

$scripts = @(
    '01-legacy-id-map.sql',
    '10-identity.sql',
    '11-catalog.sql',
    '12-procurement.sql',
    '13-quality.sql',
    '13b-quality-checklist.sql',
    '14-warehouse.sql',
    '14b-warehouse-misc.sql',
    '15-orders.sql',
    '15b-orders-detail.sql',
    '17-catalog-gaps.sql',
    '18-warehouse-balances.sql',
    '16-warm-reporting.sql',
    '19-identity-module-map.sql',
    '20-password-change-required.sql',
    '90-gates.sql'
)

$total = [Diagnostics.Stopwatch]::StartNew()

foreach ($script in $scripts) {
    if ($SkipWarm -and $script -eq '16-warm-reporting.sql') { continue }

    $step = [Diagnostics.Stopwatch]::StartNew()
    Write-Host "  $script"
    & $sqlcmd -S $Server -d $TargetDb -b -I -v "LegacyDb=$LegacyDb" -i (Join-Path $PSScriptRoot $script)
    if ($LASTEXITCODE -ne 0) { throw "Migration script $script failed with exit code $LASTEXITCODE." }
    Write-Host ("    {0:N1}s" -f $step.Elapsed.TotalSeconds)
}

Write-Host ("Migration complete in {0:N1}s." -f $total.Elapsed.TotalSeconds)
