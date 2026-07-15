# CAHIER DES CHARGES FONCTIONNEL ET TECHNIQUE

## SAFRAN TIME TRACKER

**Version consolidée : 2.1**  
**Date : 10 juillet 2026**  
**Statut : Référentiel de conception, de recette et de déploiement Windows Server**  
**Porteur fonctionnel : Alexandre BERNARD & Emmanuel MANCERON**

---

# 1. Objet du document

Le présent cahier des charges définit les besoins fonctionnels, les règles métier, les principes d’architecture, les exigences de sécurité et les critères de recette de l’application **SAFRAN TIME TRACKER**.

Il remplace les formulations précédentes dispersées et constitue la référence unique pour :

- la conception fonctionnelle ;
- la conception UX/UI ;
- le développement frontend et backend ;
- la modélisation de la base de données ;
- la préparation des données de démonstration ;
- les tests fonctionnels ;
- la recette métier ;
- les évolutions ultérieures.

L’application doit être conçue comme un produit interne pérenne, et non comme une simple maquette graphique.

---

# 2. Vision produit

SAFRAN TIME TRACKER est une plateforme interne de pilotage destinée à une organisation de **Production Applicative / RUN / MCO / Projets**.

L’objectif est de fournir un cockpit unique permettant de répondre rapidement aux questions suivantes :

- Qui travaille sur quoi ?
- Combien de temps est consacré au RUN et au hors RUN ?
- Quelle est la charge réelle par application, projet, service, équipe ou ressource ?
- Quels projets dérivent en charge, en budget ou en planning ?
- Quels jalons sont en retard ou à venir ?
- Quelles ressources sont surchargées, disponibles ou sous-utilisées ?
- Quels budgets et commandes sont proches de leur limite ?
- Quel est le coût réel des activités ?
- Quel est le coût contractuel associé aux prestations externes ?
- Quel est le différentiel financier entre coût réel et coût contractuel ?
- Quel est l’atterrissage prévisionnel des projets et commandes ?

L’application doit donner l’impression d’un véritable produit métier DSI : fiable, professionnel, cohérent, traçable et utilisable au quotidien.

---

# 3. Périmètre fonctionnel

## 3.1 Fonctionnalités incluses

SAFRAN TIME TRACKER couvre les domaines suivants :

- saisie et suivi des temps ;
- classification RUN et hors RUN ;
- suivi des incidents, changes, problems et RITM ;
- suivi des activités VABE et VSR ;
- suivi des astreintes ;
- gestion des applications comme référentiel statistique léger ;
- gestion des projets ;
- gestion des participants et affectations projet ;
- planning projet initial, ajusté et réalisé ;
- gestion des jalons ;
- gestion des ressources ;
- gestion des utilisateurs et des droits ;
- gestion de l’organisation : département, services et équipes ;
- gestion des rôles opérationnels ;
- gestion des disponibilités et absences ;
- gestion des sociétés internes et externes ;
- historique des rattachements d’une ressource à une société ;
- historique des TJM propres aux ressources ;
- historique des contrats de prestation des sociétés externes ;
- gestion des commandes ;
- gestion des budgets et rallonges ;
- calcul et historisation des coûts ;
- reporting opérationnel, capacitaire et financier ;
- imports CSV et imports SharePoint simulés ;
- comparaison de lots d’import ;
- journal d’audit.

## 3.2 Exclusions formelles

SAFRAN TIME TRACKER ne doit jamais intégrer les fonctions appartenant à DS-EYE.

Sont explicitement exclus :

- documents ;
- procédures ;
- certificats ;
- serveurs ;
- flux techniques ;
- comptes techniques ;
- Golden Data ;
- base de connaissance ;
- schémas d’architecture ;
- dépôt de fichiers documentaires ;
- gestion documentaire applicative ;
- génération de MEX, DAT, DIA ou dossiers d’architecture.

Une application enregistrée dans SAFRAN TIME TRACKER est uniquement un objet de rattachement et de statistique.

---

# 4. Principes directeurs

## 4.1 Source de vérité centralisée et portabilité de la persistance

La version consolidée cible une application réellement multi-utilisateur, déployée en priorité sur **Windows Server**.

La source de vérité des données métier est une base relationnelle centralisée accessible exclusivement par l’API backend. Le navigateur ne doit jamais accéder directement à la base.

L’architecture de persistance doit rester **interchangeable** grâce à Entity Framework Core et à une configuration explicite du provider. Les providers prévus sont :

- **PostgreSQL**, provider principal recommandé pour les environnements partagés ;
- **Microsoft SQL Server**, provider alternatif lorsqu’une contrainte d’entreprise l’impose ;
- **SQLite**, réservé au développement local, aux tests automatisés et aux démonstrations mono-utilisateur.

Le choix du provider est externalisé dans la configuration et ne doit pas nécessiter de modification du code métier.

Le navigateur ne doit pas utiliser `localStorage` comme base métier principale. Il peut uniquement être utilisé pour des préférences non sensibles, par exemple :

- thème d’affichage ;
- filtres mémorisés ;
- taille de page ;
- état de la sidebar.
## 4.2 Historisation obligatoire

Toute donnée ayant une incidence financière, capacitaire ou organisationnelle doit être historisée lorsque sa modification pourrait changer l’interprétation du passé.

Cela concerne notamment :

- les TJM des ressources ;
- les contrats des sociétés ;
- les rattachements ressource/société ;
- les affectations projet ;
- les versions de planning ;
- les rallonges de commandes ;
- les budgets ajustés ;
- les valeurs financières figées dans les saisies de temps ;
- les changements de rôle ou permission ;
- les imports.

## 4.3 Absence de recalcul rétroactif

Une modification future d’un TJM, d’un contrat ou d’un paramètre ne doit jamais modifier automatiquement les consommations déjà enregistrées.

Les valeurs utilisées lors du calcul d’une saisie de temps sont stockées dans la saisie elle-même sous forme d’instantané financier.

## 4.4 Séparation des responsabilités

Les éléments suivants doivent être indépendants :

- rôle applicatif ;
- rôles opérationnels ;
- permissions complémentaires ;
- rattachement organisationnel ;
- rattachement société ;
- affectation projet ;
- rattachement commande.

Cette séparation permet de faire évoluer la sécurité sans casser le modèle métier.

---

# 5. Utilisateurs cibles et organisation

## 5.1 Organisation cible

La structure organisationnelle est :

1. Département ;
2. Service ;
3. Équipe ;
4. Ressource.

La terminologie « Squad Leader » est supprimée de l’application. Aucune occurrence ne doit apparaître dans les pages, filtres, données, services, types, variables, libellés ou documentation.

Le terme « squad » doit être remplacé par **équipe** lorsqu’il désigne une unité opérationnelle.

## 5.2 Rôles applicatifs

Les rôles applicatifs sont :

- Ingénieur ;
- Responsable Service ;
- Responsable Département ;
- Administrateur.

## 5.3 Hiérarchie fonctionnelle

### Ingénieur

Peut notamment :

- saisir et consulter ses temps ;
- modifier ou supprimer ses propres saisies selon les règles de clôture ;
- consulter les applications et projets autorisés ;
- consulter son planning et ses affectations ;
- déclarer et suivre ses absences ;
- consulter ses indicateurs personnels non confidentiels.

### Responsable Service

Peut notamment, dans le périmètre de son service :

- consulter les temps et charges ;
- consulter les ressources et disponibilités ;
- suivre les projets et jalons ;
- créer et modifier des utilisateurs de son périmètre ;
- désactiver ou réactiver un utilisateur de son périmètre ;
- gérer les affectations et capacités ;
- consulter les indicateurs consolidés du service ;
- accéder aux données financières uniquement s’il possède la permission correspondante.

### Responsable Département

Peut notamment, dans le périmètre du département :

- consulter tous les services et équipes ;
- gérer les utilisateurs non administrateurs ;
- piloter les ressources, charges, projets et jalons ;
- accéder aux reportings consolidés ;
- arbitrer les capacités ;
- gérer les sociétés, commandes et budgets selon ses permissions ;
- accéder aux données financières uniquement s’il possède la permission correspondante.

### Administrateur

