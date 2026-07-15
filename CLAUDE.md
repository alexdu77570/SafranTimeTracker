# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Mémoire technique permanente du projet SAFRAN TIME TRACKER

> **Rôle de ce document** : ce fichier est la **référence technique permanente** du projet. Il est lu au début de chaque session de travail et doit permettre de reprendre le développement sans perte de contexte, même après une interruption longue.
>
> Le **cahier des charges** (`docs/Cahier_des_charges_SAFRAN_TIME_TRACKER_v2.1_Windows_Server.md`) reste la **référence fonctionnelle unique**. En cas de doute sur une règle métier, c'est ce document qui prévaut. `CLAUDE.md` ne redéfinit jamais une règle métier : il définit **comment on construit** ce que le cahier des charges **demande**.
>
> Documents associés : `README.md` (présentation), `docs/ARCHITECTURE.md` (architecture détaillée), `docs/DATABASE.md` (modèle de données), `docs/CONVENTIONS.md` (conventions détaillées avec exemples), `docs/ROADMAP.md` (découpage en lots), `docs/IMPLEMENTATION_STATUS.md` (avancement réel).

---

## 0. Statut actuel du projet et commandes

**Phase : socle documentaire.** Aucun code métier n'existe encore : ni projet .NET, ni projet React, ni solution Visual Studio, ni base de données, ni migration. Le dépôt n'est pas encore initialisé en Git.

**Il n'existe donc aucune commande de build, de lint ou de test à ce jour.** Ne pas supposer l'existence d'un `dotnet build`, `npm run dev`, `npm test` ou équivalent, ni inventer une commande plausible : vérifier d'abord l'arborescence réelle (`backend/`, `frontend/`, `database/`, `deploy/`, `scripts/` sont actuellement vides). Ces commandes seront définies au Lot 0 (voir `docs/ROADMAP.md`) et documentées ici et dans `README.md` dès qu'elles existeront.

Avant toute génération de code, se référer à `docs/ROADMAP.md` (Lot 0 - Fondations) et à `docs/IMPLEMENTATION_STATUS.md` pour connaître l'état réel d'avancement. Ne jamais supposer qu'une couche existe sans l'avoir vérifiée dans l'arborescence.

---

## 1. Vision produit

SAFRAN TIME TRACKER est une plateforme interne de pilotage destinée à une organisation **Production Applicative / RUN / MCO / Projets**. C'est un **cockpit unique** qui doit répondre en continu à des questions de charge, de capacité, de dérive projet et de valorisation financière (coût réel, coût contractuel, différentiel).

L'application doit se comporter comme un **véritable produit métier DSI** : fiable, professionnel, cohérent, traçable, utilisable au quotidien — jamais comme une maquette.

SAFRAN TIME TRACKER n'intègre **jamais** les fonctions de DS-EYE (documents, procédures, certificats, serveurs, flux techniques, comptes techniques, Golden Data, base de connaissance, schémas d'architecture, dépôt documentaire). Une application enregistrée dans l'outil est uniquement un objet de rattachement statistique.

---

## 2. Objectifs du projet

- Fournir une vision unifiée de la charge (RUN / hors RUN) par ressource, application, projet, service, équipe.
- Suivre les projets (planning initial / ajusté / réalisé), les jalons, les risques planning et budget.
- Historiser toute donnée à incidence financière, capacitaire ou organisationnelle sans jamais recalculer le passé.
- Séparer strictement rôle applicatif, permissions complémentaires, rattachement organisationnel, rattachement société, affectation projet et rattachement commande.
- Protéger les données financières côté serveur, jamais uniquement côté affichage.
- Rester déployable sur Windows Server / IIS dès le premier lot, avec une persistance interchangeable (PostgreSQL, SQL Server, SQLite) grâce à Entity Framework Core.

---

## 3. Choix d'architecture

