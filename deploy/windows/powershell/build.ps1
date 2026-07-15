#Requires -Version 5.1
<#
.SYNOPSIS
    Compile le backend (.NET) et le frontend (React/Vite), avec exécution optionnelle des tests
    (CLAUDE.md §19 — scripts PowerShell minimum attendus).

.DESCRIPTION
    Build de vérification (pas de publication ni de déploiement, voir publish.ps1 et deploy.ps1).
    Configuration MSBuild par défaut : Release. Suppose que ./install.ps1 a déjà été exécuté
    au moins une fois (packages NuGet et dépendances npm restaurés).

.PARAMETER Configuration
    Configuration MSBuild du backend (Release par défaut).

.PARAMETER RunTests
    Exécute également `dotnet test` sur la solution backend après le build.

.EXAMPLE
    ./deploy/windows/powershell/build.ps1 -RunTests
#>

[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [switch]$RunTests
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
$backendSolution = Join-Path $repoRoot 'backend\SafranTimeTracker.slnx'
$frontendDir = Join-Path $repoRoot 'frontend\safran-time-tracker-web'

Write-Host "=== Build backend (.NET, configuration $Configuration) ===" -ForegroundColor Cyan
dotnet build $backendSolution --configuration $Configuration --nologo
if ($LASTEXITCODE -ne 0) { throw 'Échec du build backend.' }

if ($RunTests) {
    Write-Host ''
    Write-Host '=== Tests backend (dotnet test) ===' -ForegroundColor Cyan
    dotnet test $backendSolution --configuration $Configuration --no-build --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Échec des tests backend.' }
}

Write-Host ''
Write-Host '=== Build frontend (lint + build) ===' -ForegroundColor Cyan
Push-Location $frontendDir
try {
    npm run lint
    if ($LASTEXITCODE -ne 0) { throw 'Échec du lint frontend.' }

    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'Échec du build frontend.' }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host 'Build terminé avec succès.' -ForegroundColor Green