Dispose d’un accès complet à l’administration fonctionnelle et technique de l’application.

Il peut notamment :

- créer et modifier tous les utilisateurs ;
- promouvoir ou rétrograder un utilisateur ;
- attribuer ou retirer les permissions complémentaires ;
- promouvoir un utilisateur en Administrateur ;
- retirer le rôle Administrateur ;
- gérer les référentiels ;
- gérer les paramètres ;
- consulter l’audit ;
- corriger les données selon une procédure contrôlée.

L’application ne doit pas limiter le nombre d’administrateurs.

## 5.4 Utilisateurs de démonstration

Les données initiales doivent au minimum inclure :

- Alexandre BERNARD / s636140 / Administrateur ;
- Fabien LEGRAND / Responsable Département ;
- Thierry GEORGES / Responsable Service ;
- Emmanuel MANCERON / Responsable Service ;
- Thomas FOCQUENOEY / Responsable Service ;
- Alexandre REAU / Responsable Service ;
- Reena MISHRA / Ingénieur ;
- Camille DURAND / Ingénieur ;
- Minh NGUYEN / Ingénieur ;
- Aarav PATEL / Ingénieur ;
- Julie LEFEVRE / Ingénieur ;
- Marco COSTA / Ingénieur ;
- Priya VERMA / Ingénieur.

Ces données sont modifiables depuis l’administration.

---

# 6. Sécurité et permissions

## 6.1 Modèle d’autorisation

Le modèle de sécurité repose sur deux niveaux indépendants :

1. un rôle applicatif ;
2. des permissions complémentaires.

Un changement de rôle ne doit pas supprimer automatiquement les permissions complémentaires sans confirmation explicite.

## 6.2 Permission financière

Créer au minimum la permission :

**FINANCIAL_DATA_VIEW**

Cette permission autorise l’accès aux éléments suivants :

- historique des TJM ;
- historique des contrats ;
- budgets ;
- commandes et montants ;
- consommations financières ;
- coûts réels ;
- coûts contractuels ;
- différentiels ;
- prévisions financières ;
- montants en euros ;
- exports financiers.

Sans cette permission :

- les pages financières doivent être masquées ou neutralisées ;
- les colonnes financières ne doivent pas être retournées par l’API ;
- les cartes KPI financières ne doivent pas être visibles ;
- les exports ne doivent pas contenir de données financières ;
- les montants ne doivent pas être simplement cachés par le frontend.

Le contrôle doit être appliqué côté serveur.

## 6.3 Périmètre d’accès

L’autorisation doit combiner :

- rôle ;
- permissions ;
- département ;
- service ;
- équipe ;
- propriété de la donnée ;
- statut de l’utilisateur.

## 6.4 Règles sensibles

- Seul un Administrateur peut attribuer ou retirer le rôle Administrateur.
- Seul un Administrateur peut attribuer ou retirer les permissions financières.
- Un utilisateur ne peut pas retirer son propre dernier accès administrateur si cela laisse l’application sans administrateur actif.
- La désactivation d’un utilisateur bloque son authentification mais conserve toutes ses données.
- La suppression physique d’un utilisateur lié à des temps, absences, projets, imports ou journaux est interdite.
- Les modifications de sécurité sont intégralement auditées.

## 6.5 Authentification

Pour la démonstration, une authentification applicative simulée peut être proposée.

L’architecture doit néanmoins permettre une future intégration avec :

- Active Directory ;
- LDAP ;
- OpenID Connect ;
- SSO d’entreprise.

Aucun mot de passe réel ou secret d’entreprise ne doit être intégré aux données de démonstration.

---

# 7. Architecture technique cible

## 7.1 Frontend

Technologies attendues :

- React ;
- TypeScript ;
- Vite ;
- composants réutilisables ;
- routage client ;
- gestion centralisée des appels API ;
- validation des formulaires ;
- graphiques professionnels ;
- design responsive.

Le build frontend produit des fichiers statiques pouvant être servis par IIS. Les appels vers le backend utilisent une URL relative, par exemple `/api/v1`, afin de ne pas coder le nom du serveur dans l’application.

## 7.2 Backend

Le backend doit être une API métier distincte du frontend.

Solution retenue :

- ASP.NET Core Web API, version LTS retenue au démarrage du projet ;
- Entity Framework Core ;
- architecture en couches pragmatique ;
- validation serveur ;
- gestion centralisée des erreurs ;
- journalisation structurée ;
- contrôle d’accès par rôle et permission ;
- endpoints versionnés ;
- exécution compatible Windows Server et portable vers Linux si nécessaire.

L’application ne doit pas dépendre d’API Windows spécifiques dans les couches Domain et Application.

## 7.3 Base de données et providers

Provider principal recommandé :

- PostgreSQL.

Providers alternatifs prévus :

- Microsoft SQL Server ;
- SQLite pour le développement et les tests uniquement.

Exigences :

- sélection du provider par configuration ;
- migrations versionnées et séparées par provider lorsque nécessaire ;
- scripts de création, sauvegarde et restauration ;
- contraintes d’intégrité ;
- index sur les clés de recherche ;
- clés techniques stables ;
- dates stockées de manière cohérente, en UTC lorsqu’elles représentent un instant ;
- montants stockés en type `decimal`, jamais en type flottant ;
- absence de procédures stockées ou de SQL natif spécifique à un moteur dans le cœur métier ;
- jeu de données initial idempotent ;
- tests de compatibilité au minimum sur PostgreSQL et SQLite ;
- possibilité d’activer SQL Server sans refonte des règles métier.

SQLite ne doit pas être utilisé comme base de production multi-utilisateur.

## 7.4 Cible de déploiement Windows Server

La cible principale est un serveur Windows respectant les normes d’installation suivantes :

- application : `E:\appl\SafranTimeTracker` ;
- certificats : `E:\certificats\SafranTimeTracker` ;
- packages et installateurs : `E:\CD_INSTALL\SafranTimeTracker` ;
- logs : `E:\data\logs\SafranTimeTracker` ;
- données applicatives : `E:\data\SafranTimeTracker`.

Arborescence applicative cible :

```text
E:\appl\SafranTimeTracker\
├── api\
├── web\
├── config\
├── scripts\
├── migrations\
├── backups\
├── docs\
└── VERSION
```

Arborescence des données :

```text
E:\data\SafranTimeTracker\
├── database\
├── database-backups\
├── imports\
├── exports\
├── temporary\
└── archive\
```

Arborescence des logs :

```text
E:\data\logs\SafranTimeTracker\
├── api\
├── web\
├── audit\
├── jobs\
├── installation\
└── archive\
```

Le dossier `E:\CD_INSTALL\SafranTimeTracker` conserve les prérequis, releases, scripts de déploiement, packages de rollback et notes de version.

## 7.5 Hébergement IIS

L’hébergement cible utilise IIS :

- un site ou une application IIS pour le frontend React compilé ;
- une application IIS pour l’API ASP.NET Core ;
- une publication sous un même nom DNS, avec l’API exposée sous `/api` ;
- HTTPS obligatoire hors environnement de développement ;
- certificats stockés selon les normes serveur et jamais dans Git.

Chemins physiques recommandés :

- frontend : `E:\appl\SafranTimeTracker\web` ;
- API : `E:\appl\SafranTimeTracker\api`.

L’API peut être hébergée derrière IIS via le module ASP.NET Core. L’architecture doit conserver la possibilité d’une exécution ultérieure comme service Windows ou derrière un reverse proxy Linux.

## 7.6 Configuration

Les paramètres sensibles doivent être externalisés.

Sont notamment configurables :

- provider de base de données ;
- chaîne de connexion ;
- URL publique de l’application ;
- URL relative ou publique du backend ;
- niveau et destination des logs ;
- origine CORS ;
- mode démonstration ;
- pays et calendriers ;
- heures par jour ;
- seuils d’alerte ;
- activation future du SSO.

Les configurations sont déclinées au minimum pour :

- Development ;
- Qualification ;
- Production.

Aucun secret, certificat, mot de passe ou chaîne de connexion réelle ne doit être commité dans le code source.

## 7.7 Déploiement et exploitation