- **Frontend et backend strictement séparés.** Le navigateur n'accède jamais directement à la base de données ; toute donnée transite par l'API.
- **Architecture backend en couches** : `Domain`, `Application`, `Infrastructure`, `Api`, `Tests` (voir `docs/ARCHITECTURE.md`). Les couches `Domain` et `Application` ne dépendent d'aucune API Windows spécifique, pour rester portables vers Linux.
- **Persistance interchangeable** : le provider EF Core (PostgreSQL par défaut, SQL Server en alternative, SQLite pour le développement/tests uniquement) est sélectionné par configuration, jamais par branchement de code.
- **Historisation systématique** des données à incidence financière, capacitaire ou organisationnelle (TJM, contrats, rattachements société, affectations, versions de planning, rallonges, budgets ajustés, valeurs financières figées des saisies, changements de rôle/permission, imports).
- **Absence de recalcul rétroactif.** Toute saisie de temps fige un instantané financier (`TimeEntryFinancialSnapshot`) au moment du calcul. Une modification future d'un TJM ou d'un contrat ne modifie jamais une saisie déjà enregistrée.
- **Séparation des responsabilités de sécurité** : rôle applicatif, rôles opérationnels, permissions complémentaires, rattachement organisationnel, rattachement société, affectation projet et rattachement commande sont des axes indépendants.
- **Contrôle d'accès exclusivement côté serveur.** Le frontend ne fait jamais office de barrière de sécurité : une donnée non autorisée n'est jamais renvoyée par l'API, même masquée à l'écran.
- **Calculs financiers centralisés** dans un service unique et testable (`FinancialCalculationService`), jamais dupliqués ailleurs dans le code.

---

## 4. Technologies retenues

### Backend
- **ASP.NET Core Web API**, version **.NET 10 (LTS)** — décision validée (LTS active à la date de démarrage du projet).
- **Entity Framework Core** (Code First, migrations versionnées par provider).
- **FluentValidation** pour la validation serveur.
- **Serilog** pour la journalisation structurée.
- Mapping DTO ↔ Entité : **Mapster** — voir `docs/CONVENTIONS.md`.

### Frontend
- **React 19** + **TypeScript strict** + **Vite**.
- **React Router** pour le routage client.
- **TanStack Query (React Query)** pour la gestion des appels API et du cache serveur.
- **React Hook Form** + **Zod** pour la validation de formulaires.
- Bibliothèque de graphiques professionnelle : **Recharts**.

### Base de données
- **PostgreSQL** (provider principal recommandé, environnements partagés).
- **Microsoft SQL Server** (provider alternatif si contrainte d'entreprise).
- **SQLite** (développement local, tests automatisés, démonstrations mono-utilisateur uniquement — jamais en production multi-utilisateur).

### Outillage et qualité
- Tests backend : **xUnit** + **FluentAssertions** + **NSubstitute** (mocking).
- Tests frontend : **Vitest** + **React Testing Library** ; tests end-to-end : **Playwright**.
- Documentation API : **OpenAPI/Swagger**.
- Scripts de build/déploiement : **PowerShell**.

---

## 5. Conventions de code générales

- Code **typé de bout en bout** : `TypeScript strict` côté frontend, nullable reference types activés côté backend.
- Aucune logique métier dans les contrôleurs ni dans les composants React : elle vit dans les services applicatifs (backend) et les hooks/services dédiés (frontend).
- Pas de duplication des règles métier : une règle = un seul endroit dans le code.
- Pas d'abstraction anticipée : on code ce que le lot demande, pas ce qu'un lot futur pourrait demander.
- Le détail complet (style, imports, organisation de fichiers) est dans `docs/CONVENTIONS.md`.

## 6. Conventions de nommage

- **C#** : `PascalCase` pour classes, méthodes, propriétés ; `camelCase` pour variables locales et paramètres ; interfaces préfixées `I`.
- **TypeScript/React** : composants et types en `PascalCase`, variables et fonctions en `camelCase`, hooks personnalisés préfixés `use`.
- **Fichiers** : un composant React par fichier nommé comme le composant (`KpiCard.tsx`) ; fichiers utilitaires en `camelCase.ts`.
- **Base de données** : entités C# en `PascalCase` ; mapping physique en `snake_case` via `EFCore.NamingConventions`, pour rester idiomatique sur PostgreSQL tout en gardant un code C# conventionnel.
- **Endpoints API** : segments en `kebab-case` au pluriel (`/api/v1/company-contracts`).
- **Exception de nommage actée** : l'entité fonctionnelle « application » du référentiel logiciel (cahier des charges §15) est implémentée sous le nom de classe **`ApplicationReference`** (et non `Application`), pour éviter toute collision avec le namespace de couche `SafranTimeTracker.Application`. Le service associé est `ApplicationReferenceService`. Le vocabulaire fonctionnel (« application », « référentiel d'applications ») ne change pas : seule l'implémentation technique porte ce nom. Détail : `docs/ARCHITECTURE.md` §2.
- Détails et exemples complets : `docs/CONVENTIONS.md`.

