import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { fetchPermissions } from '../api/endpoints/permissions'
import { fetchUsers } from '../api/endpoints/users'
import { useDemoIdentity } from './useDemoIdentity'

/**
 * Résout l'utilisateur de démonstration courant et ses codes de permission, en joignant
 * `UserDto.permissionIds` (GUIDs) au référentiel `GET /api/v1/permissions` (Lot 7). C'est la seule
 * source de vérité côté frontend pour `PermissionGuard` — le filtrage réel des données reste
 * exclusivement côté serveur (CLAUDE.md §17) : ce hook ne fait qu'adapter l'affichage.
 */
export function useCurrentUser() {
  const { identifiant, setIdentifiant } = useDemoIdentity()

  const usersQuery = useQuery({ queryKey: ['users', 'all'], queryFn: () => fetchUsers() })
  const permissionsQuery = useQuery({
    queryKey: ['permissions', 'all'],
    queryFn: () => fetchPermissions(),
  })

  const user = useMemo(
    () => usersQuery.data?.items.find((candidate) => candidate.identifiant === identifiant),
    [usersQuery.data, identifiant],
  )

  const permissionCodes = useMemo(() => {
    if (!user || !permissionsQuery.data) {
      return []
    }
    const permissionById = new Map(
      permissionsQuery.data.items.map((permission) => [permission.id, permission.code]),
    )
    return user.permissionIds
      .map((id) => permissionById.get(id))
      .filter((code): code is string => Boolean(code))
  }, [user, permissionsQuery.data])

  const hasPermission = (code: string) => permissionCodes.includes(code)

  return {
    identifiant,
    setIdentifiant,
    availableUsers: usersQuery.data?.items ?? [],
    user,
    permissionCodes,
    hasPermission,
    isLoading: usersQuery.isLoading || permissionsQuery.isLoading,
    isError: usersQuery.isError || permissionsQuery.isError,
  }
}