Le déploiement doit être automatisé par PowerShell et rester reproductible.

Scripts minimum attendus :

- `build.ps1` ;
- `deploy.ps1` ;
- `rollback.ps1` ;
- `backup-database.ps1` ;
- `restore-database.ps1` ;
- `health-check.ps1` ;
- `install-iis.ps1` ou documentation équivalente ;
- `purge-logs.ps1`.

Le processus de déploiement doit :

1. vérifier les prérequis ;
2. sauvegarder la version installée et la base ;
3. arrêter proprement l’application ;
4. déployer l’API et le frontend ;
5. appliquer les migrations du provider actif ;
6. restaurer ou injecter la configuration externalisée ;
7. redémarrer l’application ;
8. exécuter un health check ;
9. produire un compte rendu dans `E:\data\logs\SafranTimeTracker\installation` ;
10. permettre un rollback documenté en cas d’échec.
# 8. Design et expérience utilisateur

## 8.1 Identité visuelle

Le design doit être :

- premium ;
- corporate ;
- sobre ;
- professionnel ;
- cohérent avec un environnement DSI ;
- inspiré des bonnes pratiques de ServiceNow, Azure DevOps, Jira, Power BI et Grafana ;
- lisible sur de grands écrans professionnels ;
- utilisable sur tablette et mobile.

Principes graphiques :

- fond gris clair ;
- sidebar bleu marine ;
- cartes blanches ;
- couleur d’action principale bleu Safran ;
- typographie Inter ou Segoe UI ;
- badges de statut cohérents ;
- tableaux denses mais lisibles ;
- graphiques utiles ;
- animations discrètes ;
- aucun effet gadget.

## 8.2 Navigation principale

La sidebar contient :

- Tableau de bord ;
- Temps ;
- Applications ;
- Projets ;
- Planning projet ;
- Ressources ;
- Sociétés ;
- Commandes ;
- Budgets ;
- Charges ;
- Disponibilités ;
- Mes absences ;
- Jalons ;
- Reporting ;
- Imports ;
- Administration.

Les entrées doivent être filtrées selon les droits.

## 8.3 Exigences UX générales

- navigation fluide ;
- fil d’Ariane sur les pages de détail ;
- filtres visibles et réinitialisables ;
- états vides explicites ;
- chargements matérialisés ;
- messages d’erreur compréhensibles ;
- confirmations pour les actions destructrices ;
- formulaires validés côté client et serveur ;
- recherche et pagination sur les tableaux ;
- tri des colonnes ;
- conservation optionnelle des filtres personnels ;
- dates au format français ;
- montants formatés en euros ;
- accessibilité clavier ;
- contraste suffisant ;
- responsive sans perte des fonctions essentielles.

---

# 9. Référentiels organisationnels

## 9.1 Département

Champs minimum :

- identifiant ;
- code ;
- nom ;
- responsable ;
- statut ;
- commentaire.

## 9.2 Service

Champs minimum :

- identifiant ;
- code ;
- nom ;
- département ;
- responsable ;
- statut ;
- commentaire.

## 9.3 Équipe

Champs minimum :

- identifiant ;
- code ;
- nom ;
- service ;
- responsable fonctionnel éventuel ;
- statut ;
- commentaire.

Le responsable fonctionnel d’une équipe n’est pas un rôle applicatif distinct. Il s’agit d’un rattachement organisationnel facultatif.

---

# 10. Gestion des utilisateurs et ressources

## 10.1 Distinction utilisateur / ressource

Un utilisateur représente un compte autorisé à se connecter.

Une ressource représente une personne planifiable et valorisable.

Dans la majorité des cas, un utilisateur actif est lié à une ressource. Le modèle doit néanmoins permettre :

- une ressource sans compte actif ;
- un compte administratif non planifié ;
- la désactivation du compte sans suppression de la ressource historique.

## 10.2 Fiche utilisateur

La fiche utilisateur comporte les sections suivantes.

### Informations générales

- nom ;
- prénom ;
- identifiant ;
- email ;
- téléphone facultatif ;
- statut Actif ou Inactif ;
- date d’arrivée ;
- date de sortie ;
- commentaire.

### Organisation

- département ;
- service ;
- équipe ;
- responsable hiérarchique ;
- société courante ;
- commande par défaut ;
- rôles opérationnels ;
- capacité hebdomadaire ;
- capacité journalière.

### Sécurité

- rôle applicatif ;
- permissions complémentaires ;
- accès global ou limité ;
- date de dernière modification de sécurité ;
- auteur de la dernière modification.

### Historique des TJM

Visible uniquement avec la permission financière.

La page ou l’onglet permet :

- consulter l’historique ;
- ajouter une période ;
- modifier une période non utilisée ou corriger selon une procédure auditée ;
- clôturer une période ;
- visualiser l’auteur et la date de création ;
- détecter les trous ou chevauchements.

## 10.3 Actions utilisateurs

Les utilisateurs autorisés peuvent :

- créer ;
- consulter ;
- modifier ;
- désactiver ;
- réactiver.

La suppression physique n’est autorisée que pour un compte de test sans aucune donnée liée.

## 10.4 Rôles opérationnels

Une ressource peut cumuler plusieurs rôles opérationnels :

- RUN ;
- Build ;
- Amélioration continue ;
- Chef de Projet ;
- Coordinateur IT.

Ces rôles sont indépendants du rôle applicatif.

## 10.5 Capacité

Une ressource possède :

- une capacité journalière par défaut ;
- une capacité hebdomadaire par défaut ;
- des variations de capacité ;
- des absences ;
- des jours fériés applicables ;
- des affectations planifiées.

La capacité doit pouvoir varier selon une période.

---

# 11. Historique des TJM des ressources

## 11.1 Principe

Le TJM appartient à la personne.

Il ne dépend pas :

- du projet ;
- de la commande ;
- du type interne ou externe ;
- du rôle opérationnel.

Une ressource peut changer de TJM dans le temps.

## 11.2 Objet ResourceTjmHistory

Champs minimum :

- id ;
- resourceId ;
- startDate ;
- endDate facultative ;
- dailyRate ;
- reason ;
- comment ;
- createdBy ;
- createdAt ;
- updatedBy ;
- updatedAt ;
- status.

## 11.3 Règles d’intégrité

- Deux périodes d’une même ressource ne peuvent pas se chevaucher.
- Une seule période peut être ouverte.
- Le montant doit être strictement positif.
- La date de fin doit être postérieure ou égale à la date de début.
- Une période utilisée par des saisies ne doit pas être supprimée.
- Une correction doit être auditée.
- La recherche du TJM s’effectue à la date de la saisie de temps.
- Une modification future ne déclenche aucun recalcul rétroactif.

## 11.4 Absence de TJM

Si aucun TJM valide n’existe à la date de la saisie :

- la saisie peut être enregistrée uniquement selon une règle de configuration explicite ;
- elle est marquée « valorisation incomplète » ;
- une alerte est remontée aux utilisateurs financiers ;
- aucun montant ne doit être inventé silencieusement.

---

# 12. Sociétés et contrats de prestation

## 12.1 Gestion des sociétés

La page Sociétés permet de créer et gérer des sociétés internes et externes.

Champs minimum :

- id ;
- nom ;
- code ;
- type Interne ou Externe ;
- statut ;
- contact principal ;
- email de contact ;
- téléphone ;
- adresse facultative ;
- commentaire ;
- ressources associées ;
- commandes associées.

La fonctionnalité **Ajouter une société externe** est obligatoire.

## 12.2 Rattachement ressource / société

Créer un objet `ResourceCompanyAssignment`.

Champs :

- id ;
- resourceId ;
- companyId ;
- startDate ;
- endDate facultative ;
- assignmentType ;
- comment ;
- createdBy ;
- createdAt.

Règles :

- une ressource ne peut pas être rattachée à plusieurs sociétés sur une même période ;
- l’historique doit être conservé ;
- la société applicable à une saisie est déterminée à la date de cette saisie ;
- une période utilisée ne peut pas être supprimée sans procédure de correction.

## 12.3 Historique des contrats

Les sociétés externes possèdent un historique de contrats de prestation.

