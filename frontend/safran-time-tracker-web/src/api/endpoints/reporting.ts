import { apiClient } from '../client'
import type { ChargesReportDto, ProjectLinkedReferenceDto, ReportingFilterQuery } from '../types'

/** Réutilisé pour le détail statistique d'une application (docs/ROADMAP.md, Lot 8) : mêmes
 * agrégations que l'écran Charges (§21, Lot 5/12), filtrées par applicationId — aucune nouvelle
 * agrégation backend, seule la réutilisation d'un endpoint déjà livré. */
export async function fetchCharges(filter: ReportingFilterQuery): Promise<ChargesReportDto> {
  const response = await apiClient.get<ChargesReportDto>('/reporting/charges', { params: filter })
  return response.data
}

/** §17.7 : références RUN (INC/CHG/PRB/RITM/VABE/VSR) rattachées à un projet — dérivées de
 * TimeEntry.Reference, jamais intégrées au modèle Projet (docs/BACKLOG_METIER.md §3). */
export async function fetchProjectLinkedReferences(
  projectId: string,
): Promise<ProjectLinkedReferenceDto[]> {
  const response = await apiClient.get<ProjectLinkedReferenceDto[]>(
    `/reporting/projects/${projectId}/linked-references`,
  )
  return response.data
}
