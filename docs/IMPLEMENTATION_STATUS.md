# État d'avancement — SAFRAN TIME TRACKER

> Ce document reflète l'état **réel** du projet, à distinguer de `docs/ROADMAP.md` qui décrit le plan. Il doit être mis à jour à la fin de chaque lot (voir `CLAUDE.md` §20 — Méthode de travail par lots), et à chaque écart significatif constaté en cours de lot.

**Dernière mise à jour : 2026-07-15 — Lot 2 (Modèle financier) terminé.**

## Phase préliminaire — Socle documentaire

**Statut : Terminé (2026-07-15).**

Cette phase n'est pas un lot numéroté du cahier des charges (§40) : elle précède le `Lot 0 — Fondations`. Elle a couvert uniquement la production et la validation de la documentation de cadrage (voir « État détaillé » ci-dessous).

## Synthèse des lots

| Lot | Contenu | Statut | Dernière mise à jour |
|---|---|---|---|
| Lot 0 | Fondations (périmètre technique restreint — voir ci-dessous) | Terminé | 2026-07-15 |
| Lot 1 | Référentiels | Terminé | 2026-07-15 |
| Lot 2 | Modèle financier | Terminé | 2026-07-15 |
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

### Code — Lot 2 (2026-07-15)

- [x] **`ICurrentUser`** (`Application/Common/Security/ICurrentUser.cs`) : seule abstraction dont dépendent contrôleurs et services pour vérifier une permission ou tracer un auteur (`IsAuthenticated`, `UserId`, `Identifier`, `Permissions`, `HasPermission`). Implémentation active **`DemoCurrentUserProvider`** (`Api/Security/`) : résout l'utilisateur depuis l'en-tête `X-Demo-User` (identifiant), vérifié en base contre `User`/`UserPermission`/`Permission` réels — explicitement **pas** un login, pas de JWT, pas de session, pas de cookie, pas ASP.NET Identity (demande explicite). Enregistrée en un seul point (`Program.cs`) : remplaçable par un provider LDAP/OIDC futur sans modifier un contrôleur, un service métier ou une règle de sécurité.
- [x] Garde de permission **`RequirePermissionAttribute`** (`Api/Security/`) : filtre d'autorisation MVC (`IAsyncAuthorizationFilter`) ne dépendant que d'`ICurrentUser` — volontairement pas le système `Authorization`/`ClaimsPrincipal` d'ASP.NET Core, pour rester strictement découplé de toute notion d'authentification. Posée sur les 4 contrôleurs financiers ; sans la permission `FINANCIAL_DATA_VIEW`, le contrôleur n'est jamais atteint (403), jamais un simple masquage côté client (`CLAUDE.md` §12, §17).
- [x] Entités Domain historisées, conformément à `docs/DATABASE.md` §5 et au cahier des charges : `ResourceTjmHistory` (§11), `CompanyContractHistory` (§12.3), `ResourceCompanyAssignment` (§12.2, remplace `Resource.CompanyId` du Lot 1 comme source de vérité pour le calcul financier — `Resource.CompanyId` reste un pointeur d'affichage non historisé). Jeton de concurrence optimiste (`ConcurrencyStamp`, type `Guid` géré applicativement, portable sur les 3 providers) sur TJM et contrat, conformément à `CLAUDE.md` §11.
- [x] **`FinancialCalculationService`** (`Application/Financial/Services/`), seul point de calcul (`docs/ARCHITECTURE.md` §2) : `tempsJours = heuresSaisies / heuresParJour` (§20.1), `coutReel = tempsJours × tjmPersonneSnapshot` (§20.2), `coutContractuel = tempsJours × tjmContratSnapshot` si société externe avec contrat valide, sinon non applicable (§12.5, §20.3), `differentiel = coutContractuel - coutReel` (§20.4). Recherche du TJM/contrat/rattachement **à la date demandée**, jamais à la date courante. Absence de TJM → `FinancialValuationStatus.Incomplete`, aucun montant inventé (§11.4). Ne persiste rien : `TimeEntryFinancialSnapshot` référence `TimeEntry` (Lot 3) et reste différé — ce service est directement réutilisable tel quel par le `TimeEntryService` du Lot 3.
- [x] Conflits métier (chevauchement de périodes, concurrence optimiste) traduits en **409** via `BusinessConflictException` + `BusinessConflictExceptionHandler` (`IExceptionHandler`, `Api/Middleware/`) — jamais un 400, conformément à `CLAUDE.md` §12. Validation de format/existence FK/montant strictement positif via FluentValidation (400) ; chevauchement vérifié côté service (409).
- [x] 4 contrôleurs REST sous `/api/v1`, tous gardés par `FINANCIAL_DATA_VIEW`, endpoint entièrement refusé (403) plutôt qu'omission de champ à la pièce (ces endpoints sont exclusivement financiers) : `resource-tjm-history`, `company-contracts`, `resource-company-assignments` (GET liste/détail, POST création, PUT correction/clôture avec motif obligatoire), `financial-calculations/preview` (POST, calcul à la demande sans écriture).
- [x] Seed de démonstration idempotent (`Infrastructure/Persistence/Seed/Lot2Seed.cs`) : une société externe (Externe Conseil) et son contrat, 4 historiques TJM dont un cas d'historisation (BERNARD ouvert société interne, LEGRAND ouvert société externe avec contrat, GEORGES deux périodes successives non chevauchantes), MISHRA volontairement sans TJM (cas de valorisation incomplète, `docs/DATABASE.md` §7), 3 rattachements ressource/société.
- [x] Migration `AddLot2FinancialModel` générée pour les **trois** providers. Appliquée et validée réellement pour SQLite (`dotnet ef database update` en chaîne sur les 3 migrations + tests d'intégration `WebApplicationFactory`) ; PostgreSQL et SQL Server générés et compilables (design-time), non testés contre une instance réelle — écart reconduit du Lot 0/1.
- [x] Tests : 16 nouveaux cas ajoutés (5 tests unitaires purs `DateRangeOverlapTests` — chevauchement/adjacence/inclusion, aucune base de données, logique partagée par les 3 historiques — + 11 tests d'intégration `FinancialModelTests` sur base SQLite réelle : 403 sans en-tête, 403 avec permission manquante, 200 avec données financières correctes, calcul société interne/externe/valorisation incomplète, historisation TJM dans le temps, 409 chevauchement, 400 montant invalide, clôture puis nouvelle période). Aucune règle financière testée sur repository mocké (`CLAUDE.md` §14). Suite complète : **38/38 réussis** (16 nouveaux + 22 des Lots 0/1).
- [x] Vérification finale de clôture de lot : `dotnet build` (solution complète, 8 projets) → 0 avertissement, 0 erreur ; `dotnet test` → 38/38 ; `npm run build` (frontend, inchangé ce lot) → succès ; `npm run lint` (frontend) → 0 erreur.

## Écarts connus par rapport au cahier des charges / à la roadmap

**Écarts hérités du Lot 0 — statut au Lot 2 :**

Trois éléments listés dans la description du Lot 0 de `docs/ROADMAP.md` avaient été volontairement écartés à la clôture du Lot 0. Statut réévalué après le Lot 2 :

- **Authentification de démonstration** — **partiellement traitée au Lot 2, sur demande explicite.** `DemoCurrentUserProvider` (`ICurrentUser`) résout l'appelant depuis l'en-tête `X-Demo-User`, vérifié en base — **volontairement pas** un login, un JWT, une session, un cookie ou ASP.NET Identity (contrainte explicite de cette demande). Ce n'est donc pas l'« authentification de démonstration » complète attendue par `docs/ROADMAP.md` (Lot 0) ni par `CLAUDE.md` §17 (pas d'écran de connexion, pas de notion de compte "connecté" côté frontend), mais elle fournit une identité vérifiable côté serveur suffisante pour garder `FINANCIAL_DATA_VIEW` réellement (pas un masquage visuel). `Application/Common/CurrentActor.cs` reste le repli pour les couches non encore migrées vers `ICurrentUser` (services du Lot 1) ou pour un appel sans en-tête de démonstration.
- **Rôles et permissions** — **partiellement traité.** `RequirePermissionAttribute` applique désormais réellement `FINANCIAL_DATA_VIEW` sur les 4 contrôleurs financiers du Lot 2 (§17). Les règles complémentaires du cahier des charges §17 (seul un Administrateur peut attribuer le rôle Administrateur, un utilisateur ne peut pas retirer son propre dernier accès administrateur) restent non implémentées : hors périmètre du Lot 2 (aucun endpoint de gestion des rôles/permissions n'a été ajouté).
- **Design system** — **toujours non mis en place.** Aucun écran fonctionnel n'a été ajouté côté frontend au Lot 2 (voir ci-dessous) ; le sujet reste entier.

**Nouveaux écarts constatés au Lot 2 :**

- **Aucun écran frontend pour le modèle financier.** Comme au Lot 1, le lot a livré l'API (contrôleurs + Swagger) mais aucun composant React consommateur ; le frontend n'a subi aucune modification ce lot.
- **`TimeEntryFinancialSnapshot` (entité persistée) différée au Lot 3**, conformément à `docs/DATABASE.md` §5 : elle référence `TimeEntry`, qui n'existe pas encore. `FinancialCalculationService` calcule le sous-ensemble testable indépendamment d'une saisie et est conçu pour être réutilisé tel quel par le `TimeEntryService` du Lot 3 afin de produire ce snapshot.
- **`AuditLog` (Lot 6) toujours absent** : les corrections de TJM/contrat/rattachement (`PUT`) sont tracées uniquement via `UpdatedAt`/`UpdatedBy` (`AuditableEntity`) et un motif obligatoire en entrée (`Reason` pour TJM, `Comment` réutilisé comme motif pour contrat/rattachement, faute de champ dédié au cahier des charges §12.2/§12.3) — pas de conservation structurée de l'ancienne valeur, pas de journal d'audit métier consultable, conformément à l'écart déjà accepté au Lot 1 pour les mêmes raisons.
- **`sourceTjmPersonne`/`sourceContrat` (docs/DATABASE.md §5) non repris dans `FinancialCalculationResultDto`** : une seule source existe à ce jour (`ResourceTjmHistory`/`CompanyContractHistory`), ces champs de traçabilité n'apportent rien tant qu'aucune source alternative n'existe — à ajouter lors de la persistance réelle du snapshot (Lot 3) si le besoin se confirme.
- **PostgreSQL et SQL Server toujours non testés en conditions réelles** (reconduction de l'écart du Lot 0/1) : la migration `AddLot2FinancialModel` est générée et compilable pour les deux providers (design-time), mais seule SQLite est validée par une application réelle de migration.
- **Autorisation via `RequirePermissionAttribute` plutôt que le système `Authorization` natif d'ASP.NET Core** (policies `IAuthorizationHandler`/`ClaimsPrincipal`) : choix délibéré pour que la garde ne dépende que d'`ICurrentUser`, comme demandé explicitement, sans nécessiter `AddAuthentication()`. À réévaluer si un futur provider (OIDC/LDAP) alimente un `ClaimsPrincipal` réel — la migration vers le système natif resterait alors possible sans changer les règles de sécurité elles-mêmes, seulement leur mécanique d'évaluation.

**Écarts constatés au Lot 1 (reconduits, statut réévalué après le Lot 2) :**

- **Champs et entités explicitement différés à des lots ultérieurs**, documentés en commentaire XML dans le code source concerné (aucune anticipation, `CLAUDE.md` §20) — mis à jour : `ResourceTjmHistory`, `CompanyContractHistory` et `ResourceCompanyAssignment` (marqués « Lot 2 » au Lot 1) **existent désormais** (voir « Code — Lot 2 » ci-dessus) ; les points suivants restent différés :
  - `Resource` : variations de capacité historisées (`ResourceCapacityPeriod`, Lot 3) — seule la capacité par défaut est portée à ce jour.
  - `Order` : lien vers `Project` (Lot 4), budget/dates ajustés et rallonges (`OrderExtension`, Lot 5), consommation en jours et coûts réel/contractuel/différentiel de la commande (agrégats de saisies valorisées, Lot 3/5).
  - `ApplicationReference` : charge RUN/hors RUN, compteurs INC/CHG/PRB/RITM, projets associés — agrégats calculés à partir de `TimeEntry` (Lot 3) et `Project` (Lot 4), non modélisés à ce jour.
  - `Settings` : `HolidayCalendar`, `ActivityType`, `AbsenceType`, `MilestoneType` restent des entités séparées du cahier des charges §30, différées aux Lots 3 et 4.
- **Décision de modélisation à noter (pas un écart fonctionnel) :** la section « Organisation » de la fiche utilisateur (cahier des charges §10.2 : département, service, équipe, société courante, commande par défaut, capacité) est portée par `Resource` et non dupliquée sur `User`, conformément à la distinction utilisateur/ressource actée au §10.1 (`User` = compte de connexion, `Resource` = personne planifiable). L'information reste accessible via `User.ResourceId`.

**Autres écarts / limites techniques constatés au Lot 0 (reconduits) :**

- **Scripts PowerShell incomplets par rapport à `CLAUDE.md` §19.** Seuls `install.ps1`, `build.ps1` et `publish.ps1` existent. `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `install-iis.ps1`, `purge-logs.ps1` restent à créer — ils supposent une cible de déploiement (serveur, base) qui n'existe pas encore. Non traité aux Lots 1 et 2 (hors périmètre référentiels/modèle financier).
- **Entité `Application` renommée en `ApplicationReference`** (cahier des charges §15, §30) et service `ApplicationService` renommé `ApplicationReferenceService` (cahier des charges §31) — décision actée à la clôture de la phase documentaire pour éviter la collision avec le namespace `SafranTimeTracker.Application`. Vocabulaire fonctionnel inchangé. Appliquée de façon cohérente aux Lots 1 et 2 (`ApplicationReferenceService`, `ApplicationsController`).

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
| Identité de démonstration (Lot 2) | Abstraction `ICurrentUser` (Application) + implémentation `DemoCurrentUserProvider` (Api, en-tête `X-Demo-User` résolu contre `User`/`UserPermission` réels) — explicitement pas de login, JWT, session, cookie ni ASP.NET Identity (demande explicite) ; remplaçable par un provider LDAP/OIDC sans toucher aux contrôleurs, services ou règles de sécurité | 2026-07-15 |
| Garde de permission financière (Lot 2) | `RequirePermissionAttribute` (`IAsyncAuthorizationFilter` MVC) ne dépendant que d'`ICurrentUser`, plutôt que le système `Authorization`/`ClaimsPrincipal` natif d'ASP.NET Core, pour rester strictement découplé de toute authentification | 2026-07-15 |
| Conflits métier (chevauchement, concurrence) | `BusinessConflictException` + `IExceptionHandler` dédié → 409 ProblemDetails (CLAUDE.md §12), distinct des 400 FluentValidation (format/existence) | 2026-07-15 |
| Jeton de concurrence optimiste multi-provider | Propriété `Guid ConcurrencyStamp` gérée applicativement (`.IsConcurrencyToken()`, régénérée à chaque `UpdateAsync`) plutôt qu'un `rowversion`/`[Timestamp]` natif SQL Server, pour rester identique sur les 3 providers (`docs/DATABASE.md` §1) | 2026-07-15 |
| Rattachement société utilisé pour le calcul financier | `ResourceCompanyAssignment` (historisé, Lot 2) devient la source de vérité pour `FinancialCalculationService` ; `Resource.CompanyId` (Lot 1) reste un pointeur d'affichage "société courante" non historisé, jamais utilisé par le calcul | 2026-07-15 |
| Persistance du snapshot financier | Non persistée au Lot 2 (`TimeEntryFinancialSnapshot` référence `TimeEntry`, Lot 3) ; `FinancialCalculationService` expose un DTO de résultat calculé à la demande, réutilisable tel quel par le Lot 3 | 2026-07-15 |

Décisions techniques ouvertes à ce jour : authentification de démonstration complète (écran de connexion, notion de session applicative — le mécanisme du Lot 2 reste un en-tête de démonstration) et design system (voir « Écarts connus » ci-dessus).

## Comment mettre à jour ce document

1. À la fin de chaque lot : changer le statut (`Non démarré` → `En cours` → `Terminé`), mettre à jour la date, cocher les cases correspondantes.
2. Un lot ne passe à `Terminé` que s'il est compilable, testé réellement et démontrable (règle de méthode, `CLAUDE.md` §20).
3. Tout écart avec `docs/ROADMAP.md` ou le cahier des charges est consigné dans la section « Écarts connus ».
