import { apiClient } from '../client'
import type {
  OrderCreateRequest,
  OrderDto,
  OrderReopenRequest,
  OrderUpdateRequest,
  PagedResult,
  PaginationQuery,
} from '../types'

export interface OrderListParams extends PaginationQuery {
  companyId?: string
  statusId?: string
  projectId?: string
}

export async function fetchOrders(params?: OrderListParams): Promise<PagedResult<OrderDto>> {
  const response = await apiClient.get<PagedResult<OrderDto>>('/orders', {
    params: { pageSize: 20, ...params },
  })
  return response.data
}

export async function fetchOrderById(id: string): Promise<OrderDto> {
  const response = await apiClient.get<OrderDto>(`/orders/${id}`)
  return response.data
}

export async function createOrder(payload: OrderCreateRequest): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>('/orders', payload)
  return response.data
}

export async function updateOrder(id: string, payload: OrderUpdateRequest): Promise<OrderDto> {
  const response = await apiClient.put<OrderDto>(`/orders/${id}`, payload)
  return response.data
}

/** §13.2 : machine d'état, une action dédiée par transition plutôt qu'un statut libre. */
export async function activateOrder(id: string): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>(`/orders/${id}/activate`)
  return response.data
}

export async function suspendOrder(id: string): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>(`/orders/${id}/suspend`)
  return response.data
}

export async function markOrderConsumed(id: string): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>(`/orders/${id}/mark-consumed`)
  return response.data
}

export async function closeOrder(id: string): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>(`/orders/${id}/close`)
  return response.data
}

/** §13.4 : seule action capable de sortir une commande de Clôturée, motif obligatoire. */
export async function reopenOrder(id: string, payload: OrderReopenRequest): Promise<OrderDto> {
  const response = await apiClient.post<OrderDto>(`/orders/${id}/reopen`, payload)
  return response.data
}
