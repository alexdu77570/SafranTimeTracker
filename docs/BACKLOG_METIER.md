# Backlog métier — SAFRAN TIME TRACKER

> **Rôle de ce document** : référence **fonctionnelle** du projet — décisions métier, workflows et règles validées avec le Product Owner ou les experts métier. Il ne décrit **jamais** l'architecture technique (classes, entités, endpoints) : cette information vit dans `docs/ARCHITECTURE.md`, `docs/DATABASE.md` et `docs/IMPLEMENTATION_STATUS.md`. En cas de doute sur une règle métier déjà couverte par le cahier des charges, le cahier des charges (`docs/Cahier_des_charges_SAFRAN_TIME_TRACKER_v2.1_Windows_Server.md`) reste la référence fonctionnelle unique ; ce document capture les règles **complémentaires ou plus précises**, validées au fil des lots, qui ne figurent pas (ou pas assez précisément) dans le cahier des charges.
>
> **Règle d'alimentation (CLAUDE.md §5)** : toute nouvelle règle métier validée avec le Product Owner ou les experts métier est ajoutée ici **avant** son implémentation. Ce document est relu au même titre que `docs/ROADMAP.md`, `CLAUDE.md` et `docs/IMPLEMENTATION_STATUS.md` avant de démarrer un nouveau lot.
>
> Ce document mélange volontairement deux types de contenu, distingués par un statut explicite :
> - des règles **déjà implémentées**, documentées ici pour qu'elles restent traçables comme décisions métier et pas seulement comme code ;
> - des règles **validées mais pas encore construites** (backlog au sens propre), qui attendent un lot pour être développées.
>
> Statut employé pour chaque règle :
> - ✅ **Implémenté** — déjà appliqué dans le code (détail technique : `docs/IMPLEMENTATION_STATUS.md`).
> - 🕓 **Validé, non implémenté** — règle actée avec le Product Owner, en attente d'un lot.
> - 🔎 **À clarifier** — écart ou ambiguïté identifié entre la formulation métier et le modèle actuel ; nécessite une validation complémentaire avant implémentation.

---

## 1. Commandes

### Workflow

```
Demande d'achat → Commande → Réceptions partielles → Clôture
```

✅ **Implémenté.** Ce vocabulaire métier ne correspond pas à un nouveau jeu de statuts : il se lit sur la machine d'état déjà en place (Brouillon ≈ Demande d'achat, Active ≈ Commande, Clôturée ≈ Clôture), complétée par des événements répétables représentant les réceptions partielles. Ce vocabulaire ne doit **jamais** être réintroduit comme un nouveau statut ou une nouvelle machine d'état parallèle.

### Règles validées

- 🔎 **Une commande appartient à un budget.** Formulation à clarifier : le modèle actuel relie un objet de pilotage budgétaire à une commande (rattachement facultatif dans un sens), pas l'inverse structurellement. À rapprocher explicitement avec le Product Owner avant tout lot qui construirait ce rattachement, pour savoir si la commande doit référencer un budget de façon obligatoire, ou si le rattachement optionnel existant suffit.
- ✅ **Une société prestataire ne peut jamais être modifiée après création** de la commande.
- ✅ **La ressource affectée à une commande peut être remplacée en cours de vie** de la commande.
- ✅ **Les réceptions sont représentées par des événements append-only** (« OrderReceipt » au sens métier) : une réception est un fait daté, jamais un total recalculé en place.
- ✅ **Aucune réception n'est modifiable** après enregistrement.
- ✅ **Une correction se fait uniquement par une écriture compensatoire** (nouvelle réception, éventuellement à montant ou jours négatifs) — jamais par modification ou suppression d'une réception existante.
- ✅ **Le total réceptionné est toujours calculé à partir de l'ensemble des réceptions**, jamais stocké comme un montant à mettre à jour.
- ✅ **La clôture d'une commande respecte les règles métier déjà en place** : aucune réception, rallonge ou saisie de temps sur commande clôturée sans passer par les mécanismes de dérogation explicite déjà validés (correction, dépassement autorisé) ; sortir de Clôturée n'est possible que par une réouverture explicite et motivée.
- ✅ **Toutes les opérations sur une commande sont auditées** (création, modification, transitions de statut, rallonge, réception, dépassement autorisé).

---

## 2. Budget

🕓 **Validé, non implémenté.** Les champs ci-dessous décrivent un besoin métier plus détaillé que le modèle de pilotage budgétaire actuel (simple enveloppe consommé/reste/atterrissage, sans détail par ligne de commande/société/intervenant). Ce besoin reste à réconcilier avec le modèle existant lors d'un lot dédié — cette section ne préjuge pas de la façon dont il sera implémenté (nouvelle entité, extension du modèle Commande, ou vue agrégée).

**Décision actée à l'ouverture du Lot 11 (décision 1)** : le Lot 11 (écran Budgets) reste explicitement sur le modèle `Budget`/`BudgetVersion` existant (conforme au §14.1/§14.2 du cahier des charges) — le modèle détaillé par ligne décrit ci-dessous n'a **pas** été construit ce lot, ni anticipé sous aucune forme (aucun champ, aucune entité). Cette section reste 🕓, en attente d'un futur lot explicitement dédié à cette réconciliation.

Attributs métier attendus pour une ligne de suivi budgétaire :

