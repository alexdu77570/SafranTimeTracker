# Roadmap — SAFRAN TIME TRACKER

> Découpage recommandé par le cahier des charges (section 40), détaillé ici comme plan de référence. L'avancement réel est suivi dans `docs/IMPLEMENTATION_STATUS.md`, qui est le document à consulter pour connaître l'état effectif du projet.

## Révision de la roadmap (à la clôture du Lot 6)

Le découpage du cahier des charges (§40) ne prévoyait aucun lot dédié à la construction des écrans fonctionnels (§8 à §28) : le frontend n'a reçu aucune évolution depuis le socle technique du Lot 0. Un produit techniquement industrialisé mais sans interface fonctionnelle complète ne constitue pas une V1 utilisable. La roadmap est donc révisée, décision actée après le Lot 6 :

- l'ancien **Lot 7 — Industrialisation** est remplacé par une phase **Frontend complet (V1)**, découpée en six nouveaux lots (7 à 12), construite écran par écran sur l'API déjà livrée aux Lots 1 à 6 ;
- l'industrialisation (packaging, déploiement, exploitation) est conservée intégralement mais repoussée en dernière position, renumérotée **Lot 13**.

Les Lots 0 à 6 ne sont pas modifiés par cette révision.

## Règle commune à tous les lots

**Un lot n'est terminé que lorsqu'il produit une version compilable, testée réellement et démontrable.** On ne démarre pas le lot suivant tant que ce critère n'est pas satisfait. Aucune fonctionnalité d'un lot ultérieur n'est anticipée dans un lot antérieur.

---

## Lot 0 — Fondations

- Dépôt et conventions de code.
- Structure frontend / backend.
- Configuration multi-environnements (Development, Qualification, Production).
- Sélection de provider EF Core ; PostgreSQL par défaut ; SQLite pour les tests ; préparation du provider SQL Server.
- Migrations initiales.
- Design system.
- Authentification de démonstration.
- Rôles et permissions.
- Scripts PowerShell de build et déploiement.
- Structure IIS et chemins normalisés Windows Server.
- Compilation et tests réels avant passage au lot suivant.

## Lot 1 — Référentiels

- Organisation (département, service, équipe).
- Utilisateurs.
- Ressources.
- Applications.
- Sociétés.
- Commandes.
- Paramètres.

## Lot 2 — Modèle financier

- Historiques TJM.
- Historiques contrats.
- Rattachements société.
- Snapshots financiers.
- Permissions financières.
- Tests métier (calculs, recherche par date, chevauchements).

## Lot 3 — Temps et capacité

- Saisie des temps.
- Absences.
- Disponibilités.
- Calculs de capacité.
- Charges RUN / hors RUN.

## Lot 4 — Projets

- Projets.
- Participants.
- Planning (initial, ajusté, réalisé).
- Jalons.
- Références opérationnelles (INC/CHG/PRB/RITM/VABE/VSR).
- Écarts.

## Lot 5 — Budgets et reporting

- Budgets.
- Rallonges.
- Tableaux de bord.
- Reporting.
- Exports (CSV, Excel, PDF).

## Lot 6 — Imports et audit

- Imports CSV.
- Import SharePoint simulé.
- Comparaison de lots d'import.
- Audit complet.

---

# Phase — Frontend complet (V1)

> Objectif : livrer les écrans fonctionnels décrits au cahier des charges §8 à §28, jusqu'ici absents (seul le squelette technique du Lot 0 existe). Chaque lot consomme exclusivement l'API déjà livrée aux Lots 1 à 6 : aucune évolution backend, aucune règle métier nouvelle. Même règle de méthode que les Lots 0 à 6 (version compilable, testée réellement, démontrable, avant passage au lot suivant).

## Lot 7 — Frontend Foundation (Design System)

- Identité visuelle (palette, typographie, thème corporate — cahier des charges §8.1).
- Layout applicatif : `AppLayout`, `Sidebar` (filtrée par droits), `Header`, `Breadcrumb`.
- Composants transverses de base : `DataTable`, `StatusBadge`, `FormField`, `Select`, `DatePicker`, `Modal`, `ConfirmDialog`, `EmptyState`, `FilterBar`, `KpiCard`, `ProgressBar`, `PermissionGuard`, `FinancialValue`.
- Sélecteur d'identité de démonstration (pilote l'en-tête `X-Demo-User` existant).
- Routage complet avec garde de permissions.
- Gestion centralisée des erreurs et des états de chargement (TanStack Query).
- Formats français (dates, montants en euros).
- Aucun écran métier : ce lot rend les lots suivants mécaniques plutôt que fondateurs.

## Lot 8 — Référentiels et Administration

> **Révision de périmètre actée à l'ouverture du Lot 8** : le périmètre ci-dessous reste frontend-only sur l'API des Lots 1-7 (aucune évolution backend, conformément à la règle commune de cette phase). Cinq référentiels supplémentaires, absents du cahier des charges et validés avec le Squad Leader pendant ce lot, s'y ajoutent : **Technologies, Clients, Types de projets, Centres de coûts, Devises** (définitions et statut de validation : `docs/BACKLOG_METIER.md` §5-9). Ces cinq-là **nécessitent une évolution backend** (nouvelles entités/DTO/services/endpoints), par exception à la règle « aucune évolution backend » de cette phase — décision explicitement validée, pas une dérive de périmètre.

