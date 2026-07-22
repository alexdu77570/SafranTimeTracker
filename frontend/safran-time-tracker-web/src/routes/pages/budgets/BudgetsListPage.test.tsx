import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { DemoTestProviders } from '../../../test/testUtils'
import { BudgetsListPage } from './BudgetsListPage'

const { fetchDashboard, fetchFinancialReport } = vi.hoisted(() => ({
  fetchDashboard: vi.fn(),
  fetchFinancialReport: vi.fn(),
}))
vi.mock('../../../api/endpoints/reporting', () => ({ fetchDashboard, fetchFinancialReport }))

const { fetchBudgets, createBudget, closeBudget, reactivateBudget, fetchBudgetVersions } =
  vi.hoisted(() => ({
    fetchBudgets: vi.fn(),
    createBudget: vi.fn(),
    closeBudget: vi.fn(),
    reactivateBudget: vi.fn(),
    fetchBudgetVersions: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  }))
vi.mock('../../../api/endpoints/budgets', () => ({
  fetchBudgets,
  createBudget,
  closeBudget,
  reactivateBudget,
  fetchBudgetVersions,
}))

vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({
    items: [{ id: 'project-1', nom: 'Migration ELM' }],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/orders', () => ({
  fetchOrders: vi.fn(async () => ({
    items: [{ id: 'order-1', reference: 'CMD-ELM-2026' }],
    page: 1,
    pageSize: 100,
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

const dashboard = {
  periodFrom: '2024-01-01',
  periodTo: '2026-07-21',
  operational: {
    tempsSaisisHeures: 0,
    capaciteTheorique: 0,
    capaciteReelle: 0,
    tauxDisponibilite: 0,
    chargeRunHeures: 0,
    chargeHorsRunHeures: 0,
    incidentsOuverts: 0,
    changesEnCours: 0,
    problemsOuverts: 0,
    ritmEnCours: 0,
    projetsActifs: 5,
    jalonsEnRetard: 1,
    ressourcesSurchargees: 0,
    ressourcesSousChargees: 0,
  },
  financial: {
    budgetInitialTotal: 150000,
    budgetAjusteTotal: 180000,
    coutReelTotal: 700,
    coutContractuelTotal: 750,
    differentielGlobal: 50,
    budgetRestant: 179300,
    commandesARisque: 1,
    projetsSousFinances: 0,
    atterrissageEstime: 700,
  },
}

const financialReport = {
  periodFrom: '2024-01-01',
  periodTo: '2026-07-21',
  differentielGlobal: 50,
  budgetRestant: 179300,
  atterrissageEstime: 700,
  differentielParProjet: [
    {
      id: 'project-1',
      nom: 'Migration ELM',
      coutReel: 700,
      coutContractuel: 750,
      differentiel: 50,
    },
  ],
  differentielParCommande: [],
  differentielParSociete: [],
  differentielParRessource: [],
  consommationMensuelle: [
    { annee: 2026, mois: 6, coutReel: 700, coutContractuel: 750, differentiel: 50 },
  ],
  besoinsRallonge: [],
  commandesARenouveler: [],
  sourcesMontants: [],
}

const budget = {
  id: 'budget-1',
  name: 'Budget ELM',
  projectId: 'project-1',
  orderId: null,
  initialAmount: 150000,
  adjustedAmount: 180000,
  status: 0,
  alertThreshold: 90,
  startDate: '2026-01-01',
  endDate: null,
  comment: null,
  coutReelConsomme: 700,
  coutContractuelConsomme: 750,
  differentiel: 50,
  montantRestant: 179300,
  atterrissageEstime: 700,
  risqueDepassement: false,
}

function pagedResult<T>(items: T[]) {
  return { items, page: 1, pageSize: 100, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <BudgetsListPage />
    </DemoTestProviders>,
  )
}

describe('BudgetsListPage', () => {
  it('shows the financial indicators and the monthly consumption chart', async () => {
    setStoredIdentifiant('s636140')
    fetchDashboard.mockResolvedValue(dashboard)
    fetchFinancialReport.mockResolvedValue(financialReport)
    fetchBudgets.mockResolvedValue(pagedResult([budget]))

    renderPage()

    expect(await screen.findByText('Budget initial total')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('180000 €')).toBeInTheDocument())
    expect(screen.getByText('Consommation mensuelle')).toBeInTheDocument()
  })

  it('hides financial data without FINANCIAL_DATA_VIEW', async () => {
    fetchBudgets.mockResolvedValue(pagedResult([]))

    renderPage()

    expect(await screen.findByText('Donnée financière non accessible.')).toBeInTheDocument()
    expect(screen.queryByText('Budget initial total')).not.toBeInTheDocument()
  })

  it('displays a budget line and closes it', async () => {
    setStoredIdentifiant('s636140')
    fetchDashboard.mockResolvedValue(dashboard)
    fetchFinancialReport.mockResolvedValue(financialReport)
    fetchBudgets.mockResolvedValue(pagedResult([budget]))
    closeBudget.mockResolvedValue({ ...budget, status: 1 })
    const user = userEvent.setup()

    renderPage()
    expect(await screen.findByText('Budget ELM')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Clôturer' }))

    await waitFor(() => expect(closeBudget).toHaveBeenCalledWith('budget-1'))
  })

  it('creates a budget from the create form and refreshes the list', async () => {
    setStoredIdentifiant('s636140')
    fetchDashboard.mockResolvedValue(dashboard)
    fetchFinancialReport.mockResolvedValue(financialReport)
    fetchBudgets.mockResolvedValue(pagedResult([]))
    createBudget.mockResolvedValue(budget)
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchBudgets).toHaveBeenCalled())

    await user.click(await screen.findByRole('button', { name: 'Créer un budget' }))
    const dialog = await screen.findByRole('dialog')

    await user.type(within(dialog).getByLabelText('Nom'), 'Nouveau budget')
    await user.type(within(dialog).getByLabelText('Montant initial (€)'), '50000')
    await user.type(within(dialog).getByLabelText('Date de début'), '2026-01-01')

    await user.click(within(dialog).getByRole('button', { name: 'Créer' }))

    await waitFor(() => expect(createBudget).toHaveBeenCalled())
  })
})
