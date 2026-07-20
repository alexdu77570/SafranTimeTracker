import { apiClient } from '../client'
import type { PagedResult, ResourceTjmHistoryDto } from '../types'

/** Gardé par FINANCIAL_DATA_VIEW côté serveur (§11.2) : un appelant sans la permission reçoit un 403. */
export async function fetchResourceTjmHistory(resourceId: string): Promise<PagedResult<ResourceTjmHistoryDto>> {
  const response = await apiClient.get<PagedResult<ResourceTjmHistoryDto>>('/resource-tjm-history', {
    params: { resourceId, pageSize: 50 },
  })
  return response.data
}
