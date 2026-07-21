import { apiClient } from '../client'
import type {
  OrderExtensionCreateRequest,
  OrderExtensionDto,
  PagedResult,
  PaginationQuery,
} from '../types'

/** Rallonges de commande (§13.3), sous-ressource de composition forte — append-only. */
export async function fetchOrderExtensions(
  orderId: string,
  params?: PaginationQuery,
): Promise<PagedResult<OrderExtensionDto>> {
  const response = await apiClient.get<PagedResult<OrderExtensionDto>>(
    `/orders/${orderId}/extensions`,
    {
      params: { pageSize: 100, ...params },
    },
  )
  return response.data
}

export async function createOrderExtension(
  orderId: string,
  payload: OrderExtensionCreateRequest,
): Promise<OrderExtensionDto> {
  const response = await apiClient.post<OrderExtensionDto>(`/orders/${orderId}/extensions`, payload)
  return response.data
}
