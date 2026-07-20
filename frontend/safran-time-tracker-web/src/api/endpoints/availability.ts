import { apiClient } from '../client'
import type { AvailabilityResultDto } from '../types'

/** Sous-ressource de composition forte (Lot 3) : capacité théorique/réelle/taux de disponibilité
 * et charge RUN/hors RUN pour une ressource sur une période (§29.1-29.4) — seul point de calcul,
 * jamais reproduit côté frontend (CLAUDE.md §5). */
export async function fetchAvailability(resourceId: string, startDate: string, endDate: string): Promise<AvailabilityResultDto> {
  const response = await apiClient.get<AvailabilityResultDto>(`/resources/${resourceId}/availability`, {
    params: { startDate, endDate },
  })
  return response.data
}
