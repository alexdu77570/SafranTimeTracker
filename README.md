# SAFRAN TIME TRACKER

Plateforme interne de pilotage des temps, charges, capacités, projets, jalons, ressources, commandes, budgets et valorisations financières, destinée à une organisation **Production Applicative / RUN / MCO / Projets**.

> **Statut actuel : socle documentaire.** Aucun code métier n'a encore été généré (pas de projet .NET, pas de projet React, pas de base de données). Ce dépôt contient uniquement la documentation de cadrage nécessaire au démarrage du développement.

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
├── backend/        (vide — réservé au code ASP.NET Core, Lot 0)
├── frontend/       (vide — réservé au code React, Lot 0)
├── database/       (vide — réservé aux migrations et scripts SQL, Lot 0)
├── deploy/         (vide — réservé aux scripts PowerShell et à la configuration IIS, Lot 0)
├── scripts/        (vide — réservé aux scripts d'outillage)
├── CLAUDE.md
├── README.md
└── .gitignore
```

## Méthode de travail

Le projet est développé **par lots successifs** (voir `docs/ROADMAP.md`), du Lot 0 (Fondations) au Lot 7 (Industrialisation). Chaque lot ne passe au suivant que lorsqu'il produit une version **compilable, testée et démontrable**. L'avancement réel est suivi dans `docs/IMPLEMENTATION_STATUS.md`.

## Démarrage

Le code n'existant pas encore, il n'y a pas de procédure d'installation à ce stade. Elle sera documentée ici dès la fin du Lot 0 (structure de solution, configuration multi-environnements, scripts de build).

## Usage

Projet interne. Toute contribution doit respecter `CLAUDE.md` (règles techniques) et le cahier des charges (règles fonctionnelles).