Le contrat appartient à la société et ne dépend pas d’une ressource.

Créer un objet `CompanyContractHistory`.

Champs minimum :

- id ;
- companyId ;
- contractNumber facultatif ;
- startDate ;
- endDate facultative ;
- contractDailyRate ;
- currency ;
- comment ;
- createdBy ;
- createdAt ;
- updatedBy ;
- updatedAt ;
- status.

## 12.4 Règles des contrats

- Deux périodes actives d’une même société ne peuvent pas se chevaucher.
- Une seule période peut être ouverte.
- Le montant doit être strictement positif.
- Une période utilisée par des saisies ne peut pas être supprimée.
- Le contrat applicable est recherché à la date de la saisie.
- Une modification future ne modifie pas les saisies historiques.
- Les contrats sont confidentiels.

## 12.5 Sociétés internes

Pour une société interne :

- le coût réel peut être calculé à partir du TJM de la ressource ;
- aucun coût contractuel externe n’est obligatoire ;
- le coût contractuel et le différentiel peuvent être indiqués comme non applicables.

---

# 13. Commandes

## 13.1 Principe

Une commande représente une enveloppe contractuelle ou budgétaire consommable.

Le TJM n’est plus une propriété simple ou autoritaire de la commande. La valorisation contractuelle est déterminée par l’historique de contrat de la société applicable à la date de la saisie.

## 13.2 Champs

Une commande contient :

- id ;
- référence ;
- libellé ;
- société ;
- projet lié facultatif ;
- budget financier initial ;
- budget financier ajusté ;
- budget en jours initial ;
- budget en jours ajusté ;
- date de début ;
- date de fin initiale ;
- date de fin ajustée ;
- statut ;
- ressources autorisées ;
- consommation en jours ;
- coût réel consommé ;
- coût contractuel consommé ;
- différentiel ;
- reste financier ;
- reste en jours ;
- seuil d’alerte ;
- commentaire.

Statuts :

- Brouillon ;
- Active ;
- Suspendue ;
- Consommée ;
- Clôturée.

## 13.3 Rallonges

Créer un objet `OrderExtension`.

Champs :

- id ;
- orderId ;
- extensionDate ;
- amountAdded ;
- daysAdded ;
- previousEndDate ;
- newEndDate ;
- reason ;
- comment ;
- createdBy ;
- createdAt.

Toute rallonge :

- augmente le budget ajusté ;
- conserve le budget initial ;
- est visible dans l’historique ;
- déclenche une entrée d’audit ;
- met à jour les prévisions.

## 13.4 Règles

- Une commande peut être liée ou non à un projet.
- Une ressource peut utiliser une commande différente de la commande par défaut.
- Une saisie liée à une commande clôturée doit être bloquée, sauf droit de correction.
- La commande doit être compatible avec la société de la ressource à la date de la saisie, sauf dérogation auditée.
- La consommation historique repose sur les valeurs figées des saisies.

---

# 14. Budgets

## 14.1 Objet Budget

Un budget permet de suivre une enveloppe liée à un projet, une commande ou un périmètre de pilotage.

Champs minimum :

- id ;
- name ;
- projectId facultatif ;
- orderId facultatif ;
- initialAmount ;
- adjustedAmount ;
- consumedRealCost ;
- consumedContractCost ;
- remainingAmount ;
- forecastAmount ;
- status ;
- alertThreshold ;
- startDate ;
- endDate ;
- comment.

## 14.2 Versions et ajustements

Créer un objet `BudgetVersion` ou un historique équivalent.

Chaque ajustement conserve :

- ancienne valeur ;
- nouvelle valeur ;
- motif ;
- auteur ;
- date ;
- pièce de référence textuelle éventuelle, sans dépôt documentaire.

## 14.3 Indicateurs

La page Budgets affiche :

- budget total initial ;
- budget total ajusté ;
- coût réel consommé ;
- coût contractuel consommé ;
- budget restant ;
- consommation par projet ;
- consommation par commande ;
- consommation par société ;
- consommation par ressource ;
- consommation mensuelle ;
- différentiel global ;
- rallonges ;
- projets sous-financés ;
- commandes à risque ;
- atterrissage estimé ;
- besoin de rallonge.

---

# 15. Applications

## 15.1 Principe

Une application est un référentiel léger utilisé pour rattacher les temps, projets et indicateurs.

Elle ne contient aucune documentation.

## 15.2 Champs

- id ;
- nom ;
- code ;
- service ;
- équipe ;
- criticité ;
- responsable ;
- statut ;
- charge RUN ;
- charge hors RUN ;
- nombre d’incidents ;
- nombre de changes ;
- nombre de problems ;
- nombre de RITM ;
- projets associés ;
- commentaire.

Exemples :

- IBM ELM ;
- VTOM ;
- ServiceNow.

## 15.3 Actions

- créer ;
- modifier ;
- activer ;
- désactiver ;
- consulter le détail statistique ;
- archiver si plus utilisée.

La suppression est interdite si l’application est référencée.

---

# 16. Projets

## 16.1 Vue liste

La page Projets utilise une vue liste professionnelle, et non des vignettes.

Filtres minimum :

- statut ;
- application ;
- pilote ;
- département ;
- service ;
- équipe ;
- niveau de risque ;
- période ;
- présence d’une alerte budget ;
- présence d’une alerte planning.

## 16.2 Champs projet

- id ;
- nom ;
- code ;
- application principale ;
- description courte ;
- pilote ;
- département ;
- service ;
- équipe ;
- participants ;
- statut ;
- date de début ;
- date de fin prévue initiale ;
- date de fin ajustée ;
- date de fin réelle ;
- charge totale prévue initiale ;
- charge ajustée ;
- charge consommée ;
- charge restante ;
- budget initial ;
- budget ajusté ;
- coût réel consommé ;
- coût contractuel consommé ;
- différentiel ;
- budget restant ;
- commandes liées ;
- avancement ;
- niveau de risque ;
- commentaire.

Statuts :

- Actif ;
- Suspendu ;
- Terminé ;
- Archivé.

## 16.3 Actions

- créer ;
- modifier ;
- archiver ;
- réactiver ;
- ouvrir le détail ;
- comparer initial, ajusté et réalisé ;
- consulter les temps ;
- consulter les coûts selon permission ;
- consulter les commandes ;
- ajouter une rallonge ;
- gérer les jalons.

La suppression n’est autorisée que si aucune donnée liée n’existe.

---

# 17. Détail projet

## 17.1 Onglet Synthèse

Afficher :

- informations générales ;
- statut ;
- avancement ;
- pilote ;
- périmètre organisationnel ;
- budget initial ;
- budget ajusté ;
- coût réel consommé ;
- coût contractuel consommé ;
- différentiel ;
- budget restant ;
- charge initiale ;
- charge ajustée ;
- charge consommée ;
- charge restante ;
- dates initiale, ajustée et réelle ;
- risque planning ;
- risque budget ;
- alertes principales.

Les données financières sont conditionnées par la permission financière.

## 17.2 Onglet Participants

Afficher :

- ressource ;
- rôle opérationnel ;
- société applicable ;
- commande par défaut ;
- période d’affectation ;
- capacité prévue ;
- temps consommé ;
- reste à faire ;
- TJM personne applicable, si autorisé ;
- TJM contrat applicable, si autorisé ;
- coût réel ;
- coût contractuel ;
- différentiel.

## 17.3 Onglet Planning

Afficher une grille hebdomadaire par ressource :

- planning initial ;
- planning ajusté ;
- réalisé ;
- reste à faire ;
- capacité disponible ;
- écart prévu/réalisé ;
- écart initial/ajusté ;
- surcharge ;
- sous-charge.

Les charges peuvent varier chaque semaine.

## 17.4 Onglet Budget

Afficher :

- commandes liées ;
- budget initial ;
- ajustements ;
- rallonges ;
- coût réel ;
- coût contractuel ;
- différentiel ;
- reste à consommer ;
- prévision d’atterrissage ;
- alertes.

## 17.5 Onglet Temps

Afficher :

- temps détaillés ;
- temps par personne ;
- temps par activité ;
- temps par semaine ;
- références liées ;
- historique de modification.

