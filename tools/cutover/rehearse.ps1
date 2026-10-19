param(
    [string] $StagingDb = 'UltraMaverickDry_Staging',
    [string] $LegacyDb = 'ElixirDepotDry',
    [string] $Server = '.\SQLEXPRESS',
    [string] $ApiUrl = 'http://localhost:5080',
    [string] $JwtKey = 'cutover-rehearsal-signing-key-0123456789',
    [string] $JwtIssuer = 'https://localhost:5007',
    [string] $JwtAudience = 'ultramaverick-client',
    [int] $StartupTimeoutSeconds = 120,
    [int] $DrainTimeoutSeconds = 900,
    [switch] $SkipMigration,
    [switch] $SkipReplay,
    [switch] $SkipBuild
)

$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$sqlcmd = (Get-Command sqlcmd -ErrorAction Stop).Source
$api = $null
$results = @()

function Invoke-Sql {
    param([string]$Database, [string]$Query)
    $output = & $sqlcmd -S $Server -d $Database -h -1 -W -s '|' -b -I -Q $Query 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed against $Database`: $output" }
    return @($output | Where-Object { $_ -match '\S' } | ForEach-Object { $_.Trim() })
}

function ConvertTo-Base64Url {
    param([string]$Text)
    [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Text)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function New-SmokeToken {
    $header = '{"alg":"HS256","typ":"JWT"}'
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $payload = [ordered]@{
        sub         = '1'
        unique_name = 'smoke'
        name        = 'Cutover Smoke'
        role        = '1'
        modules     = @('Identity', 'Catalog', 'Procurement', 'Quality', 'Laboratory', 'Warehouse', 'Orders', 'Manufacturing', 'Reporting')
        iss         = $JwtIssuer
        aud         = $JwtAudience
        nbf         = $now - 60
        exp         = $now + 3600
    } | ConvertTo-Json -Compress

    $unsigned = (ConvertTo-Base64Url $header) + '.' + (ConvertTo-Base64Url $payload)
    $hmac = [System.Security.Cryptography.HMACSHA256]::new()
    $hmac.Key = [Text.Encoding]::UTF8.GetBytes($JwtKey)
    $signature = [Convert]::ToBase64String(
        $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($unsigned))
    ).TrimEnd('=').Replace('+', '-').Replace('/', '_')

    return "$unsigned.$signature"
}

function Get-ReportingCounts {
    $query = @'
SELECT
    (SELECT COUNT(*) FROM [Reporting].[InventoryPositions]),
    (SELECT ISNULL(SUM([Quantity]), 0) FROM [Reporting].[InventoryPositions]),
    (SELECT COUNT(*) FROM [Reporting].[OrderFulfillment]),
    (SELECT COUNT(*) FROM [Reporting].[QcStatus]),
    (SELECT ISNULL(SUM([NetQuantity]), 0) FROM [Reporting].[QcStatus]);
'@
    $row = (Invoke-Sql -Database $StagingDb -Query $query | Select-Object -First 1)
    $parts = $row -split '\|'
    return [pscustomobject]@{
        InventoryRows = [int]$parts[0]
        InventoryQty  = [decimal]::Parse($parts[1], [Globalization.CultureInfo]::InvariantCulture)
        OrderRows     = [int]$parts[2]
        QcRows        = [int]$parts[3]
        QcQty         = [decimal]::Parse($parts[4], [Globalization.CultureInfo]::InvariantCulture)
    }
}

function Wait-OutboxDrain {
    $query = @'
SELECT
    (SELECT COUNT(*) FROM [Infrastructure].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Catalog].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Procurement].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Quality].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Warehouse].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Laboratory].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Orders].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL) +
    (SELECT COUNT(*) FROM [Manufacturing].[OutboxMessages] WHERE [PublishedAtUtc] IS NULL);
'@
    $timer = [Diagnostics.Stopwatch]::StartNew()
    while ($timer.Elapsed.TotalSeconds -lt $DrainTimeoutSeconds) {
        if ($api -and $api.HasExited) {
            throw "API exited while draining the outbox. See $PSScriptRoot\api.stderr.log."
        }

        $pending = [int]((Invoke-Sql -Database $StagingDb -Query $query | Select-Object -First 1))
        if ($pending -eq 0) {
            $timer.Stop()
            return [math]::Round($timer.Elapsed.TotalSeconds, 1)
        }

        Start-Sleep -Seconds 2
    }

    throw "Outbox did not drain within $DrainTimeoutSeconds seconds."
}

function Format-Counts {
    param($Counts)
    return "inventory {0} rows / {1} qty; orders {2} rows; qc {3} rows / {4} qty" -f `
        $Counts.InventoryRows, $Counts.InventoryQty, $Counts.OrderRows, $Counts.QcRows, $Counts.QcQty
}

