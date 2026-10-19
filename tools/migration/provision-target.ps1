param(
    [Parameter(Mandatory = $true)][string] $TargetDb,
    [string] $Server = '.\SQLEXPRESS',
    [string] $RepoRoot = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

$connection = "Server=$Server;Database=$TargetDb;Trusted_Connection=True;TrustServerCertificate=True"
$sqlcmd = (Get-Command sqlcmd -ErrorAction Stop).Source

# Identity first so the reference data every later load depends on exists, then the
# transactional modules, then the read-only projections.
$modules = @(
    @{ Name = 'Identity';      Project = 'src\Modules\Identity\Ultramaverick.Identity.Persistence' },
    @{ Name = 'Catalog';       Project = 'src\Modules\Catalog\Ultramaverick.Catalog.Persistence' },
    @{ Name = 'Procurement';   Project = 'src\Modules\Procurement\Ultramaverick.Procurement.Persistence' },
    @{ Name = 'Quality';       Project = 'src\Modules\Quality\Ultramaverick.Quality.Persistence' },
    @{ Name = 'Warehouse';     Project = 'src\Modules\Warehouse\Ultramaverick.Warehouse.Persistence' },
    @{ Name = 'Laboratory';    Project = 'src\Modules\Laboratory\Ultramaverick.Laboratory.Persistence' },
    @{ Name = 'Orders';        Project = 'src\Modules\Orders\Ultramaverick.Orders.Persistence' },
    @{ Name = 'Manufacturing'; Project = 'src\Modules\Manufacturing\Ultramaverick.Manufacturing.Persistence' },
    @{ Name = 'Reporting';     Project = 'src\Modules\Reporting\Ultramaverick.Reporting.Persistence' }
)

Write-Host "Provisioning '$TargetDb' on '$Server'."

& $sqlcmd -S $Server -d master -b -I -Q "IF DB_ID(N'$TargetDb') IS NULL CREATE DATABASE [$TargetDb];"
if ($LASTEXITCODE -ne 0) { throw "Could not create database $TargetDb." }

foreach ($module in $modules) {
    $project = Join-Path $RepoRoot $module.Project
    Write-Host "  applying $($module.Name) migrations"

    & dotnet ef database update `
        --project $project `
        --startup-project $project `
        --connection $connection
    if ($LASTEXITCODE -ne 0) { throw "Migration failed for module $($module.Name)." }
}

& $sqlcmd -S $Server -d $TargetDb -b -I -i (Join-Path $PSScriptRoot '01-legacy-id-map.sql')
if ($LASTEXITCODE -ne 0) { throw 'Could not create dbo.LegacyIdMap.' }

Write-Host "Provisioning complete."
