import { apiClient } from '../client'
import type {
  PagedResult,
  PaginationQuery,
  ProjectPlanningRowDto,
  ProjectPlanningSynthesisDto,
  ProjectPlanVersionAdjustmentRequest,
  ProjectPlanVersionCreateRequest,
  ProjectPlanVersionDto,
  ProjectWeeklyPlanDto,
  ProjectWeeklyPlanLineRequest,
} from '../types'

export async function fetchProjectPlanVersions(
  projectId: string,
  params?: PaginationQuery,
): Promise<PagedResult<ProjectPlanVersionDto>> {
  const response = await apiClient.get<PagedResult<ProjectPlanVersionDto>>(
    `/projects/${projectId}/plan-versions`,
    { params: { pageSize: 100, ...params } },
  )
  return response.data
}

export async function createInitialPlanVersion(
  projectId: string,
  payload: ProjectPlanVersionCreateRequest,
): Promise<ProjectPlanVersionDto> {
  const response = await apiClient.post<ProjectPlanVersionDto>(
    `/projects/${projectId}/plan-versions/initial`,
    payload,
  )
  return response.data
}

/** Archive automatiquement la version Ajustée Active précédente côté serveur (§18.3). */
export async function createAdjustedPlanVersion(
  projectId: string,
  payload: ProjectPlanVersionAdjustmentRequest,
): Promise<ProjectPlanVersionDto> {
  const response = await apiClient.post<ProjectPlanVersionDto>(
    `/projects/${projectId}/plan-versions/adjusted`,
    payload,
  )
  return response.data
}

export async function fetchWeeklyPlans(
  projectId: string,
  versionId: string,
): Promise<ProjectWeeklyPlanDto[]> {
  const response = await apiClient.get<ProjectWeeklyPlanDto[]>(
    `/projects/${projectId}/plan-versions/${versionId}/weekly-plans`,
  )
  return response.data
}

export async function setWeeklyPlans(
  projectId: string,
  versionId: string,
  lines: ProjectWeeklyPlanLineRequest[],
): Promise<ProjectWeeklyPlanDto[]> {
  const response = await apiClient.post<ProjectWeeklyPlanDto[]>(
    `/projects/${projectId}/plan-versions/${versionId}/weekly-plans`,
    lines,
  )
  return response.data
}

/** Seul point de calcul des écarts/risques (§18.1, §29.5) — jamais recalculé côté frontend. */
export async function fetchProjectPlanningSynthesis(
  projectId: string,
): Promise<ProjectPlanningSynthesisDto> {
  const response = await apiClient.get<ProjectPlanningSynthesisDto>(
    `/projects/${projectId}/planning`,
  )
  return response.data
}

export interface ProjectPlanningOverviewParams extends PaginationQuery {
  projectId?: string
  resourceId?: string
  serviceId?: string
  departmentId?: string
  teamId?: string
  from?: string
  to?: string
  surcharge?: boolean
}

/** Vue transverse "Planning projet" (§18.2) : agrégation entièrement serveur (GET /project-planning),
 * décision actée avec l'utilisateur à l'ouverture du Lot 10 pour éviter les N appels frontend. */
export async function fetchProjectPlanningOverview(
  params?: ProjectPlanningOverviewParams,
): Promise<PagedResult<ProjectPlanningRowDto>> {
  const response = await apiClient.get<PagedResult<ProjectPlanningRowDto>>('/project-planning', {
    params: { pageSize: 50, ...params },
  })
  return response.data
}
