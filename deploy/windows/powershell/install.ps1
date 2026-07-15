#Requires -Version 5.1
<#
.SYNOPSIS
    Vérifie les prérequis et prépare l'environnement de développement/build local
    (CLAUDE.md §19 — scripts PowerShell minimum attendus).

.DESCRIPTION
    Contrôle la présence du SDK .NET 10, de Node.js et npm, restaure les outils .NET locaux
    (dotnet-ef, voir .config/dotnet-tools.json), restaure les packages NuGet du backend et
    installe les dépendances npm du frontend. N'effectue aucune action destructive et peut
    être relancé sans risque (idempotent).

    Ce script prépare un poste de développement ou un agent de build ; il ne déploie rien
    sur un serveur (voir deploy.ps1, hors périmètre du Lot 0).

.EXAMPLE
    ./deploy/windows/powershell/install.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
$backendSolution = Join-Path $repoRoot 'backend\SafranTimeTracker.slnx'
$frontendDir = Join-Path $repoRoot 'frontend\safran-time-tracker-web'

function Test-CommandVersion {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Command,
        [Parameter(Mandatory)][string]$VersionArgs
    )

    $exe = Get-Command $Command -ErrorAction SilentlyContinue
    if (-not $exe) {
        throw "$Name est introuvable dans le PATH. Installez-le avant de continuer."
    }

    $version = & $Command $VersionArgs.Split(' ') 2>&1 | Select-Object -First 1
    Write-Host "[OK] $Name détecté : $version" -ForegroundColor Green
}

Write-Host '=== SAFRAN TIME TRACKER — Vérification des prérequis ===' -ForegroundColor Cyan

Test-CommandVersion -Name '.NET SDK' -Command 'dotnet' -VersionArgs '--version'
Test-CommandVersion -Name 'Node.js' -Command 'node' -VersionArgs '--version'
Test-CommandVersion -Name 'npm' -Command 'npm' -VersionArgs '--version'

Write-Host ''
Write-Host '=== Restauration des outils .NET locaux (dotnet-ef, cf. .config/dotnet-tools.json) ===' -ForegroundColor Cyan
Push-Location $repoRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Échec de dotnet tool restore.' }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host '=== Restauration des packages NuGet (backend) ===' -ForegroundColor Cyan
dotnet restore $backendSolution
if ($LASTEXITCODE -ne 0) { throw 'Échec de dotnet restore.' }

Write-Host ''
Write-Host '=== Installation des dépendances npm (frontend) ===' -ForegroundColor Cyan
Push-Location $frontendDir
try {
    npm install
    if ($LASTEXITCODE -ne 0) { throw 'Échec de npm install.' }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host 'Environnement prêt.' -ForegroundColor Green
