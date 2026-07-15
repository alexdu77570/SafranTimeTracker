# État d'avancement — SAFRAN TIME TRACKER

> Ce document reflète l'état **réel** du projet, à distinguer de `docs/ROADMAP.md` qui décrit le plan. Il doit être mis à jour à la fin de chaque lot (voir `CLAUDE.md` §20 — Méthode de travail par lots), et à chaque écart significatif constaté en cours de lot.

**Dernière mise à jour : 2026-07-15 — Lot 3 (Temps et capacité) terminé.**

## Phase préliminaire — Socle documentaire

**Statut : Terminé (2026-07-15).**

Cette phase n'est pas un lot numéroté du cahier des charges (§40) : elle précède le `Lot 0 — Fondations`. Elle a couvert uniquement la production et la validation de la documentation de cadrage (voir « État détaillé » ci-dessous).

## Synthèse des lots

| Lot | Contenu | Statut | Dernière mise à jour |
|---|---|---|---|
| Lot 0 | Fondations (périmètre technique restreint — voir ci-dessous) | Terminé | 2026-07-15 |
| Lot 1 | Référentiels | Terminé | 2026-07-15 |
| Lot 2 | Modèle financier | Terminé | 2026-07-15 |
| Lot 3 | Temps et capacité | Terminé | 2026-07-15 |
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

### Code — Lot 3 (2026-07-15)

