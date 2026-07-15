# État d'avancement — SAFRAN TIME TRACKER

> Ce document reflète l'état **réel** du projet, à distinguer de `docs/ROADMAP.md` qui décrit le plan. Il doit être mis à jour à la fin de chaque lot (voir `CLAUDE.md` §20 — Méthode de travail par lots), et à chaque écart significatif constaté en cours de lot.

**Dernière mise à jour : 2026-07-15 — Lot 1 (Référentiels) terminé.**

## Phase préliminaire — Socle documentaire

**Statut : Terminé (2026-07-15).**

Cette phase n'est pas un lot numéroté du cahier des charges (§40) : elle précède le `Lot 0 — Fondations`. Elle a couvert uniquement la production et la validation de la documentation de cadrage (voir « État détaillé » ci-dessous).

## Synthèse des lots

| Lot | Contenu | Statut | Dernière mise à jour |
|---|---|---|---|
| Lot 0 | Fondations (périmètre technique restreint — voir ci-dessous) | Terminé | 2026-07-15 |
| Lot 1 | Référentiels | Terminé | 2026-07-15 |
| Lot 2 | Modèle financier | Non démarré | — |
| Lot 3 | Temps et capacité | Non démarré | — |
| Lot 4 | Projets | Non démarré | — |
| Lot 5 | Budgets et reporting | Non démarré | — |
| Lot 6 | Imports et audit | Non démarré | — |
| Lot 7 | Industrialisation | Non démarré | — |

## État détaillé

### Documentation
- [x] Cahier des charges fonctionnel intégré au dépôt (`docs/`).
- [x] `CLAUDE.md` (mémoire technique permanente) créé.
- [x] `README.md` créé.
- [x] `docs/ARCHITECTURE.md` créé.
- [x] `docs/DATABASE.md` créé.
- [x] `docs/CONVENTIONS.md` créé.
- [x] `docs/ROADMAP.md` créé.
- [x] `.gitignore` créé.

### Code — Lot 0 (2026-07-15)

- [x] Dépôt Git initialisé (fait à la clôture de la phase documentaire).
- [x] Solution .NET créée : `backend/SafranTimeTracker.slnx` (nouveau format `.slnx`, par défaut depuis le SDK .NET 10).
- [x] Projets Clean Architecture créés et référencés conformément à `docs/ARCHITECTURE.md` §2 : `SafranTimeTracker.Domain`, `SafranTimeTracker.Application`, `SafranTimeTracker.Infrastructure`, `SafranTimeTracker.Api`, `SafranTimeTracker.Tests`. Aucune entité métier, aucun contrôleur métier, aucune page fonctionnelle — conformément à la demande explicite.
- [x] Entity Framework Core configuré : `AppDbContext` (vide, sans DbSet), sélection du provider par configuration (`Database:Provider`), mapping snake_case uniforme (`EFCore.NamingConventions`).
- [x] PostgreSQL, SQLite et SQL Server configurés comme providers EF Core, chacun avec son propre projet de migrations (`database/{sqlite,postgresql,sqlserver}/SafranTimeTracker.Migrations.*`, pattern officiel Microsoft pour éviter tout mélange de migrations entre providers).
- [x] Migration initiale (vide) générée et vérifiée pour les **trois** providers. Appliquée réellement (`dotnet ef database update`) et validée sur SQLite ; PostgreSQL et SQL Server générés avec succès en mode design-time uniquement (aucune instance locale disponible pour tester une connexion réelle — voir écarts).
- [x] Serilog (journalisation structurée) configuré : console + fichier par environnement (Qualification/Production → `E:/data/logs/SafranTimeTracker/api/`).
- [x] Swagger/OpenAPI configuré (Swashbuckle.AspNetCore), document `v1` valide, aucun endpoint métier documenté.
- [x] Endpoint technique `/health` exposé (health checks ASP.NET Core natifs, sans dépendance métier).
- [x] CORS configuré par environnement (`Cors:AllowedOrigins`), vide par défaut hors développement.
- [x] `appsettings.json` + variantes `Development` / `Qualification` / `Production` créées (aucun secret réel).
- [x] Projet de tests `SafranTimeTracker.Tests` créé (xUnit + FluentAssertions + NSubstitute + Mvc.Testing) avec 2 tests d'intégration d'infrastructure (`/health`, `/swagger/v1/swagger.json`) — 2/2 réussis.
- [x] Frontend créé : `frontend/safran-time-tracker-web` (React 19 + TypeScript strict + Vite), avec React Router, TanStack Query, React Hook Form, Zod, Recharts, axios installés conformément à `CLAUDE.md` §4. Squelette de dossiers (`api/`, `components/ui/`, `features/`, `hooks/`, `routes/`, `lib/`) créé, une seule page technique de vérification (aucun écran fonctionnel).
- [x] ESLint (flat config) + Prettier configurés côté frontend.
- [x] `Directory.Build.props` (racine) + `.editorconfig` créés.
- [x] Scripts PowerShell créés et testés de bout en bout : `deploy/windows/powershell/install.ps1`, `build.ps1` (option `-RunTests`), `publish.ps1`.
- [x] Vérification finale : `dotnet build` (solution complète, 3 projets de migrations inclus) → 0 avertissement, 0 erreur ; `dotnet test` → 2/2 ; `npm run lint` / `npm run format:check` / `npm run build` → tous verts.

