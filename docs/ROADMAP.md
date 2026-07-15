# Roadmap — SAFRAN TIME TRACKER

> Découpage recommandé par le cahier des charges (section 40), détaillé ici comme plan de référence. L'avancement réel est suivi dans `docs/IMPLEMENTATION_STATUS.md`, qui est le document à consulter pour connaître l'état effectif du projet.

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

## Lot 7 — Industrialisation

- Tests (couverture élargie, non-régression).
- Optimisation.
- Documentation (installation, exploitation, fonctionnelle, API, guides utilisateur/administrateur).
- Sécurité (revue complète).
- Sauvegarde et restauration (procédures testées).
- Packaging portable.

---

## Livrables attendus en fin de projet

Code source frontend et backend, scripts SQL et migrations, données de démonstration, fichiers de configuration exemples, scripts Windows de build/déploiement, documentation d'installation et d'exploitation, documentation fonctionnelle et API, plan de tests et rapport de tests, guide utilisateur et administrateur, procédure de sauvegarde/restauration, scripts PowerShell (`build.ps1`, `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `purge-logs.ps1`), configuration IIS documentée, modèles de configuration par environnement, scripts de migration PostgreSQL (et SQL Server si activé), procédure d'installation respectant les chemins Windows Server normalisés.
