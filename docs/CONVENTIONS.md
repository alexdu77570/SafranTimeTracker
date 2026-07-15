# Conventions détaillées — SAFRAN TIME TRACKER

> Ce document détaille, avec exemples, les conventions résumées dans `CLAUDE.md` (sections 5 à 14). En cas de divergence, `CLAUDE.md` prévaut.

## 1. Nommage C# (backend)

| Élément | Convention | Exemple |
|---|---|---|
| Classe, enum, méthode publique, propriété | `PascalCase` | `TimeEntryService`, `CalculateRealCost()` |
| Interface | `I` + `PascalCase` | `ITimeEntryRepository` |
| Variable locale, paramètre | `camelCase` | `dailyRate`, `startDate` |
| Constante | `PascalCase` | `DefaultHoursPerDay` |
| Champ privé | `_camelCase` | `_dbContext` |
| Fichier | nom de la classe/interface qu'il contient | `TimeEntryService.cs` |
| Projet | `SafranTimeTracker.<Couche>` | `SafranTimeTracker.Application` |

## 2. Nommage TypeScript / React (frontend)

| Élément | Convention | Exemple |
|---|---|---|
| Composant | `PascalCase` | `KpiCard`, `WeeklyPlanningGrid` |
| Fichier composant | nom du composant | `KpiCard.tsx` |
| Hook personnalisé | `use` + `camelCase` | `useTimeEntries()`, `useFinancialPermission()` |
| Fonction, variable | `camelCase` | `formatCurrency()`, `pageSize` |
| Type / interface TypeScript | `PascalCase` | `TimeEntryDto`, `ProjectFilter` |
| Fichier utilitaire | `camelCase.ts` | `dateUtils.ts`, `currencyUtils.ts` |
| Dossier de feature | `kebab-case` | `features/time-entries/` |

## 3. Nommage base de données

- Entités C# en `PascalCase`, mapping physique en `snake_case` via `EFCore.NamingConventions` (ex. entité `ResourceTjmHistory` → table `resource_tjm_history`).
- Clés étrangères : `<entité_référencée>_id` (ex. `resource_id`, `company_id`).
- Tables d'historique suffixées `_history` (`resource_tjm_history`, `company_contract_history`) ; tables de version suffixées `_version` (`project_plan_version`, `budget_version`).

## 4. Nommage API REST

- Ressources au pluriel, `kebab-case`, versionnées : `/api/v1/company-contracts`, `/api/v1/resource-tjm-history`.
- Sous-ressources imbriquées uniquement si la relation est de composition forte (ex. `/api/v1/projects/{id}/participants`), sinon filtre en query string (ex. `/api/v1/time-entries?projectId=...`).
- Paramètres de requête : `page`, `pageSize`, `sort` (`sort=startDate,-endDate`), filtres nommés explicitement (`status`, `departmentId`, `from`, `to`).

## 5. Conventions DTO — exemples de nommage

```text
Lecture liste paginée   : ProjectListItemDto
Lecture détail          : ProjectDetailDto
Création                : ProjectCreateRequest
Mise à jour              : ProjectUpdateRequest
Sous-objet financier     : ProjectFinancialSummaryDto   (omis si permission absente)
```

Règle : un DTO de détail avec données financières expose ces données dans un sous-objet ou des champs clairement regroupés, pour permettre au service applicatif de les omettre proprement selon la permission `FINANCIAL_DATA_VIEW`, sans avoir à reconstruire un DTO parallèle à la main à chaque fois.

## 6. Style de code backend

- `Nullable reference types` activé sur tous les projets.
- Une classe = une responsabilité. Un service applicatif par domaine métier (voir liste dans `docs/ARCHITECTURE.md`).
- Pas de logique métier dans les migrations EF Core au-delà du seed idempotent.
- Les exceptions métier attendues (chevauchement, période close, permission manquante) sont des exceptions typées, traduites en `ProblemDetails` par le middleware central — pas de `catch` générique silencieux.
- FluentValidation : un validateur par DTO d'entrée (`ProjectCreateRequestValidator`), exécuté dans le pipeline avant d'atteindre le service applicatif.

## 7. Style de code frontend

- TypeScript strict, pas de `any` sauf cas documenté et isolé.
- Un composant = une responsabilité d'affichage ; la logique de récupération de données passe par un hook dédié (`useProjectDetail(projectId)`), pas directement dans le composant.
- Les appels API passent tous par le client centralisé (`src/api/`), jamais par un `fetch` ou `axios` isolé dans un composant.
- Les schémas de validation Zod sont colocalisés avec la feature (`features/time-entries/schemas.ts`) et reflètent les règles serveur correspondantes (sans s'y substituer).

## 8. Organisation des tests

```text
SafranTimeTracker.Tests/
├── Unit/
│   ├── Application/        (services, calculs financiers, capacité, classification RUN)
│   └── Domain/              (règles d'entités : chevauchement, intégrité)
└── Integration/
    ├── Api/                 (endpoints sensibles : financier, sécurité, import)
    └── Persistence/         (EF Core, migrations, contraintes)
```

Nommage : `NomMethode_Scenario_ResultatAttendu` (ex. `GetApplicableTjm_NoValidPeriodAtDate_ReturnsIncompleteValuation`).

Frontend :

```text
src/
├── features/time-entries/__tests__/    (tests unitaires Vitest colocalisés)
e2e/                                     (scénarios Playwright, cahier des charges §38.3)
```

## 9. Formatage et outillage

- Backend : `dotnet format` / analyzers activés en CI (détail d'outillage à figer en Lot 0).
- Frontend : ESLint + Prettier, configuration stricte TypeScript.
- Pas de règle de style « maison » qui contredirait les valeurs par défaut de ces outils sans raison documentée.

## 10. Documentation en ligne du code

- Commentaire uniquement lorsque le **pourquoi** n'est pas évident (contrainte métier cachée, contournement, invariant non visible dans le nom) — jamais pour décrire ce que le code fait déjà de façon lisible.
- Toute règle de calcul financier ou capacitaire complexe référence la section correspondante du cahier des charges en commentaire (ex. `// cf. cahier des charges §20.3 — coût contractuel`).
