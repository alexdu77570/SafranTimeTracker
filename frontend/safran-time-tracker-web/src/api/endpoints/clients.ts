import { apiClient } from '../client'
import type { ClientCreateRequest, ClientDto, ClientUpdateRequest, PagedResult, PaginationQuery } from '../types'

export async function fetchClients(params?: PaginationQuery): Promise<PagedResult<ClientDto>> {
  const response = await apiClient.get<PagedResult<ClientDto>>('/clients', { params: { pageSize: 50, ...params } })
  return response.data
}
export async function createClient(payload: ClientCreateRequest): Promise<ClientDto> {
  const response = await apiClient.post<ClientDto>('/clients', payload)
  return response.data
}
export async function updateClient(id: string, payload: ClientUpdateRequest): Promise<ClientDto> {
  const response = await apiClient.put<ClientDto>(`/clients/${id}`, payload)
  return response.data
}
