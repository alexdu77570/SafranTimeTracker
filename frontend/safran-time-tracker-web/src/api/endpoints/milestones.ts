import { apiClient } from '../client'
import type {
  MilestoneCreateRequest,
  MilestoneDto,
  MilestoneStatus,
  MilestoneUpdateRequest,
  PagedResult,
  PaginationQuery,
} from '../types'

export interface MilestoneListParams extends PaginationQuery {
  projectId?: string
  responsableId?: string
  statut?: MilestoneStatus
  enRetard?: boolean
}

export async function fetchMilestones(
  params?: MilestoneListParams,
): Promise<PagedResult<MilestoneDto>> {
  const response = await apiClient.get<PagedResult<MilestoneDto>>('/milestones', {
    params: { pageSize: 100, ...params },
  })
  return response.data
}

export async function createMilestone(payload: MilestoneCreateRequest): Promise<MilestoneDto> {
  const response = await apiClient.post<MilestoneDto>('/milestones', payload)
  return response.data
}

export async function updateMilestone(
  id: string,
  payload: MilestoneUpdateRequest,
): Promise<MilestoneDto> {
  const response = await apiClient.put<MilestoneDto>(`/milestones/${id}`, payload)
  return response.data
}
