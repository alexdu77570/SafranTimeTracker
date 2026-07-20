import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, ProjectTypeCreateRequest, ProjectTypeDto, ProjectTypeUpdateRequest } from '../types'

export async function fetchProjectTypes(params?: PaginationQuery): Promise<PagedResult<ProjectTypeDto>> {
  const response = await apiClient.get<PagedResult<ProjectTypeDto>>('/project-types', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createProjectType(payload: ProjectTypeCreateRequest): Promise<ProjectTypeDto> {
  const response = await apiClient.post<ProjectTypeDto>('/project-types', payload)
  return response.data
}
export async function updateProjectType(id: string, payload: ProjectTypeUpdateRequest): Promise<ProjectTypeDto> {
  const response = await apiClient.put<ProjectTypeDto>(`/project-types/${id}`, payload)
  return response.data
}
