import { apiClient } from '../client'
import type {
  DepartmentCreateRequest,
  DepartmentDto,
  PagedResult,
  PaginationQuery,
  ServiceCreateRequest,
  ServiceDto,
  TeamCreateRequest,
  TeamDto,
} from '../types'

export async function fetchDepartments(params?: PaginationQuery): Promise<PagedResult<DepartmentDto>> {
  const response = await apiClient.get<PagedResult<DepartmentDto>>('/departments', { params: { pageSize: 100, ...params } })
  return response.data
}
export async function createDepartment(payload: DepartmentCreateRequest): Promise<DepartmentDto> {
  const response = await apiClient.post<DepartmentDto>('/departments', payload)
  return response.data
}

export async function fetchServices(params?: PaginationQuery & { departmentId?: string }): Promise<PagedResult<ServiceDto>> {
  const response = await apiClient.get<PagedResult<ServiceDto>>('/services', { params: { pageSize: 100, ...params } })
  return response.data
}
export async function createService(payload: ServiceCreateRequest): Promise<ServiceDto> {
  const response = await apiClient.post<ServiceDto>('/services', payload)
  return response.data
}

export async function fetchTeams(params?: PaginationQuery & { serviceId?: string }): Promise<PagedResult<TeamDto>> {
  const response = await apiClient.get<PagedResult<TeamDto>>('/teams', { params: { pageSize: 100, ...params } })
  return response.data
}
export async function createTeam(payload: TeamCreateRequest): Promise<TeamDto> {
  const response = await apiClient.post<TeamDto>('/teams', payload)
  return response.data
}
