import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../test/testUtils'
import { setStoredIdentifiant } from './demoIdentityStorage'
import { PermissionGuard } from './PermissionGuard'

vi.mock('../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => ({
    items: [
      {
        id: 'user-1',
        nom: 'Dupont',
        prenom: 'Alice',
        identifiant: 's636140',
        email: 'alice.dupont@safran.com',
        telephone: null,
        statut: 0,
        dateArrivee: '2024-01-01',
        dateSortie: null,
        commentaire: null,
        resourceId: null,
        roleId: 'role-1',
        accesGlobal: false,
        permissionIds: ['perm-audit'],
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))

vi.mock('../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => ({
    items: [
      { id: 'perm-audit', code: 'AUDIT_VIEW', libelle: 'Consultation audit', description: null },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))

afterEach(() => {
  localStorage.clear()
})

describe('PermissionGuard', () => {
  it('renders the fallback (nothing by default) while no identity is chosen', async () => {
    render(
      <DemoTestProviders>
        <PermissionGuard code="AUDIT_VIEW">
          <span>Journal d'audit</span>
        </PermissionGuard>
      </DemoTestProviders>,
    )

    await waitFor(() => expect(screen.queryByText("Journal d'audit")).not.toBeInTheDocument())
  })

  it('renders children once the current identity holds the required permission', async () => {
    setStoredIdentifiant('s636140')

    render(
      <DemoTestProviders>
        <PermissionGuard code="AUDIT_VIEW">
          <span>Journal d'audit</span>
        </PermissionGuard>
      </DemoTestProviders>,
    )

    expect(await screen.findByText("Journal d'audit")).toBeInTheDocument()
  })

  it('renders an explicit fallback when provided and the permission is missing', async () => {
    setStoredIdentifiant('s636140')

    render(
      <DemoTestProviders>
        <PermissionGuard code="FINANCIAL_DATA_VIEW" fallback={<span>Accès restreint</span>}>
          <span>Budgets</span>
        </PermissionGuard>
      </DemoTestProviders>,
    )

    expect(await screen.findByText('Accès restreint')).toBeInTheDocument()
    expect(screen.queryByText('Budgets')).not.toBeInTheDocument()
  })
})
