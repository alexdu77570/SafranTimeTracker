# Architecture — SAFRAN TIME TRACKER

> Ce document détaille l'architecture technique définie dans `CLAUDE.md`. En cas de divergence, `CLAUDE.md` prévaut sur ce document, et le cahier des charges prévaut sur les deux pour toute règle fonctionnelle.

## 1. Vue d'ensemble

```text
┌─────────────────────────┐        HTTPS / JSON        ┌──────────────────────────┐
│  Frontend React (SPA)   │  ───────────────────────▶  │  API ASP.NET Core        │
│  build statique (IIS)   │  ◀───────────────────────  │  /api/v1/...             │
└─────────────────────────┘                             └────────────┬─────────────┘
                                                                       │ EF Core
                                                                       ▼
                                                          ┌──────────────────────────┐
                                                          │  Base relationnelle       │
                                                          │  PostgreSQL / SQL Server  │
                                                          │  / SQLite (dev-test)      │
                                                          └──────────────────────────┘
```

Principes non négociables :

- Le navigateur ne dialogue **jamais** directement avec la base de données.
- Toute autorisation est vérifiée côté API, quelle que soit l'affichage côté frontend.
- Le provider de base de données est un détail de configuration, jamais une dépendance du code métier (`Domain`/`Application`).

## 2. Architecture backend en couches

```text
SafranTimeTracker.Domain/          Entités, énumérations, règles métier pures, aucune dépendance externe
SafranTimeTracker.Application/     Services applicatifs, DTO, validation, orchestration, interfaces de dépôt
SafranTimeTracker.Infrastructure/  Implémentation EF Core, providers, dépôts, services techniques (mail, fichiers, etc.)
SafranTimeTracker.Api/             Contrôleurs, middlewares, authentification/autorisation, configuration HTTP, OpenAPI
SafranTimeTracker.Tests/           Tests unitaires, tests d'intégration
```

Règles de dépendance (sens autorisé uniquement) :

```text
Api ──▶ Application ──▶ Domain
Api ──▶ Infrastructure ──▶ Application ──▶ Domain
```

`Domain` ne dépend de rien. `Application` ne dépend que de `Domain` (et d'abstractions qu'elle définit elle-même, implémentées par `Infrastructure`). `Infrastructure` implémente les interfaces définies par `Application`. `Api` assemble le tout par injection de dépendances.

Aucune couche `Domain` ou `Application` ne doit référencer une bibliothèque spécifique à Windows : la portabilité Linux (hébergement en option, voir `deploy/linux`) doit rester possible sans réécriture métier.

> **Décision de nommage actée** : le cahier des charges nomme fonctionnellement une entité « application » (référentiel léger d'applications, section 15), qui aurait collisionné avec le projet/namespace `SafranTimeTracker.Application` (couche architecturale). Pour lever toute ambiguïté, cette entité est implémentée sous le nom de classe **`ApplicationReference`**, et le service applicatif associé est **`ApplicationReferenceService`** (au lieu de `ApplicationService`). Le vocabulaire fonctionnel du cahier des charges (« application », « référentiel d'applications ») reste inchangé : seule l'implémentation technique porte ce nom.

### Services applicatifs attendus

`AuthService`, `AuthorizationService`, `UserService`, `ResourceService`, `OrganizationService`, `CompanyService`, `ContractService`, `ResourceTjmService`, `OrderService`, `BudgetService`, `ApplicationReferenceService`, `ProjectService`, `ProjectPlanningService`, `TimeEntryService`, `FinancialCalculationService`, `AbsenceService`, `AvailabilityService`, `MilestoneService`, `ReportingService`, `ExportService`, `ImportService`, `AuditService`, `SettingsService`.

> Le cahier des charges (§31) nomme ce service `ApplicationService` ; il est renommé `ApplicationReferenceService` par cohérence avec le renommage de l'entité `ApplicationReference` ci-dessus.

Le `FinancialCalculationService` est le **seul** point de calcul des coûts réels, coûts contractuels et différentiels : aucun autre service ne doit dupliquer cette logique.

## 3. Architecture frontend

```text
frontend/safran-time-tracker-web/
├── src/
│   ├── api/           Client HTTP centralisé, un module par famille d'endpoints
│   ├── auth/           Identité de démonstration, résolution des permissions, PermissionGuard (Lot 7)
│   ├── components/ui/ Composants réutilisables transverses (voir liste ci-dessous)
│   ├── routes/         Déclaration du routage/navigation ; pages et écrans métier sous routes/pages/<domaine>/
│   ├── theme/           Thème MUI (identité visuelle §8.1, Lot 7)
│   ├── test/            Utilitaires et configuration de test partagés (Lot 7)
│   └── lib/            Utilitaires (dateUtils, currencyUtils, validationUtils, ...)
```

> **Écart avec la structure initialement décrite** : `features/` et `hooks/` (dossiers vides créés au Lot 0) ne sont finalement pas utilisés — les écrans métier vivent sous `routes/pages/<domaine>/` (ex. `routes/pages/resources/`, `routes/pages/administration/`), convention établie par le `PlaceholderPage` du Lot 7 et suivie par tous les écrans du Lot 8. Un formulaire ou un hook spécifique à un écran reste colocalisé dans le même dossier plutôt que dans un `hooks/` partagé, tant qu'il n'est pas réellement réutilisé par un second écran.

