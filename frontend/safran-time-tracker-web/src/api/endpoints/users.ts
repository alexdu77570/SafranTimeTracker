import { apiClient } from '../client'
import type { PagedResult, UserDto } from '../types'

/** Sert à la fois aux futurs écrans Administration et au sélecteur d'identité de démonstration
 * (Header) — un seul point d'appel, jamais dupliqué par écran (CLAUDE.md §9). */
export async function fetchUsers(pageSize = 100): Promise<PagedResult<UserDto>> {
  const response = await apiClient.get<PagedResult<UserDto>>('/users', { params: { pageSize } })
  return response.data
}

/** Gardées par USER_ADMINISTRATION côté serveur (§28.3, Lot 6) : un appelant sans la permission
 * reçoit un 403. */
export async function deactivateUser(id: string): Promise<void> {
  await apiClient.post(`/users/${id}/deactivate`)
}
export async function reactivateUser(id: string): Promise<void> {
  await apiClient.post(`/users/${id}/reactivate`)
}
