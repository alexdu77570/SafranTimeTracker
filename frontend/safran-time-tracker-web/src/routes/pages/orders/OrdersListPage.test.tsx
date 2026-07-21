import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { DemoTestProviders } from '../../../test/testUtils'
import { OrdersListPage } from './OrdersListPage'

const { fetchOrders, createOrder } = vi.hoisted(() => ({
  fetchOrders: vi.fn(),
  createOrder: vi.fn(),
}))
vi.mock('../../../api/endpoints/orders', () => ({ fetchOrders, createOrder }))

vi.mock('../../../api/endpoints/orderStatuses', () => ({
  fetchOrderStatuses: vi.fn(async () => ({
    items: [
      { id: 'status-active', code: 'ACTIVE', libelle: 'Active', ordre: 2 },
      { id: 'status-cloturee', code: 'CLOTUREE', libelle: 'Clôturée', ordre: 5 },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 2,
  })),
}))
vi.mock('../../../api/endpoints/companies', () => ({
  fetchCompanies: vi.fn(async () => ({
    items: [
      {
        id: 'company-1',
        nom: 'IBM France',
        code: 'IBM',
        companyTypeId: 'type-externe',
        statut: 0,
        contactPrincipal: 'Contact',
        emailContact: 'contact@ibm.fr',
        telephone: null,
        adresse: null,
        commentaire: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({
    items: [{ id: 'project-1', nom: 'Migration ELM' }],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/resources', () => ({
  fetchResources: vi.fn(async () => ({
    items: [{ id: 'resource-1', nom: 'BERNARD', prenom: 'Alexandre' }],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => ({
    items: [
      {
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

const order = {
  id: 'order-1',
  reference: 'CMD-ELM-2026',
  libelle: 'Migration IBM ELM',
  companyId: 'company-1',
  projectId: 'project-1',
  budgetFinancierInitial: 100000,
  budgetFinancierAjuste: 120000,
  budgetJoursInitial: null,
  budgetJoursAjuste: null,
  dateDebut: '2026-01-01',
  dateFinInitiale: '2026-12-31',
  dateFinAjustee: null,
  statusId: 'status-active',
  seuilAlerte: null,
  commentaire: null,
  authorizedResourceIds: [],
  financialSummary: {
    consommationJours: 10,
    coutReelConsomme: 6500,
    coutContractuelConsomme: 7000,
    differentiel: 500,
    restFinancier: 113500,
    restJours: null,
  },
}

function pagedResult(items: (typeof order)[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <MemoryRouter>
        <Routes>
          <Route path="/" element={<OrdersListPage />} />
        </Routes>
      </MemoryRouter>
    </DemoTestProviders>,
  )
}

describe('OrdersListPage', () => {
  it('displays the seeded orders with their status and financial differential', async () => {
    setStoredIdentifiant('s636140')
    fetchOrders.mockResolvedValue(pagedResult([order]))

    renderPage()

    expect(await screen.findByText('CMD-ELM-2026')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Active')).toBeInTheDocument())
    await waitFor(() => expect(screen.getByText(/500,00\s?€/)).toBeInTheDocument())
  })

  it('re-queries with the selected company and statut filters', async () => {
    setStoredIdentifiant('s636140')
    fetchOrders.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchOrders).toHaveBeenCalled())
    fetchOrders.mockClear()

    await user.click(screen.getByLabelText('Société'))
    await user.click(await screen.findByRole('option', { name: 'IBM France' }))
    await waitFor(() =>
      expect(fetchOrders).toHaveBeenCalledWith(expect.objectContaining({ companyId: 'company-1' })),
    )

    await user.click(screen.getByLabelText('Statut'))
    await user.click(await screen.findByRole('option', { name: 'Clôturée' }))
    await waitFor(() =>
      expect(fetchOrders).toHaveBeenCalledWith(
        expect.objectContaining({ statusId: 'status-cloturee' }),
      ),
    )
  })

  it('creates an order from the create form and refreshes the list', async () => {
    setStoredIdentifiant('s636140')
    fetchOrders.mockResolvedValue(pagedResult([]))
    createOrder.mockResolvedValue(order)
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchOrders).toHaveBeenCalled())

    await user.click(screen.getByRole('button', { name: 'Créer une commande' }))
    const dialog = await screen.findByRole('dialog')

    await user.type(within(dialog).getByLabelText('Référence'), 'CMD-TEST-2026')
    await user.type(within(dialog).getByLabelText('Libellé'), 'Commande de test')
    await user.click(within(dialog).getByLabelText('Société'))
    await user.click(await screen.findByRole('option', { name: 'IBM France' }))
    await user.type(within(dialog).getByLabelText('Budget financier initial (€)'), '10000')
    await user.type(within(dialog).getByLabelText('Date de début'), '2026-01-01')
    await user.type(within(dialog).getByLabelText('Date de fin initiale'), '2026-12-31')

    fetchOrders.mockClear()
    await user.click(within(dialog).getByRole('button', { name: 'Créer' }))

    await waitFor(() => expect(createOrder).toHaveBeenCalled())
    await waitFor(() => expect(fetchOrders).toHaveBeenCalled())
  })
})