## 7. Règles de développement

- Ne jamais accéder à la base de données depuis le frontend : tout passe par l'API versionnée.
- Toute validation présente côté client doit être **rejouée côté serveur** ; le serveur ne fait jamais confiance au client.
- Les paramètres configurables (heures par jour, seuils d'alerte, activation du workflow d'absence, mode démonstration, etc.) vivent dans l'entité `Settings` / la configuration externalisée, jamais en dur dans le code.
- Toute donnée financière fige un instantané au moment du calcul ; aucun recalcul silencieux, même en masse.
- Le provider de persistance ne doit jamais être supposé : le code métier ne contient aucun SQL natif ni procédure stockée spécifique à un moteur.
- Les montants sont systématiquement de type `decimal`, jamais `float`/`double`.
- Les dates représentant un instant sont stockées en UTC ; la conversion d'affichage (format français) est une responsabilité du frontend.
- La suppression physique de données liées à des temps, absences, projets, imports ou journaux est interdite : on utilise un statut (désactivation, archivage) et jamais une suppression physique, sauf cas explicitement prévu par le cahier des charges (ex. compte de test sans donnée liée).
- La terminologie « Squad Leader » est proscrite partout (code, données, libellés, documentation) ; on utilise « équipe ».

## 8. Règles Git

- Dépôt à initialiser en Lot 0. Branche principale `main`, protégée : pas de push direct, pas de force-push sur `main`.
- Branches de travail : `feature/<lot>-<sujet>`, `fix/<sujet>`, `chore/<sujet>` (ex. `feature/lot0-structure-solution`).
- Messages de commit au format **Conventional Commits** (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`).
- Une Pull Request est requise avant toute fusion sur `main`.
- Chaque lot se termine par un état **compilable, testé et démontrable**, idéalement tagué (`lot-0`, `lot-1`, …).
- Aucun secret, certificat, mot de passe ou chaîne de connexion réelle n'est jamais commité (voir `.gitignore` et section 17).

## 9. Conventions React

- Composants fonctionnels uniquement, avec hooks. Pas de composants classe.
- Un seul point d'entrée centralisé pour les appels API (client HTTP unique avec intercepteurs), jamais d'appel `fetch` dispersé dans les composants.
- Gestion de l'état serveur via TanStack Query ; l'état local React (`useState`/`useReducer`) est réservé à l'état d'interface pur.
- `localStorage` réservé aux préférences non sensibles (thème, filtres mémorisés, taille de page, état de la sidebar) — jamais aux données métier.
- Organisation par fonctionnalité (feature folders), composants réutilisables regroupés dans un dossier `components/ui` partagé.
- Formulaires validés avec React Hook Form + Zod, avec des schémas de validation partagés autant que possible avec les DTO backend (mêmes règles, exprimées deux fois par nécessité technique).
- Accessibilité : navigation clavier, contraste suffisant, libellés explicites sur tous les champs et actions.

## 10. Conventions ASP.NET Core

- Architecture en couches pragmatique : `Domain` (entités, règles), `Application` (services, DTO, validation, orchestration), `Infrastructure` (EF Core, providers, implémentations techniques), `Api` (contrôleurs, middlewares, configuration HTTP).
- Contrôleurs **minces** : ils valident la requête, appellent un service applicatif, retournent un résultat. Aucune règle métier dans un contrôleur.
- Injection de dépendances systématique, pas de service statique portant de la logique métier.
- Gestion centralisée des erreurs via middleware, réponses au format `ProblemDetails`.
- Configuration via le pattern `IOptions<T>`, déclinée au minimum pour `Development`, `Qualification`, `Production`.
- Endpoint de santé (`/health`) exposé pour les scripts de déploiement et de supervision.
- Documentation OpenAPI générée et tenue à jour à chaque endpoint ajouté.
- Autorisation basée sur des policies combinant rôle, permissions complémentaires et périmètre organisationnel (département/service/équipe/propriété de la donnée).

## 11. Conventions Entity Framework Core

- Approche **Code First** : le modèle C# est la source de vérité, les migrations sont générées à partir de lui.
- Configuration des entités via `IEntityTypeConfiguration<T>` dédiées (pas d'annotations de données pour les règles complexes : clés composites, contraintes anti-chevauchement, index).
- Migrations **versionnées et séparées par provider** (`database/postgresql/migrations`, `database/sqlserver/migrations`, `database/sqlite`), sans divergence du modèle logique entre providers.
- Concurrence optimiste (jeton de concurrence) sur les entités sensibles (TJM, contrats, budgets, commandes).
- Statut plutôt que suppression physique sur toutes les entités concernées par l'historisation ou par des règles métier de rétention.
- Index explicites sur les clés de recherche fréquentes : dates, ressource, projet, commande, société.
- Jeu de données initial (seed) **idempotent**, rejouable sans erreur ni duplication.
- Le provider actif est résolu uniquement via la configuration (`appsettings` / variables d'environnement), jamais codé en dur.

## 12. Conventions API REST

- Endpoints versionnés sous `/api/v1/...`, ressources au pluriel, en `kebab-case` (`/api/v1/resource-tjm-history`).
- Pagination serveur systématique sur les listes (paramètres `page`, `pageSize`), tri (`sort`) et filtres explicites en query string.
- Codes HTTP cohérents : `200`/`201`/`204` pour les succès, `400` pour les erreurs de validation, `401`/`403` pour l'authentification/autorisation, `404` pour les ressources absentes, `409` pour les conflits métier (chevauchement, période close, etc.).
- Erreurs fonctionnelles explicites et non techniques, formatées en `ProblemDetails`.
- Les endpoints exposant des données financières exigent explicitement la permission `FINANCIAL_DATA_VIEW` côté serveur ; en son absence, les champs financiers sont **absents de la réponse**, jamais simplement à `null` ou masqués côté client.
- Documentation OpenAPI obligatoire pour chaque endpoint.

## 13. Conventions DTO

- L'API n'expose **jamais** directement une entité EF Core : chaque endpoint utilise des DTO dédiés.
- Nommage : `XxxDto` (lecture), `XxxCreateRequest` / `XxxUpdateRequest` (écriture). Pas de DTO générique fourre-tout.
- Un DTO de lecture ne contient que les champs autorisés pour le rôle/permission de l'appelant : la projection se fait côté service applicatif, pas par filtrage a posteriori.
- Les DTO financiers (montants, TJM, coûts, différentiels) sont isolés dans des DTO ou des sous-objets dédiés, pour permettre de les omettre facilement selon la permission `FINANCIAL_DATA_VIEW`.
- Mapping entité ↔ DTO explicite et testé, via **Mapster** (pas de mapping « magique » non vérifié).

## 14. Conventions de tests

- **Tests unitaires** en priorité sur la logique à forte valeur métier : recherche du TJM par date, détection des chevauchements, recherche du contrat par date, calcul du coût réel/contractuel/différentiel, calcul capacitaire, classification RUN/hors RUN, écarts projet, alertes budget, comparaison d'import.
- **Tests d'intégration** : création d'une saisie avec snapshot, modification d'un taux sans impact rétroactif, gestion d'une rallonge, désactivation d'un utilisateur, contrôle d'accès financier, import transactionnel, audit des modifications.
- **Tests end-to-end** couvrant au minimum les scénarios de la section 38.3 du cahier des charges.
- Nommage des tests : `NomMethode_Scenario_ResultatAttendu`. Structure **Arrange / Act / Assert**.
- Les tests d'intégration s'exécutent sur un provider réel (SQLite en priorité, PostgreSQL en CI si disponible), jamais sur une base mockée pour la logique financière.
- Un lot n'est considéré terminé que si sa logique critique est couverte par des tests qui passent réellement (voir section 20).

## 15. Conventions de logging

- Journalisation **structurée** (Serilog), jamais de simples `Console.WriteLine`.
- Niveaux standards : `Debug` (détails techniques), `Information` (événements métier normaux), `Warning` (situation anormale non bloquante), `Error` (échec applicatif), `Fatal` (échec critique).
- Sur Windows Server : logs API sous `E:\data\logs\SafranTimeTracker\api`, logs frontend/IIS sous `E:\data\logs\SafranTimeTracker\web`, logs de jobs sous `E:\data\logs\SafranTimeTracker\jobs`, logs d'installation sous `E:\data\logs\SafranTimeTracker\installation`.
- Aucune donnée sensible (secret, chaîne de connexion, mot de passe) n'est jamais journalisée.
- Le journal applicatif (technique) est distinct du **journal d'audit** (métier) : voir section 16. Les logs applicatifs ne remplacent jamais l'audit.

## 16. Conventions d'audit

- Le journal d'audit est une donnée métier persistée (`AuditLog`), pas un simple fichier de log.
- Toute action listée en section 28.3 du cahier des charges (création/modification/suppression logique d'une saisie, gestion des utilisateurs, rôles, permissions, TJM, contrats, commandes, rallonges, sociétés, paramètres, recalcul financier explicite, imports, exports financiers, etc.) génère une entrée d'audit.
- Chaque entrée contient : auteur, date/heure, action, type d'objet, identifiant d'objet, ancienne valeur, nouvelle valeur, motif éventuel, contexte technique si disponible.
- L'écriture d'audit se fait **dans la même transaction** que le changement métier qu'elle décrit.
- Le journal d'audit n'est **jamais modifiable** depuis l'interface standard ; centralisé via `AuditService`.

## 17. Conventions de sécurité

- Contrôle d'accès **exclusivement côté serveur** ; le frontend adapte l'affichage mais ne constitue jamais une protection.
- Modèle d'autorisation à deux niveaux indépendants : rôle applicatif + permissions complémentaires, combinés au périmètre organisationnel (département/service/équipe), à la propriété de la donnée et au statut de l'utilisateur.
- Permission `FINANCIAL_DATA_VIEW` obligatoire pour tout accès à une donnée financière (TJM, contrats, budgets, commandes, coûts, différentiels, montants, exports financiers) ; sans elle, la donnée est absente de la réponse API.
- Seul un Administrateur peut attribuer/retirer le rôle Administrateur ou les permissions financières. Un utilisateur ne peut pas retirer son propre dernier accès administrateur si cela laisse l'application sans administrateur actif.
- Validation systématique de toutes les entrées (FluentValidation côté API), protection contre les injections, absence de SQL natif non paramétré.
- Origines CORS explicitement configurées par environnement, jamais en `*` hors développement local.
- Authentification de démonstration acceptée pour le MVP, mais l'architecture doit permettre une intégration future avec Active Directory, LDAP, OpenID Connect ou un SSO d'entreprise, sans refonte du modèle d'autorisation.
- Aucun secret, certificat ou mot de passe réel dans le dépôt Git (voir `.gitignore`) ; HTTPS obligatoire hors environnement de développement ; certificats stockés sous `E:\certificats\SafranTimeTracker`, jamais dans Git.
- Principe du moindre privilège appliqué systématiquement.

## 18. Chemins Windows Server retenus

Cible de déploiement principale : Windows Server + IIS.

```text
E:\appl\SafranTimeTracker\        Application (arborescence ci-dessous)
E:\certificats\SafranTimeTracker\ Certificats (jamais dans Git)
E:\CD_INSTALL\SafranTimeTracker\  Prérequis, releases, scripts de déploiement, packages de rollback, notes de version
E:\data\SafranTimeTracker\        Données applicatives
E:\data\logs\SafranTimeTracker\   Logs
```

Arborescence applicative (`E:\appl\SafranTimeTracker\`) :

```text
api\        migrations\
web\        backups\
config\     docs\
scripts\    VERSION
```

Arborescence des données (`E:\data\SafranTimeTracker\`) :

```text
database\             imports\      temporary\
database-backups\     exports\      archive\
```

Arborescence des logs (`E:\data\logs\SafranTimeTracker\`) :

```text
api\    audit\   installation\
web\    jobs\    archive\
```

Chemins physiques IIS recommandés : frontend → `E:\appl\SafranTimeTracker\web`, API → `E:\appl\SafranTimeTracker\api`. Publication sous un même nom DNS, API exposée sous `/api`. HTTPS obligatoire hors développement.

## 19. Règles de déploiement

- Déploiement **automatisé par PowerShell**, reproductible. Scripts minimum attendus : `build.ps1`, `deploy.ps1`, `rollback.ps1`, `backup-database.ps1`, `restore-database.ps1`, `health-check.ps1`, `install-iis.ps1` (ou documentation équivalente), `purge-logs.ps1`.
- Séquence de déploiement obligatoire :
  1. vérification des prérequis ;
  2. sauvegarde de la version installée et de la base ;
  3. arrêt propre de l'application ;
  4. déploiement API + frontend ;
  5. application des migrations du provider actif ;
  6. restauration/injection de la configuration externalisée ;
  7. redémarrage de l'application ;
  8. exécution d'un health check ;
  9. compte rendu dans `E:\data\logs\SafranTimeTracker\installation` ;
  10. rollback documenté disponible en cas d'échec.
- Sauvegarde de la base sous `E:\data\SafranTimeTracker\database-backups`, journalisation des opérations de sauvegarde/restauration sous `E:\data\logs\SafranTimeTracker\jobs`. Procédure de sauvegarde obligatoire avant toute mise à jour de production.
- Aucun secret, certificat ou chaîne de connexion réelle n'est déployé via Git : la configuration sensible est injectée à part, par environnement (Development / Qualification / Production).
- L'exécution doit rester compatible Windows Server, avec une portabilité Linux conservée en option (dossier `deploy/linux` réservé, non prioritaire pour le MVP).

## 20. Méthode de travail par lots

Le projet est découpé en lots (voir `docs/ROADMAP.md`, détaillé à partir de la section 40 du cahier des charges) :

`Lot 0` Fondations → `Lot 1` Référentiels → `Lot 2` Modèle financier → `Lot 3` Temps et capacité → `Lot 4` Projets → `Lot 5` Budgets et reporting → `Lot 6` Imports et audit → `Lot 7` Industrialisation.

Règles de méthode :

- **Un lot n'est terminé que lorsqu'il produit une version compilable, testée réellement et démontrable.** On ne démarre pas le lot suivant avant que le lot courant satisfasse ce critère.
- Chaque lot met à jour `docs/IMPLEMENTATION_STATUS.md` en fin de réalisation (statut, date, écarts éventuels par rapport à `docs/ROADMAP.md`).
- Toute décision d'architecture ou de convention prise pendant un lot qui modifie ce document doit être répercutée dans `CLAUDE.md` **avant** de passer au lot suivant : ce fichier doit toujours refléter l'état réel des décisions techniques, pas seulement les intentions initiales.
- Aucune fonctionnalité d'un lot ultérieur n'est anticipée dans un lot antérieur (pas de sur-ingénierie préventive).
- Aucun lot ne doit réintroduire une des régressions interdites par la section 42 du cahier des charges (dépendance du TJM au projet/à la commande, recalcul rétroactif silencieux, terminologie « Squad Leader », gestion documentaire, fonctionnalité DS-EYE, protection financière uniquement visuelle, suppression physique de données historiques liées, dépendance à un moteur de base unique, chemins non conformes, secrets commités).

---

## Références

- Cahier des charges fonctionnel : `docs/Cahier_des_charges_SAFRAN_TIME_TRACKER_v2.1_Windows_Server.md`
- Architecture détaillée : `docs/ARCHITECTURE.md`
- Modèle de données : `docs/DATABASE.md`
- Conventions détaillées (exemples) : `docs/CONVENTIONS.md`
- Découpage en lots : `docs/ROADMAP.md`
- Avancement réel : `docs/IMPLEMENTATION_STATUS.md`
