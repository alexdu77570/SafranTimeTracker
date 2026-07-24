import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { DemoTestProviders } from '../../../test/testUtils'
import { demoPermissionFixture, demoSessionFixture, demoUserFixture, pagedResult } from '../../../test/fixtures'
import { ReportingPage } from './ReportingPage'

const { fetchOperationalReport, fetchFinancialReport, exportOperational, exportFinancial } =
  vi.hoisted(() => ({
    fetchOperationalReport: vi.fn(),
    fetchFinancialReport: vi.fn(),
    exportOperational: vi.fn(),
    exportFinancial: vi.fn(),
  }))
vi.mock('../../../api/endpoints/reporting', () => ({
  fetchOperationalReport,
  fetchFinancialReport,
  exportOperational,
  exportFinancial,
}))

vi.mock('../../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => pagedResult([demoUserFixture()], 100)),
}))
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => pagedResult([demoPermissionFixture()], 100)),
}))

const operationalReport = {
  periodFrom: '2024-01-01',
  periodTo: '2026-07-21',
  chargeParEquipe: [{ id: 'team-1', nom: 'Équipe Projets A', chargeHeures: 50 }],
  chargeParService: [],
  chargeParDepartement: [],
  consommationParProjet: [],
  consommationParCommande: [],
  jalonsEnRetard: [
    { id: 'm1', nom: 'GO PROD Migration ELM', projectId: 'p1', datePrevue: '2025-06-01' },
  ],
  ressourcesSurchargees: [],
  ressourcesSousUtilisees: [],
  capaciteEtDisponibilite: [
    {
      resourceId: 'r1',
      nom: 'Alexandre BERNARD',
      capaciteTheorique: 120,
      capaciteReelle: 110,
      tauxDisponibilite: 91.6,
    },
  ],
}

const financialReport = {
  periodFrom: '2024-01-01',
  periodTo: '2026-07-21',
  differentielGlobal: 50,
  budgetRestant: 179300,
  atterrissageEstime: 700,
  differentielParProjet: [
    { id: 'p1', nom: 'Migration ELM', coutReel: 700, coutContractuel: 750, differentiel: 50 },
  ],
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
      <ReportingPage />
    </DemoTestProviders>,
  )
}

describe('ReportingPage', () => {
  it('displays the operational report and hides the financial report without permission', async () => {
    fetchOperationalReport.mockResolvedValue(operationalReport)
    fetchFinancialReport.mockResolvedValue(null)

    renderPage()

    expect(await screen.findByText('Équipe Projets A')).toBeInTheDocument()
    expect(screen.getByText('GO PROD Migration ELM')).toBeInTheDocument()
    expect(screen.getByText('Donnée financière non accessible.')).toBeInTheDocument()
  })

  it('displays the financial report once the current identity holds FINANCIAL_DATA_VIEW', async () => {
    setStoredIdentifiant('s636140')
    fetchOperationalReport.mockResolvedValue(operationalReport)
    fetchFinancialReport.mockResolvedValue(financialReport)

    renderPage()

    expect(await screen.findByText('Rapport financier (§26.2)')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Migration ELM')).toBeInTheDocument())
  })

  it('triggers the operational export when clicked', async () => {
    fetchOperationalReport.mockResolvedValue(operationalReport)
    fetchFinancialReport.mockResolvedValue(null)
    exportOperational.mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Équipe Projets A')

    await user.click(screen.getByRole('button', { name: 'Exporter' }))

    await waitFor(() => expect(exportOperational).toHaveBeenCalled())
  })
})
