/**
 * Types partagés miroir des DTO backend (CLAUDE.md §13 : l'API n'expose jamais une entité EF Core
 * directement, ces types reflètent donc les DTO, pas le modèle de données). Un enum C# sans
 * `JsonStringEnumConverter` explicite (voir Program.cs) sérialise en entier côté API : les enums
 * ci-dessous sont donc représentés en constantes numériques, pas en union de chaînes.
 */

export interface PaginationQuery {
  page?: number
  pageSize?: number
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export const ReferentialStatus = {
  Actif: 0,
  Inactif: 1,
} as const
export type ReferentialStatus = (typeof ReferentialStatus)[keyof typeof ReferentialStatus]

export interface UserDto {
  id: string
  nom: string
  prenom: string
  identifiant: string
  email: string
  telephone: string | null
  statut: ReferentialStatus
  dateArrivee: string
  dateSortie: string | null
  commentaire: string | null
  resourceId: string | null
  roleId: string
  accesGlobal: boolean
  permissionIds: string[]
}

export interface PermissionDto {
  id: string
  code: string
  libelle: string
  description: string | null
}
