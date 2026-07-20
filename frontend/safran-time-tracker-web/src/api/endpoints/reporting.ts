import { apiClient } from '../client'
import type { ChargesReportDto, ReportingFilterQuery } from '../types'

/** Réutilisé pour le détail statistique d'une application (docs/ROADMAP.md, Lot 8) : mêmes
 * agrégations que l'écran Charges (§21, Lot 5/12), filtrées par applicationId — aucune nouvelle
 * agrégation backend, seule la réutilisation d'un endpoint déjà livré. */
export async function fetchCharges(filter: ReportingFilterQuery): Promise<ChargesReportDto> {
  const response = await apiClient.get<ChargesReportDto>('/reporting/charges', { params: filter })
  return response.data
}
