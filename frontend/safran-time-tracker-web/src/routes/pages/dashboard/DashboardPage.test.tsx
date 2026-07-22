import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { DemoTestProviders } from '../../../test/testUtils'
import { DashboardPage } from './DashboardPage'

const { fetchDashboard, fetchCharges, fetchFinancialReport } = vi.hoisted(() => ({
  fetchDashboard: vi.fn(),
  fetchCharges: vi.fn(),
  fetchFinancialReport: vi.fn(),
}))
vi.mock('../../../api/endpoints/reporting', () => ({
  fetchDashboard,
  fetchCharges,
  fetchFinancialReport,
}))

vi.mock('../../../api/endpoints/milestones', () => ({
  fetchMilestones: vi.fn(async () => ({ items: [], page: 1, pageSize: 200, totalCount: 0 })),
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

const dashboard = {
  periodFrom: '2026-07-01',
  periodTo: '2026-07-21',
  operational: {
    tempsSaisisHeures: 100,
    capaciteTheorique: 120,
    capaciteReelle: 110,
    tauxDisponibilite: 91.6,
    chargeRunHeures: 60,
    chargeHorsRunHeures: 40,
    incidentsOuverts: 3,
    changesEnCours: 2,
    problemsOuverts: 1,
    ritmEnCours: 4,
    projetsActifs: 5,
    jalonsEnRetard: 1,
    ressourcesSurchargees: 1,
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

const charges = {
  periodFrom: '2026-07-01',
  periodTo: '2026-07-21',
  chargeTotaleHeures: 100,
  chargeRunHeures: 60,
  chargeHorsRunHeures: 40,
  nombreIncidents: 0,
  nombreChanges: 0,
  nombreProblems: 0,
  nombreRitm: 0,
  nombreVabe: 0,
  nombreVsr: 0,
  topApplications: [],
  topUtilisateurs: [],
  topProjets: [],
  topCommandes: [],
  ressourcesSurchargees: [],
  ressourcesSousChargees: [],
  evolutionMensuelle: [],
  heatmap: [],
  prevuVsRealise: { chargePrevue: 90, chargeRealisee: 100 },
}

const financialReport = {
  periodFrom: '2026-07-01',
  periodTo: '2026-07-21',
  differentielGlobal: 50,
  budgetRestant: 179300,
  atterrissageEstime: 700,
  differentielParProjet: [],
  differentielParCommande: [],
  differentielParSociete: [],
  differentielParRessource: [],
  consommationMensuelle: [],
  besoinsRallonge: [],
  commandesARenouveler: [],
  sourcesMontants: [],
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <DashboardPage />
    </DemoTestProviders>,
  )
}

describe('DashboardPage', () => {
  it('always shows operational KPIs', async () => {
    fetchDashboard.mockResolvedValue({ ...dashboard, financial: null })
    fetchCharges.mockResolvedValue(charges)
    fetchFinancialReport.mockResolvedValue(null)

    renderPage()

    expect(await screen.findByText('Temps saisis')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('100 h')).toBeInTheDocument())
    expect(screen.queryByText('Budget initial total')).not.toBeInTheDocument()
  })

  it('shows financial KPIs once the current identity holds FINANCIAL_DATA_VIEW', async () => {
    setStoredIdentifiant('s636140')
    fetchDashboard.mockResolvedValue(dashboard)
    fetchCharges.mockResolvedValue(charges)
    fetchFinancialReport.mockResolvedValue(financialReport)

    renderPage()

    expect(await screen.findByText('Budget initial total')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('180000 €')).toBeInTheDocument())
  })
})
