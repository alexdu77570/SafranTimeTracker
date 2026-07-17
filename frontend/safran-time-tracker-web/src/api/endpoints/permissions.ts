import { apiClient } from '../client'
import type { PagedResult, PermissionDto } from '../types'

/** Référentiel des permissions (Lot 7) : seul point de résolution GUID → Code, utilisé par
 * `useCurrentUser` pour évaluer `PermissionGuard`. */
export async function fetchPermissions(pageSize = 100): Promise<PagedResult<PermissionDto>> {
  const response = await apiClient.get<PagedResult<PermissionDto>>('/permissions', {
    params: { pageSize },
  })
  return response.data
}
