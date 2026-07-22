import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { fetchUsers } from '../api/endpoints/users'
import { useDemoIdentity } from './useDemoIdentity'

/**
 * Résout l'utilisateur de démonstration courant et ses permissions effectives
 * (`UserDto.effectivePermissionCodes`, rôle + exceptions individuelles — Lot 13). C'est la seule
 * source de vérité côté frontend pour `PermissionGuard` — le filtrage réel des données reste
 * exclusivement côté serveur (CLAUDE.md §17) : ce hook ne fait qu'adapter l'affichage.
 */
export function useCurrentUser() {
  const { identifiant, setIdentifiant } = useDemoIdentity()

  const usersQuery = useQuery({ queryKey: ['users', 'all'], queryFn: () => fetchUsers() })

  const user = useMemo(
    () => usersQuery.data?.items.find((candidate) => candidate.identifiant === identifiant),
    [usersQuery.data, identifiant],
  )

  const permissionCodes = user?.effectivePermissionCodes ?? []

  const hasPermission = (code: string) => permissionCodes.includes(code)

  return {
    identifiant,
    setIdentifiant,
    availableUsers: usersQuery.data?.items ?? [],
    user,
    permissionCodes,
    hasPermission,
    isLoading: usersQuery.isLoading,
    isError: usersQuery.isError,
  }
}
