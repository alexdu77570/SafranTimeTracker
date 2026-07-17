import type { ReactNode } from 'react'
import { useCurrentUser } from './useCurrentUser'

interface PermissionGuardProps {
  /** Code de permission requis (voir PermissionCodes côté backend, ex. AUDIT_VIEW). */
  code: string
  children: ReactNode
  /** Affiché à la place de `children` quand la permission est absente. Rien par défaut : une
   * entrée de navigation non autorisée disparaît, elle ne s'affiche jamais désactivée. */
  fallback?: ReactNode
}

/**
 * Adapte uniquement l'affichage (CLAUDE.md §17, ARCHITECTURE.md §3) : `PermissionGuard` ne
 * masque jamais une donnée financière ou métier déjà présente dans la réponse API — le serveur ne
 * renvoie tout simplement pas la donnée non autorisée. Ce composant sert à cacher des éléments
 * d'interface (entrées de navigation, actions) dont l'autorisation est déjà vérifiée côté serveur.
 */
export function PermissionGuard({ code, children, fallback = null }: PermissionGuardProps) {
  const { hasPermission } = useCurrentUser()
  return hasPermission(code) ? <>{children}</> : <>{fallback}</>
}
