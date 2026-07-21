import { apiClient } from '../client'
import type {
  PagedResult,
  PaginationQuery,
  ProjectCreateRequest,
  ProjectDto,
  ProjectRiskLevel,
  ProjectUpdateRequest,
} from '../types'

export interface ProjectListParams extends PaginationQuery {
  statusId?: string
  applicationId?: string
  piloteId?: string
  departmentId?: string
  serviceId?: string
  teamId?: string
  niveauRisque?: ProjectRiskLevel
  from?: string
  to?: string
  alertePlanning?: boolean
  alerteBudget?: boolean
}

export async function fetchProjects(params?: ProjectListParams): Promise<PagedResult<ProjectDto>> {
  const response = await apiClient.get<PagedResult<ProjectDto>>('/projects', {
    params: { pageSize: 100, ...params },
  })
  return response.data
}

export async function fetchProjectById(id: string): Promise<ProjectDto> {
  const response = await apiClient.get<ProjectDto>(`/projects/${id}`)
  return response.data
}

export async function createProject(payload: ProjectCreateRequest): Promise<ProjectDto> {
  const response = await apiClient.post<ProjectDto>('/projects', payload)
  return response.data
}

export async function updateProject(
  id: string,
  payload: ProjectUpdateRequest,
): Promise<ProjectDto> {
  const response = await apiClient.put<ProjectDto>(`/projects/${id}`, payload)
  return response.data
}

/** §16.3 : pas de suppression physique (CLAUDE.md §7), l'archivage en tient lieu. */
export async function archiveProject(id: string): Promise<ProjectDto> {
  const response = await apiClient.post<ProjectDto>(`/projects/${id}/archive`)
  return response.data
}

export async function reactivateProject(id: string): Promise<ProjectDto> {
  const response = await apiClient.post<ProjectDto>(`/projects/${id}/reactivate`)
  return response.data
}
