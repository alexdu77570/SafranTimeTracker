import dayjs from 'dayjs'
import type { MilestoneDto } from '../api/types'
import { MilestoneStatus } from '../api/types'

/** "À venir sous 30 jours" (§24.2, fenêtre fixe documentée comme les autres simplifications
 * §26.2) : partagé entre DashboardPage (liste des 8 prochains) et MilestonesListPage (compteur)
 * pour ne pas dupliquer ce prédicat (CLAUDE.md §5). */
export function isMilestoneUpcoming(
  milestone: MilestoneDto,
  referenceDate: string,
  windowDays = 30,
): boolean {
  const horizon = dayjs(referenceDate).add(windowDays, 'day').format('YYYY-MM-DD')
  return (
    milestone.datePrevue >= referenceDate &&
    milestone.datePrevue <= horizon &&
    milestone.statut !== MilestoneStatus.Termine &&
    milestone.statut !== MilestoneStatus.Annule
  )
}
