import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../test/testUtils'
import { demoPermissionFixture, demoSessionFixture, demoUserFixture, pagedResult } from '../test/fixtures'
import { setStoredIdentifiant } from './demoIdentityStorage'
import { PermissionGuard } from './PermissionGuard'

vi.mock('../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () =>
    pagedResult(
      [
        demoUserFixture({
          nom: 'Dupont',
          prenom: 'Alice',
          email: 'alice.dupont@safran.com',
          dateArrivee: '2024-01-01',
          resourceId: null,
          accesGlobal: false,
          permissionIds: ['perm-audit'],
          effectivePermissionCodes: ['AUDIT_VIEW'],
        }),
      ],
      100,
    ),
  ),
}))

vi.mock('../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () =>
    pagedResult([demoPermissionFixture({ id: 'perm-audit', code: 'AUDIT_VIEW', libelle: 'Consultation audit' })], 100),
  ),
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
