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
