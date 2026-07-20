import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, TechnologyCreateRequest, TechnologyDto, TechnologyUpdateRequest } from '../types'

export async function fetchTechnologies(
  params?: PaginationQuery & { applicationId?: string; resourceId?: string },
): Promise<PagedResult<TechnologyDto>> {
  const response = await apiClient.get<PagedResult<TechnologyDto>>('/technologies', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createTechnology(payload: TechnologyCreateRequest): Promise<TechnologyDto> {
  const response = await apiClient.post<TechnologyDto>('/technologies', payload)
  return response.data
}
export async function updateTechnology(id: string, payload: TechnologyUpdateRequest): Promise<TechnologyDto> {
  const response = await apiClient.put<TechnologyDto>(`/technologies/${id}`, payload)
  return response.data
}