Composants UI transverses attendus : `AppLayout`, `Sidebar`, `Header`, `Breadcrumb`, `KpiCard`, `DataTable`, `StatusBadge`, `ProgressBar`, `Modal`, `Drawer`, `FormField`, `Select`, `DatePicker`, `ChartCard`, `Timeline`, `CalendarView`, `EmptyState`, `ConfirmDialog`, `FilterBar`, `ImportWizard`, `DiffViewer`, `BudgetGauge`, `WorkloadHeatmap`, `WeeklyPlanningGrid`, `PermissionGuard`, `FinancialValue`, `AuditTimeline`. Implémentés depuis le Lot 7 (fondations) : `AppLayout`, `Sidebar`, `Header`, `Breadcrumb`, `KpiCard`, `DataTable` (+ prop `onRowClick` optionnelle depuis le Lot 8, navigation liste → fiche), `StatusBadge`, `ProgressBar`, `Modal`, `FormField` (+ `FormSelect`/`FormDatePicker`), `EmptyState`, `ConfirmDialog`, `FilterBar`, `PermissionGuard`, `FinancialValue`. Le Lot 8 ajoute une coquille d'écran (pas un composant transverse au sens de cette liste) : `ReferentialAdminTab` (`routes/pages/administration/`), liste + dialog de création/édition partagés par la majorité des onglets du panneau Administration. Le Lot 9 (écran Disponibilités, calendrier coloré §22.2) n'introduit **pas** `CalendarView` comme composant partagé : un seul écran en a besoin à ce stade, l'implémentation vit directement dans `routes/pages/availability/AvailabilityPage.tsx` (`CLAUDE.md` §5 — pas d'abstraction anticipée pour un unique consommateur) ; à extraire si un futur écran (Lots 10-12) a besoin d'un rendu calendaire similaire. Les autres (`Drawer`, `ChartCard`, `Timeline`, `ImportWizard`, `DiffViewer`, `BudgetGauge`, `WorkloadHeatmap`, `WeeklyPlanningGrid`, `AuditTimeline`) sont spécifiques à un écran métier et attendent le lot qui les introduit (Lots 10 à 12, `docs/ROADMAP.md`).

Le composant `PermissionGuard` et le composant `FinancialValue` **n'affichent jamais** une donnée financière non autorisée : ils s'appuient sur l'absence du champ dans la réponse API (voir `CLAUDE.md` §12-13), pas sur un masquage visuel.

Build : les fichiers statiques produits par Vite sont servis par IIS. Les appels API utilisent une URL relative (`/api/v1`) pour ne jamais coder en dur le nom du serveur.

## 4. Sécurité et autorisation

Modèle à deux niveaux indépendants (voir `CLAUDE.md` §17) :

1. **Rôle applicatif** : Ingénieur, Responsable Service, Responsable Département, Administrateur.
2. **Permissions complémentaires**, dont au minimum `FINANCIAL_DATA_VIEW`.

L'autorisation effective combine rôle et permissions (modèle RBAC, Lot 13 : `RolePermission` accorde des permissions par défaut au rôle, `UserPermission.Effect` complète ou retire des exceptions individuelles, calcul centralisé dans `PermissionResolutionService`) — jamais par filtrage a posteriori côté frontend. **Implémentation réelle** : un filtre MVC dédié (`RequirePermissionAttribute`), volontairement découplé du système `Authorization`/policies natif d'ASP.NET Core pour rester indépendant de tout mécanisme d'authentification concret (CLAUDE.md §10, §17) — pas des policies ASP.NET Core au sens strict du framework. Département/service/équipe/propriété de la donnée restent hors périmètre (`docs/BACKLOG_METIER.md` §18, Décision 3, report explicite).

L'authentification de démonstration (MVP) doit être remplaçable sans refonte du modèle d'autorisation par : Active Directory, LDAP, OpenID Connect, ou un SSO d'entreprise. **Depuis le Lot 13**, une session serveur réelle existe (`IAuthenticationProvider`/`DemoAuthenticationProvider`, cookie HttpOnly opaque, `UserSession` persistée) — l'abstraction reste `ICurrentUser`/`IAuthenticationProvider`, remplaçable sans toucher aux contrôleurs, services ou règles d'autorisation.

## 5. Configuration et environnements

Environnements minimum : `Development`, `Qualification`, `Production`.

Paramètres externalisés (jamais commités avec une valeur réelle) : provider de base de données, chaîne de connexion, URL publique de l'application, URL du backend, niveau/destination des logs, origines CORS, mode démonstration, pays/calendriers, heures par jour, seuils d'alerte, activation future du SSO.

## 6. Hébergement Windows Server / IIS

- Un site ou une application IIS pour le frontend React compilé.
- Une application IIS pour l'API ASP.NET Core (hébergée via le module ASP.NET Core, avec possibilité future d'exécution en service Windows ou derrière un reverse proxy Linux).
- Publication sous un même nom DNS, API exposée sous `/api`.
- HTTPS obligatoire hors développement ; certificats gérés selon les normes serveur, jamais dans Git.

Chemins physiques et arborescences détaillés : `CLAUDE.md` §18 et `docs/DATABASE.md` (sauvegardes).

## 7. Observabilité

- Journalisation structurée (Serilog) séparée du journal d'audit métier (`AuditLog`).
- Endpoint `/health` pour les scripts de déploiement (`health-check.ps1`) et la supervision.
- Documentation OpenAPI exhaustive, tenue à jour à chaque endpoint.

## 8. Portabilité Linux (hors MVP)

Le dossier `deploy/linux` est réservé mais non prioritaire. La contrainte d'architecture (aucune dépendance Windows dans `Domain`/`Application`, providers EF Core interchangeables) garantit qu'une portabilité future reste possible sans refonte métier.