- Budget annuel (ex. « Métier 2025 », « Métier 2026 », ...) — un budget est rattaché à un exercice, pas seulement à une période libre.
- Rattachement à un département.
- Rattachement à un projet.
- Société prestataire.
- Intervenant (ressource).
- Nombre de jours commandés.
- TJM commandé.
- Code SAP.
- DDA (demande d'achat).
- PR (référence d'achat).
- Numéro de commande.
- Montant commandé.
- Montant réceptionné.
- Reste à consommer.
- Montant réceptionné, ventilé par mois.
- Statut : en cours / clos.
- Date de fin estimée de la prestation.

**Règle métier explicite** : cette gestion budgétaire (engagement, réception, reste à consommer par ligne) est **indépendante du consommé réel des projets**. Le suivi budgétaire par ligne de commande (ce que l'on a engagé/réceptionné auprès d'un prestataire) et le coût réel consommé par un projet (calculé à partir des saisies de temps valorisées) sont deux vues distinctes, qui ne doivent pas être fusionnées ou confondues dans l'implémentation future.

---

## 3. Projet — références opérationnelles

✅ **Implémenté.**

- Les références INC / CHG / PRB / RITM / VABE / VSR correspondent au **RUN** (activité récurrente, par opposition à un projet).
- Elles peuvent être **rattachées à un projet** (une référence RUN peut concerner un projet en cours, sans devenir une donnée projet).
- Elles ne font **jamais partie du modèle Projet lui-même** : une référence opérationnelle reste une propriété de la saisie de temps qui la porte, jamais un champ ou une liste stockée sur le projet.

---

## 4. Ressources — TJM, historisation, recalcul, snapshots financiers

✅ **Implémenté.**

- **Le TJM appartient à la personne** (la ressource), jamais au projet, à la commande, à la société ou au rôle opérationnel — un même TJM s'applique quel que soit le projet ou la commande sur lesquels la ressource est affectée à une date donnée.
- **Historisation** : le TJM d'une ressource est suivi dans le temps (périodes avec date de début / date de fin), jamais remplacé en place. Un changement de TJM ouvre une nouvelle période, ne modifie pas la précédente.
- **Recherche à la date de la saisie** : le TJM applicable à une saisie de temps est toujours celui en vigueur à la date de la saisie, jamais celui en vigueur à la date où l'on consulte ou recalcule — aucun recalcul rétroactif silencieux d'une saisie déjà valorisée.
- **Recalcul explicite et audité** : un recalcul d'une saisie déjà valorisée n'est possible que par une action explicite, réservée à un droit dédié, avec motif obligatoire et conservation de l'ancienne valeur dans le journal d'audit — jamais un recalcul de masse silencieux.
- **Snapshot financier** : chaque saisie de temps valorisée fige, au moment du calcul, le TJM de la personne, le TJM du contrat société (le cas échéant), le coût réel, le coût contractuel, le différentiel et le statut de valorisation (complet ou incomplet si aucun TJM n'est disponible à la date). Ce figement garantit qu'une modification ultérieure d'un TJM ou d'un contrat ne modifie jamais une saisie déjà enregistrée.

---

## 5. Technologies

✅ **Implémenté (Lot 8).** Référentiel administrable `Technology` (code, libellé, statut actif/archivé), sans hiérarchie ni catégorisation supplémentaire.

- Une technologie peut être rattachée à une ou plusieurs Applications (`ApplicationReference`) — stack technique d'une application, visible dans le détail statistique de l'application (§ Lot 8 roadmap).
- Une technologie peut être rattachée à une ou plusieurs Ressources (`Resource`) — compétences maîtrisées par une personne.
- Relation many-to-many dans les deux cas, aucun impact sur le calcul financier ni sur la capacité.

---

## 6. Clients

✅ **Implémenté (Lot 8).** `Client` est un nouvel axe, distinct de la `Company` (§12 du cahier des charges) qui reste exclusivement la société prestataire fournissant la ressource et son TJM contractuel.

- Le Client représente le donneur d'ordre / bénéficiaire d'un projet (rattachement facultatif `Project.ClientId`).
- Référentiel administrable simple (code, libellé, statut), sans incidence financière ni capacitaire — n'intervient à aucun titre dans `FinancialCalculationService`.
- Ne remplace ni ne fusionne avec `Company` : les deux référentiels coexistent, un projet peut avoir un Client sans que cela contraigne la Société des ressources qui y travaillent.

---

## 7. Types de projets

✅ **Implémenté (Lot 8).** `ProjectType` est un nouvel axe de classification du projet (ex. Forfait / Régie / Interne / RUN), indépendant du `ProjectStatus` existant (Lot 4 — cycle de vie Actif/Archivé du projet).

- Référentiel administrable simple (code, libellé, statut), rattachement facultatif `Project.ProjectTypeId`.
- Ne remplace pas `ProjectStatus` : un projet a toujours un statut de cycle de vie (Actif/Archivé/…) et, indépendamment, un type optionnel qui qualifie sa nature contractuelle/organisationnelle.

---

## 8. Centres de coûts

✅ **Implémenté (Lot 8).** `CostCenter` est un axe organisationnel analytique, rattaché à `Department` et/ou `Service` (Lot 1), pas aux Commandes ni aux Budgets.

- Référentiel administrable simple (code, libellé, statut).
- Rattachement facultatif à un `Department` et/ou à un `Service`.
- Aucun impact sur le calcul financier existant (`FinancialCalculationService`, `BudgetService`) : purement un attribut analytique organisationnel à ce stade.

---

## 9. Devises

✅ **Implémenté (Lot 8).** Périmètre volontairement limité à un référentiel de consultation, **pas** un support multi-devises.

- Référentiel administrable simple `Currency` (code ISO 4217, libellé, symbole), lecture seule pour les utilisateurs standard.
- Aucun impact sur `FinancialCalculationService`, `ResourceTjmHistory`, `CompanyContractHistory`, `Order`, `Budget` : tous les montants restent implicitement en EUR, comme aujourd'hui (`FinancialValue` du frontend, Lot 7).
- Un véritable support multi-devises (TJM/Budgets/Commandes/Contrats dans des devises différentes, conversion, taux de change) est explicitement **hors périmètre** du Lot 8 et mériterait son propre lot dédié s'il est un jour validé — ne jamais l'anticiper ici (`CLAUDE.md` §5 : pas d'abstraction anticipée).

---

## 10. Saisie des temps — filtres serveur étendus

✅ **Implémenté (Lot 9).** Décision actée avec l'utilisateur à l'ouverture du Lot 9 : `GET /api/v1/time-entries` ne filtrait jusque-là que par `resourceId`/`from`/`to`, alors que le cahier des charges §19.4 exige de filtrer également par type d'activité, projet et commande.

- Ajout des paramètres optionnels `activityTypeId`, `projectId`, `orderId` à l'action `GetList` **déjà existante** de `TimeEntriesController` — aucune nouvelle entité, aucune migration, aucune nouvelle règle de calcul.
- `TimeEntry.ActivityTypeId`/`ProjectId`/`OrderId` existent déjà sur l'entité depuis le Lot 3/4 : il s'agit uniquement d'exposer un filtrage serveur sur des colonnes déjà présentes, pas d'une évolution de modèle.
- Le filtre par « application » (§19.4) reste **hors périmètre** de cette décision : `TimeEntry` ne porte aucun `ApplicationId` (voir §11 — décision distincte, non retenue pour ce lot).
- Filtres réellement câblés sur l'écran `/temps` (`TimeEntriesPage.tsx`, revue de clôture Lot 9) : Ressource, Type d'activité, **Projet**, **Commande**, Du/Au et **Semaine** (calcule les bornes lundi-dimanche via `weekBounds`, `lib/dateUtils.ts`, et les applique à Du/Au). Le §19.4 est donc couvert à l'exception du seul filtre « application ». **Totalisation automatique** (§19.4) également livrée : `KpiCard` sommant les heures du filtre courant sur une requête dédiée (même endpoint, `pageSize` élargi), pas un nouvel agrégat backend.

---

## 11. Application non rattachée à une saisie de temps — écart assumé

🔎 **À clarifier** (documenté, non résolu). Le §19.1 liste « application » comme champ d'une saisie de temps ; l'entité `TimeEntry` (Lot 3) ne le porte pas — seuls `ActivityTypeId`, `ProjectId` (facultatif) et `OrderId` (facultatif) existent. Une saisie de type RUN (Incident/Change/Problem/RITM/Astreinte/Support, cahier des charges §29.4) n'a généralement pas de projet : elle ne peut donc actuellement être rattachée à aucune application, ce qui limite le filtrage/reporting « charge par application » pour ce type de saisie.

- **Décision actée à l'ouverture du Lot 9** : ne pas ajouter `TimeEntry.ApplicationId` dans ce lot (Option A retenue explicitement face à l'ajout d'un champ + migration, Option B). Le formulaire de saisie des temps n'expose donc pas de sélecteur Application, et aucun filtre « par application » n'est proposé sur l'écran Saisie des temps.
- Cet écart est **antérieur** au Lot 9 (introduit au Lot 3, jamais documenté avant cette analyse) : il n'est pas créé par ce lot, seulement constaté et volontairement non corrigé ici, pour rester dans le périmètre frontend-only de la roadmap.
- Une correction future (ajout du champ) relèverait d'un lot dédié au modèle `TimeEntry`, avec sa propre validation — jamais anticipée ici (`CLAUDE.md` §5).

---

## 12. Mes absences — modification d'un brouillon

✅ **Implémenté (Lot 9).** Décision actée avec l'utilisateur à l'ouverture du Lot 9 : le cahier des charges §23.2 exige de pouvoir « modifier tant que permis » une absence ; `AbsenceService`/`AbsencesController` n'exposaient jusque-là aucune opération de modification (seuls `Create`, `Submit`, `Validate`, `Refuse`, `Cancel` existaient).

- Ajout de `PUT /api/v1/absences/{id}`, **restreint au statut Brouillon** (toute tentative sur un autre statut est un conflit métier 409, même principe que les autres transitions de `AbsenceService`).
- Amélioration UX volontairement acceptée par l'utilisateur, en alternative à l'approche « annuler puis recréer » (fonctionnellement équivalente mais moins ergonomique) qui aurait autrement suffi sans toucher au backend.
- Ne modifie ni le workflow de statuts (§23.3) ni la validation d'impact sur la capacité — la modification reste limitée aux champs de la demande elle-même (type, dates, demi-journée, commentaire), jamais au statut ou à la décision.

---

## 13. Projets — décisions actées à l'ouverture du Lot 10

### Filtres et vue transverse (backend)

✅ **Implémenté.**

- `GET /api/v1/projects` étendu de 5 filtres optionnels (§16.1) : équipe, niveau de risque, période, alerte planning, alerte budget — évolution ponctuelle sur une action déjà existante, aucune nouvelle entité.
- `GET /api/v1/project-planning` créé (§18.2) : vue transverse "Planning projet" agrégée entièrement côté serveur (une ligne par Projet/Ressource/Semaine), en remplacement d'une agrégation par N appels frontend — décision explicitement validée par l'utilisateur pour rester compatible avec une pagination/un tri serveur corrects.
- `GET /api/v1/projects/{id}/plan-versions/{versionId}/weekly-plans` ajouté (§18.3) : lecture des lignes hebdomadaires, jusqu'ici en écriture seule.
- `GET /api/v1/project-statuses` ajouté (§16.2, §30) : voir §14 ci-dessous.

### « Avancement » (%)

🔎 **À clarifier.** Le §16.2 liste l'avancement comme champ du projet, mais aucune formule n'est définie ailleurs dans le cahier des charges (le §29.5 ne couvre que charge/planning, le §29.6 que surcharge/sous-charge). **Décision actée à l'ouverture du Lot 10** : ne jamais l'inventer, ni côté frontend ni côté serveur — affiché « — » sur la fiche projet tant qu'aucune formule n'est validée avec le Product Owner. Une future validation devra préciser si l'avancement se déduit de la charge consommée/planifiée, des jalons franchis, ou d'une saisie manuelle dédiée.

### « Commandes liées »

✅ **Implémenté (dérivé, pas un champ stocké).** Le §16.2 liste "commandes liées" comme champ du projet ; `Project` ne porte aucune relation directe vers `Order`. **Décision actée à l'ouverture du Lot 10** : dérivé à l'affichage sur la fiche détail, en combinant les commandes déjà visibles via `Budget.OrderId` (`GET /budgets?projectId=`) et `ProjectParticipant.DefaultOrderId` — composition d'affichage sur des données déjà exposées, pas une nouvelle règle métier ni un nouveau champ. Pas de colonne dédiée sur la vue liste (calcul non trivial à l'échelle d'une liste paginée).

### Gantt

🕓 **Non retenu pour ce lot, reporté au backlog.** Un besoin de Gantt a été évoqué en dehors du cahier des charges et de `docs/ROADMAP.md`, qui ne nomment que `WeeklyPlanningGrid`/`Timeline` pour le Lot 10 (§32.2). **Décision actée à l'ouverture du Lot 10** : ne pas construire de Gantt avancé (dépendances visuelles entre tâches, glisser-déposer) ce lot — seule une `Timeline` simple (axe chronologique, sans dépendance visuelle) a été livrée pour les jalons. À valider explicitement avec le Product Owner avant tout lot futur qui l'implémenterait.

### `ProjectParticipant` — pas d'action de modification

✅ **Implémenté (statu quo assumé).** Le §17.2 liste des champs à afficher pour un participant, pas une action "modifier" explicite (contrairement à "créer"/formulaire de retrait déjà couverts). **Décision actée à l'ouverture du Lot 10** : ne pas ajouter d'endpoint `PUT` ni d'écran de modification — un changement de rôle/capacité/période passe par un retrait (désactivation) suivi d'un nouvel ajout. À reconsidérer si l'usage réel démontre que ce n'est pas suffisant.

### Coût et reste à faire par participant

🔎 **À clarifier (documenté, non résolu).** Le §17.2 liste "coût réel"/"coût contractuel"/"différentiel"/"reste à faire" comme champs du participant ; seul le TJM applicable est calculé côté backend (`ProjectParticipantFinancialSummaryDto`), jamais ces montants. **Décision actée à l'ouverture du Lot 10** : le temps consommé par participant affiché à l'écran est une agrégation frontend sur les saisies de temps déjà filtrées par permission (même principe que la totalisation automatique du Lot 9, §10 ci-dessus) — pas une nouvelle règle serveur. Le "reste à faire" en découle (capacité prévue − consommé), mais aucun coût réel/contractuel/différentiel par participant n'est calculé : cela suppose une évolution du backend (agrégation `TimeEntryFinancialSnapshot` groupée par ressource ET projet), non construite ce lot.

## 14. `ProjectStatus` — endpoint `GET` manquant, corrigé

✅ **Implémenté (Lot 10, Décision 10).** `ProjectStatus` est un référentiel (entité, pas un enum) au même titre que `CompanyType`/`Role` (§8 ci-dessus, écart constaté au Lot 8) : jamais d'endpoint de lecture, seules les valeurs seedées existaient. **Deux options ont été soumises à l'ouverture du Lot 10** : (a) ajouter `GET /api/v1/project-statuses` (contrôleur minimal, même forme qu'`ActivityTypesController`) ; (b) contourner comme `CompanyType`/`Role` via `knownReferentials.ts`. **Option (a) retenue explicitement par l'utilisateur** : le lot touchant déjà le backend pour d'autres raisons (§13 ci-dessus), il n'était pas justifié d'accumuler un second contournement. `CompanyType`/`Role` restent en revanche non résolus (hors périmètre de ce lot) — de même que l'écart apparenté constaté sur `OperationalRole` (§17.2, jamais discuté avec l'utilisateur, donc contourné via `knownReferentials.ts` plutôt que traité comme `ProjectStatus`).

## Exception de gouvernance — Lot 10

> **Rôle de cette section** : comme aux Lots 8 et 9, documenter explicitement pourquoi des évolutions backend ont été autorisées pendant un lot que `docs/ROADMAP.md` décrit comme construit sur l'API déjà livrée.

**Constat.** Quatre évolutions backend ont été identifiées et validées individuellement avec l'utilisateur à l'ouverture du Lot 10, avant toute ligne de code : l'extension des filtres de `GET /api/v1/projects` (§13 ci-dessus), la création de `GET /api/v1/project-planning` (§13), la lecture des lignes hebdomadaires (§13), et l'ajout de `GET /api/v1/project-statuses` (§14).

**Nature de l'exception.** Plus large que celle du Lot 9 (extension de filtres + une action) mais toujours strictement bornée : `GET /api/v1/project-planning` est le seul nouvel endpoint d'agrégation du lot, et il ne fait que composer des données/formules déjà existantes (`ProjectPlanningCalculator`, Lot 4 ; `AvailabilityService`, Lot 3) — aucune nouvelle règle de calcul, aucune nouvelle entité, aucune migration de schéma (seul le seed de démonstration a nécessité une migration de données, §13/IMPLEMENTATION_STATUS.md).

**Écarts explicitement non corrigés.** L'« avancement » (%, §13), le Gantt (§13), l'`Update` de `ProjectParticipant` (§13), les coûts par participant (§13) et l'endpoint `GET` d'`OperationalRole` (§14) ont tous été identifiés dans la même analyse d'ouverture de lot mais volontairement laissés en l'état, chacun pour un motif documenté séparément ci-dessus — décision de ne pas étendre le modèle ou le périmètre au-delà du strict nécessaire (`CLAUDE.md` §5).

## Exception de gouvernance — Lot 9

> **Rôle de cette section** : comme au Lot 8, documenter explicitement pourquoi deux évolutions backend ont été autorisées pendant un lot que `docs/ROADMAP.md` décrit comme « frontend only ».

**Constat.** Deux écarts réels entre le cahier des charges (§19.4, §23.2) et l'API livrée aux Lots 1-6 ont été identifiés à l'ouverture du Lot 9, tous deux **validés individuellement avec l'utilisateur avant implémentation** (§10 et §12 ci-dessus) :

- l'extension des filtres de `GET /api/v1/time-entries` (§10) — ajout de paramètres de requête sur une action déjà existante, sur des colonnes déjà existantes ;
- l'ajout de `PUT /api/v1/absences/{id}` restreint au Brouillon (§12) — une nouvelle action, sur une entité déjà existante, pour un besoin explicitement exprimé par le cahier des charges et jusqu'ici non couvert.

**Nature de l'exception.** Contrairement au Lot 8 (5 référentiels entièrement nouveaux), ces deux évolutions sont **ponctuelles et de portée minimale** : aucune nouvelle entité, aucune migration pour l'extension de filtres, une seule action supplémentaire restreinte à un seul statut pour l'absence. Aucune des deux n'introduit de nouveau calcul, de nouvelle donnée financière ou de nouvelle règle de sécurité.

**Écart explicitement non corrigé.** Le manque de rattachement `TimeEntry` ↔ Application (§11) a été identifié dans la même analyse mais **volontairement laissé en l'état** — décision de ne pas étendre le modèle au-delà du strict nécessaire (`CLAUDE.md` §5).

---

## Exception de gouvernance — Lot 8

> **Rôle de cette section** : documenter explicitement une exception au processus normal de lot, pour qu'un futur développement comprenne *pourquoi* une évolution backend a été autorisée pendant un lot que `docs/ROADMAP.md` décrivait initialement comme « frontend only ». Ce n'est pas une règle métier au même titre que les sections 1 à 9 ci-dessus : c'est le compte rendu de la décision de gouvernance qui les a rendues possibles.

**Constat de départ.** Les référentiels `Technology`, `Client`, `ProjectType`, `CostCenter` et `Currency` (sections 5 à 9 ci-dessus) **n'existaient dans aucune version précédente du modèle** : absents du cahier des charges, absents de `docs/ROADMAP.md`, absents de `docs/BACKLOG_METIER.md` avant l'ouverture du Lot 8, et absents du code (aucune entité, aucun contrôleur, aucune migration). `docs/ROADMAP.md` décrivait la phase Lots 7-12 comme consommant exclusivement l'API déjà livrée aux Lots 1-6, sans évolution backend.

**Validation avant implémentation.** Leur création a fait l'objet d'une **validation fonctionnelle explicite, un référentiel à la fois, pendant cette session, avant toute ligne de code** : pour chacun, plusieurs définitions alternatives ont été soumises (ex. « Client est-il un nouvel axe, ou une simple confusion avec Company ? », « Devises est-il un référentiel de consultation, ou un vrai support multi-devises ? ») et la définition retenue a été consignée dans les sections 5 à 9 avant d'être codée, conformément à la règle d'alimentation de `CLAUDE.md` §5.

**Motif de l'exception.** Ces cinq référentiels ont été ajoutés **afin de permettre l'implémentation complète du périmètre du Lot 8** tel que demandé (fiches Ressource/Société/Application et panneau Administration incluant ces domaines) : sans backend réel, les écrans correspondants auraient dû soit être omis, soit reposer sur des données fictives côté frontend — les deux étant incompatibles avec les conventions du projet (`CLAUDE.md` §7, §17).

**Nature strictement descriptive.** Les cinq référentiels sont **exclusivement des référentiels descriptifs** — classification (`Technology`, `ProjectType`), rattachement (`Client`, `CostCenter`), ou consultation (`Currency`). Aucun n'introduit de workflow, de machine d'état, de calcul dérivé ou de règle de validation métier nouvelle au-delà de l'existence du référentiel lui-même (code/libellé/statut, éventuellement un rattachement optionnel).

**Aucun impact sur l'existant.** Ces cinq référentiels **n'introduisent aucune nouvelle logique métier** et **n'impactent aucun calcul financier, aucun TJM, aucun Budget, aucune Commande, aucun Contrat, ni aucun service métier existant** :

- `FinancialCalculationService` (Lot 2), `ProjectPlanningCalculator` (Lot 4), `BudgetService` (Lot 5) : code inchangé, aucune de leurs méthodes ne référence l'un de ces cinq référentiels.
- `ResourceTjmHistory`, `CompanyContractHistory`, `Order`, `Budget` : entités inchangées, aucune nouvelle colonne, aucune nouvelle règle de validation.
- `Currency` en particulier reste un référentiel de consultation pur (§9) : tous les montants du projet restent implicitement en EUR, exactement comme avant le Lot 8 — un véritable support multi-devises est explicitement hors périmètre et non anticipé.
- Les seuls points d'intégration avec le modèle existant sont des **clés étrangères optionnelles** (`Project.ProjectTypeId`, `Project.ClientId`) ou des **tables de jointure** (`ApplicationTechnology`, `ResourceTechnology`) : leur absence de valeur ne bloque et ne modifie aucun comportement préexistant.

**Documentation volontaire de l'exception.** Cette section est ajoutée **volontairement**, en plus des sections 5 à 9, pour qu'un futur lot ou une future session ne confonde pas cette exception ponctuelle et validée avec une dérive de périmètre généralisée : la règle « aucune évolution backend » reste la règle par défaut de la phase Lots 7-12 (`docs/ROADMAP.md`) pour tout ce qui n'est pas explicitement listé ici. Toute nouvelle demande d'évolution backend dans un lot « frontend only » doit suivre le même processus — validation explicite avec l'utilisateur, consignation ici *avant* implémentation — et non se prévaloir de ce précédent sans validation propre.

---

## 15. Commandes, Budgets et Jalons — décisions actées à l'ouverture du Lot 11

### Décision 1 — Modèle Budget

Voir §2 ci-dessus : le modèle détaillé par ligne budgétaire (société prestataire, intervenant, DDA, PR, code SAP, numéro de commande, montant réceptionné ventilé par mois) reste hors périmètre du Lot 11, explicitement reporté à un futur lot dédié.

### Décision 2 — `OrderStatus` — endpoint `GET` manquant, corrigé

✅ **Implémenté.** `OrderStatus` (5 statuts de commande, §13.2) avait la même lacune que `CompanyType`/`Role` (Lot 8, contournés) et que `ProjectStatus` avant sa correction (Lot 10, Décision 10) : jamais d'endpoint de lecture, seules les valeurs seedées existaient. **Option retenue explicitement par l'utilisateur** : un vrai endpoint (`GET /api/v1/order-statuses`, réplique exacte du pattern `ProjectStatusesController`) plutôt qu'un contournement via `knownReferentials.ts` — la Commande devient cette fois un écran CRUD complet piloté par ce statut (boutons de transition contextuels), ce qui justifie un référentiel réel. `CompanyType`/`Role`/`OperationalRole` restent en revanche non résolus (hors périmètre de ce lot).

### Décision 3 — Consommation mensuelle (§14.3)

✅ **Implémenté.** `GET /api/v1/reporting/financial` (`FinancialReportDto`, Lot 5) couvrait déjà différentiel global/par projet/par commande/par société/par ressource, besoins de rallonge et commandes à renouveler, mais pas de ventilation mensuelle exigée par §14.3. **Option retenue** : extension minimale de `FinancialReportDto`/`ReportingService.GetFinancialReportAsync` (nouveau `ConsommationMensuelle`, même style de `GroupBy` que les listes `DifferentielParX` déjà présentes, sur les mêmes `TimeEntryFinancialSnapshot` déjà chargés) — aucune nouvelle entité, aucune migration, aucune duplication de logique financière. Un mois n'apparaît que s'il porte au moins une saisie valorisée sur la période demandée.

### Décision 4 — Filtre Application sur les Jalons (§24.3)

✅ **Implémenté.** `GET /api/v1/milestones` ne filtrait que par `projectId`/`responsableId`/`statut`/`enRetard` ; §24.3 exige aussi un filtre par application. **Option retenue** : paramètre optionnel `applicationId` ajouté à l'action `GetList` déjà existante (`MilestoneService.GetListAsync`), même précédent que les extensions de filtres des Lots 9/10 — aucune nouvelle entité, filtre serveur (pas de filtrage frontend sur une liste déjà récupérée).

## Exception de gouvernance — Lot 11

> **Rôle de cette section** : comme aux Lots 8, 9 et 10, documenter explicitement pourquoi des évolutions backend ont été autorisées pendant un lot que `docs/ROADMAP.md` décrit comme construit sur l'API déjà livrée.

**Constat.** Trois évolutions backend ont été identifiées et validées individuellement avec l'utilisateur à l'ouverture du Lot 11, avant toute ligne de code : l'ajout de `GET /api/v1/order-statuses` (Décision 2), l'extension de `FinancialReportDto` avec la consommation mensuelle (Décision 3), et l'extension du filtre `applicationId` sur `GET /api/v1/milestones` (Décision 4).

**Nature de l'exception.** Strictement bornée, du même ordre que les Lots 9/10 : un nouveau contrôleur de lecture seule répliquant un pattern déjà existant (Décision 2), une extension d'agrégation réutilisant des données déjà chargées sans nouvelle règle de calcul (Décision 3), et un paramètre de filtre supplémentaire sur une action déjà existante (Décision 4). Aucune nouvelle entité, aucune migration.

**Décision explicitement déclinée.** Le modèle Budget détaillé par ligne (Décision 1, §2) a été identifié dans la même analyse d'ouverture de lot mais **volontairement non construit** — décision de ne pas étendre le modèle au-delà du strict nécessaire (`CLAUDE.md` §5), reporté à un futur lot dédié.

---

## 16. Charges, Tableau de bord, Reporting et Imports — décisions actées à l'ouverture du Lot 12

### Décision 1 — Évolution mensuelle et heatmap de charge (§21.3, §25.3)

✅ **Implémenté.** `ChargesReportDto` ne portait aucune dimension temporelle (courbe mensuelle, §21.3) ni matricielle (heatmap, `WorkloadHeatmap` nommé par `docs/ROADMAP.md`) — seuls des totaux globaux et des top N existaient. **Option retenue** : deux nouvelles listes sur le DTO existant, `EvolutionMensuelle` (heures totales/RUN/hors RUN par mois) et `Heatmap` (heures par ressource × semaine, même granularité que `WeeklyPlanningGrid`, Lot 10), calculées dans `ReportingService.GetChargesReportAsync` par un `GroupBy` sur les mêmes saisies déjà chargées pour cette méthode — même style que la consommation mensuelle financière (Lot 11, Décision 3). Aucune nouvelle entité, aucune migration.

### Décision 2 — Export dédié du rapport opérationnel (§26.1, §26.3)

✅ **Implémenté.** Le rapport opérationnel (§26.1 : charge par équipe/service/département, jalons en retard, capacité et disponibilité) a un contenu distinct des Charges (§21) ; seuls `/reporting/charges/export` et `/reporting/financial/export` existaient. **Option retenue** : `ReportingService.GetOperationalTableAsync` + `GET /reporting/operational/export`, même pattern minimal que les deux exports déjà existants (Lot 5) — aucune nouvelle logique de calcul, le moteur d'export générique (`ExportService`) reste inchangé.

### Décision 3 — Capacité vs réalisé et Prévu vs réalisé à l'échelle du portefeuille (§21.2/§21.3/§25.3)

**Capacité vs réalisé : composé côté frontend, aucune évolution backend.** `DashboardOperationalKpisDto` (`GetDashboardAsync`, Lot 5) calcule déjà `CapaciteTheorique`/`CapaciteReelle`/`ChargeRunHeures`/`ChargeHorsRunHeures` comme des totaux agrégés sur le périmètre filtré — des scalaires déjà calculés, pas une liste à paginer. Les écrans Charges et Tableau de bord réutilisent tel quel `GET /reporting/dashboard` pour ce graphique.

**Prévu vs réalisé : ✅ implémenté, agrégation backend dédiée — justification explicite (demande de l'utilisateur avant développement).** Contrairement à la capacité, aucun agrégat de charge planifiée (`ProjectWeeklyPlan`) n'existait dans `ReportingService` avant ce lot. Une composition frontend a été explicitement écartée pour 4 raisons vérifiées avant implémentation :
1. **Aucune donnée déjà exposée à réutiliser** : recherche exhaustive dans `ReportingService.cs`, aucune référence à `ProjectWeeklyPlan`/charge planifiée avant ce lot.
2. **Parité des filtres** : la seule agrégation de planning existante (`GET /project-planning`, Lot 10) filtre par `projectId/resourceId/serviceId/departmentId/teamId/from/to/surcharge` — aucun `applicationId`/`orderId`/`activityTypeId`/`operationalRoleId`, alors que ce sont précisément des filtres de Charges (§21.1). Une composition frontend aurait dû rejoindre manuellement ces dimensions, dupliquant la logique de jointure déjà présente dans `ReportingService.GetChargesReportAsync` côté « réalisé ».
3. **Agrégation sur données paginées, pas une liste déjà chargée** : `GET /project-planning` renvoie un `PagedResult` (une ligne par projet × ressource × semaine) — contrairement aux petites listes entièrement chargées (8 projets, 15 jalons) qui justifiaient une composition frontend aux Lots 9-11, sommer un total de portefeuille aurait exigé de récupérer toutes les pages, contournant la pagination serveur.
4. **Localité de la règle métier** : `ProjectPlanningService.GetOverviewAsync` code en dur la règle « seule la version Ajustée Active compte » — cette règle vit à un seul endroit (`CLAUDE.md` §5) ; l'appliquer à l'échelle du portefeuille filtré appartient à côté de sa définition existante, pas reconstruite côté frontend.

Nouvelle méthode `ReportingService` agrégeant `ChargePlanifieeInitiale`/`ChargePlanifieeAjustee` (version Ajustée Active uniquement, même règle que `ProjectPlanningService`) sur tous les projets du périmètre filtré, comparée à la charge réalisée déjà calculée (`ChargeTotaleHeures`). Aucune nouvelle entité, aucune migration.

### Décision 4 — Mapping de colonnes de l'assistant d'import (§27.3 étape 5)

✅ **Confirmé, aucun changement.** Le Lot 6 avait différé la construction d'un mapping interactif (aucun écran n'existait alors pour le remettre en cause) : les en-têtes CSV doivent correspondre exactement aux noms de propriété du DTO cible (`CsvRequestBinder.Bind<T>`). **Décision reconfirmée explicitement à l'ouverture du Lot 12** (premier lot construisant réellement l'écran) : le mapping reste en lecture seule (affichage des en-têtes détectés vs attendus, `ImportPreviewDto`, déjà exposé), sans glisser-déposer ni réassociation de colonnes. `CsvRequestBinder`/`ImportService` restent inchangés.

## Exception de gouvernance — Lot 12

> **Rôle de cette section** : comme aux Lots 8 à 11, documenter explicitement pourquoi des évolutions backend ont été autorisées pendant un lot que `docs/ROADMAP.md` décrit comme construit sur l'API déjà livrée.

**Constat.** Trois évolutions backend ont été identifiées et validées individuellement avec l'utilisateur à l'ouverture du Lot 12, avant toute ligne de code : l'extension de `ChargesReportDto` avec l'évolution mensuelle et la heatmap (Décision 1), l'ajout de `GET /reporting/operational/export` (Décision 2), et l'agrégation dédiée « Prévu vs réalisé » (Décision 3).

**Nature de l'exception.** La plus substantielle des quatre derniers lots sur le point « Prévu vs réalisé » (nouvelle méthode d'agrégation multi-projets, justifiée explicitement ci-dessus après une demande directe de vérification de l'utilisateur avant tout développement) ; les deux autres restent du même ordre que les Lots 9-11 (extension d'un DTO existant par `GroupBy`, nouvel export répliquant un pattern déjà en place). Aucune nouvelle entité, aucune migration dans les trois cas.

**Décision explicitement révisée en cours d'analyse.** « Capacité vs réalisé » avait initialement été bundlée avec « Prévu vs réalisé » dans la même question de validation ; une vérification plus poussée demandée par l'utilisateur a montré que l'agrégat existait déjà (`DashboardOperationalKpisDto`) — la recommandation a été corrigée avant tout développement pour ne construire que ce qui est réellement nécessaire (`CLAUDE.md` §5).

## 17. Authentification, RBAC, Sécurisation API, CI/CD et Qualité — décisions actées à l'ouverture du Lot 13

### Décision 1 — Authentification simulée sessionnée (cahier des charges §6.5)

✅ **Implémenté.** Le mécanisme reposait entièrement sur l'en-tête `X-Demo-User`, rejoué sur chaque requête, falsifiable par construction (aucun secret, aucune signature). **Décision validée par l'utilisateur** : rester sur une identité simulée (aucun mot de passe, aucun compte local, aucun JWT définitif, aucun écran de connexion réel, aucune intégration LDAP/AD réelle ce lot), mais introduire une vraie session serveur.

- `IAuthenticationProvider` (Application, `Common/Security/`) : cycle de vie de session (`CreateSessionAsync`/`RevokeSessionAsync`/`ResolveSessionAsync`/`ResolveDirectIdentifierAsync`), seule abstraction connue du reste de l'application — même principe que `ICurrentUser`.
- `DemoAuthenticationProvider` (Api, composition root) : implémentation de démonstration, seule à connaître le mécanisme concret.
- `UserSession` (Domain) : session persistée en base (`Id` = jeton opaque, `UserId`, `IsPersistent`, `CreatedAt`, `LastActivityAt`, `ExpiresAt`, `RevokedAt`). **`IsPersistent` conçu dès ce lot** pour porter la distinction session navigateur / session persistante ("se souvenir de moi") sans migration supplémentaire quand cette fonctionnalité sera exposée à l'écran — jamais actionnée aujourd'hui (toujours `false` à la création, aucune case à cocher construite).
- `AuthController` (`POST`/`DELETE /api/v1/auth/sessions`) : pose/efface un cookie HttpOnly, `SameSite=Strict`, `Secure` hors Development, portant uniquement le jeton de session (jamais de données métier dans le cookie).
- `IdentityResolutionMiddleware` : résout l'identité une seule fois par requête (cookie, puis repli sur l'en-tête si autorisé) et calcule les permissions effectives, dépose le résultat dans `HttpContext.Items` — `DemoCurrentUserProvider` (implémentation d'`ICurrentUser`, forme inchangée) le lit de façon synchrone, sans jamais bloquer sur une tâche asynchrone.
- **Façade de compatibilité des tests existants** : `Authentication:AllowDirectDemoHeader` (config, `true` en Development/Test uniquement, `false` en Qualification/Production) permet à l'en-tête `X-Demo-User` de continuer à résoudre une identité sans passer par une session — les 194 tests d'intégration existants (`CreateClient(identifiant)`) fonctionnent sans aucune modification.

### Décision 2 — Modèle RBAC (cahier des charges §6.1)

✅ **Implémenté.** `Role` (référentiel) et `Permission`/`UserPermission` (octroi individuel) coexistaient sans lien structurel — le rôle d'un utilisateur n'avait aucun effet sur ses droits, contrairement au modèle à deux niveaux attendu ("rôle applicatif + permissions complémentaires").

- `RolePermission` (Domain, nouvelle jointure `RoleId`/`PermissionId`) : permissions accordées par défaut à un rôle.
- `UserPermission.Effect` (nouvelle colonne, enum `UserPermissionEffect.Grant`/`Revoke`, `Grant = 0` pour que les lignes historiques des Lots 1/5/6 restent des octrois sans migration de données) : une exception individuelle peut désormais compléter (`Grant`) ou retirer (`Revoke`) un droit, priment toujours sur le rôle.
- `PermissionResolutionService` (Application, `Common/Security/`) : calcul centralisé et déterministe — `effectives = (permissions du rôle) ∪ (octrois individuels) − (retraits individuels)`. Réutilisé par la résolution d'`ICurrentUser` et par `UserService` (octroi/retrait, `UserDto.EffectivePermissionCodes`).
- **Compatibilité vérifiée avant implémentation** (demande explicite) : matrice `RolePermission` dérivée des affectations déjà seedées, sans en modifier une seule — `ADMINISTRATEUR` reçoit les 7 permissions (Bernard, seul administrateur seedé, les détient déjà toutes individuellement) ; les trois autres rôles (`RESPONSABLE_DEPARTEMENT`, `RESPONSABLE_SERVICE`, `INGENIEUR`) ne reçoivent aucune permission de rôle — le seul utilisateur `RESPONSABLE_DEPARTEMENT` (Legrand, `FINANCIAL_DATA_VIEW`) reste une exception individuelle plutôt qu'une règle de rôle inventée à partir d'un unique point de données (voir `Lot13Seed.cs`).
- `UserService.GrantPermissionAsync`/`RevokePermissionAsync` (§28.3) rendus RBAC-conscients : un octroi est en conflit si la permission est déjà **effective** (rôle ou individuelle), pas seulement si une ligne individuelle existe déjà ; un retrait qui laisserait la permission effective via le rôle matérialise une ligne de retrait explicite plutôt qu'un simple retrait d'octroi individuel. Même contrat HTTP qu'avant (`POST`/`DELETE /api/v1/users/{id}/permissions/{code}`), sémantique interne enrichie.

### Décision 3 — Périmètre organisationnel : reporté (cahier des charges §6.3)

**Explicitement différé.** Département/service/équipe/propriété de la donnée/périmètre métier/visibilité organisationnelle restent hors périmètre de ce lot — aucune règle partielle ou implicite introduite, pour ne jamais donner un faux sentiment de sécurité. Écart reconduit depuis le Lot 2, à traiter dans un lot dédié après validation fonctionnelle avec le Product Owner.

### Décision 4 — Pipeline GitHub Actions et qualité

✅ **Implémenté.** `.github/workflows/ci.yml` — deux jobs (`backend`, `frontend`), déclenchés sur `pull_request`/`push` vers `main`, plus `workflow_call` pour être invocable par un futur workflow de déploiement sans duplication. Backend : restauration/build/tests avec couverture (`dotnet test --collect:"XPlat Code Coverage"`), seuils vérifiés par `scripts/ci/check-backend-coverage.ps1` (projets `SafranTimeTracker.Migrations.*` explicitement exclus — code généré EF Core, fausserait la mesure). Frontend : `npm ci`, contrôle TypeScript, lint, tests avec couverture (seuils déclarés dans `vite.config.ts`, `@vitest/coverage-v8`), build. Rapports de couverture publiés comme artefacts GitHub Actions. **Aucun SonarQube/SonarCloud** (décision explicite de l'utilisateur), **aucun secret applicatif**, structure pensée pour accueillir plus tard Sonar (un pas d'analyse après les rapports déjà produits), Docker (un job séparé consommant les mêmes builds) et un déploiement DEV/QUAL/PROD (`cd.yml` distinct invoquant `ci.yml` via `workflow_call`) sans réorganisation — détail dans les commentaires du fichier lui-même.

### Sécurisation de l'API (périmètre initial du Lot 13, hors des 4 décisions ci-dessus)

En-têtes de sécurité (HSTS hors Development, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Content-Security-Policy: default-src 'none'` — l'API ne sert que du JSON), CORS étendu à `AllowCredentials()` (nécessaire au cookie de session, compatible avec `WithOrigins` explicites — jamais combiné à `AllowAnyOrigin`), limitation de débit native .NET (`Microsoft.AspNetCore.RateLimiting`, fenêtre fixe, ciblée sur `POST /auth/sessions`), limite de taille de requête sur les 4 endpoints d'upload d'import (10 Mo, Lot 6). Aucune extension de `[RequirePermission]` aux contrôleurs non gardés aujourd'hui (référentiels, projets, ressources, etc.) — non demandée, proche du périmètre organisationnel explicitement reporté (Décision 3).

## Exception de gouvernance — Lot 13

> **Rôle de cette section** : comme aux Lots 8 à 12, documenter explicitement pourquoi des évolutions backend ont été autorisées pendant un lot que `docs/ROADMAP.md` (« Industrialisation ») ne décrivait, avant ce lot, que par « Sécurité (revue complète) » et « Tests (couverture élargie) » — sans nommer authentification, session, RBAC ni CI/CD.

**Constat.** Le périmètre réellement demandé par l'utilisateur à l'ouverture du Lot 13 (authentification, sessions, autorisation/rôles/permissions, sécurisation API, GitHub Actions, CI/CD, qualité, préparation multi-utilisateur) dépasse la description alors présente dans `docs/ROADMAP.md`. Quatre décisions d'architecture ont été validées explicitement avant tout développement (ci-dessus), avec des ajustements supplémentaires validés dans un second temps : `IAuthenticationProvider`/`DemoAuthenticationProvider` (nommage générique plutôt que "simulé"), modèle `UserSession` compatible dès ce lot avec la distinction session navigateur/persistante, structure CI extensible pour Sonar/Docker/déploiement sans refonte.

**Nature de l'exception.** Deux nouvelles entités (`UserSession`, `RolePermission`) et une colonne additionnelle (`UserPermission.Effect`) — une migration schéma réelle, contrairement aux Lots 9-12 qui n'étendaient que des DTO/services sur des entités déjà existantes. Compensée par une vérification de compatibilité explicite avant implémentation (Décision 2) garantissant qu'aucun utilisateur seedé ne perd ou ne gagne d'accès effectif.

---

## Comment mettre à jour ce document

1. Toute règle métier nouvellement validée avec le Product Owner ou un expert métier est ajoutée ici **avant** d'être implémentée, avec le statut 🕓 **Validé, non implémenté**.
2. Quand un lot implémente une règle listée ici, son statut passe à ✅ **Implémenté** à la clôture du lot (même étape que la mise à jour de `docs/IMPLEMENTATION_STATUS.md`).
3. Un écart constaté entre une règle métier telle que formulée ici et le modèle technique réel est noté 🔎 **À clarifier**, jamais résolu unilatéralement dans le code sans validation.
4. Ce document est relu avant chaque nouveau lot, au même titre que `docs/ROADMAP.md`, `CLAUDE.md` et `docs/IMPLEMENTATION_STATUS.md`.