- [x] **`ActivityType`** (§19.2, §30, entité obligatoire) enrichie de métadonnées de validation (`ReferenceRequired`, `ReferenceFormatRegex`, `ReferenceExample` — demande explicite) : la validation de référence d'une saisie (§19.3) est entièrement pilotée par la donnée, aucun type d'activité codé en dur dans un validateur. Classification RUN/hors RUN (§29.4) portée par `ActivityType.IsRun`, seedée pour les 13 types.
- [x] **`TimeEntry`** (§19.1) + **`TimeEntryFinancialSnapshot`** (§19.5, enfin persistée — différée depuis le Lot 2) : `TimeEntryService` orchestre la création/modification et revalorise via `FinancialCalculationService` (Lot 2, réutilisé sans modification de sa logique de calcul, seulement enrichi de `GetApplicableCompanyIdAsync` pour éviter une duplication de la recherche société-à-date, et de `SourceTjmPersonne`/`SourceContrat` désormais renseignés). Sous-objet financier du DTO omis sans `FINANCIAL_DATA_VIEW` — projection faite dans le service (`CLAUDE.md` §13), premier DTO « mixte » du projet (financier + non financier), contrairement aux endpoints intégralement financiers du Lot 2.
- [x] Règles métier bloquantes en 409 (`BusinessConflictException`) : ressource inactive, période close (via `Settings.DelaiModificationTempsJours`, Lot 1 — aucune nouvelle entité), incompatibilité commande/société à la date de la saisie (§13.4, désormais vérifiable).
- [x] **`ITimeEntryRevaluationService`** (`Application/Financial/Services/`) : interface posée dès ce lot (demande explicite) pour préparer le recalcul explicite audité du Lot 6 (§19.6) sans refonte d'architecture ; implémentation `TimeEntryRevaluationService` volontairement `NotImplementedException` (dépend d'`AuditLog`, Lot 6), enregistrée en DI — seul le corps de la méthode sera remplacé au Lot 6.
- [x] **`Absence`** (§23) : workflow Brouillon→Soumis→Validé/Refusé/Annulé (§23.3), piloté par `Settings.ActivationValidationAbsences` (Lot 1) — soumission avec workflow désactivé vaut validation immédiate, de sorte qu'`AvailabilityService` n'a besoin de filtrer que sur le statut Validé sans connaître ce paramètre. `AbsenceType`/statut modélisés en enum C#, absents de la liste d'entités minimum du §30 (contrairement à `ActivityType`) — correction d'une imprécision de l'écart Lot 1 qui les y annonçait à tort.
- [x] **`ResourceCapacityPeriod`** (§10.5, §30, entité obligatoire) et **`HolidayCalendar`** (§22.2, §30, entité obligatoire) créées — les deux étaient différées depuis le Lot 1.
- [x] **`AvailabilityService`** (`Application/Capacity/`) : capacité théorique/réelle/taux de disponibilité (§29.1-29.3, calcul jour par jour, variations de capacité + jours fériés + absences validées) et agrégation charge RUN/hors RUN (§29.4, pilotée par `ActivityType.IsRun`). `GetApplicableDailyCapacity` extraite en fonction pure publique, testée unitairement sans base de données (`CLAUDE.md` §14).
- [x] 6 contrôleurs REST sous `/api/v1` : `time-entries`, `absences` (+ `submit`/`validate`/`refuse`/`cancel`), `activity-types`, `resource-capacity-periods`, `holiday-calendar`, et `resources/{resourceId}/availability` (sous-ressource de composition forte, `docs/CONVENTIONS.md` §4). Aucun n'est gardé par `FINANCIAL_DATA_VIEW` au niveau contrôleur : la protection financière de `time-entries` est au niveau champ (voir ci-dessus), les autres ne portent aucune donnée financière.
- [x] Seed de démonstration idempotent (`Infrastructure/Persistence/Seed/Lot3Seed.cs`) : 13 types d'activité, 5 jours fériés français, une variation de capacité (temps partiel), 6 saisies de temps couvrant les cas notables (société interne, société externe avec différentiel positif, historisation TJM dans le temps sur la même ressource, valorisation incomplète), 4 absences dans des statuts différents, une ressource inactive dédiée au test du blocage §19.4.
- [x] Migration `AddLot3TimeAndCapacity` générée pour les **trois** providers. Appliquée et validée réellement sur SQLite ; PostgreSQL et SQL Server générés et compilables (design-time), non testés contre une instance réelle — écart reconduit.
- [x] Tests : 21 nouveaux cas ajoutés (4 tests unitaires purs `AvailabilityCapacityTests` sur `GetApplicableDailyCapacity` + 16 tests d'intégration `TimeAndCapacityTests` sur base SQLite réelle : snapshot correct interne/externe/incomplet, sous-objet financier omis sans permission, absence de recalcul rétroactif démontrée par l'historisation TJM au sein même des saisies, 409 commande/société incompatible, 409 ressource inactive, 409 période close, 400 référence manquante/format invalide, revalorisation après modification, workflow absence complet, 409 transition invalide, calcul de capacité/charge RUN-hors-RUN via l'API, 404 ressource inconnue — + 1 test `AbsenceWorkflowDisabledTests` isolé dans sa propre base pour la validation automatique quand le workflow est désactivé). Un bug de suivi EF Core a été détecté et corrigé pendant l'écriture des tests (voir « Décisions actées »). Aucune règle capacitaire testée sur repository mocké (`CLAUDE.md` §14). Suite complète : **59/59 réussis** (21 nouveaux + 38 des Lots 0-2).
- [x] Vérification finale de clôture de lot : `dotnet build` (solution complète, 8 projets) → 0 avertissement, 0 erreur ; `dotnet test` → 59/59 ; `npm run build` (frontend, inchangé ce lot) → succès ; `npm run lint` / `npm run format:check` (frontend) → 0 erreur.

## Écarts connus par rapport au cahier des charges / à la roadmap

**Écarts hérités du Lot 0 — statut au Lot 3 :**

Trois éléments listés dans la description du Lot 0 de `docs/ROADMAP.md` avaient été volontairement écartés à la clôture du Lot 0. Statut réévalué après le Lot 3 :

- **Authentification de démonstration** — **toujours partielle, inchangée au Lot 3.** `DemoCurrentUserProvider` (`ICurrentUser`) résout l'appelant depuis l'en-tête `X-Demo-User`, vérifié en base — **volontairement pas** un login, un JWT, une session, un cookie ou ASP.NET Identity (contrainte explicite actée au Lot 2). `TimeEntryService`/`AbsenceService`/les services référentiels du Lot 3 utilisent tous `ICurrentUser.Identifier` pour `CreatedBy`/`UpdatedBy`.
- **Rôles et permissions** — **toujours partiel.** Aucun endpoint de gestion des rôles/permissions n'existe. Le Lot 3 n'ajoute aucune garde de permission nouvelle : les saisies de temps/absences/disponibilités ne portent pas de donnée financière propre (seul le sous-objet `FinancialSnapshot` de `TimeEntryDto` l'est, et reste protégé par `FINANCIAL_DATA_VIEW` au niveau champ). La restriction « seul le responsable hiérarchique peut valider une absence » (§23.3) n'est **pas** appliquée : `AbsenceService.ValidateAsync`/`RefuseAsync` sont fonctionnellement corrects (machine à états) mais accessibles à tout appelant, faute de périmètre organisationnel construit (écart déjà noté au Lot 2).
- **Design system** — **toujours non mis en place.** Aucun écran fonctionnel n'a été ajouté côté frontend au Lot 3 (voir ci-dessous) ; le sujet reste entier.

**Nouveaux écarts constatés au Lot 3 :**

- **Aucun écran frontend pour le temps et la capacité.** Comme aux Lots 1 et 2, le lot a livré l'API (contrôleurs + Swagger) mais aucun composant React consommateur ; le frontend n'a subi aucune modification ce lot.
- **Recalcul explicite audité (§19.6) différé au Lot 6**, conformément au plan validé : `ITimeEntryRevaluationService` (interface + implémentation `NotImplementedException`) est posée et enregistrée en DI pour qu'aucun contrôleur, service ou règle de sécurité n'ait à changer lors de son implémentation réelle, qui dépend d'`AuditLog` (permission dédiée, confirmation, motif, conservation de l'ancienne valeur en audit).
- **`AuditLog` (Lot 6) toujours absent** : les corrections de saisies de temps (`PUT /time-entries/{id}`) et les décisions d'absence sont tracées uniquement via `UpdatedAt`/`UpdatedBy` (`AuditableEntity`) — même écart qu'aux Lots 1 et 2, pour les mêmes raisons.
- **PostgreSQL et SQL Server toujours non testés en conditions réelles** (reconduction de l'écart du Lot 0) : la migration `AddLot3TimeAndCapacity` est générée et compilable pour les deux providers (design-time), mais seule SQLite est validée par une application réelle de migration.
- **Simplifications de modélisation par rapport à une lecture littérale du §19.1/§22/§29, documentées en commentaire XML dans le code :**
  - Le champ « utilisateur » de la saisie de temps (§19.1, distinct de « ressource ») n'est pas modélisé séparément : `TimeEntry.ResourceId` porte la personne concernée, `CreatedBy`/`UpdatedBy` (`ICurrentUser`) portent l'auteur réel — un champ dédié serait redondant dans le cas courant et le cahier ne précise pas de cas où il diffère.
  - Le champ « semaine » (§19.1) n'est pas stocké : calculé à la volée depuis `Date` pour le filtre (§19.4), pour éviter une donnée dérivée pouvant devenir incohérente.
  - « Indisponibilités » (§29.2) n'a pas de mécanisme dédié : couvert par `AbsenceType.Indisponible`, un type d'absence validée comme un autre.
  - « Jour ouvré » (§29.1) est interprété comme lundi-vendredi (cohérent avec `Settings.JoursOuvresParSemaine = 5` par défaut), pas un calendrier de jours ouvrés paramétrable par ressource/pays.
  - `DashboardKpi` (§30, catégorie « Paramétrage ») **différé** : aucun lien avec « saisie des temps/absences/disponibilités/calculs capacité/charges RUN-hors RUN » (ROADMAP Lot 3), relève du tableau de bord (§25, plutôt Lot 5).

**Écarts constatés au Lot 2 (reconduits, statut réévalué après le Lot 3) :**

- **`TimeEntryFinancialSnapshot` : existe désormais** (voir « Code — Lot 3 » ci-dessus) — écart résolu.
- **`sourceTjmPersonne`/`sourceContrat` : renseignés désormais** dans `FinancialCalculationResultDto` et persistés dans le snapshot (valeur constante "ResourceTjmHistory"/"CompanyContractHistory", une seule source existant à ce jour) — écart résolu.
- **`AuditLog` (Lot 6) toujours absent** pour les corrections de TJM/contrat/rattachement — écart reconduit, inchangé au Lot 3 (hors périmètre).
- **Autorisation via `RequirePermissionAttribute`** — choix reconduit sans changement au Lot 3 (voir « Décisions actées » du Lot 2).

**Écarts constatés au Lot 1 (reconduits, statut réévalué après le Lot 3) :**

- **`ResourceCapacityPeriod` : existe désormais** (voir « Code — Lot 3 » ci-dessus) — écart résolu. Points encore différés :
  - `Order` : lien vers `Project` (Lot 4), budget/dates ajustés et rallonges (`OrderExtension`, Lot 5), consommation en jours et coûts réel/contractuel/différentiel de la commande (agrégats de saisies valorisées, Lot 5).
  - `ApplicationReference` : charge RUN/hors RUN, compteurs INC/CHG/PRB/RITM, projets associés — agrégats calculables dès à présent à partir de `TimeEntry` (Lot 3), mais projets associés dépend de `Project` (Lot 4) ; non modélisés à ce jour (pas d'endpoint d'agrégation par application).
  - `Settings` : `MilestoneType` reste une entité séparée du cahier des charges §30, différée au Lot 4. (`HolidayCalendar` et `ActivityType` existent désormais ; `AbsenceType` ne figure en réalité **pas** dans la liste minimum du §30 — l'écart Lot 1 l'annonçait à tort, corrigé au Lot 3, voir ci-dessus.)
- **Décision de modélisation à noter (pas un écart fonctionnel) :** la section « Organisation » de la fiche utilisateur (cahier des charges §10.2 : département, service, équipe, société courante, commande par défaut, capacité) est portée par `Resource` et non dupliquée sur `User`, conformément à la distinction utilisateur/ressource actée au §10.1 (`User` = compte de connexion, `Resource` = personne planifiable). L'information reste accessible via `User.ResourceId`.

**Autres écarts / limites techniques constatés au Lot 0 (reconduits) :**

- **Scripts PowerShell incomplets par rapport à `CLAUDE.md` §19.** Seuls `install.ps1`, `build.ps1` et `publish.ps1` existent. `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `install-iis.ps1`, `purge-logs.ps1` restent à créer — ils supposent une cible de déploiement (serveur, base) qui n'existe pas encore. Non traité aux Lots 1, 2 et 3 (hors périmètre de chacun).
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
| Métadonnées de validation de référence sur `ActivityType` (Lot 3, demande explicite) | `ReferenceRequired`/`ReferenceFormatRegex`/`ReferenceExample` portés par la donnée plutôt que codés en dur dans un validateur — un seul validateur (`TimeEntryCreateRequestValidator`/`UpdateRequestValidator`) sert tous les types d'activité présents et futurs | 2026-07-15 |
| Préparation du recalcul explicite (Lot 3, demande explicite) | `ITimeEntryRevaluationService` posée dès le Lot 3 (implémentation `NotImplementedException`, enregistrée en DI) pour que le Lot 6 (§19.6, dépend d'AuditLog) n'exige aucune refonte de contrôleur, service ou règle de sécurité | 2026-07-15 |
| Sous-objet financier omis au niveau champ (Lot 3) | `TimeEntryDto.FinancialSnapshot` projeté à `null` dans `TimeEntryService` sans `FINANCIAL_DATA_VIEW` (CLAUDE.md §13) — premier DTO mixte financier/non financier du projet, à la différence des endpoints intégralement financiers du Lot 2 gardés par `RequirePermissionAttribute` | 2026-07-15 |
| Clé du snapshot financier | `TimeEntryFinancialSnapshot.Id` réutilise `TimeEntryId` (clé partagée) plutôt qu'un `Guid` indépendant — corrige un bug de suivi EF Core découvert pendant les tests : une entité chargée via `IReadRepository.Query()` (`AsNoTracking`) ne peut pas être mutée puis sauvegardée par `SaveChangesAsync()`, contrairement à `GetByIdAsync()` (suivi) | 2026-07-15 |
| Workflow d'absence et paramètre désactivé (§23.3) | Soumission avec `Settings.ActivationValidationAbsences = false` vaut validation immédiate (transition directe vers Validé) plutôt qu'un statut intermédiaire non prévu par le cahier — `AvailabilityService` n'a alors besoin de filtrer que sur le statut Validé, sans connaître ce paramètre | 2026-07-15 |
| Règle de blocage « période close » (§19.4) | Réutilise `Settings.DelaiModificationTempsJours` (Lot 1) plutôt qu'une nouvelle entité de calendrier de clôture | 2026-07-15 |

Décisions techniques ouvertes à ce jour : authentification de démonstration complète (écran de connexion, notion de session applicative — le mécanisme des Lots 2/3 reste un en-tête de démonstration), design system, et restriction de la validation d'absence au seul responsable hiérarchique (voir « Écarts connus » ci-dessus).

## Comment mettre à jour ce document

1. À la fin de chaque lot : changer le statut (`Non démarré` → `En cours` → `Terminé`), mettre à jour la date, cocher les cases correspondantes.
2. Un lot ne passe à `Terminé` que s'il est compilable, testé réellement et démontrable (règle de méthode, `CLAUDE.md` §20).
3. Tout écart avec `docs/ROADMAP.md` ou le cahier des charges est consigné dans la section « Écarts connus ».
