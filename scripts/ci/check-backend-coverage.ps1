<#
.SYNOPSIS
    Vérifie la couverture de code backend (Lot 13, CI/CD) contre des seuils minimaux par projet,
    à partir des rapports Cobertura produits par `dotnet test --collect:"XPlat Code Coverage"`.

.DESCRIPTION
    Les projets SafranTimeTracker.Migrations.* (code généré EF Core, aucune logique métier) sont
    volontairement exclus du calcul : les compter fausserait la mesure vers le haut sans rien dire
    de la qualité réelle des tests (docs/IMPLEMENTATION_STATUS.md, Lot 13).

    Seuils mesurés et fixés en configuration Release (`dotnet test --configuration Release`) — la
    même que celle utilisée par `.github/workflows/ci.yml` : une mesure en Debug donne des chiffres
    sensiblement différents pour SafranTimeTracker.Domain (92-93 % en Debug contre 82,5 % en
    Release, l'inlining du compilateur réduisant la granularité mesurable des accesseurs triviaux
    des entités) — piégeant la CI dès sa première exécution réelle si le seuil est calé sur une
    mesure Debug. Mesure Release au moment de l'introduction : Api 70,5 %/40,9 % lignes/branches,
    Application 77,1 %/57 %, Domain 82,5 %/100 %, Infrastructure 97,3 %/64,3 %. Seuils fixés en
    dessous avec une marge de sécurité — objectif de les relever progressivement, jamais de les
    baisser sans justification écrite ici.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$CoverageDirectory
)

$ErrorActionPreference = 'Stop'

$thresholds = @{
    'SafranTimeTracker.Api'            = @{ Lines = 60; Branches = 30 }
    'SafranTimeTracker.Application'    = @{ Lines = 70; Branches = 50 }
    'SafranTimeTracker.Domain'         = @{ Lines = 75; Branches = 0 }
    'SafranTimeTracker.Infrastructure' = @{ Lines = 90; Branches = 0 }
}

$reportFiles = Get-ChildItem -Path $CoverageDirectory -Filter 'coverage.cobertura.xml' -Recurse
if (-not $reportFiles) {
    Write-Error "Aucun rapport coverage.cobertura.xml trouvé sous '$CoverageDirectory'."
    exit 1
}

# `dotnet test` peut régénérer un rapport par exécution : on prend le plus récent.
$reportFile = $reportFiles | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
Write-Host "Rapport analysé : $($reportFile.FullName)"

[xml]$report = Get-Content -Path $reportFile.FullName -Raw
$failures = @()

foreach ($package in $report.coverage.packages.package) {
    $name = $package.name
    if (-not $thresholds.ContainsKey($name)) {
        continue
    }

    $linePercent = [math]::Round([double]$package.'line-rate' * 100, 1)
    $branchPercent = [math]::Round([double]$package.'branch-rate' * 100, 1)
    $expected = $thresholds[$name]

    Write-Host "${name}: lignes $linePercent% (seuil $($expected.Lines)%), branches $branchPercent% (seuil $($expected.Branches)%)"

    if ($linePercent -lt $expected.Lines) {
        $failures += "$name : couverture de lignes $linePercent% < seuil $($expected.Lines)%"
    }
    if ($expected.Branches -gt 0 -and $branchPercent -lt $expected.Branches) {
        $failures += "$name : couverture de branches $branchPercent% < seuil $($expected.Branches)%"
    }
}

if ($failures.Count -gt 0) {
    Write-Error "Seuils de couverture non respectés :`n$($failures -join "`n")"
    exit 1
}

Write-Host 'Tous les seuils de couverture backend sont respectés.'