try {
    if (-not $SkipMigration) {
        Write-Host "Dropping staging database $StagingDb."
        & $sqlcmd -S $Server -d master -b -I -Q "IF DB_ID(N'$StagingDb') IS NOT NULL BEGIN ALTER DATABASE [$StagingDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$StagingDb]; END" | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Could not drop $StagingDb." }

        Write-Host "Running the migration from $LegacyDb into $StagingDb."
        $migration = [Diagnostics.Stopwatch]::StartNew()
        & (Join-Path $RepoRoot 'tools\migration\99-run-all.ps1') `
            -TargetDb $StagingDb -LegacyDb $LegacyDb -Server $Server -RepoRoot $RepoRoot
        $migration.Stop()
    }

    if (-not $SkipBuild) {
        & dotnet build (Join-Path $RepoRoot 'Ultramaverick.slnx') -v q --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    }

    $apiDll = Join-Path $RepoRoot 'src\Ultramaverick.Api\bin\Debug\net10.0\Ultramaverick.Api.dll'
    if (-not (Test-Path $apiDll)) { throw "API assembly not found at $apiDll." }

    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    $env:ConnectionStrings__Default = "Server=$Server;Database=$StagingDb;Trusted_Connection=True;TrustServerCertificate=True"
    $env:Jwt__Issuer = $JwtIssuer
    $env:Jwt__Audience = $JwtAudience
    $env:Jwt__Key = $JwtKey
    $env:Jwt__AccessTokenMinutes = '60'
    $env:Outbox__BatchSize = '500'
    $env:Outbox__PollSeconds = '1'

    $stdout = Join-Path $PSScriptRoot 'api.stdout.log'
    $stderr = Join-Path $PSScriptRoot 'api.stderr.log'
    Remove-Item $stdout, $stderr -ErrorAction SilentlyContinue

    $api = Start-Process -FilePath 'dotnet' `
        -ArgumentList @($apiDll, '--urls', $ApiUrl) `
        -WorkingDirectory (Join-Path $RepoRoot 'src\Ultramaverick.Api') `
        -RedirectStandardOutput $stdout -RedirectStandardError $stderr `
        -PassThru -WindowStyle Hidden

    $coldStart = [Diagnostics.Stopwatch]::StartNew()
    $healthy = $false
    while ($coldStart.Elapsed.TotalSeconds -lt $StartupTimeoutSeconds) {
        if ($api.HasExited) {
            throw "API exited during startup. See $stderr."
        }

        try {
            $probe = Invoke-WebRequest "$ApiUrl/health" -UseBasicParsing -TimeoutSec 2
            if ($probe.StatusCode -eq 200) { $healthy = $true; break }
        } catch { }

        Start-Sleep -Milliseconds 500
    }
    $coldStart.Stop()

    if (-not $healthy) { throw "API did not become healthy within $StartupTimeoutSeconds seconds." }

    $token = New-SmokeToken
    $headers = @{ Authorization = "Bearer $token" }
    $smoke = @(
        @{ Module = 'Identity';      Url = '/api/User/GetById/1' },
        @{ Module = 'Catalog';       Url = '/api/RawMaterial/GetAllRawMaterialsWithPagination?page=1&pageSize=1' },
        @{ Module = 'Catalog reference'; Url = '/api/RawMaterial/GetAllItemCategory' },
        @{ Module = 'Procurement';   Url = '/api/Ordering/GetPurchaseOrders?page=1&pageSize=1' },
        @{ Module = 'Quality';       Url = '/api/Receiving/GetQcReceiptsWithPagination?page=1&pageSize=1' },
        @{ Module = 'Laboratory';    Url = '/api/LaboratoryTest/GetLabTestRequests?page=1&pageSize=1' },
        @{ Module = 'Warehouse';     Url = '/api/WarehouseReceiving/GetWarehouseReceiptsWithPagination?page=1&pageSize=1' },
        @{ Module = 'Orders';        Url = '/api/Ordering/GetOrders?page=1&pageSize=1' },
        @{ Module = 'Manufacturing'; Url = '/api/Manufacturing/GetTransformationRequests?page=1&pageSize=1' },
        @{ Module = 'Reporting';     Url = '/api/Reports/InventoryPosition' }
    )

    foreach ($item in $smoke) {
        $timer = [Diagnostics.Stopwatch]::StartNew()
        try {
            $response = Invoke-WebRequest ($ApiUrl + $item.Url) -Headers $headers -UseBasicParsing -TimeoutSec 120
            $timer.Stop()
            $results += [pscustomobject]@{
                Module = $item.Module
                Status = [int]$response.StatusCode
                Ms     = [math]::Round($timer.Elapsed.TotalMilliseconds)
                Detail = $response.Content.Substring(0, [Math]::Min(140, $response.Content.Length))
            }
        } catch {
            $timer.Stop()
            $status = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
            $results += [pscustomobject]@{
                Module = $item.Module
                Status = $status
                Ms     = [math]::Round($timer.Elapsed.TotalMilliseconds)
                Detail = $_.Exception.Message
            }
        }
    }

    $baseline = Get-ReportingCounts
    $replay = $null

    if (-not $SkipReplay) {
        Write-Host "Enqueueing synthetic Reporting events."
        & $sqlcmd -S $Server -d $StagingDb -b -I -i (Join-Path $RepoRoot 'tools\migration\21-replay-reporting.sql')
        if ($LASTEXITCODE -ne 0) { throw 'Replay enqueue failed.' }

        $drainSeconds = Wait-OutboxDrain
        $afterFirst = Get-ReportingCounts

        $reset = @'
UPDATE [Warehouse].[OutboxMessages] SET [PublishedAtUtc] = NULL WHERE [AggregateType] LIKE 'Replay.%';
UPDATE [Orders].[OutboxMessages] SET [PublishedAtUtc] = NULL WHERE [AggregateType] LIKE 'Replay.%';
UPDATE [Quality].[OutboxMessages] SET [PublishedAtUtc] = NULL WHERE [AggregateType] LIKE 'Replay.%';
'@
        Invoke-Sql -Database $StagingDb -Query $reset | Out-Null
        $secondDrainSeconds = Wait-OutboxDrain
        $afterSecond = Get-ReportingCounts

        $replay = [pscustomobject]@{
            DrainSeconds       = $drainSeconds
            SecondDrainSeconds = $secondDrainSeconds
            First              = $afterFirst
            Second             = $afterSecond
            Baseline           = $baseline
            MatchesBaseline    = (
                $afterFirst.InventoryRows -eq $baseline.InventoryRows -and
                $afterFirst.InventoryQty -eq $baseline.InventoryQty -and
                $afterFirst.OrderRows -eq $baseline.OrderRows -and
                $afterFirst.QcRows -eq $baseline.QcRows -and
                $afterFirst.QcQty -eq $baseline.QcQty)
            Idempotent         = (
                $afterSecond.InventoryRows -eq $afterFirst.InventoryRows -and
                $afterSecond.InventoryQty -eq $afterFirst.InventoryQty -and
                $afterSecond.OrderRows -eq $afterFirst.OrderRows -and
                $afterSecond.QcRows -eq $afterFirst.QcRows -and
                $afterSecond.QcQty -eq $afterFirst.QcQty)
        }
    }

    $report = @()
    $report += '# Cutover rehearsal'
    $report += ''
    $report += "Run: $(Get-Date -Format o)"
    $report += "Staging database: $StagingDb (source $LegacyDb, server $Server)"
    if ($migration) { $report += "Migration: $([math]::Round($migration.Elapsed.TotalSeconds, 1)) s" }
    $report += "Cold start to /health: $([math]::Round($coldStart.Elapsed.TotalSeconds, 1)) s"
    $report += ''
    $report += '## Module smoke'
    $report += ''
    $report += ($results | Format-Table Module, Status, Ms, Detail -AutoSize | Out-String).Trim()
    $report += ''
    $report += '## Reporting baseline'
    $report += ''
    $report += Format-Counts $baseline
    if ($replay) {
        $report += ''
        $report += '## Outbox replay'
        $report += ''
        $report += "- First drain: $($replay.DrainSeconds) s"
        $report += "- Second drain (same EventIds): $($replay.SecondDrainSeconds) s"
        $report += "- After first drain:  $(Format-Counts $replay.First)"
        $report += "- After second drain: $(Format-Counts $replay.Second)"
        $report += "- Matches warm baseline: $($replay.MatchesBaseline)"
        $report += "- Idempotent on redelivery: $($replay.Idempotent)"
        $report += ''
        $report += 'The warm backfill reads Warehouse.InventoryBalances; the replay projects StockReceived only. The replay proves the projection path and idempotency, not a full on-hand rebuild: Warehouse.InventoryBalances remains authoritative for stock.'
    }
    $report += ''
    $report += '## Verdict'
    $report += ''
    $smokeFailed = @($results | Where-Object { $_.Status -ne 200 })
    $verdict = ($smokeFailed.Count -eq 0) -and (-not $replay -or $replay.Idempotent)
    $report += if ($verdict) { 'PASS' } else { 'FAIL' }

    $reportPath = Join-Path $PSScriptRoot 'rehearsal-report.md'
    Set-Content -Path $reportPath -Value ($report -join [Environment]::NewLine) -Encoding UTF8
    Write-Host "Rehearsal report written to $reportPath."
    Write-Host $report[-1]
}
finally {
    if ($api -and -not $api.HasExited) {
        Stop-Process -Id $api.Id -Force -ErrorAction SilentlyContinue
    }
}
