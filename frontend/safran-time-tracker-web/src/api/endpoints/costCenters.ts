import { apiClient } from '../client'
import type { CostCenterCreateRequest, CostCenterDto, CostCenterUpdateRequest, PagedResult, PaginationQuery } from '../types'

export async function fetchCostCenters(
  params?: PaginationQuery & { departmentId?: string; serviceId?: string },
): Promise<PagedResult<CostCenterDto>> {
  const response = await apiClient.get<PagedResult<CostCenterDto>>('/cost-centers', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createCostCenter(payload: CostCenterCreateRequest): Promise<CostCenterDto> {
  const response = await apiClient.post<CostCenterDto>('/cost-centers', payload)
  return response.data
}
export async function updateCostCenter(id: string, payload: CostCenterUpdateRequest): Promise<CostCenterDto> {
  const response = await apiClient.put<CostCenterDto>(`/cost-centers/${id}`, payload)
  return response.data
}
