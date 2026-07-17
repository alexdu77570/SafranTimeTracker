# Backlog métier — SAFRAN TIME TRACKER

> **Rôle de ce document** : référence **fonctionnelle** du projet — décisions métier, workflows et règles validées avec le Product Owner, le Squad Leader ou les experts métier. Il ne décrit **jamais** l'architecture technique (classes, entités, endpoints) : cette information vit dans `docs/ARCHITECTURE.md`, `docs/DATABASE.md` et `docs/IMPLEMENTATION_STATUS.md`. En cas de doute sur une règle métier déjà couverte par le cahier des charges, le cahier des charges (`docs/Cahier_des_charges_SAFRAN_TIME_TRACKER_v2.1_Windows_Server.md`) reste la référence fonctionnelle unique ; ce document capture les règles **complémentaires ou plus précises**, validées au fil des lots, qui ne figurent pas (ou pas assez précisément) dans le cahier des charges.
>
> **Règle d'alimentation (CLAUDE.md §5)** : toute nouvelle règle métier validée avec le Product Owner, le Squad Leader ou les experts métier est ajoutée ici **avant** son implémentation. Ce document est relu au même titre que `docs/ROADMAP.md`, `CLAUDE.md` et `docs/IMPLEMENTATION_STATUS.md` avant de démarrer un nouveau lot.
>
> Ce document mélange volontairement deux types de contenu, distingués par un statut explicite :
> - des règles **déjà implémentées**, documentées ici pour qu'elles restent traçables comme décisions métier et pas seulement comme code ;
> - des règles **validées mais pas encore construites** (backlog au sens propre), qui attendent un lot pour être développées.
>
> Statut employé pour chaque règle :
> - ✅ **Implémenté** — déjà appliqué dans le code (détail technique : `docs/IMPLEMENTATION_STATUS.md`).
> - 🕓 **Validé, non implémenté** — règle actée avec le Squad Leader/PO, en attente d'un lot.
> - 🔎 **À clarifier** — écart ou ambiguïté identifié entre la formulation métier et le modèle actuel ; nécessite une validation complémentaire avant implémentation.

---

## 1. Commandes

### Workflow

```
Demande d'achat → Commande → Réceptions partielles → Clôture
```

✅ **Implémenté.** Ce vocabulaire métier ne correspond pas à un nouveau jeu de statuts : il se lit sur la machine d'état déjà en place (Brouillon ≈ Demande d'achat, Active ≈ Commande, Clôturée ≈ Clôture), complétée par des événements répétables représentant les réceptions partielles. Ce vocabulaire ne doit **jamais** être réintroduit comme un nouveau statut ou une nouvelle machine d'état parallèle.

### Règles validées

- 🔎 **Une commande appartient à un budget.** Formulation à clarifier : le modèle actuel relie un objet de pilotage budgétaire à une commande (rattachement facultatif dans un sens), pas l'inverse structurellement. À rapprocher explicitement avec le Squad Leader avant tout lot qui construirait ce rattachement, pour savoir si la commande doit référencer un budget de façon obligatoire, ou si le rattachement optionnel existant suffit.
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

## Comment mettre à jour ce document

1. Toute règle métier nouvellement validée avec le Product Owner, le Squad Leader ou un expert métier est ajoutée ici **avant** d'être implémentée, avec le statut 🕓 **Validé, non implémenté**.
2. Quand un lot implémente une règle listée ici, son statut passe à ✅ **Implémenté** à la clôture du lot (même étape que la mise à jour de `docs/IMPLEMENTATION_STATUS.md`).
3. Un écart constaté entre une règle métier telle que formulée ici et le modèle technique réel est noté 🔎 **À clarifier**, jamais résolu unilatéralement dans le code sans validation.
4. Ce document est relu avant chaque nouveau lot, au même titre que `docs/ROADMAP.md`, `CLAUDE.md` et `docs/IMPLEMENTATION_STATUS.md`.
