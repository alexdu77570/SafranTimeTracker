# Roadmap — SAFRAN TIME TRACKER

> Découpage recommandé par le cahier des charges (section 40), détaillé ici comme plan de référence. L'avancement réel est suivi dans `docs/IMPLEMENTATION_STATUS.md`, qui est le document à consulter pour connaître l'état effectif du projet.

## Révision de la roadmap (à la clôture du Lot 6)

Le découpage du cahier des charges (§40) ne prévoyait aucun lot dédié à la construction des écrans fonctionnels (§8 à §28) : le frontend n'a reçu aucune évolution depuis le socle technique du Lot 0. Un produit techniquement industrialisé mais sans interface fonctionnelle complète ne constitue pas une V1 utilisable. La roadmap est donc révisée, décision actée après le Lot 6 :

- l'ancien **Lot 7 — Industrialisation** est remplacé par une phase **Frontend complet (V1)**, découpée en six nouveaux lots (7 à 12), construite écran par écran sur l'API déjà livrée aux Lots 1 à 6 ;
- l'industrialisation (packaging, déploiement, exploitation) est conservée intégralement mais repoussée en dernière position, renumérotée **Lot 13**.

Les Lots 0 à 6 ne sont pas modifiés par cette révision.

## Révision de la roadmap (à l'ouverture du Lot 13)

L'ancien **Lot 13 — Industrialisation** (tests élargis, optimisation, documentation d'exploitation, revue de sécurité, sauvegarde/restauration, packaging portable) ne nommait ni authentification, ni session, ni modèle RBAC, ni CI/CD par leur nom — seule la ligne « Sécurité (revue complète) » s'en rapprochait. Le périmètre explicitement demandé à l'ouverture de ce lot (authentification simulée sessionnée, autorisation RBAC, sécurisation de l'API, pipeline GitHub Actions, qualité, préparation multi-utilisateur) dépasse cette description et constitue un sujet à part entière, distinct du packaging/déploiement. La roadmap est donc révisée une seconde fois, décision actée à l'ouverture du Lot 13 :

- le **Lot 13** est redéfini pour couvrir authentification/sessions/RBAC/sécurisation API/CI-CD/qualité (détail ci-dessous) ;
- l'ancien contenu du Lot 13 (packaging, déploiement, sauvegarde/restauration, documentation d'exploitation) est conservé intégralement mais renuméroté **Lot 14**.

Les Lots 0 à 12 ne sont pas modifiés par cette seconde révision.

## Révision de la roadmap (à l'ouverture du Lot 14)

Avant le démarrage du Lot 14 tel que renuméroté à l'ouverture du Lot 13 (packaging, déploiement, sauvegarde/restauration, documentation d'exploitation), une revue d'architecture complète du dépôt (Lots 0 à 13) a été menée à la demande du Product Owner, en lecture seule, pour identifier la dette technique, les incohérences et les risques accumulés avant une première mise en production. Cette revue a mis en évidence des écarts (sécurité, EF Core/performance, cohérence documentaire, tests) jugés préférables à fermer avant tout travail d'industrialisation plutôt qu'après. La roadmap est donc révisée une troisième fois, décision actée à l'ouverture du Lot 14 :

- le **Lot 14** est redéfini : **Audit / Refactoring / Qualité** — fermeture des écarts constatés par la revue d'architecture, sans aucune fonctionnalité métier nouvelle ;
- l'ancien contenu du Lot 14 (packaging, déploiement, sauvegarde/restauration, documentation d'exploitation) est conservé intégralement mais renuméroté **Lot 15**.

Les Lots 0 à 13 ne sont pas modifiés par cette troisième révision.

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

> **Précision actée à la clôture du Lot 10** : périmètre livré tel que décrit ci-dessous. Deux points appellent une lecture attentive : (1) « alertes surcharge/sous-charge » n'est livré que pour la surcharge (comparaison directe charge/capacité) — la sous-charge exigerait un seuil configurable absent de `Settings`, non inventé (`CLAUDE.md` §7) ; (2) la validation visuelle en navigateur (captures Playwright/Chromium, `CLAUDE.md` §20) n'a pas pu être réalisée cette session, l'accès à l'outil de navigation ayant été explicitement décliné par l'utilisateur — remplacée par une vérification directe des endpoints et du frontend servi (détail : `docs/IMPLEMENTATION_STATUS.md`). Le Gantt évoqué en dehors de ce document n'a jamais fait partie du périmètre du lot (seuls `WeeklyPlanningGrid`/`Timeline` sont nommés ci-dessous) et reste au backlog.

