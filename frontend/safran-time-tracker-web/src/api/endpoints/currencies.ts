import { apiClient } from '../client'
import type { CurrencyCreateRequest, CurrencyDto, CurrencyUpdateRequest, PagedResult, PaginationQuery } from '../types'

export async function fetchCurrencies(params?: PaginationQuery): Promise<PagedResult<CurrencyDto>> {
  const response = await apiClient.get<PagedResult<CurrencyDto>>('/currencies', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createCurrency(payload: CurrencyCreateRequest): Promise<CurrencyDto> {
  const response = await apiClient.post<CurrencyDto>('/currencies', payload)
  return response.data
}
export async function updateCurrency(id: string, payload: CurrencyUpdateRequest): Promise<CurrencyDto> {
  const response = await apiClient.put<CurrencyDto>(`/currencies/${id}`, payload)
  return response.data
}
