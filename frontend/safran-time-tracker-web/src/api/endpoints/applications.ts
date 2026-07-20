import { apiClient } from '../client'
import type {
  ApplicationReferenceCreateRequest,
  ApplicationReferenceDto,
  PagedResult,
  PaginationQuery,
} from '../types'

export async function fetchApplications(
  params?: PaginationQuery & { serviceId?: string },
): Promise<PagedResult<ApplicationReferenceDto>> {
  const response = await apiClient.get<PagedResult<ApplicationReferenceDto>>('/applications', {
    params: { pageSize: 20, ...params },
  })
  return response.data
}

export async function fetchApplicationById(id: string): Promise<ApplicationReferenceDto> {
  const response = await apiClient.get<ApplicationReferenceDto>(`/applications/${id}`)
  return response.data
}

export async function createApplication(payload: ApplicationReferenceCreateRequest): Promise<ApplicationReferenceDto> {
  const response = await apiClient.post<ApplicationReferenceDto>('/applications', payload)
  return response.data
}
