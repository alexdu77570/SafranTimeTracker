import { apiClient } from '../client'
import type {
  BudgetAdjustRequest,
  BudgetCreateRequest,
  BudgetDto,
  BudgetUpdateRequest,
  BudgetVersionDto,
  PagedResult,
  PaginationQuery,
} from '../types'

export interface BudgetListParams extends PaginationQuery {
  projectId?: string
  orderId?: string
}

/** Ressource intégralement financière (§14) : 403 côté serveur sans FINANCIAL_DATA_VIEW. */
export async function fetchBudgets(params?: BudgetListParams): Promise<PagedResult<BudgetDto>> {
  const response = await apiClient.get<PagedResult<BudgetDto>>('/budgets', {
    params: { pageSize: 100, ...params },
  })
  return response.data
}

export async function createBudget(payload: BudgetCreateRequest): Promise<BudgetDto> {
  const response = await apiClient.post<BudgetDto>('/budgets', payload)
  return response.data
}

export async function updateBudget(id: string, payload: BudgetUpdateRequest): Promise<BudgetDto> {
  const response = await apiClient.put<BudgetDto>(`/budgets/${id}`, payload)
  return response.data
}

/** §14.1 : pas de suppression physique, la clôture en tient lieu. */
export async function closeBudget(id: string): Promise<BudgetDto> {
  const response = await apiClient.post<BudgetDto>(`/budgets/${id}/close`)
  return response.data
}

export async function reactivateBudget(id: string): Promise<BudgetDto> {
  const response = await apiClient.post<BudgetDto>(`/budgets/${id}/reactivate`)
  return response.data
}

export async function fetchBudgetVersions(
  id: string,
  params?: PaginationQuery,
): Promise<PagedResult<BudgetVersionDto>> {
  const response = await apiClient.get<PagedResult<BudgetVersionDto>>(`/budgets/${id}/versions`, {
    params: { pageSize: 100, ...params },
  })
  return response.data
}

/** §14.2 : chaque ajustement conserve ancienne valeur, nouvelle valeur, motif, auteur, date. */
export async function adjustBudget(
  id: string,
  payload: BudgetAdjustRequest,
): Promise<BudgetVersionDto> {
  const response = await apiClient.post<BudgetVersionDto>(`/budgets/${id}/versions`, payload)
  return response.data
}
