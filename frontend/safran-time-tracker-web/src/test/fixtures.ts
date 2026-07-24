import type { AuthSessionDto, PagedResult, PermissionDto, UserDto } from '../api/types'

/**
 * Sous-lot 14.1 (rapport d'audit du Lot 14, constat TST-2) : fabrique partagée remplaçant la
 * fixture utilisateur/permission/session dupliquée à l'identique dans 12 à 15 fichiers de test
 * frontend (vérifié byte pour byte identique entre plusieurs d'entre eux avant cette factorisation).
 * Toute évolution du contrat `UserDto`/session (déjà arrivée une fois, `effectivePermissionCodes`
 * au Lot 13) ne touche plus qu'un seul fichier.
 */
export function demoUserFixture(overrides?: Partial<UserDto>): UserDto {
  return {
    id: 'user-1',
    nom: 'BERNARD',
    prenom: 'Alexandre',
    identifiant: 's636140',
    email: 's636140@safran.local',
    telephone: null,
    statut: 0,
    dateArrivee: '2021-01-01',
    dateSortie: null,
    commentaire: null,
    resourceId: 'resource-1',
    roleId: 'role-1',
    accesGlobal: true,
    permissionIds: ['perm-financial'],
    effectivePermissionCodes: ['FINANCIAL_DATA_VIEW'],
    ...overrides,
  }
}

export function demoPermissionFixture(overrides?: Partial<PermissionDto>): PermissionDto {
  return {
    id: 'perm-financial',
    code: 'FINANCIAL_DATA_VIEW',
    libelle: 'Données financières',
    description: null,
    ...overrides,
  }
}

export function demoSessionFixture(overrides?: Partial<AuthSessionDto>): AuthSessionDto {
  return {
    userId: 'user-1',
    identifiant: 's636140',
    expiresAt: '2026-01-01T00:00:00Z',
    isPersistent: false,
    ...overrides,
  }
}

export function pagedResult<T>(items: T[], pageSize = 20): PagedResult<T> {
  return { items, page: 1, pageSize, totalCount: items.length }
}