## 17.6 Onglet Jalons

Afficher :

- timeline ;
- jalons en retard ;
- jalons à venir ;
- jalons terminés ;
- impact sur le planning.

## 17.7 Onglet Références liées

Références autorisées :

- INC ;
- CHG ;
- PRB ;
- RITM ;
- VABE ;
- VSR ;
- projet interne.

Aucun document ne doit être déposé dans cet onglet.

---

# 18. Planning projet

## 18.1 Objectif

La page Planning projet compare :

- initial ;
- ajusté ;
- réalisé ;
- reste à faire ;
- capacité réelle.

## 18.2 Vue

- tableau semaine par semaine ;
- filtre projet ;
- filtre ressource ;
- filtre service ;
- filtre équipe ;
- filtre période ;
- charges prévues ;
- charges réalisées ;
- écarts ;
- capacité disponible ;
- alertes surcharge ;
- alertes sous-charge.

## 18.3 Versions de planning

Créer :

- `ProjectPlanVersion` ;
- `ProjectWeeklyPlan`.

Une version contient :

- type Initial ou Ajusté ;
- date de création ;
- auteur ;
- motif ;
- statut ;
- lignes hebdomadaires.

Le réalisé provient des saisies de temps et ne doit pas être saisi manuellement comme une troisième version.

---

# 19. Saisie et suivi des temps

## 19.1 Champs

Une saisie de temps contient :

- id ;
- date ;
- semaine ;
- utilisateur ;
- ressource ;
- application ;
- projet facultatif ;
- commande facultative ;
- type d’activité ;
- référence facultative ;
- durée en heures ;
- commentaire ;
- statut ;
- date de création ;
- auteur de création ;
- date de modification ;
- auteur de modification.

## 19.2 Types d’activité

- RUN ;
- Incident ;
- Change ;
- Problem ;
- RITM ;
- Projet ;
- Amélioration continue ;
- Support ;
- Astreinte ;
- Réunion ;
- Formation ;
- VABE ;
- VSR.

## 19.3 Références

Formats possibles :

- INCxxxxxxx ;
- CHGxxxxxxx ;
- PRBxxxxxxx ;
- RITMxxxxxxx ;
- VABE-xxxx ;
- VSR-xxxx ;
- référence de projet interne.

La validation du format doit dépendre du type d’activité.

## 19.4 Fonctionnalités

- ajouter ;
- modifier ;
- supprimer selon droits ;
- dupliquer une saisie ;
- filtrer par période ;
- filtrer par semaine ;
- filtrer par type ;
- filtrer par application ;
- filtrer par projet ;
- filtrer par commande ;
- totaliser automatiquement ;
- afficher l’historique ;
- recalculer les agrégats après modification ;
- empêcher la saisie sur une ressource inactive ;
- empêcher la saisie sur une période clôturée.

## 19.5 Valorisation financière figée

Lors de la création ou d’une modification autorisée de la saisie, le backend détermine :

1. le TJM personne applicable à la date ;
2. la société applicable à la date ;
3. le contrat société applicable à la date ;
4. le coût réel ;
5. le coût contractuel ;
6. le différentiel.

La saisie conserve au minimum :

- `tjmPersonneSnapshot` ;
- `sourceTjmPersonne` ;
- `resourceTjmHistoryId` ;
- `tjmContratSnapshot` ;
- `sourceContrat` ;
- `companyContractHistoryId` ;
- `companyIdSnapshot` ;
- `coutReelCalcule` ;
- `coutContratCalcule` ;
- `differentielCalcule` ;
- `calculationDate` ;
- `calculationStatus`.

Ces valeurs sont figées.

## 19.6 Recalcul d’une saisie

Un recalcul est uniquement possible :

- sur action explicite ;
- avec une permission dédiée ;
- avec confirmation ;
- avec motif obligatoire ;
- avec conservation des anciennes valeurs dans l’audit.

Aucun recalcul en masse implicite n’est autorisé.

---

# 20. Modèle financier et règles de calcul

## 20.1 Temps en jours

`tempsJours = heuresSaisies / heuresParJour`

Valeur par défaut :

`heuresParJour = 7,75`

Le paramètre peut être modifié pour les calculs futurs, sans altérer les instantanés historiques.

## 20.2 Coût réel

`coutReel = tempsJours × tjmPersonneSnapshot`

## 20.3 Coût contractuel

Pour une ressource rattachée à une société externe avec un contrat valide :

`coutContrat = tempsJours × tjmContratSnapshot`

Pour une société interne sans contrat :

- coût contractuel : non applicable ;
- différentiel : non applicable.

## 20.4 Différentiel

`differentiel = coutContrat - coutReel`

Le différentiel peut être :

- positif ;
- nul ;
- négatif.

Il ne doit pas être présenté automatiquement comme une marge comptable sans validation du métier. Le libellé fonctionnel reste « différentiel ».

## 20.5 Sources

Les rapports financiers doivent pouvoir afficher :

- source du TJM personne ;
- période TJM utilisée ;
- source du contrat ;
- période contrat utilisée ;
- date de calcul.

## 20.6 Agrégations

Les agrégats financiers utilisent uniquement les valeurs historisées dans les saisies :

- somme des coûts réels ;
- somme des coûts contractuels ;
- somme des différentiels.

Ils ne doivent pas rechercher les taux actuels pour recalculer le passé.

---

# 21. Charges

## 21.1 Filtres

- période ;
- application ;
- projet ;
- commande ;
- département ;
- service ;
- équipe ;
- utilisateur ;
- type d’activité ;
- rôle opérationnel.

## 21.2 Indicateurs

- charge totale ;
- charge RUN ;
- charge hors RUN ;
- incidents ;
- changes ;
- problems ;
- RITM ;
- projets ;
- VABE/VSR ;
- top applications ;
- top utilisateurs ;
- top projets ;
- top commandes ;
- ressources surchargées ;
- ressources sous-chargées ;
- capacité vs consommé ;
- prévu vs réalisé.

## 21.3 Graphiques

- barres par ingénieur ;
- barres par application ;
- répartition RUN/hors RUN ;
- courbe mensuelle ;
- capacité vs réalisé ;
- prévu vs réalisé ;
- heatmap de charge.

---

# 22. Disponibilités

## 22.1 Accès

Accessible :

- à l’Administrateur pour l’ensemble du périmètre ;
- au Responsable Département pour son département ;
- au Responsable Service pour son service ;
- à l’Ingénieur pour sa propre visibilité selon configuration.

## 22.2 Vues

- mensuelle ;
- hebdomadaire ;
- filtre utilisateur ;
- filtre service ;
- filtre équipe ;
- filtre période ;
- calendrier coloré ;
- résumé capacitaire ;
- jours fériés ;
- absences ;
- disponibilités restantes ;
- surcharges.

## 22.3 Types de disponibilité

- Disponible ;
- Congé ;
- RTT ;
- Maladie ;
- Formation ;
- Télétravail ;
- Déplacement ;
- Indisponible.

Le télétravail n’est pas une absence et ne réduit pas la capacité par défaut.

## 22.4 Calculs

- jours ouvrés ;
- jours fériés ;
- jours d’absence ;
- capacité théorique ;
- capacité réelle ;
- taux de disponibilité ;
- capacité planifiée ;
- capacité restante.

---

# 23. Mes absences

## 23.1 Champs

- id ;
- resourceId ;
- type ;
- date de début ;
- date de fin ;
- demi-journée ;
- commentaire ;
- statut ;
- créé par ;
- date de création ;
- validé ou refusé par ;
- date de décision.

Statuts :

- Brouillon ;
- Soumis ;
- Validé ;
- Refusé ;
- Annulé.

## 23.2 Fonctionnalités

- créer ;
- modifier tant que permis ;
- supprimer un brouillon ;
- soumettre ;
- annuler ;
- consulter le total mensuel ;
- consulter le total annuel ;
- visualiser l’impact sur la capacité.

## 23.3 Validation

Le workflow de validation peut être activé ou désactivé par paramètre.

Lorsqu’il est activé :

- le responsable hiérarchique traite la demande ;
- la décision est auditée ;
- seule une absence validée réduit la capacité réelle, sauf paramètre contraire.