- Projets : vue liste (filtres multiples) et détail à 7 onglets (Synthèse, Participants, Planning, Budget, Temps, Jalons, Références liées).
- Planning projet : vue transverse tous projets, comparaison initial/ajusté/réalisé, alertes surcharge/sous-charge.
- Composants `WeeklyPlanningGrid`, `Timeline`.

## Lot 11 — Commandes, Budgets et Jalons

> **Précision actée à la clôture du Lot 11** : périmètre livré tel que décrit ci-dessous, sur la base de trois évolutions backend ponctuelles validées avant implémentation (`GET /order-statuses`, consommation mensuelle du reporting financier, filtre `applicationId` des jalons — détail : `docs/BACKLOG_METIER.md` §15). Le modèle budgétaire détaillé par ligne évoqué dans `docs/BACKLOG_METIER.md` §2 (société prestataire, intervenant, DDA, PR, code SAP) n'a explicitement **pas** été construit ce lot — décision actée à l'ouverture du lot, reportée à un futur lot dédié ; l'écran Budgets reste sur le modèle `Budget`/`BudgetVersion` existant, conforme au §14.1/§14.2 du cahier des charges.

- Commandes (CRUD, machine d'état à 5 statuts, rallonges, réceptions partielles — Lot 6).
- Budgets (indicateurs, versions/ajustements historisés, `BudgetGauge`).
- Jalons (timeline, calendrier, tableau, compteur à 30 jours, mise en évidence des retards).

> **Précision actée à la clôture du Lot 12** : périmètre livré tel que décrit ci-dessous, sur la base de trois évolutions backend ponctuelles (évolution mensuelle et heatmap sur `GET /reporting/charges`, export du rapport opérationnel, comparaison Prévu vs réalisé). Cette dernière a fait l'objet d'une réserve explicite de l'utilisateur avant tout développement : l'agrégation « Capacité vs réalisé », initialement recommandée côté backend au même titre que « Prévu vs réalisé », a été **révisée en cours d'analyse** vers une composition entièrement frontend (donnée déjà exposée par `DashboardOperationalKpisDto`, Lot 5) — seule « Prévu vs réalisé » justifiait un réel besoin backend (détail complet des 4 raisons : `docs/BACKLOG_METIER.md` §16). « Assistant en 12 étapes » ci-dessous se lit comme les 7 écrans successifs de l'`ImportWizard` (Type → Mode → Fichier → Aperçu → Simulation → Confirmation → Compte rendu), qui recouvrent le pipeline Preview/Simulate/Execute du §27.3 — la roadmap ne prescrivait pas un compte d'étapes littéral.

## Lot 12 — Charges, Tableau de bord, Reporting et Imports

- Charges (filtres, indicateurs, graphiques dont `WorkloadHeatmap`).
- Tableau de bord (KPI opérationnels et financiers, graphiques adaptés au périmètre de l'utilisateur).
- Reporting (rapports opérationnels et financiers, exports réels CSV/Excel/PDF).
- Imports (assistant en 12 étapes, `ImportWizard`, `DiffViewer`, comparaison SharePoint simulée).

---

> **Précision actée à l'ouverture du Lot 13** : périmètre redéfini par rapport à la description originale du cahier des charges §40 (voir « Révision de la roadmap » ci-dessus) — authentification/sessions/RBAC/sécurisation API/CI-CD/qualité plutôt que packaging/déploiement, repoussés au Lot 14, puis au Lot 15, puis finalement au **Lot 17** (voir « Révision de la roadmap (figeage jusqu'à la V1) » ci-dessous).

## Lot 13 — Authentification, RBAC, Sécurisation API, CI/CD et Qualité

- Authentification simulée sessionnée (§6.5) : session serveur (`UserSession`, cookie HttpOnly), sans mot de passe ni JWT définitif ni écran de connexion réel — `IAuthenticationProvider` remplaçable par un futur provider LDAP/OIDC/AD sans refonte.
- Modèle RBAC (§6.1) : `RolePermission` (permissions par défaut du rôle), `UserPermission.Effect` (octroi/retrait individuel), calcul centralisé des permissions effectives.
- Sécurisation de l'API : en-têtes de sécurité, CORS à crédentials, limitation de débit, limite de taille de requête sur les imports.
- Pipeline GitHub Actions (build, tests, couverture backend et frontend), sans SonarQube/SonarCloud ni Docker ce lot — structure prête à les accueillir sans refonte.
- Périmètre organisationnel (département/service/équipe/propriété de la donnée, §6.3) : explicitement reporté à un lot dédié, non construit ici.

## Lot 14 — Audit, Refactoring et Qualité

