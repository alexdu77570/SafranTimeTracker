# État d'avancement — SAFRAN TIME TRACKER

> Ce document reflète l'état **réel** du projet, à distinguer de `docs/ROADMAP.md` qui décrit le plan. Il doit être mis à jour à la fin de chaque lot (voir `CLAUDE.md` §20 — Méthode de travail par lots), et à chaque écart significatif constaté en cours de lot.

**Dernière mise à jour : 2026-07-15 — Lot Documentation clôturé.**

## Phase préliminaire — Socle documentaire

**Statut : Terminé (2026-07-15).**

Cette phase n'est pas un lot numéroté du cahier des charges (§40) : elle précède le `Lot 0 — Fondations`, qui reste `Non démarré` tant qu'aucun code n'existe. Elle couvre uniquement la production et la validation de la documentation de cadrage (voir « État détaillé » ci-dessous).

## Synthèse des lots

| Lot | Contenu | Statut | Dernière mise à jour |
|---|---|---|---|
| Lot 0 | Fondations | Non démarré | 2026-07-15 |
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

### Code
- [ ] Dépôt Git initialisé.
- [ ] Solution .NET créée.
- [ ] Projet React créé.
- [ ] Base de données créée.
- [ ] Migration initiale créée.

Aucun code métier n'existe à ce stade, conformément à la décision explicite de ne produire que le socle documentaire avant le démarrage du Lot 0.

## Écarts connus par rapport au cahier des charges / à la roadmap

- L'entité `Application` (cahier des charges §15, §30) et le service `ApplicationService` (cahier des charges §31) sont implémentés sous les noms `ApplicationReference` et `ApplicationReferenceService` (voir « Décisions actées » ci-dessous). Le vocabulaire fonctionnel du cahier des charges n'est pas modifié, seule l'implémentation technique diverge du nom littéral employé dans le cahier des charges.

## Décisions actées

| Décision | Choix retenu | Date |
|---|---|---|
| Version .NET LTS | .NET 10 | 2026-07-15 |
| Mapping DTO ↔ entité | Mapster | 2026-07-15 |
| Mocking tests backend | NSubstitute | 2026-07-15 |
| Bibliothèque de graphiques frontend | Recharts | 2026-07-15 |
| Document `docs/DEPLOYMENT_WINDOWS.md` séparé | Non créé — contenu conservé dans `CLAUDE.md` §18-19 | 2026-07-15 |
| Collision entité `Application` / namespace `SafranTimeTracker.Application` | Entité renommée `ApplicationReference`, service `ApplicationReferenceService` | 2026-07-15 |

Aucune décision technique ouverte à ce jour.

## Comment mettre à jour ce document

1. À la fin de chaque lot : changer le statut (`Non démarré` → `En cours` → `Terminé`), mettre à jour la date, cocher les cases correspondantes.
2. Un lot ne passe à `Terminé` que s'il est compilable, testé réellement et démontrable (règle de méthode, `CLAUDE.md` §20).
3. Tout écart avec `docs/ROADMAP.md` ou le cahier des charges est consigné dans la section « Écarts connus ».