---

# 24. Jalons

## 24.1 Types

- Kick-off ;
- Architecture ;
- VABE ;
- VSR ;
- GO DEV ;
- GO QUAL ;
- GO VAL ;
- GO PPROD ;
- GO PROD ;
- CAB ;
- Hypercare.

Les types sont administrables.

## 24.2 Champs

- id ;
- nom ;
- type ;
- projet ;
- application ;
- responsable ;
- date prévue ;
- date réelle ;
- statut ;
- commentaire ;
- criticité ;
- dépendance éventuelle.

Statuts :

- À venir ;
- En cours ;
- Terminé ;
- En retard ;
- Annulé.

## 24.3 Vues

- timeline ;
- calendrier ;
- tableau ;
- filtre projet ;
- filtre application ;
- filtre responsable ;
- filtre statut ;
- compteur à 30 jours ;
- mise en évidence des retards ;
- impact visuel sur la dérive planning.

---

# 25. Tableau de bord

## 25.1 KPI opérationnels

- temps saisis sur la période ;
- capacité théorique ;
- capacité réelle ;
- taux de disponibilité ;
- charge RUN ;
- charge hors RUN ;
- incidents ouverts ;
- changes en cours ;
- problems ouverts ;
- RITM en cours ;
- projets actifs ;
- jalons en retard ;
- ressources surchargées ;
- ressources sous-chargées.

## 25.2 KPI financiers

Visibles uniquement avec permission :

- budget initial total ;
- budget ajusté total ;
- coût réel total ;
- coût contractuel total ;
- différentiel global ;
- budget restant ;
- commandes à risque ;
- projets sous-financés ;
- atterrissage estimé.

## 25.3 Graphiques

- RUN vs hors RUN ;
- charge par application ;
- charge par service ;
- charge par équipe ;
- charge par ingénieur ;
- évolution mensuelle ;
- jalons à venir ;
- budget par projet ;
- budget par commande ;
- capacité vs réalisé ;
- prévu vs réalisé ;
- différentiel par société ;
- différentiel par projet.

Les widgets doivent s’adapter au périmètre de l’utilisateur.

---

# 26. Reporting

## 26.1 Rapports opérationnels

- RUN / hors RUN ;
- charge équipe ;
- charge service ;
- charge département ;
- consommation projet ;
- consommation commande ;
- jalons en retard ;
- ressources surchargées ;
- ressources sous-utilisées ;
- capacité et disponibilité.

## 26.2 Rapports financiers

Visibles avec permission :

- coût réel total ;
- coût contractuel total ;
- différentiel global ;
- différentiel par projet ;
- différentiel par commande ;
- différentiel par société ;
- différentiel par ressource ;
- budget restant ;
- atterrissage ;
- besoins de rallonge ;
- commandes à renouveler ;
- sources des montants.

## 26.3 Exports

- CSV réel ;
- Excel réel ;
- PDF réel ou généré côté serveur ;
- respect des filtres ;
- respect du périmètre ;
- respect des permissions ;
- date et auteur de l’export ;
- journalisation des exports financiers.

Les exports ne doivent pas être de simples boutons simulés dans la version multi-utilisateur cible.

---

# 27. Imports

## 27.1 Types importables

- ressources ;
- utilisateurs ;
- sociétés ;
- rattachements ressource/société ;
- historiques TJM ;
- historiques de contrats ;
- projets ;
- budgets ;
- commandes ;
- affectations projet ;
- plannings ;
- temps ;
- absences ;
- jalons ;
- applications ;
- organisation.

## 27.2 Modes

- Ajout ;
- Mise à jour ;
- Complet, avec confirmation renforcée.

Le mode Complet ne doit pas supprimer directement les données liées. Il doit désactiver ou archiver selon les règles métier.

## 27.3 Assistant d’import

Étapes :

1. choix du type ;
2. choix du mode ;
3. chargement du fichier ;
4. détection de l’encodage et du séparateur ;
5. mapping des colonnes ;
6. prévisualisation ;
7. validation ;
8. affichage des erreurs ;
9. simulation ;
10. confirmation ;
11. exécution ;
12. compte rendu.

## 27.4 Import SharePoint simulé

Le MVP permet de charger un export CSV ou JSON représentant une vue SharePoint.

Fonctions :

- comparer au chargement précédent ;
- afficher les ajouts ;
- afficher les modifications ;
- afficher les suppressions ;
- afficher les champs modifiés ;
- historiser le lot ;
- ne pas modifier les données avant confirmation.

## 27.5 ImportBatch

Champs :

- id ;
- type ;
- source ;
- importDate ;
- userId ;
- mode ;
- fileName ;
- lineCount ;
- addCount ;
- updateCount ;
- deleteCount ;
- errorCount ;
- status ;
- errors ;
- checksum.

## 27.6 ImportDiff

Champs :

- id ;
- importBatchId ;
- entityType ;
- entityId ;
- diffType ;
- fieldName ;
- oldValue ;
- newValue.

---

# 28. Administration

## 28.1 Onglets

- Utilisateurs ;
- Département ;
- Services ;
- Équipes ;
- Applications ;
- Types d’activités ;
- Types d’absences ;
- Types de jalons ;
- Sociétés ;
- Commandes ;
- Paramètres ;
- Permissions ;
- Audit.

## 28.2 Paramètres

- heures par jour ;
- jours ouvrés par semaine ;
- pays par défaut ;
- jours fériés France ;
- jours fériés Inde ;
- types d’activités ;
- types d’absences ;
- seuil surcharge ;
- seuil sous-charge ;
- seuil alerte budget ;
- seuil alerte commande ;
- délai de modification des temps ;
- activation de la validation des absences ;
- autorisation d’une saisie sans valorisation ;
- devise par défaut.

## 28.3 Audit

Le journal doit couvrir au minimum :

- création, modification et suppression logique d’une saisie ;
- création et modification d’un projet ;
- archivage d’un projet ;
- création et modification d’un jalon ;
- création, modification et désactivation d’un utilisateur ;
- changement de rôle ;
- changement de permission ;
- promotion ou retrait Administrateur ;
- modification de paramètres ;
- création et modification d’une commande ;
- rallonge ;
- création et modification d’une société ;
- création et modification d’un TJM ;
- création et modification d’un contrat ;
- recalcul financier explicite ;
- import ;
- comparaison d’import ;
- export financier.

Chaque entrée comprend :

- auteur ;
- date et heure ;
- action ;
- type d’objet ;
- identifiant d’objet ;
- ancienne valeur ;
- nouvelle valeur ;
- motif éventuel ;
- adresse ou contexte technique si disponible.

Les journaux ne doivent pas être modifiables depuis l’interface standard.

---

# 29. Règles de calcul capacitaire et projet

## 29.1 Capacité théorique

`capacitéThéorique = joursOuvrés × heuresParJour × ressourcesActives`

Pour une ressource :

`capacitéThéoriqueRessource = joursOuvrés × capacitéJournalière`

## 29.2 Capacité réelle

`capacitéRéelle = capacitéThéorique - absencesValidées - joursFériés - indisponibilités`

## 29.3 Taux de disponibilité

`tauxDisponibilité = capacitéRéelle / capacitéThéorique × 100`

## 29.4 Classification RUN

Charge RUN :

- RUN ;
- Incident ;
- Change ;
- Problem ;
- RITM ;
- Astreinte ;
- Support.

Charge hors RUN :

- Projet ;
- Amélioration continue ;
- Réunion ;
- Formation ;
- VABE ;
- VSR.

Cette classification doit être administrable à terme.

## 29.5 Projet

- `chargeRestante = chargeAjustée - chargeConsommée`
- `écartCharge = réalisé - prévu`
- `dériveCharge = chargeAjustée - chargeInitiale`
- `atterrissageCharge = chargeConsommée + resteÀFaire`
- `dérivePlanning = dateFinAjustée - dateFinInitiale`
- risque planning si la date ajustée dépasse la date initiale ;
- risque budget si l’atterrissage financier dépasse le budget ajusté.

## 29.6 Surcharge et sous-charge

