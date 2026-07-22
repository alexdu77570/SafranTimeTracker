import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { DemoTestProviders } from '../../../test/testUtils'
import { OrderDetailPage } from './OrderDetailPage'

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
  statusId: 'status-brouillon',
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

const {
  fetchOrderById,
  activateOrder,
  suspendOrder,
  markOrderConsumed,
  closeOrder,
  reopenOrder,
  updateOrder,
} = vi.hoisted(() => ({
  fetchOrderById: vi.fn(),
  activateOrder: vi.fn(),
  suspendOrder: vi.fn(),
  markOrderConsumed: vi.fn(),
  closeOrder: vi.fn(),
  reopenOrder: vi.fn(),
  updateOrder: vi.fn(),
}))
vi.mock('../../../api/endpoints/orders', () => ({
  fetchOrderById,
  activateOrder,
  suspendOrder,
  markOrderConsumed,
  closeOrder,
  reopenOrder,
  updateOrder,
}))

const { fetchOrderExtensions, createOrderExtension } = vi.hoisted(() => ({
  fetchOrderExtensions: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  createOrderExtension: vi.fn(),
}))
vi.mock('../../../api/endpoints/orderExtensions', () => ({
  fetchOrderExtensions,
  createOrderExtension,
}))

const { fetchOrderReceipts, fetchOrderReceiptSummary, createOrderReceipt } = vi.hoisted(() => ({
  fetchOrderReceipts: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  fetchOrderReceiptSummary: vi.fn(async () => ({
    totalReceivedAmount: 0,
    totalReceivedDays: 0,
    remainingReceivableAmount: 120000,
    remainingReceivableDays: null,
  })),
  createOrderReceipt: vi.fn(),
}))
vi.mock('../../../api/endpoints/orderReceipts', () => ({
  fetchOrderReceipts,
  fetchOrderReceiptSummary,
  createOrderReceipt,
}))

vi.mock('../../../api/endpoints/orderStatuses', () => ({
  fetchOrderStatuses: vi.fn(async () => ({
    items: [
      { id: 'status-brouillon', code: 'BROUILLON', libelle: 'Brouillon', ordre: 1 },
      { id: 'status-active', code: 'ACTIVE', libelle: 'Active', ordre: 2 },
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
vi.mock('../../../api/endpoints/budgets', () => ({
  fetchBudgets: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/timeEntries', () => ({
  fetchTimeEntries: vi.fn(async () => ({ items: [], page: 1, pageSize: 200, totalCount: 0 })),
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
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <MemoryRouter initialEntries={['/order-1']}>
        <Routes>
          <Route path="/:id" element={<OrderDetailPage />} />
        </Routes>
      </MemoryRouter>
    </DemoTestProviders>,
  )
}

describe('OrderDetailPage', () => {
  it('displays the order synthesis with its financial summary', async () => {
    setStoredIdentifiant('s636140')
    fetchOrderById.mockResolvedValue(order)

    renderPage()

    expect(await screen.findByText('CMD-ELM-2026')).toBeInTheDocument()
    expect(screen.getByText('Migration IBM ELM')).toBeInTheDocument()
    const differentielRow = (await screen.findByText('Différentiel')).closest('tr')
    if (!differentielRow) {
      throw new Error('Différentiel row not found')
    }
    await waitFor(() =>
      expect(within(differentielRow).getByText(/^500,00\s?€$/)).toBeInTheDocument(),
    )
  })

  it('hides financial data without FINANCIAL_DATA_VIEW', async () => {
    fetchOrderById.mockResolvedValue(order)

    renderPage()

    expect(await screen.findByText('CMD-ELM-2026')).toBeInTheDocument()
    expect(screen.getByText('Donnée financière non accessible.')).toBeInTheDocument()
  })

  it('activates a Brouillon order from the header action', async () => {
    setStoredIdentifiant('s636140')
    fetchOrderById.mockResolvedValue(order)
    activateOrder.mockResolvedValue({ ...order, statusId: 'status-active' })
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('CMD-ELM-2026')

    await user.click(screen.getByRole('button', { name: 'Activer' }))

    await waitFor(() => expect(activateOrder).toHaveBeenCalledWith('order-1'))
  })

  it('creates an extension from the Rallonges tab', async () => {
    setStoredIdentifiant('s636140')
    fetchOrderById.mockResolvedValue(order)
    createOrderExtension.mockResolvedValue({})
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('CMD-ELM-2026')
    await user.click(screen.getByRole('tab', { name: 'Rallonges' }))
    await user.click(screen.getByRole('button', { name: 'Créer une rallonge' }))

    const dialog = await screen.findByRole('dialog')
    await user.type(within(dialog).getByLabelText('Montant ajouté (€)'), '5000')
    await user.type(within(dialog).getByLabelText('Nouvelle date de fin'), '2027-03-31')
    await user.type(within(dialog).getByLabelText('Motif'), 'Extension de périmètre (test).')
    await user.click(within(dialog).getByRole('button', { name: 'Créer la rallonge' }))

    await waitFor(() =>
      expect(createOrderExtension).toHaveBeenCalledWith(
        'order-1',
        expect.objectContaining({ amountAdded: 5000 }),
      ),
    )
  })

  it('shows the receipt summary on the Réceptions tab', async () => {
    setStoredIdentifiant('s636140')
    fetchOrderById.mockResolvedValue(order)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('CMD-ELM-2026')
    await user.click(screen.getByRole('tab', { name: 'Réceptions' }))

    expect(await screen.findByText('Reste réceptionnable (€)')).toBeInTheDocument()
    expect(screen.getByText('Total réceptionné (€)')).toBeInTheDocument()
    expect(screen.getByText('0 €')).toBeInTheDocument()
  })
})
