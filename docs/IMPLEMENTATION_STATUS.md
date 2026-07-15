# État d'avancement — SAFRAN TIME TRACKER

> Ce document reflète l'état **réel** du projet, à distinguer de `docs/ROADMAP.md` qui décrit le plan. Il doit être mis à jour à la fin de chaque lot (voir `CLAUDE.md` §20 — Méthode de travail par lots), et à chaque écart significatif constaté en cours de lot.

**Dernière mise à jour : 2026-07-15 — Lot 0 (périmètre technique restreint) terminé.**

## Phase préliminaire — Socle documentaire

**Statut : Terminé (2026-07-15).**

Cette phase n'est pas un lot numéroté du cahier des charges (§40) : elle précède le `Lot 0 — Fondations`. Elle a couvert uniquement la production et la validation de la documentation de cadrage (voir « État détaillé » ci-dessous).

## Synthèse des lots

| Lot | Contenu | Statut | Dernière mise à jour |
|---|---|---|---|
| Lot 0 | Fondations (périmètre technique restreint — voir ci-dessous) | Terminé | 2026-07-15 |
| Lot 1 | Référentiels | Non démarré | — |
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

## Écarts connus par rapport au cahier des charges / à la roadmap

**Périmètre du Lot 0 volontairement restreint par rapport à `docs/ROADMAP.md`.** À la demande explicite formulée pour cette exécution du Lot 0, les trois éléments suivants — pourtant listés dans la description du Lot 0 de `docs/ROADMAP.md` — n'ont **pas** été traités, car ils empiètent sur du contenu fonctionnel ou sur des entités métier du cahier des charges :

- **Authentification de démonstration** (non implémentée — dépend de `User`, une entité métier du cahier des charges §30).
- **Rôles et permissions** (non implémentés — `Role`, `Permission`, `UserPermission` sont des entités métier du cahier des charges §30).
- **Design system** (non mis en place — l'identité visuelle du cahier des charges §8.1 relève de l'habillage fonctionnel, pas de la fondation technique ; seule une page technique minimale sans style de marque a été créée).

Ces trois points restent à traiter — soit en complément du Lot 0, soit en ouverture du Lot 1 — avant de considérer la fondation applicative complète au sens de `docs/ROADMAP.md`.

**Autres écarts / limites techniques constatés :**

- **Scripts PowerShell incomplets par rapport à `CLAUDE.md` §19.** Seuls `install.ps1`, `build.ps1` et `publish.ps1` ont été créés (périmètre explicitement demandé). `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `install-iis.ps1`, `purge-logs.ps1` restent à créer — ils supposent une cible de déploiement (serveur, base) qui n'existe pas encore.
- **PostgreSQL et SQL Server non testés en conditions réelles.** Aucune instance locale n'était disponible dans l'environnement de build ; les migrations sont générées et compilables (design-time), mais seule SQLite a été validée par une application réelle de migration (`dotnet ef database update`) sur un fichier de base.
- **Entité `Application` renommée en `ApplicationReference`** (cahier des charges §15, §30) et service `ApplicationService` renommé `ApplicationReferenceService` (cahier des charges §31) — décision actée à la clôture de la phase documentaire pour éviter la collision avec le namespace `SafranTimeTracker.Application`. Vocabulaire fonctionnel inchangé.

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

Aucune décision technique ouverte à ce jour, en dehors des trois points de périmètre listés dans « Écarts connus » ci-dessus.

## Comment mettre à jour ce document

1. À la fin de chaque lot : changer le statut (`Non démarré` → `En cours` → `Terminé`), mettre à jour la date, cocher les cases correspondantes.
2. Un lot ne passe à `Terminé` que s'il est compilable, testé réellement et démontrable (règle de méthode, `CLAUDE.md` §20).
3. Tout écart avec `docs/ROADMAP.md` ou le cahier des charges est consigné dans la section « Écarts connus ».
