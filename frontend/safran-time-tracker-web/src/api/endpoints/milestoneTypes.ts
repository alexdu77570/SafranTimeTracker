import { apiClient } from '../client'
import type { MilestoneTypeCreateRequest, MilestoneTypeDto, PagedResult, PaginationQuery } from '../types'

export async function fetchMilestoneTypes(params?: PaginationQuery): Promise<PagedResult<MilestoneTypeDto>> {
  const response = await apiClient.get<PagedResult<MilestoneTypeDto>>('/milestone-types', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createMilestoneType(payload: MilestoneTypeCreateRequest): Promise<MilestoneTypeDto> {
  const response = await apiClient.post<MilestoneTypeDto>('/milestone-types', payload)
  return response.data
}