> Lot non fonctionnel (voir « Révision de la roadmap » à l'ouverture du Lot 14, ci-dessus) : aucune capacité métier nouvelle. Objectif unique — fermer, avant une première mise en production, les écarts constatés par la revue d'architecture menée à l'ouverture de ce lot sur les Lots 1 à 13 (sécurité, EF Core/performance, frontend, tests, documentation). Classification complète (bug / dette technique / risque d'architecture / optimisation facultative), gravité, arbitrage V1/V2 et découpage en sous-lots : rapport d'audit dédié produit à l'ouverture du lot.

- Sécurité : garde de permission manquante sur les endpoints financiers de commande (rallonges, réceptions) et filtrage complet des champs financiers d'`OrderDto`, à l'identique du correctif déjà appliqué sur `UsersController.Create` (v0.13.1).
- Base de données : jetons de concurrence optimiste sur `Budget`/`Order` (écart avec `CLAUDE.md` §11), index manquants (`TimeEntry`, `Resource`, `Budget`), index unique `ProjectParticipant(ProjectId, ResourceId)`, volumétrie du seed alignée sur `docs/DATABASE.md` §7.
- Backend : pagination réellement bornée côté base (`ProjectService` branche `alerteBudget`, `ProjectPlanningService.GetOverviewAsync`) ; validateur manquant sur `ReportingFilterQuery`.
- Tests : couverture des domaines backend à 0 % (périodes de capacité, calendrier des jours fériés, types de jalons, équipes, rattachements société), fabrique de fixtures frontend partagée.
- Documentation : cohérence entre `CLAUDE.md`, `docs/ROADMAP.md`, `docs/IMPLEMENTATION_STATUS.md`, `docs/BACKLOG_METIER.md`, `docs/DATABASE.md`, purge de la terminologie proscrite.
- Selon calendrier, repoussable après la V1 sans bloquer une première mise en production (à assumer explicitement, pas par défaut) : décomposition de `ReportingService`, hooks frontend partagés, découpage des composants surdimensionnés.

*Catégorie : Technique / Qualité — Complexité : ★★★★☆ (Élevée) — Charge : XL (plusieurs semaines d'effort net) — Risque : Moyen.*

---

## Révision de la roadmap (figeage jusqu'à la V1)

L'ensemble des lots restant jusqu'à la première mise en production a été défini et validé par le Product Owner, sur la base du cahier des charges (§6.3, §14, §17.2, §27.3, §36, §37, §40), de `docs/BACKLOG_METIER.md`, des décisions actées aux Lots 1 à 13 et du rapport d'audit du Lot 14. Cette roadmap est désormais **figée jusqu'à la V1** : le Lot 15 n'est plus directement l'Industrialisation (renumérotée **Lot 17**) — cinq lots s'intercalent (15 à 19), détaillés ci-dessous. Chaque lot précise catégorie, complexité qualitative, charge approximative (ordre de grandeur, jamais un calendrier daté faute de vélocité d'équipe mesurée), dépendances, et critères d'entrée/sortie.

Volontairement exclu de toute la roadmap V1 (aucun des lots 15 à 19) : le modèle budgétaire détaillé par ligne (`docs/BACKLOG_METIER.md` §2, 🕓 non implémenté, non bloquant), le Gantt avancé (backlog, jamais validé comme requis), l'assistant d'import interactif à mapping de colonnes (§27.3 étape 5, non bloquant pour la recette §37.8), le remplacement de `RequirePermissionAttribute` par les policies natives ASP.NET Core (non tranché, aucun gain fonctionnel identifié).

## Lot 15 — Complétude fonctionnelle & Administration

> Le nom reflète le contenu réel du lot, plus large que le seul périmètre organisationnel initialement envisagé.

- Périmètre organisationnel (§6.3) : département/service/équipe/propriété de la donnée comme axe d'autorisation, appliqué où le cahier des charges le nomme explicitement — validation d'absence (§23.3, responsable hiérarchique), modification projet/jalon (pilote/responsable), transitions commande/budget/administration/import/audit. Vérification croisée `ResourceId`/appelant sur TimeEntry, Absence, ProjectParticipant, ResourceCapacityPeriod.
- Administration : écran frontend de gestion rôle/permissions par utilisateur (API existante depuis le Lot 13, jamais câblée côté écran).
- Référentiels : `GET` manquants (`CompanyType`, `Role`, `OperationalRole`).
- CRUD fonctionnels : `ProjectParticipant.Update` (§17.2), formule d'« avancement » (%, §16.2).
- Exclu : authentification réelle (LDAP/AD/OIDC — architecture prête, non implémentée ce lot), modèle budgétaire détaillé, nettoyage des sessions expirées (déplacé au Lot 17).

*Catégorie : Fonctionnel — Complexité : ★★★★★ (la plus élevée de la roadmap) — Charge : XL (le plus gros lot, à cadrer précisément avant chiffrage) — Risque : Élevé — Dépend du Lot 14.*

