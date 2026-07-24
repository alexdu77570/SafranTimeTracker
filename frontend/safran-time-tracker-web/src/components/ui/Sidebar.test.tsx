import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { DemoTestProviders } from '../../test/testUtils'
import { demoPermissionFixture, demoSessionFixture, demoUserFixture, pagedResult } from '../../test/fixtures'
import { setStoredIdentifiant } from '../../auth/demoIdentityStorage'
import { Sidebar } from './Sidebar'

vi.mock('../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../api/endpoints/users', () => ({
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
        }),
      ],
      100,
    ),
  ),
}))

vi.mock('../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => pagedResult([demoPermissionFixture({ libelle: 'Accès financier' })], 100)),
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
