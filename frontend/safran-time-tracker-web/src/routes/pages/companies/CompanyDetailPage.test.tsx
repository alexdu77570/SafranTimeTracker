import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { CompanyDetailPage } from './CompanyDetailPage'

vi.mock('../../../api/endpoints/companies', () => ({
  fetchCompanyById: vi.fn(async () => ({
    id: 'company-1',
    nom: 'Externe Conseil',
    code: 'EXTCONSEIL',
    companyTypeId: '00000000-0000-0000-0004-000000000002',
    statut: 0,
    contactPrincipal: 'Jean Martin',
    emailContact: 'jean.martin@externe.fr',
    telephone: null,
    adresse: null,
    commentaire: null,
  })),
  fetchCompanyContracts: vi.fn(async () => ({
    items: [
      {
        id: 'contract-1',
        companyId: 'company-1',
        contractNumber: 'CTR-001',
        startDate: '2024-01-01',
        endDate: null,
        contractDailyRate: 650,
        currency: 'EUR',
        comment: null,
        status: 0,
        createdAt: '2024-01-01T00:00:00Z',
        createdBy: 'system-seed',
        updatedAt: null,
        updatedBy: null,
      },
    ],
    page: 1,
    pageSize: 50,
    totalCount: 1,
  })),
}))

vi.mock('../../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => ({
    userId: 'user-1',
    identifiant: 's636140',
    expiresAt: '2026-01-01T00:00:00Z',
    isPersistent: false,
  })),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => ({
    items: [
      {
        id: 'user-1',
        nom: 'Bernard',
        prenom: 'Alexandre',
        identifiant: 's636140',
        email: 'a@safran.com',
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
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => ({
    items: [
      {
        id: 'perm-financial',
        code: 'FINANCIAL_DATA_VIEW',
        libelle: 'Données financières',
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

function renderPage() {
  render(
    <DemoTestProviders>
      <MemoryRouter initialEntries={['/societes/company-1']}>
        <Routes>
          <Route path="/societes/:id" element={<CompanyDetailPage />} />
        </Routes>
      </MemoryRouter>
    </DemoTestProviders>,
  )
}

/** L'historique des contrats est confidentiel (§12.4) : ce test vérifie que le composant respecte
 * la garde de permission côté client (adaptation d'affichage uniquement, CLAUDE.md §17) — la
 * protection réelle reste le 403 serveur, déjà couvert côté backend (Lot 2 FinancialModelTests). */
describe('CompanyDetailPage', () => {
  it('hides the contract history without FINANCIAL_DATA_VIEW', async () => {
    renderPage()

    expect(await screen.findByText('Externe Conseil')).toBeInTheDocument()
    await waitFor(() =>
      expect(screen.getByText('Donnée financière non accessible.')).toBeInTheDocument(),
    )
    expect(screen.queryByText('CTR-001')).not.toBeInTheDocument()
  })

  it('shows the contract history once the current identity holds FINANCIAL_DATA_VIEW', async () => {
    setStoredIdentifiant('s636140')
    renderPage()

    expect(await screen.findByText('CTR-001')).toBeInTheDocument()
  })
})
