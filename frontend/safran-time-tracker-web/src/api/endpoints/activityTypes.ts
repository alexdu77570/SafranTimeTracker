import { apiClient } from '../client'
import type { ActivityTypeCreateRequest, ActivityTypeDto, PagedResult, PaginationQuery } from '../types'

export async function fetchActivityTypes(params?: PaginationQuery): Promise<PagedResult<ActivityTypeDto>> {
  const response = await apiClient.get<PagedResult<ActivityTypeDto>>('/activity-types', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createActivityType(payload: ActivityTypeCreateRequest): Promise<ActivityTypeDto> {
  const response = await apiClient.post<ActivityTypeDto>('/activity-types', payload)
  return response.data
}
