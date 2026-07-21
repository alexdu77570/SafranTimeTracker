import { apiClient } from '../client'
import type {
  OrderReceiptCreateRequest,
  OrderReceiptDto,
  OrderReceiptSummaryDto,
  PagedResult,
  PaginationQuery,
} from '../types'

/** Réceptions partielles (règle métier validée Lot 6), sous-ressource de composition forte —
 * append-only, aucune mise à jour : une correction est une nouvelle réception. */
export async function fetchOrderReceipts(
  orderId: string,
  params?: PaginationQuery,
): Promise<PagedResult<OrderReceiptDto>> {
  const response = await apiClient.get<PagedResult<OrderReceiptDto>>(
    `/orders/${orderId}/receipts`,
    {
      params: { pageSize: 100, ...params },
    },
  )
  return response.data
}

export async function fetchOrderReceiptSummary(orderId: string): Promise<OrderReceiptSummaryDto> {
  const response = await apiClient.get<OrderReceiptSummaryDto>(
    `/orders/${orderId}/receipts/summary`,
  )
  return response.data
}

export async function createOrderReceipt(
  orderId: string,
  payload: OrderReceiptCreateRequest,
): Promise<OrderReceiptDto> {
  const response = await apiClient.post<OrderReceiptDto>(`/orders/${orderId}/receipts`, payload)
  return response.data
}
