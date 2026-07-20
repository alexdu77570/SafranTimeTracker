import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, TimeEntryCreateRequest, TimeEntryDto, TimeEntryRecalculationRequest, TimeEntryUpdateRequest } from '../types'

export interface TimeEntryListParams extends PaginationQuery {
  resourceId?: string
  from?: string
  to?: string
  activityTypeId?: string
  projectId?: string
  orderId?: string
}

export async function fetchTimeEntries(params?: TimeEntryListParams): Promise<PagedResult<TimeEntryDto>> {
  const response = await apiClient.get<PagedResult<TimeEntryDto>>('/time-entries', { params: { pageSize: 20, ...params } })
  return response.data
}

export async function createTimeEntry(payload: TimeEntryCreateRequest): Promise<TimeEntryDto> {
  const response = await apiClient.post<TimeEntryDto>('/time-entries', payload)
  return response.data
}

export async function updateTimeEntry(id: string, payload: TimeEntryUpdateRequest): Promise<TimeEntryDto> {
  const response = await apiClient.put<TimeEntryDto>(`/time-entries/${id}`, payload)
  return response.data
}

export async function deleteTimeEntry(id: string): Promise<void> {
  await apiClient.delete(`/time-entries/${id}`)
}

/** Gardé par TIME_ENTRY_RECALCULATION côté serveur (§19.6) : un appelant sans la permission reçoit un 403. */
export async function recalculateTimeEntry(id: string, payload: TimeEntryRecalculationRequest): Promise<TimeEntryDto> {
  const response = await apiClient.post<TimeEntryDto>(`/time-entries/${id}/recalculate`, payload)
  return response.data
}
