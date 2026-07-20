import { apiClient } from '../client'
import type { OrderDto, PagedResult, PaginationQuery } from '../types'

export async function fetchOrders(params?: PaginationQuery): Promise<PagedResult<OrderDto>> {
  const response = await apiClient.get<PagedResult<OrderDto>>('/orders', { params: { pageSize: 20, ...params } })
  return response.data
}
