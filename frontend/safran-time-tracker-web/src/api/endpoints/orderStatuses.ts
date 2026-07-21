import { apiClient } from '../client'
import type { OrderStatusDto, PagedResult, PaginationQuery } from '../types'

/** Référentiel en lecture seule (Lot 11, décision 2) : mêmes conventions que projectStatuses.ts (Lot 10). */
export async function fetchOrderStatuses(
  params?: PaginationQuery,
): Promise<PagedResult<OrderStatusDto>> {
  const response = await apiClient.get<PagedResult<OrderStatusDto>>('/order-statuses', {
    params: { pageSize: 100, ...params },
  })
  return response.data
}
