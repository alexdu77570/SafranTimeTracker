import { apiClient } from '../client'
import type {
  CompanyContractHistoryDto,
  CompanyCreateRequest,
  CompanyDto,
  CompanyUpdateRequest,
  PagedResult,
  PaginationQuery,
} from '../types'

export async function fetchCompanies(params?: PaginationQuery): Promise<PagedResult<CompanyDto>> {
  const response = await apiClient.get<PagedResult<CompanyDto>>('/companies', { params: { pageSize: 20, ...params } })
  return response.data
}

export async function fetchCompanyById(id: string): Promise<CompanyDto> {
  const response = await apiClient.get<CompanyDto>(`/companies/${id}`)
  return response.data
}

export async function createCompany(payload: CompanyCreateRequest): Promise<CompanyDto> {
  const response = await apiClient.post<CompanyDto>('/companies', payload)
  return response.data
}

export async function updateCompany(id: string, payload: CompanyUpdateRequest): Promise<CompanyDto> {
  const response = await apiClient.put<CompanyDto>(`/companies/${id}`, payload)
  return response.data
}

/** Historique des contrats (§12.3) : confidentiel, gardé par FINANCIAL_DATA_VIEW côté serveur —
 * un appelant sans la permission reçoit un 403 explicite, jamais une liste tronquée. */
export async function fetchCompanyContracts(companyId: string): Promise<PagedResult<CompanyContractHistoryDto>> {
  const response = await apiClient.get<PagedResult<CompanyContractHistoryDto>>('/company-contracts', {
    params: { companyId, pageSize: 50 },
  })
  return response.data
}
