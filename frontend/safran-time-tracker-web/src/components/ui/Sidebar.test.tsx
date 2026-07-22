import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { DemoTestProviders } from '../../test/testUtils'
import { setStoredIdentifiant } from '../../auth/demoIdentityStorage'
import { Sidebar } from './Sidebar'

vi.mock('../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => ({
    userId: 'user-1',
    identifiant: 's636140',
    expiresAt: '2026-01-01T00:00:00Z',
    isPersistent: false,
  })),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../api/endpoints/users', () => ({
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
        permissionIds: ['perm-financial'],
        effectivePermissionCodes: ['FINANCIAL_DATA_VIEW'],
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))

vi.mock('../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => ({
    items: [
      {
        id: 'perm-financial',
        code: 'FINANCIAL_DATA_VIEW',
        libelle: 'Accès financier',
        description: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))

afterEach(() => {
  localStorage.clear()
})

function renderSidebar() {
  return render(
    <MemoryRouter>
      <DemoTestProviders>
        <Sidebar />
      </DemoTestProviders>
    </MemoryRouter>,
  )
}

describe('Sidebar', () => {
  it('always shows navigation entries that have no permission requirement', () => {
    renderSidebar()

    expect(screen.getByText('Tableau de bord')).toBeInTheDocument()
    expect(screen.getByText('Projets')).toBeInTheDocument()
  })

  it('hides Budgets and Imports while no identity (and therefore no permission) is resolved', async () => {
    renderSidebar()

    await waitFor(() => expect(screen.queryByText('Tableau de bord')).toBeInTheDocument())
    expect(screen.queryByText('Budgets')).not.toBeInTheDocument()
    expect(screen.queryByText('Imports')).not.toBeInTheDocument()
  })

  it('shows Budgets once the current identity holds FINANCIAL_DATA_VIEW, but not Imports', async () => {
    setStoredIdentifiant('s636140')

    renderSidebar()

    expect(await screen.findByText('Budgets')).toBeInTheDocument()
    expect(screen.queryByText('Imports')).not.toBeInTheDocument()
  })
})
