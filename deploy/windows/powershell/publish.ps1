#Requires -Version 5.1
<#
.SYNOPSIS
    Produit des artefacts de publication prêts à être copiés sur un serveur Windows
    (CLAUDE.md §19 — scripts PowerShell minimum attendus).

.DESCRIPTION
    Génère localement, sous ./artifacts/publish, une arborescence "api" et "web" qui reflète
    la cible Windows Server documentée (CLAUDE.md §18 : E:\appl\SafranTimeTracker\api et \web).
    Ce script NE déploie PAS sur un serveur distant : il prépare uniquement les artefacts.
    Le déploiement effectif (arrêt de service, copie vers E:\appl, migrations, health check,
    rollback) relève de deploy.ps1, hors périmètre du Lot 0 (pas encore de cible Windows
    Server réelle à déployer).

    Publication API : framework-dependent (dépend du runtime .NET 10 installé sur le serveur,
    conformément à l'hébergement IIS via le module ASP.NET Core décrit dans docs/ARCHITECTURE.md §6).

.PARAMETER OutputPath
    Dossier de sortie des artefacts (./artifacts/publish par défaut).

.PARAMETER Configuration
    Configuration MSBuild du backend (Release par défaut).

.EXAMPLE
    ./deploy/windows/powershell/publish.ps1
#>

[CmdletBinding()]
param(
    [string]$OutputPath,
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
$apiProject = Join-Path $repoRoot 'backend\SafranTimeTracker.Api\SafranTimeTracker.Api.csproj'
$frontendDir = Join-Path $repoRoot 'frontend\safran-time-tracker-web'
$frontendDist = Join-Path $frontendDir 'dist'

if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot 'artifacts\publish'
}
$apiOutput = Join-Path $OutputPath 'api'
$webOutput = Join-Path $OutputPath 'web'

Write-Host "=== Nettoyage des artefacts précédents ($OutputPath) ===" -ForegroundColor Cyan
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $apiOutput -Force | Out-Null
New-Item -ItemType Directory -Path $webOutput -Force | Out-Null

Write-Host ''
Write-Host "=== Publication de l'API (.NET, configuration $Configuration, framework-dependent) ===" -ForegroundColor Cyan
dotnet publish $apiProject --configuration $Configuration --output $apiOutput --no-self-contained
if ($LASTEXITCODE -ne 0) { throw "Échec de dotnet publish sur l'API." }

Write-Host ''
Write-Host '=== Build du frontend ===' -ForegroundColor Cyan
Push-Location $frontendDir
try {
    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'Échec du build frontend.' }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host "=== Copie des fichiers statiques frontend vers $webOutput ===" -ForegroundColor Cyan
Copy-Item -Path (Join-Path $frontendDist '*') -Destination $webOutput -Recurse -Force

Write-Host ''
Write-Host '=== Fichier VERSION ===' -ForegroundColor Cyan
$gitHash = $null
try {
    $gitHash = (& git -C $repoRoot rev-parse --short HEAD) 2>$null
}
catch {
    $gitHash = $null
}
$versionContent = @(
    "BuildDate: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    "Commit: $(if ($gitHash) { $gitHash } else { 'inconnu (pas de dépôt Git ou aucun commit)' })"
    "Configuration: $Configuration"
) -join [Environment]::NewLine
Set-Content -Path (Join-Path $OutputPath 'VERSION') -Value $versionContent -Encoding UTF8

Write-Host ''
Write-Host "Artefacts prêts sous $OutputPath (api/ et web/)." -ForegroundColor Green
Write-Host 'Rappel : ce script ne déploie rien sur un serveur (voir deploy.ps1, à venir).' -ForegroundColor Yellow