- surcharge si charge planifiée > capacité réelle ;
- sous-charge si charge planifiée < seuil minimum configuré ;
- les seuils sont configurables ;
- les alertes doivent indiquer la période concernée.

---

# 30. Modèle de données minimum

Les types ou entités suivants doivent exister :

- User ;
- Role ;
- Permission ;
- UserPermission ;
- OperationalRole ;
- Department ;
- Service ;
- Team ;
- Resource ;
- ResourceCapacityPeriod ;
- ResourceTjmHistory ;
- Company ;
- CompanyType ;
- ResourceCompanyAssignment ;
- CompanyContractHistory ;
- Application ;
- Project ;
- ProjectStatus ;
- ProjectParticipant ;
- ProjectPlanVersion ;
- ProjectWeeklyPlan ;
- TimeEntry ;
- TimeEntryFinancialSnapshot ;
- Absence ;
- Milestone ;
- ActivityType ;
- Order ;
- OrderStatus ;
- OrderExtension ;
- Budget ;
- BudgetVersion ;
- ImportBatch ;
- ImportDiff ;
- AuditLog ;
- Settings ;
- HolidayCalendar ;
- DashboardKpi.

## 30.1 Contraintes générales

- identifiants stables ;
- dates de création et modification ;
- auteur de création et modification ;
- statut plutôt que suppression physique ;
- contraintes anti-chevauchement pour les historiques ;
- clés étrangères ;
- montants décimaux ;
- version de concurrence pour les données sensibles ;
- index sur dates, ressources, projets, commandes et sociétés.

---

# 31. Services applicatifs

Le code doit séparer au minimum :

- AuthService ;
- AuthorizationService ;
- UserService ;
- ResourceService ;
- OrganizationService ;
- CompanyService ;
- ContractService ;
- ResourceTjmService ;
- OrderService ;
- BudgetService ;
- ApplicationService ;
- ProjectService ;
- ProjectPlanningService ;
- TimeEntryService ;
- FinancialCalculationService ;
- AbsenceService ;
- AvailabilityService ;
- MilestoneService ;
- ReportingService ;
- ExportService ;
- ImportService ;
- AuditService ;
- SettingsService.

Les calculs financiers doivent être centralisés dans un service métier unique et testable.

---

# 32. Utilitaires et composants

## 32.1 Utilitaires

- capacityCalculator ;
- dateUtils ;
- holidayCalendar ;
- workloadCalculator ;
- budgetCalculator ;
- tjmCalculator ;
- contractCalculator ;
- financialSnapshotCalculator ;
- projectVarianceCalculator ;
- importDiffCalculator ;
- statusUtils ;
- csvParser ;
- validationUtils ;
- currencyUtils ;
- permissionUtils.

## 32.2 Composants UI

- AppLayout ;
- Sidebar ;
- Header ;
- Breadcrumb ;
- KpiCard ;
- DataTable ;
- StatusBadge ;
- ProgressBar ;
- Modal ;
- Drawer ;
- FormField ;
- Select ;
- DatePicker ;
- ChartCard ;
- Timeline ;
- CalendarView ;
- EmptyState ;
- ConfirmDialog ;
- FilterBar ;
- ImportWizard ;
- DiffViewer ;
- BudgetGauge ;
- WorkloadHeatmap ;
- WeeklyPlanningGrid ;
- PermissionGuard ;
- FinancialValue ;
- AuditTimeline.

---

# 33. Arborescence logicielle indicative

```text
SAFRAN-TIME-TRACKER/
├── docs/
│   ├── Cahier_des_charges.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE.md
│   ├── DEPLOYMENT_WINDOWS.md
│   ├── ROADMAP.md
│   └── IMPLEMENTATION_STATUS.md
├── backend/
│   ├── SafranTimeTracker.Domain/
│   ├── SafranTimeTracker.Application/
│   ├── SafranTimeTracker.Infrastructure/
│   ├── SafranTimeTracker.Api/
│   └── SafranTimeTracker.Tests/
├── frontend/
│   └── safran-time-tracker-web/
├── database/
│   ├── postgresql/
│   │   ├── migrations/
│   │   └── scripts/
│   ├── sqlserver/
│   │   ├── migrations/
│   │   └── scripts/
│   ├── sqlite/
│   └── seed/
├── deploy/
│   ├── windows/
│   │   ├── iis/
│   │   ├── powershell/
│   │   └── templates/
│   └── linux/
│       └── README.md
├── scripts/
├── CLAUDE.md
├── README.md
├── Directory.Build.props
└── .gitignore
```

Le dossier `deploy/linux` conserve uniquement les éléments nécessaires à une portabilité future. La cible officielle du MVP reste Windows Server.
# 34. API attendue

L’API doit exposer des endpoints cohérents et versionnés.

Familles minimum :

- `/api/v1/auth`
- `/api/v1/users`
- `/api/v1/resources`
- `/api/v1/departments`
- `/api/v1/services`
- `/api/v1/teams`
- `/api/v1/companies`
- `/api/v1/company-contracts`
- `/api/v1/resource-tjm-history`
- `/api/v1/orders`
- `/api/v1/budgets`
- `/api/v1/applications`
- `/api/v1/projects`
- `/api/v1/project-plans`
- `/api/v1/time-entries`
- `/api/v1/absences`
- `/api/v1/availability`
- `/api/v1/milestones`
- `/api/v1/reporting`
- `/api/v1/imports`
- `/api/v1/audit`
- `/api/v1/settings`

Exigences :

- pagination ;
- filtres ;
- tri ;
- validation ;
- codes HTTP cohérents ;
- erreurs fonctionnelles explicites ;
- contrôle d’accès serveur ;
- protection des données financières ;
- documentation OpenAPI ;
- tests des endpoints sensibles.

---

# 35. Données de démonstration

Le jeu initial doit contenir au minimum :

- 10 applications ;
- 8 projets ;
- 12 ressources ;
- 5 sociétés, dont plusieurs externes ;
- 8 commandes ;
- 5 budgets projet ;
- 80 saisies de temps ;
- 20 références INC/CHG/PRB/RITM ;
- 15 jalons ;
- 10 absences ;
- plusieurs départements, services et équipes ;
- plusieurs rattachements ressource/société ;
- plusieurs historiques TJM ;
- plusieurs historiques de contrats ;
- plusieurs rallonges ;
- plusieurs écarts initial/ajusté/réalisé ;
- des cas avec différentiel positif, nul et négatif ;
- des cas de valorisation incomplète contrôlée ;
- des utilisateurs avec et sans permission financière.

Les données doivent être cohérentes entre elles et permettre de démontrer tous les écrans.

---

# 36. Exigences non fonctionnelles

## 36.1 Performance

- affichage initial fluide ;
- pagination serveur pour les listes importantes ;
- agrégations optimisées ;
- absence de recalcul complet inutile ;
- temps de réponse acceptable pour une utilisation interne.

## 36.2 Fiabilité

- transactions pour les opérations financières ;
- contrôle de concurrence ;
- reprise propre en cas d’erreur ;
- messages d’erreur non techniques pour l’utilisateur ;
- logs détaillés côté serveur.

## 36.3 Sécurité

- contrôle d’accès côté serveur ;
- données financières jamais exposées sans permission ;
- validation de toutes les entrées ;
- protection contre les injections ;
- configuration des origines autorisées ;
- absence de secrets dans le dépôt ;
- journalisation des actions sensibles ;
- principe du moindre privilège.

## 36.4 Sauvegarde et restauration

La documentation d’exploitation doit inclure :

- sauvegarde automatisée de la base selon le provider actif ;
- sauvegarde de la configuration externalisée ;
- sauvegarde de la version applicative avant déploiement ;
- script PowerShell de restauration ;
- procédure de test de restauration ;
- politique de rétention configurable ;
- stockage des sauvegardes sous `E:\data\SafranTimeTracker\database-backups` ;
- journalisation des opérations sous `E:\data\logs\SafranTimeTracker\jobs` ;
- procédure obligatoire avant toute mise à jour de production.
## 36.5 Maintenabilité

- code typé ;
- architecture en couches ;
- absence de duplication des règles métier ;
- tests unitaires ;
- tests d’intégration ;
- conventions de nommage ;
- documentation technique ;
- migrations versionnées.