### Code — Lot 1 (2026-07-15)

- [x] Entités Domain créées, conformément à `docs/DATABASE.md` §4 et au cahier des charges : `Department`, `Service`, `Team` (§9), `Resource`, `OperationalRole`, `ResourceOperationalRole` (§10), `Company`, `CompanyType` (§12.1), `Order`, `OrderStatus`, `OrderAuthorizedResource` (§13), `ApplicationReference`, `ApplicationCriticality` (§15), `User`, `Role`, `Permission`, `UserPermission` (§10.2, §6.2), `Settings` (§28.2). Base commune `AuditableEntity` / `ReferentialStatus` (statut plutôt que suppression physique, `CLAUDE.md` §7).
- [x] Configuration EF Core par entité (`IEntityTypeConfiguration<T>`) sous `Infrastructure/Persistence/Configurations/`, mapping snake_case uniforme, conformément à `CLAUDE.md` §11.
- [x] Persistance générique : `IRepository<T>` / `IReadRepository<T>` (`Application/Common/Persistence`) implémentées par `EfRepository<T>` (`Infrastructure/Persistence/EfRepository.cs`) — aucun dépôt dédié par entité, le Lot 1 ne contenant aucune règle métier de recherche complexe (chevauchement, date de saisie, etc.) qui le justifierait.
- [x] 9 contrôleurs REST sous `/api/v1` (`kebab-case`, pluriel) : `departments`, `services`, `teams`, `users`, `resources`, `applications`, `companies`, `orders`, `settings`. Pagination serveur (`PagedResult<T>`, `PaginationQuery`) sur toutes les listes, conformément à `CLAUDE.md` §12.
- [x] Validation serveur systématique via FluentValidation (un validateur par `XxxCreateRequest`/`XxxUpdateRequest`), rejouée côté API indépendamment de toute validation client (`CLAUDE.md` §7) ; `FluentValidation.DependencyInjectionExtensions` ajouté pour l'enregistrement automatique des validateurs (`Application/DependencyInjection.cs`).
- [x] Mapping DTO ↔ entité via Mapster (`OrderMapsterConfig`, `ResourceMapsterConfig`, `UserMapsterConfig`) pour les projections non triviales.
- [x] Seed de démonstration idempotent (`Infrastructure/Persistence/Seed/Lot1Seed.cs`, via `HasData`, données et horodatage strictement déterministes) : 1 département (DSI), 4 services, 2 équipes, les 13 utilisateurs/ressources nommés au cahier des charges §5.4, la société interne SAFRAN, une commande de démonstration, 3 applications (IBM ELM, VTOM, ServiceNow), les référentiels de rôles/permissions/rôles opérationnels/statuts de commande, et les paramètres par défaut. Ne couvre pas le jeu de données volumétrique complet du §35 (dépend d'entités non encore créées : `Project`, `TimeEntry`, etc. — hors périmètre Lot 1).
- [x] Migration `AddLot1Referentials` générée pour les **trois** providers (`database/{sqlite,postgresql,sqlserver}/Migrations/`). Appliquée et validée réellement pour SQLite via les tests d'intégration (`WebApplicationFactory` + `dotnet ef` implicite au démarrage) ; PostgreSQL et SQL Server générés et compilables (design-time), non testés contre une instance réelle — même écart qu'au Lot 0 (voir « Écarts connus »).
- [x] `Program.cs` : câblage de `builder.Services.AddApplication()` (validateurs + Mapster) ; correction d'un bug de démarrage (bootstrap logger Serilog à deux phases incompatible avec `WebApplicationFactory`, qui réexécute `Program` pour chaque instance d'hôte de test — voir « Décisions actées »).
- [x] Tests : 20 nouveaux cas ajoutés (13 tests d'intégration API sur les 7 référentiels dans `ReferentialsTests.cs` — liste seedée, création valide, rejet d'une création invalide, 404 — + 2 tests unitaires de validateur avec 5 cas `[Theory]` dans `SettingsUpdateRequestValidatorTests.cs`). Suite complète : **22/22 réussis** (20 nouveaux + 2 tests d'infrastructure du Lot 0).
- [x] Vérification finale de clôture de lot : `dotnet build` (solution complète, 8 projets) → 0 avertissement, 0 erreur ; `dotnet test` → 22/22 ; `npm run build` (frontend, inchangé ce lot) → succès ; `npm run lint` (frontend) → 0 erreur.

## Écarts connus par rapport au cahier des charges / à la roadmap

**Écarts hérités du Lot 0 — statut au Lot 1 :**

Trois éléments listés dans la description du Lot 0 de `docs/ROADMAP.md` avaient été volontairement écartés à la clôture du Lot 0. Le Lot 1 s'étant concentré strictement sur son propre périmètre (référentiels), aucune fonctionnalité anticipée d'un lot ultérieur (`CLAUDE.md` §20), ces trois points **restent ouverts** :

- **Authentification de démonstration** — **toujours non implémentée.** `User`, `Role`, `Permission`, `UserPermission` existent désormais comme entités référentielles (Lot 1), mais aucun mécanisme de connexion (login, session, jeton) n'est branché. `Application/Common/CurrentActor.cs` matérialise ce manque explicitement (valeur `"api-anonymous"` utilisée en `CreatedBy`/`UpdatedBy` en attendant une identité authentifiée réelle). Aucune policy d'autorisation ASP.NET Core combinant rôle/permission/périmètre organisationnel (`docs/ARCHITECTURE.md` §4) n'est donc encore appliquée sur les contrôleurs du Lot 1 — tous les endpoints sont actuellement ouverts. **Ce point doit être traité avant toute exposition hors environnement de développement.**
- **Rôles et permissions** — **partiellement traité.** Les entités `Role`, `Permission`, `UserPermission` existent et sont seedées (§6.2, `FINANCIAL_DATA_VIEW`), mais l'application effective des règles du cahier des charges §17 (seul un Administrateur peut attribuer le rôle Administrateur, un utilisateur ne peut pas retirer son propre dernier accès administrateur, etc.) n'est pas implémentée : elle dépend de l'authentification, toujours absente (point ci-dessus).
- **Design system** — **toujours non mis en place.** Aucun écran fonctionnel n'a été ajouté côté frontend au Lot 1 (voir ci-dessous) ; le sujet reste entier.

**Nouveaux écarts constatés au Lot 1 :**

- **Aucun écran frontend pour les référentiels du Lot 1.** Le lot a livré l'API (contrôleurs + Swagger) mais aucun composant React consommateur (`DataTable`, formulaires) ; le frontend n'a subi aucune modification ce lot. À statuer : écrans de référentiels en complément du Lot 1, ou différés à un lot ultérieur.
- **PostgreSQL et SQL Server toujours non testés en conditions réelles** (reconduction de l'écart du Lot 0) : la migration `AddLot1Referentials` est générée et compilable pour les deux providers (design-time), mais seule SQLite est validée par une application réelle de migration, via les tests d'intégration (`WebApplicationFactory`).
- **Champs et entités explicitement différés à des lots ultérieurs**, documentés en commentaire XML dans le code source concerné (aucune anticipation, `CLAUDE.md` §20) :
  - `Resource` : historique des TJM (`ResourceTjmHistory`, Lot 2) et variations de capacité historisées (`ResourceCapacityPeriod`, Lot 3) — seule la capacité par défaut est portée en Lot 1.
  - `Company` : historique des contrats (`CompanyContractHistory`, Lot 2) et rattachement ressource/société historisé (`ResourceCompanyAssignment`, Lot 2) — `Resource.CompanyId` reste un pointeur simple non historisé en Lot 1.
  - `Order` : lien vers `Project` (Lot 4), budget/dates ajustés et rallonges (`OrderExtension`, Lot 5), consommation en jours et coûts réel/contractuel/différentiel (calculés à partir des saisies de temps valorisées, Lot 2/3).
  - `ApplicationReference` : charge RUN/hors RUN, compteurs INC/CHG/PRB/RITM, projets associés — agrégats calculés à partir de `TimeEntry` (Lot 3) et `Project` (Lot 4), non modélisés en Lot 1.
  - `Settings` : `HolidayCalendar`, `ActivityType`, `AbsenceType`, `MilestoneType` restent des entités séparées du cahier des charges §30, différées aux Lots 3 et 4.
- **Décision de modélisation à noter (pas un écart fonctionnel) :** la section « Organisation » de la fiche utilisateur (cahier des charges §10.2 : département, service, équipe, société courante, commande par défaut, capacité) est portée par `Resource` et non dupliquée sur `User`, conformément à la distinction utilisateur/ressource actée au §10.1 (`User` = compte de connexion, `Resource` = personne planifiable). L'information reste accessible via `User.ResourceId`.

**Autres écarts / limites techniques constatés au Lot 0 (reconduits) :**

- **Scripts PowerShell incomplets par rapport à `CLAUDE.md` §19.** Seuls `install.ps1`, `build.ps1` et `publish.ps1` existent. `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `install-iis.ps1`, `purge-logs.ps1` restent à créer — ils supposent une cible de déploiement (serveur, base) qui n'existe pas encore. Non traité au Lot 1 (hors périmètre référentiels).
- **Entité `Application` renommée en `ApplicationReference`** (cahier des charges §15, §30) et service `ApplicationService` renommé `ApplicationReferenceService` (cahier des charges §31) — décision actée à la clôture de la phase documentaire pour éviter la collision avec le namespace `SafranTimeTracker.Application`. Vocabulaire fonctionnel inchangé. Appliquée de façon cohérente au Lot 1 (`ApplicationReferenceService`, `ApplicationsController`).

## Décisions actées

| Décision | Choix retenu | Date |
|---|---|---|
| Version .NET LTS | .NET 10 (SDK 10.0.302 testé) | 2026-07-15 |
| Mapping DTO ↔ entité | Mapster | 2026-07-15 |
| Mocking tests backend | NSubstitute | 2026-07-15 |
| Bibliothèque de graphiques frontend | Recharts | 2026-07-15 |
| Document `docs/DEPLOYMENT_WINDOWS.md` séparé | Non créé — contenu conservé dans `CLAUDE.md` §18-19 | 2026-07-15 |
| Collision entité `Application` / namespace `SafranTimeTracker.Application` | Entité renommée `ApplicationReference`, service `ApplicationReferenceService` | 2026-07-15 |
| Format de solution .NET | `.slnx` (nouveau format XML, par défaut sur le SDK .NET 10) | 2026-07-15 |
| Architecture des migrations EF Core multi-providers | Un projet de migrations dédié par provider sous `database/{provider}/` (pattern officiel Microsoft), plutôt qu'un assembly unique | 2026-07-15 |
| Version FluentAssertions | Épinglée à `7.2.2` (dernière version sous licence Apache 2.0) — la v8+ exige une licence commerciale payante (changement de licence Xceed, janvier 2025), incompatible avec un usage interne sans achat de licence | 2026-07-15 |
| Vulnérabilité `SQLitePCLRaw.lib.e_sqlite3` (NU1903, GHSA-2m69-gcr7-jv3q) | Épinglage explicite de `SQLitePCLRaw.bundle_e_sqlite3` en `2.1.12` (version corrigée) dans tous les projets référençant le provider SQLite | 2026-07-15 |
| Linter frontend | ESLint (flat config) + Prettier, à la place d'`oxlint` (nouveau défaut du template Vite) — conforme au choix déjà documenté dans `docs/CONVENTIONS.md` §9 | 2026-07-15 |
| Persistance Lot 1 | `EfRepository<T>` générique unique (`IRepository<T>`/`IReadRepository<T>`), pas de dépôt dédié par entité — le Lot 1 ne contient aucune règle de recherche métier complexe qui le justifierait | 2026-07-15 |
| Enregistrement des validateurs FluentValidation | `FluentValidation.DependencyInjectionExtensions` (scan automatique de l'assembly `Application`) plutôt qu'un enregistrement manuel par validateur | 2026-07-15 |
| Bootstrap logger Serilog dans `Program.cs` | Retiré (motif technique, pas fonctionnel) : le `Log.Logger` statique à deux phases entre en conflit avec `WebApplicationFactory`, qui réexécute `Program` à chaque instanciation d'hôte de test (« logger already frozen ») ; la configuration Serilog passe désormais uniquement par `builder.Host.UseSerilog(...)` | 2026-07-15 |
| Rattachement organisationnel de la fiche utilisateur (§10.2) | Porté par `Resource` (département, service, équipe, société courante, commande par défaut, capacité), pas dupliqué sur `User`, pour respecter la distinction utilisateur/ressource du §10.1 | 2026-07-15 |
| Application réelle des migrations dans les tests d'intégration | Le projet `SafranTimeTracker.Tests` référence directement `SafranTimeTracker.Migrations.Sqlite` (`ProjectReference`) pour que `WebApplicationFactory` applique les migrations SQLite réelles au démarrage de chaque test, plutôt qu'un simple `EnsureCreated()` | 2026-07-15 |

Décisions techniques ouvertes à ce jour : authentification de démonstration et design system (voir « Écarts connus » ci-dessus — hérités du Lot 0, non traités au Lot 1).

## Comment mettre à jour ce document

1. À la fin de chaque lot : changer le statut (`Non démarré` → `En cours` → `Terminé`), mettre à jour la date, cocher les cases correspondantes.
2. Un lot ne passe à `Terminé` que s'il est compilable, testé réellement et démontrable (règle de méthode, `CLAUDE.md` §20).
3. Tout écart avec `docs/ROADMAP.md` ou le cahier des charges est consigné dans la section « Écarts connus ».
