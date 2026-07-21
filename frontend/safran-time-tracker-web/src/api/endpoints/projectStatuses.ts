import { apiClient } from '../client'
import type { PagedResult, PaginationQuery, ProjectStatusDto } from '../types'

export async function fetchProjectStatuses(
  params?: PaginationQuery,
): Promise<PagedResult<ProjectStatusDto>> {
  const response = await apiClient.get<PagedResult<ProjectStatusDto>>('/project-statuses', {
    params: { pageSize: 50, ...params },
  })
  return response.data
}
