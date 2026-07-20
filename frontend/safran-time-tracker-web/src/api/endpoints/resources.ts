import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, ResourceDto } from '../types'

export async function fetchResources(
  params?: PaginationQuery & { departmentId?: string; serviceId?: string },
): Promise<PagedResult<ResourceDto>> {
  const response = await apiClient.get<PagedResult<ResourceDto>>('/resources', { params: { pageSize: 20, ...params } })
  return response.data
}

export async function fetchResourceById(id: string): Promise<ResourceDto> {
  const response = await apiClient.get<ResourceDto>(`/resources/${id}`)
  return response.data
}
