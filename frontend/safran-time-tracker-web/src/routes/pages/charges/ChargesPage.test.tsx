import { render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TestProviders } from '../../../test/testUtils'
import { ChargesPage } from './ChargesPage'

const { fetchCharges, fetchDashboard } = vi.hoisted(() => ({
  fetchCharges: vi.fn(),
  fetchDashboard: vi.fn(),
}))
vi.mock('../../../api/endpoints/reporting', () => ({ fetchCharges, fetchDashboard }))

vi.mock('../../../api/endpoints/applications', () => ({
  fetchApplications: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/orders', () => ({
  fetchOrders: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/organisation', () => ({
  fetchDepartments: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  fetchServices: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  fetchTeams: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/resources', () => ({
  fetchResources: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/activityTypes', () => ({
  fetchActivityTypes: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))

const charges = {
  periodFrom: '2024-01-01',
  periodTo: '2026-07-21',
  chargeTotaleHeures: 100,
  chargeRunHeures: 60,
  chargeHorsRunHeures: 40,
  nombreIncidents: 3,
  nombreChanges: 2,
  nombreProblems: 1,
  nombreRitm: 4,
  nombreVabe: 0,
  nombreVsr: 0,
  topApplications: [{ id: 'app-1', nom: 'IBM ELM', chargeHeures: 50 }],
  topUtilisateurs: [{ id: 'res-1', nom: 'Alexandre BERNARD', chargeHeures: 40 }],
  topProjets: [{ id: 'proj-1', nom: 'Migration ELM', chargeHeures: 30 }],
  topCommandes: [{ id: 'ord-1', nom: 'CMD-ELM-2026', chargeHeures: 20 }],
  ressourcesSurchargees: [
    { resourceId: 'res-1', nom: 'Alexandre BERNARD', chargeHeures: 40, capaciteReelle: 35 },
  ],
  ressourcesSousChargees: [],
  evolutionMensuelle: [
    { annee: 2026, mois: 6, chargeTotaleHeures: 100, chargeRunHeures: 60, chargeHorsRunHeures: 40 },
  ],
  heatmap: [
    {
      resourceId: 'res-1',
      nom: 'Alexandre BERNARD',
      weekStartDate: '2026-06-01',
      chargeHeures: 40,
    },
  ],
  prevuVsRealise: { chargePrevue: 90, chargeRealisee: 100 },
}

const dashboard = {
  periodFrom: '2024-01-01',
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
  financial: null,
}

function renderPage() {
  render(
    <TestProviders>
      <ChargesPage />
    </TestProviders>,
  )
}

describe('ChargesPage', () => {
  it('displays the indicators and top lists from the seeded report', async () => {
    fetchCharges.mockResolvedValue(charges)
    fetchDashboard.mockResolvedValue(dashboard)

    renderPage()

    expect(await screen.findByText('Charge totale')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('100 h')).toBeInTheDocument())
    expect(screen.getByText('IBM ELM')).toBeInTheDocument()
    expect(screen.getByText('Migration ELM')).toBeInTheDocument()
  })

  it('shows the heatmap and monthly evolution chart cards', async () => {
    fetchCharges.mockResolvedValue(charges)
    fetchDashboard.mockResolvedValue(dashboard)

    renderPage()

    expect(await screen.findByText('Heatmap de charge')).toBeInTheDocument()
    expect(screen.getByText('Courbe mensuelle')).toBeInTheDocument()
    expect(screen.getAllByText('Alexandre BERNARD').length).toBeGreaterThan(0)
  })

  it('shows a non-computable message when the plan comparison is null', async () => {
    fetchCharges.mockResolvedValue({
      ...charges,
      prevuVsRealise: { chargePrevue: null, chargeRealisee: 100 },
    })
    fetchDashboard.mockResolvedValue(dashboard)

    renderPage()

    expect(await screen.findByText('Non calculable')).toBeInTheDocument()
  })
})
