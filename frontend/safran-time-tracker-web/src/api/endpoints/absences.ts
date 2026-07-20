import { apiClient } from '../client'
import type {
  AbsenceCreateRequest,
  AbsenceDecisionRequest,
  AbsenceDto,
  AbsenceStatus,
  AbsenceUpdateRequest,
  PagedResult,
  PaginationQuery,
} from '../types'

export async function fetchAbsences(
  params?: PaginationQuery & { resourceId?: string; statut?: AbsenceStatus },
): Promise<PagedResult<AbsenceDto>> {
  const response = await apiClient.get<PagedResult<AbsenceDto>>('/absences', { params: { pageSize: 50, ...params } })
  return response.data
}

export async function createAbsence(payload: AbsenceCreateRequest): Promise<AbsenceDto> {
  const response = await apiClient.post<AbsenceDto>('/absences', payload)
  return response.data
}

/** Restreint au statut Brouillon côté serveur (409 sinon, docs/BACKLOG_METIER.md §12). */
export async function updateAbsence(id: string, payload: AbsenceUpdateRequest): Promise<AbsenceDto> {
  const response = await apiClient.put<AbsenceDto>(`/absences/${id}`, payload)
  return response.data
}

export async function submitAbsence(id: string): Promise<AbsenceDto> {
  const response = await apiClient.post<AbsenceDto>(`/absences/${id}/submit`)
  return response.data
}

export async function validateAbsence(id: string): Promise<AbsenceDto> {
  const response = await apiClient.post<AbsenceDto>(`/absences/${id}/validate`)
  return response.data
}

export async function refuseAbsence(id: string, payload: AbsenceDecisionRequest): Promise<AbsenceDto> {
  const response = await apiClient.post<AbsenceDto>(`/absences/${id}/refuse`, payload)
  return response.data
}

/** Couvre aussi « supprimer un brouillon » (§23.2) : une annulation, jamais une suppression physique. */
export async function cancelAbsence(id: string): Promise<AbsenceDto> {
  const response = await apiClient.post<AbsenceDto>(`/absences/${id}/cancel`)
  return response.data
}
