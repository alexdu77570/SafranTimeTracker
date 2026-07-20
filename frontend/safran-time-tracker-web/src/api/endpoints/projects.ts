import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, ProjectDto } from '../types'

export async function fetchProjects(params?: PaginationQuery): Promise<PagedResult<ProjectDto>> {
  const response = await apiClient.get<PagedResult<ProjectDto>>('/projects', { params: { pageSize: 100, ...params } })
  return response.data
}
