# SAFRAN TIME TRACKER

Plateforme interne de pilotage des temps, charges, capacités, projets, jalons, ressources, commandes, budgets et valorisations financières, destinée à une organisation **Production Applicative / RUN / MCO / Projets**.

> **Statut actuel : Lots 0 à 13 terminés, Lot 14 (Audit, Refactoring et Qualité) en cours.** Backend ASP.NET Core, frontend React et migrations EF Core (SQLite/PostgreSQL/SQL Server) existent et sont fonctionnels. Roadmap complète jusqu'à la V1 figée dans `docs/ROADMAP.md` — voir `docs/IMPLEMENTATION_STATUS.md` pour l'état réel détaillé.

## Présentation

SAFRAN TIME TRACKER doit fournir un cockpit unique permettant de répondre à des questions telles que :

- Qui travaille sur quoi, et combien de temps est consacré au RUN et au hors RUN ?
- Quelle est la charge réelle par application, projet, service, équipe ou ressource ?
- Quels projets dérivent en charge, en budget ou en planning ?
- Quel est le coût réel, le coût contractuel et le différentiel des activités ?

L'application est conçue comme un **produit interne pérenne**, avec une exigence de fiabilité, de traçabilité et d'historisation des données à incidence financière, capacitaire ou organisationnelle.

## Documentation

| Document | Contenu |
|---|---|
| `docs/Cahier_des_charges_SAFRAN_TIME_TRACKER_v2.1_Windows_Server.md` | Référence fonctionnelle unique : besoins, règles métier, exigences de sécurité, critères de recette. |
| `CLAUDE.md` | Référence technique permanente : architecture, technologies, conventions, règles de développement, Git, déploiement. |
| `docs/ARCHITECTURE.md` | Architecture logicielle détaillée (couches backend, structure frontend, hébergement, configuration). |
| `docs/DATABASE.md` | Modèle de données, stratégie de providers, règles d'historisation et de migration. |
| `docs/CONVENTIONS.md` | Conventions de code détaillées avec exemples (nommage, style, organisation des fichiers). |
| `docs/ROADMAP.md` | Découpage du projet en lots livrables. |
| `docs/IMPLEMENTATION_STATUS.md` | Avancement réel du projet, lot par lot. |

## Architecture cible (résumé)

- **Frontend** : React + TypeScript + Vite, servi en fichiers statiques (IIS), appels API en URL relative (`/api/v1`).
- **Backend** : ASP.NET Core Web API en couches (`Domain`, `Application`, `Infrastructure`, `Api`), Entity Framework Core.
- **Base de données** : PostgreSQL (principal), Microsoft SQL Server (alternatif), SQLite (développement et tests uniquement) — provider sélectionné par configuration.
- **Déploiement cible** : Windows Server + IIS, scripts PowerShell automatisés, chemins normalisés sous `E:\appl`, `E:\certificats`, `E:\CD_INSTALL`, `E:\data`.

Détails complets : `docs/ARCHITECTURE.md` et `CLAUDE.md`.

## Structure du dépôt

```text
SafranTimeTracker/
├── docs/           Documentation fonctionnelle et technique
├── backend/        SafranTimeTracker.{Domain,Application,Infrastructure,Api,Tests}
├── frontend/       safran-time-tracker-web (React + TypeScript + Vite)
├── database/       Migrations EF Core par provider (postgresql, sqlite, sqlserver)
├── deploy/         Scripts PowerShell et configuration IIS (Lot 17 — Industrialisation)
├── scripts/        Scripts d'outillage (dont scripts/ci/ pour la CI)
├── .github/        Pipeline GitHub Actions (build/tests/couverture)
├── CLAUDE.md
├── README.md
└── .gitignore
```

## Méthode de travail

Le projet est développé **par lots successifs** (voir `docs/ROADMAP.md`, figée jusqu'à la V1 : Lot 14 Audit/Refactoring/Qualité → Lot 15 Complétude fonctionnelle & Administration → Lot 16 UX/Responsive/Performance → Lot 17 Industrialisation → Lot 18 Pré-production → Lot 19 Mise en production). Chaque lot ne passe au suivant que lorsqu'il produit une version **compilable, testée et démontrable**. L'avancement réel est suivi dans `docs/IMPLEMENTATION_STATUS.md`.

## Démarrage

- Backend : `dotnet build`, `dotnet test --project backend/SafranTimeTracker.Tests` depuis la racine du dépôt.
- Frontend : `cd frontend/safran-time-tracker-web && npm install && npm run dev`.
- Base de données de développement : SQLite (`backend/SafranTimeTracker.Api/App_Data`), migrations appliquées via `dotnet ef database update --project database/sqlite --context AppDbContext`.

Procédure d'installation Windows Server/IIS complète : prévue au Lot 17 (Industrialisation).

## Usage

Projet interne. Toute contribution doit respecter `CLAUDE.md` (règles techniques) et le cahier des charges (règles fonctionnelles).