- Ressources / Utilisateurs (fiche à 4 sections : Général, Organisation, Sécurité, Historique des TJM sous permission financière).
- Sociétés (+ historique des contrats, confidentiel).
- Applications (+ détail statistique, technologies rattachées).
- Administration (13 onglets : Utilisateurs, Département, Services, Équipes, Applications, Types d'activités, Types d'absences, Types de jalons, Sociétés, Commandes, Paramètres, Permissions, Audit) + onglets des 5 nouveaux référentiels (Technologies, Clients, Types de projets, Centres de coûts, Devises).
- Backend (nouveau, exception validée) : entités `Technology` (liaisons many-to-many avec `ApplicationReference` et `Resource`), `Client` (`Project.ClientId` nullable), `ProjectType` (`Project.ProjectTypeId` nullable), `CostCenter` (rattaché à `Department`/`Service`), `Currency` (référentiel de consultation, aucun impact sur `FinancialCalculationService`).

## Lot 9 — Temps et Disponibilités

> **Précision actée à la clôture du Lot 9** : « accès filtré par rôle » (Disponibilités, ci-dessous) est délivré comme un **filtrage d'affichage frontend uniquement** (filtres département/service/équipe sur des listes déjà entièrement accessibles à toute identité) — aucune nouvelle règle de sécurité serveur (garde de permission, périmètre organisationnel) n'a été introduite ce lot, cohérent avec la règle « aucune évolution backend » par défaut de cette phase (décision validée avec l'utilisateur à l'ouverture du lot, §3.6 ; détail : `docs/IMPLEMENTATION_STATUS.md`, « Nouveaux écarts constatés au Lot 9 »). La restriction par périmètre organisationnel réel reste un écart ouvert, reconduit depuis les Lots 2 à 8.

- Saisie des temps (formulaire, validation de référence dynamique par type d'activité, historique, snapshot financier conditionnel).
- Mes absences (workflow Brouillon → Soumis → Validé/Refusé/Annulé, totaux mensuel/annuel).
- Disponibilités (vues mensuelle/hebdomadaire, calendrier coloré, accès filtré par rôle).

## Lot 10 — Projets et Planning

- Projets : vue liste (filtres multiples) et détail à 7 onglets (Synthèse, Participants, Planning, Budget, Temps, Jalons, Références liées).
- Planning projet : vue transverse tous projets, comparaison initial/ajusté/réalisé, alertes surcharge/sous-charge.
- Composants `WeeklyPlanningGrid`, `Timeline`.

## Lot 11 — Commandes, Budgets et Jalons

- Commandes (CRUD, machine d'état à 5 statuts, rallonges, réceptions partielles — Lot 6).
- Budgets (indicateurs, versions/ajustements historisés, `BudgetGauge`).
- Jalons (timeline, calendrier, tableau, compteur à 30 jours, mise en évidence des retards).

## Lot 12 — Charges, Tableau de bord, Reporting et Imports

- Charges (filtres, indicateurs, graphiques dont `WorkloadHeatmap`).
- Tableau de bord (KPI opérationnels et financiers, graphiques adaptés au périmètre de l'utilisateur).
- Reporting (rapports opérationnels et financiers, exports réels CSV/Excel/PDF).
- Imports (assistant en 12 étapes, `ImportWizard`, `DiffViewer`, comparaison SharePoint simulée).

---

# Phase — Industrialisation

## Lot 13 — Industrialisation (ancien Lot 7)

- Tests (couverture élargie, non-régression).
- Optimisation.
- Documentation (installation, exploitation, fonctionnelle, API, guides utilisateur/administrateur).
- Sécurité (revue complète).
- Sauvegarde et restauration (procédures testées).
- Packaging portable — deux profils distincts :
  - **PortableDemo** : package ZIP autonome, API self-contained Windows x64, frontend statique servi par l'API elle-même, base SQLite locale, sans Internet ni droits administrateur.
  - **ServerIIS** : déploiement Windows Server / IIS, base SQL Server en priorité (PostgreSQL conservé comme provider compatible), scripts de déploiement/sauvegarde/restauration/rollback/health-check, chemins normalisés `E:\appl`, `E:\certificats`, `E:\CD_INSTALL`, `E:\data`.

---

## Livrables attendus en fin de projet

Code source frontend et backend, scripts SQL et migrations, données de démonstration, fichiers de configuration exemples, scripts Windows de build/déploiement, documentation d'installation et d'exploitation, documentation fonctionnelle et API, plan de tests et rapport de tests, guide utilisateur et administrateur, procédure de sauvegarde/restauration, scripts PowerShell (`build.ps1`, `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `purge-logs.ps1`), configuration IIS documentée, modèles de configuration par environnement, scripts de migration PostgreSQL (et SQL Server si activé), procédure d'installation respectant les chemins Windows Server normalisés.