## Lot 16 — UX, Responsive et Performance frontend

> Ferme une exigence explicite du cahier des charges jusqu'ici entièrement non couverte (§36.6 : « responsive desktop, tablette et mobile ») — ce n'est pas une optimisation facultative pour une V1 conforme au cahier des charges.

- Responsive sur les 21 routes de l'application (priorité aux pages identifiées desktop-only par l'audit du Lot 14).
- Découpage de code (`React.lazy` par route), mémoïsation ciblée, virtualisation des tables denses.
- Exclu : toute refonte visuelle (identité graphique actée au Lot 7, non remise en cause), nouveaux écrans.

*Catégorie : UX — Complexité : ★★★☆☆ (Moyenne) — Charge : M (1-2 semaines) — Risque : Moyen — Dépend des Lots 14 (sous-lot 14.5) et 15.*

## Lot 17 — Industrialisation et packaging (ancien Lot 15, ancien Lot 14, ancien Lot 13, ancien Lot 7)

- Tests (couverture élargie, non-régression).
- Optimisation.
- Documentation (installation, exploitation, fonctionnelle, API, guides utilisateur/administrateur).
- Sécurité (revue complète).
- Sauvegarde et restauration (procédures testées).
- Nettoyage des sessions expirées (tâche planifiée).
- Packaging portable — deux profils distincts :
  - **PortableDemo** : package ZIP autonome, API self-contained Windows x64, frontend statique servi par l'API elle-même, base SQLite locale, sans Internet ni droits administrateur.
  - **ServerIIS** : déploiement Windows Server / IIS, base SQL Server en priorité (PostgreSQL conservé comme provider compatible), scripts de déploiement/sauvegarde/restauration/rollback/health-check, chemins normalisés `E:\appl`, `E:\certificats`, `E:\CD_INSTALL`, `E:\data`.

*Catégorie : Industrialisation — Complexité : ★★★★☆ (Élevée) — Charge : L (2-4 semaines) — Risque : Moyen (première exécution réelle hors SQLite) — Dépend des Lots 14, 15, 16.*

## Lot 18 — Pré-production

> Porte de sortie avant la mise en production : vérification, mesure et décision — aucun nouveau développement. Comprend explicitement une recette fonctionnelle complète, une validation métier, des tests utilisateurs réels, et impose un gel fonctionnel (aucune évolution fonctionnelle acceptée une fois ce lot entamé, hors correctif bloquant identifié pendant la recette).

- Recette fonctionnelle complète : critères §37.1 à §37.9 du cahier des charges.
- Validation métier : relecture et signature du Product Owner sur chaque règle sensible (financière, sécurité, workflow).
- Tests utilisateurs réels, sur l'environnement cible du Lot 17.
- Test de charge/performance à un volume représentatif du déploiement visé (§36.1) — jamais exécuté à ce jour.
- Décision explicite actée : authentification de démonstration acceptée pour ce déploiement, ou authentification réelle requise (risque non tranché, voir rapport d'audit du Lot 14).
- Revue de sécurité finale (§36.3), répétition de sauvegarde/restauration en conditions réelles.

*Catégorie : Pré-production — Complexité : ★★★☆☆ (Moyenne) — Charge : M (beaucoup de vérification, peu de développement) — Risque : Moyen — Dépend du Lot 17, bloque le Lot 19 sans exception.*

## Lot 19 — Mise en production V1

> Lot court et procédural, jamais un lot de développement.

- Séquence de déploiement documentée (`CLAUDE.md` §19) exécutée sur l'environnement de production réel, plan de rollback prêt et testé.
- Surveillance rapprochée des premiers jours d'usage réel.
- Tout besoin détecté ici suit le circuit correctif dédié, jamais un ajout improvisé pendant la bascule.

*Catégorie : Production — Complexité : ★☆☆☆☆ (Faible) — Charge : S (quelques jours) — Risque : Faible (à condition que le Lot 18 n'ait pas été raccourci) — Dépend du Lot 18.*

---

## Livrables attendus en fin de projet

Code source frontend et backend, scripts SQL et migrations, données de démonstration, fichiers de configuration exemples, scripts Windows de build/déploiement, documentation d'installation et d'exploitation, documentation fonctionnelle et API, plan de tests et rapport de tests, guide utilisateur et administrateur, procédure de sauvegarde/restauration, scripts PowerShell (`build.ps1`, `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `purge-logs.ps1`), configuration IIS documentée, modèles de configuration par environnement, scripts de migration PostgreSQL (et SQL Server si activé), procédure d'installation respectant les chemins Windows Server normalisés.
