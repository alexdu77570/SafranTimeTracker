import dayjs from 'dayjs'
import 'dayjs/locale/fr'

/**
 * Semaine lundi-dimanche (`.locale('fr')` explicite) : le projet ne fixe pas de locale dayjs
 * globale (seul `LocalizationProvider adapterLocale="fr"` pilote les DatePicker MUI) — un
 * `dayjs().startOf('week')` sans précision retomberait sur la locale par défaut (dimanche).
 * Partagé entre `/temps` (filtre « semaine », §19.4) et `/disponibilites` (vue hebdomadaire,
 * §22.2) pour ne pas dupliquer ce calcul (CLAUDE.md §5).
 */
export function weekBounds(referenceDate: string): { start: string; end: string } {
  return {
    start: dayjs(referenceDate).locale('fr').startOf('week').format('YYYY-MM-DD'),
    end: dayjs(referenceDate).locale('fr').endOf('week').format('YYYY-MM-DD'),
  }
}

/** Libellés mensuels abrégés (§14.3/§21.3) : partagé entre BudgetsListPage (consommation
 * mensuelle) et ChargesPage (répartition mensuelle) pour ne pas dupliquer ce tableau (CLAUDE.md §5). */
export const MONTH_LABELS = [
  'janv.',
  'févr.',
  'mars',
  'avr.',
  'mai',
  'juin',
  'juil.',
  'août',
  'sept.',
  'oct.',
  'nov.',
  'déc.',
]
