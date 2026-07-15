# Modèle de données — SAFRAN TIME TRACKER

> Ce document décrit la stratégie de persistance et l'inventaire des entités attendues (cahier des charges, section 30). Il ne remplace pas la modélisation détaillée (colonnes, types précis, contraintes SQL) qui sera produite lors du Lot 0 et des lots suivants sous forme de migrations EF Core versionnées.

## 1. Stratégie de providers

| Provider | Rôle | Usage |
|---|---|---|
| **PostgreSQL** | Principal recommandé | Environnements partagés (Qualification, Production par défaut) |
| **Microsoft SQL Server** | Alternatif | Si contrainte d'entreprise l'impose |
| **SQLite** | Développement / tests | Développement local, tests automatisés, démonstrations mono-utilisateur — **jamais** en production multi-utilisateur |

Le provider est sélectionné par configuration (`appsettings`, variable d'environnement), jamais par branchement de code. Le modèle logique (entités, relations, contraintes) reste identique entre providers ; seules les migrations physiques diffèrent.

Structure des migrations (voir `docs/ARCHITECTURE.md` et arborescence indicative du cahier des charges) :

```text
database/
├── postgresql/
│   ├── migrations/
│   └── scripts/        (création, sauvegarde, restauration)
├── sqlserver/
│   ├── migrations/
│   └── scripts/
├── sqlite/
└── seed/                (jeu de données initial, idempotent)
```

## 2. Convention de nommage physique

Les entités C# sont nommées en `PascalCase`. Le mapping physique (tables, colonnes) utilise `snake_case`, via le package `EFCore.NamingConventions`, pour rester idiomatique côté PostgreSQL tout en conservant un code C# conventionnel — cette convention s'applique identiquement aux trois providers pour éviter toute divergence de modèle logique.

## 3. Règles générales de modélisation

- Identifiants techniques stables (clé primaire technique, jamais une valeur métier mutable).
- Dates de création et de modification systématiques (`CreatedAt`, `UpdatedAt`) avec auteur associé (`CreatedBy`, `UpdatedBy`) sur les entités concernées par l'audit ou l'historisation.
- **Statut plutôt que suppression physique** sur toute entité référencée par des temps, absences, projets, imports ou journaux.
- Contraintes anti-chevauchement pour toutes les entités d'historique (une période active à la fois, pas de recouvrement de dates).
- Clés étrangères explicites, jamais de relation implicite non contrainte.
- Montants systématiquement en `decimal`, jamais en type flottant.
- Dates représentant un instant stockées en UTC.
- Jeton de concurrence optimiste sur les entités sensibles (TJM, contrats, budgets, commandes).
- Index sur les colonnes de recherche fréquentes : dates, ressource, projet, commande, société.

## 4. Inventaire des entités (cahier des charges §30)

### Sécurité et organisation
`User`, `Role`, `Permission`, `UserPermission`, `OperationalRole`, `Department`, `Service`, `Team`.

### Ressources et capacité
`Resource`, `ResourceCapacityPeriod`, `ResourceTjmHistory`.

### Sociétés et contrats
`Company`, `CompanyType`, `ResourceCompanyAssignment`, `CompanyContractHistory`.

### Applications et projets
`ApplicationReference` (référentiel d'applications, nommé `Application` dans le cahier des charges §30 — renommé pour éviter la collision avec le namespace de couche `SafranTimeTracker.Application`, voir `docs/ARCHITECTURE.md` §2), `Project`, `ProjectStatus`, `ProjectParticipant`, `ProjectPlanVersion`, `ProjectWeeklyPlan`.

### Temps et absences
`TimeEntry`, `TimeEntryFinancialSnapshot`, `Absence`.

### Jalons et activités
`Milestone`, `ActivityType`.

### Financier
`Order`, `OrderStatus`, `OrderExtension`, `Budget`, `BudgetVersion`.

### Imports et audit
`ImportBatch`, `ImportDiff`, `AuditLog`.

### Paramétrage
`Settings`, `HolidayCalendar`, `DashboardKpi`.

## 5. Historisation — principe commun

Toute entité d'historique (`ResourceTjmHistory`, `CompanyContractHistory`, `ResourceCompanyAssignment`, `ProjectPlanVersion`, `OrderExtension`, `BudgetVersion`, …) partage les mêmes règles d'intégrité :

- une seule période **ouverte** (sans date de fin) à la fois par sujet (ressource, société, …) ;
- deux périodes actives ne peuvent pas se chevaucher ;
- une période déjà utilisée par une saisie ne peut pas être supprimée ;
- la valeur applicable à une date donnée se recherche **à la date de la saisie**, jamais à la date courante ;
- une modification future ne déclenche aucun recalcul rétroactif des saisies déjà valorisées ;
- toute correction est auditée (`AuditLog`), jamais silencieuse.

### Instantané financier (`TimeEntryFinancialSnapshot`)

Une saisie de temps fige, au moment du calcul, au minimum : `tjmPersonneSnapshot`, `sourceTjmPersonne`, `resourceTjmHistoryId`, `tjmContratSnapshot`, `sourceContrat`, `companyContractHistoryId`, `companyIdSnapshot`, `coutReelCalcule`, `coutContratCalcule`, `differentielCalcule`, `calculationDate`, `calculationStatus`. Ces valeurs ne sont jamais recalculées automatiquement ; un recalcul n'est possible que sur action explicite, avec permission dédiée, confirmation, motif obligatoire et conservation de l'ancienne valeur dans l'audit.

## 6. Sauvegarde et restauration (Windows Server)

- Sauvegardes de base sous `E:\data\SafranTimeTracker\database-backups`.
- Journalisation des opérations de sauvegarde/restauration sous `E:\data\logs\SafranTimeTracker\jobs`.
- Scripts PowerShell dédiés : `backup-database.ps1`, `restore-database.ps1` (voir `CLAUDE.md` §19).
- Sauvegarde obligatoire avant toute mise à jour de production ; politique de rétention configurable ; procédure de test de restauration documentée.

## 7. Jeu de données de démonstration (volumétrie minimale — cahier des charges §35)

10 applications, 8 projets, 12 ressources, 5 sociétés (dont plusieurs externes), 8 commandes, 5 budgets projet, 80 saisies de temps, 20 références INC/CHG/PRB/RITM, 15 jalons, 10 absences, plusieurs départements/services/équipes, plusieurs rattachements ressource/société, plusieurs historiques TJM et de contrats, plusieurs rallonges, des écarts initial/ajusté/réalisé, des cas de différentiel positif/nul/négatif, des cas de valorisation incomplète contrôlée, des utilisateurs avec et sans permission financière. Le jeu de données doit être **idempotent** (rejouable sans erreur ni duplication).

## 8. Tests de compatibilité provider

Minimum requis : PostgreSQL et SQLite testés à chaque évolution significative du modèle. SQL Server doit pouvoir être activé sans refonte des règles métier, même s'il n'est pas testé en continu dès le Lot 0.
