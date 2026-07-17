import { apiClient } from '../client'
import type { PagedResult, UserDto } from '../types'

/** Sert à la fois aux futurs écrans Administration et au sélecteur d'identité de démonstration
 * (Header) — un seul point d'appel, jamais dupliqué par écran (CLAUDE.md §9). */
export async function fetchUsers(pageSize = 100): Promise<PagedResult<UserDto>> {
  const response = await apiClient.get<PagedResult<UserDto>>('/users', { params: { pageSize } })
  return response.data
}
