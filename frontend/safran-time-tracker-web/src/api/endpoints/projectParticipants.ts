import { apiClient } from '../client'
import type {
  PagedResult,
  PaginationQuery,
  ProjectParticipantCreateRequest,
  ProjectParticipantDto,
} from '../types'

export async function fetchProjectParticipants(
  projectId: string,
  params?: PaginationQuery,
): Promise<PagedResult<ProjectParticipantDto>> {
  const response = await apiClient.get<PagedResult<ProjectParticipantDto>>(
    `/projects/${projectId}/participants`,
    { params: { pageSize: 100, ...params } },
  )
  return response.data
}

export async function createProjectParticipant(
  projectId: string,
  payload: ProjectParticipantCreateRequest,
): Promise<ProjectParticipantDto> {
  const response = await apiClient.post<ProjectParticipantDto>(
    `/projects/${projectId}/participants`,
    payload,
  )
  return response.data
}

/** Retrait (désactivation, CLAUDE.md §7) — pas une suppression physique. */
export async function removeProjectParticipant(
  projectId: string,
  participantId: string,
): Promise<ProjectParticipantDto> {
  const response = await apiClient.delete<ProjectParticipantDto>(
    `/projects/${projectId}/participants/${participantId}`,
  )
  return response.data
}