## 36.6 Accessibilité et compatibilité

- navigateurs d’entreprise récents ;
- utilisation clavier ;
- contrastes suffisants ;
- libellés explicites ;
- tableaux utilisables avec zoom ;
- responsive desktop, tablette et mobile.

---

# 37. Critères de recette fonctionnelle

Le MVP est accepté lorsque les critères suivants sont satisfaits.

## 37.1 Navigation et sécurité

- chaque rôle voit uniquement les pages autorisées ;
- les utilisateurs sans permission financière ne reçoivent aucun montant confidentiel ;
- un Administrateur peut promouvoir un autre Administrateur ;
- une promotion ou rétrogradation est auditée ;
- un utilisateur inactif ne peut plus se connecter.

## 37.2 Utilisateurs

- création, modification, désactivation et réactivation fonctionnent ;
- un utilisateur lié à des données ne peut pas être supprimé physiquement ;
- les anciennes et nouvelles valeurs sont visibles dans l’audit.

## 37.3 TJM

- ajout d’une période possible ;
- chevauchement bloqué ;
- recherche correcte du TJM selon la date ;
- modification future sans impact rétroactif ;
- données masquées sans permission.

## 37.4 Contrats

- ajout d’une société externe possible ;
- ajout d’un contrat possible ;
- chevauchement bloqué ;
- recherche correcte du contrat selon la date ;
- contrat masqué sans permission.

## 37.5 Temps et valorisation

- création d’une saisie met à jour les KPI ;
- le coût réel utilise le TJM personne de la date ;
- le coût contractuel utilise le contrat société de la date ;
- le différentiel est correct ;
- les valeurs sont figées ;
- un changement de taux n’altère pas l’historique.

## 37.6 Projets et planning

- les versions initiale et ajustée sont distinctes ;
- le réalisé provient des temps ;
- les écarts sont calculés ;
- les surcharges sont visibles ;
- les jalons en retard sont identifiés.

## 37.7 Budgets et commandes

- les consommations reposent sur les snapshots ;
- une rallonge conserve l’historique ;
- les alertes se mettent à jour ;
- l’atterrissage est calculé ;
- les données sont protégées par permission.

## 37.8 Imports

- prévisualisation fonctionnelle ;
- lignes en erreur identifiées ;
- simulation sans écriture ;
- confirmation avant écriture ;
- historique du lot ;
- comparaison de deux imports SharePoint simulés.

## 37.9 Reporting

- filtres fonctionnels ;
- exports cohérents avec l’écran ;
- respect du périmètre ;
- respect des permissions ;
- totaux identiques aux données sources.

---

# 38. Tests attendus

## 38.1 Tests unitaires

Priorité sur :

- recherche du TJM par date ;
- détection des chevauchements ;
- recherche du contrat par date ;
- calcul du coût réel ;
- calcul du coût contractuel ;
- calcul du différentiel ;
- calcul capacitaire ;
- classification RUN/hors RUN ;
- calcul des écarts projet ;
- calcul des alertes budget ;
- comparaison d’import.

## 38.2 Tests d’intégration

- création d’une saisie avec snapshot ;
- modification d’un taux sans recalcul historique ;
- gestion d’une rallonge ;
- désactivation d’un utilisateur ;
- contrôle d’accès financier ;
- import transactionnel ;
- audit des modifications.

## 38.3 Tests end-to-end

Scénarios minimum :

1. un Ingénieur saisit son temps ;
2. un Responsable Service consulte la charge de son service ;
3. un Responsable Département consulte les KPI consolidés ;
4. un Administrateur attribue une permission financière ;
5. un utilisateur autorisé ajoute un TJM ;
6. une société externe reçoit un nouveau contrat ;
7. une saisie est valorisée avec les bons taux historiques ;
8. un projet dérive et déclenche une alerte ;
9. une commande reçoit une rallonge ;
10. un import est comparé à un lot précédent.

---

# 39. Livrables

Les livrables attendus sont :

- code source frontend ;
- code source backend ;
- scripts SQL et migrations ;
- données de démonstration ;
- fichiers de configuration exemples ;
- scripts Windows de lancement et build ;
- documentation d’installation ;
- documentation d’exploitation ;
- documentation fonctionnelle ;
- documentation API ;
- plan de tests ;
- rapport de tests ;
- guide utilisateur ;
- guide administrateur ;
- procédure de sauvegarde et restauration ;
- scripts PowerShell de build, déploiement, rollback et health check ;
- configuration IIS documentée ;
- modèles de configuration Development, Qualification et Production ;
- scripts de migrations pour PostgreSQL et, si activé, SQL Server ;
- procédure d’installation respectant les chemins `E:\appl`, `E:\certificats`, `E:\CD_INSTALL` et `E:\data`.

---

# 40. Découpage recommandé

## Lot 0 - Fondations

- dépôt et conventions de code ;
- structure frontend/backend ;
- configuration multi-environnements ;
- sélection de provider EF Core ;
- PostgreSQL par défaut ;
- SQLite pour les tests ;
- préparation du provider SQL Server ;
- migrations initiales ;
- design system ;
- authentification de démonstration ;
- rôles et permissions ;
- scripts PowerShell de build et déploiement ;
- structure IIS et chemins normalisés Windows Server ;
- compilation et tests réels avant passage au lot suivant.
## Lot 1 - Référentiels

- organisation ;
- utilisateurs ;
- ressources ;
- applications ;
- sociétés ;
- commandes ;
- paramètres.

## Lot 2 - Modèle financier

- historiques TJM ;
- historiques contrats ;
- rattachements société ;
- snapshots financiers ;
- permissions financières ;
- tests métier.

## Lot 3 - Temps et capacité

- saisie des temps ;
- absences ;
- disponibilités ;
- calculs capacité ;
- charges RUN/hors RUN.

## Lot 4 - Projets

- projets ;
- participants ;
- planning ;
- jalons ;
- références opérationnelles ;
- écarts.

## Lot 5 - Budgets et reporting

- budgets ;
- rallonges ;
- tableaux de bord ;
- reporting ;
- exports.

## Lot 6 - Imports et audit

- imports CSV ;
- import SharePoint simulé ;
- comparaison ;
- audit complet.

## Lot 7 - Industrialisation

- tests ;
- optimisation ;
- documentation ;
- sécurité ;
- sauvegarde ;
- packaging portable.

Chaque lot doit produire une version compilable, démontrable et testable.

---

# 41. Définitions

**TJM personne** : taux journalier propre à la ressource, historisé dans le temps.

**TJM contrat** : taux journalier du contrat de prestation d’une société externe, historisé dans le temps.

**Coût réel** : valorisation du temps à partir du TJM personne.

**Coût contractuel** : valorisation du temps à partir du TJM du contrat société.

**Différentiel** : coût contractuel moins coût réel.

**Snapshot financier** : copie figée des taux, sources et montants utilisés lors de la valorisation d’une saisie.

**RUN** : activités récurrentes de maintien en conditions opérationnelles.

**Hors RUN** : projets, amélioration continue, formation, réunions et jalons spécifiques selon la classification retenue.

**Atterrissage** : estimation finale calculée à partir du consommé et du reste à faire.

**Rallonge** : augmentation historisée d’un budget, d’un volume de jours ou d’une date de fin.

---

# 42. Règle finale de cohérence

Aucune fonctionnalité de SAFRAN TIME TRACKER ne doit réintroduire :

- une dépendance du TJM au projet ;
- une dépendance autoritaire du TJM à la commande ;
- un recalcul rétroactif silencieux ;
- la terminologie « Squad Leader » ;
- une gestion documentaire ;
- une fonctionnalité appartenant à DS-EYE ;
- une protection financière uniquement visuelle ;
- une suppression physique de données historiques liées ;
- une dépendance métier à un moteur de base unique ;
- des chemins d’installation contraires aux normes Windows Server définies ;
- des secrets ou certificats intégrés au dépôt Git.

Le produit final doit constituer un cockpit intégré de pilotage des temps, charges, capacités, projets, jalons, ressources, commandes, budgets et valorisations financières.
