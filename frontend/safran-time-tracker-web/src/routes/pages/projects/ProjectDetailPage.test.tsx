import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { demoPermissionFixture, demoSessionFixture, demoUserFixture, pagedResult as sharedPagedResult } from '../../../test/fixtures'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { ProjectDetailPage } from './ProjectDetailPage'

const project = {
  id: 'project-1',
  nom: 'Migration ELM',
  code: 'PRJ-ELM-2026',
  applicationId: 'app-1',
  descriptionCourte: 'Migration de la plateforme.',
  piloteId: 'resource-1',
  departmentId: 'dept-1',
  serviceId: 'service-1',
  teamId: null,
  statusId: 'status-actif',
  projectTypeId: null,
  clientId: null,
  dateDebut: '2026-01-01',
  dateFinPrevueInitiale: '2026-12-31',
  dateFinAjustee: null,
  dateFinReelle: null,
  niveauRisque: 1,
  commentaire: null,
  financialSummary: {
    budgetInitial: 150000,
    coutReelConsomme: 700,
    coutContractuelConsomme: 750,
    differentiel: 50,
    budgetRestant: 149300,
  },
}

const { fetchProjectById, updateProject } = vi.hoisted(() => ({
  fetchProjectById: vi.fn(),
  updateProject: vi.fn(),
}))
vi.mock('../../../api/endpoints/projects', () => ({ fetchProjectById, updateProject }))

vi.mock('../../../api/endpoints/projectStatuses', () => ({
  fetchProjectStatuses: vi.fn(async () => ({
    items: [{ id: 'status-actif', code: 'ACTIF', libelle: 'Actif', ordre: 1 }],
    page: 1,
    pageSize: 50,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/applications', () => ({
  fetchApplications: vi.fn(async () => ({
    items: [
      {
        id: 'app-1',
        nom: 'IBM ELM',
        code: 'ELM',
        serviceId: 's',
        teamId: null,
        criticite: 0,
        responsableId: null,
        statut: 0,
        commentaire: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/resources', () => ({
  fetchResources: vi.fn(async () => ({
    items: [
      {
        id: 'resource-1',
        nom: 'BERNARD',
        prenom: 'Alexandre',
        departmentId: 'dept-1',
        serviceId: 'service-1',
        teamId: null,
        responsableHierarchiqueId: null,
        companyId: null,
        defaultOrderId: null,
        dailyCapacity: 7.75,
        weeklyCapacity: 38.75,
        statut: 0,
        commentaire: null,
        operationalRoleIds: [],
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/organisation', () => ({
  fetchDepartments: vi.fn(async () => ({
    items: [
      { id: 'dept-1', code: 'DSI', nom: 'DSI', responsableId: null, statut: 0, commentaire: null },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
  fetchServices: vi.fn(async () => ({
    items: [
      {
        id: 'service-1',
        code: 'PROD',
        nom: 'Production applicative',
        departmentId: 'dept-1',
        responsableId: null,
        statut: 0,
        commentaire: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
  fetchTeams: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/projectTypes', () => ({
  fetchProjectTypes: vi.fn(async () => ({ items: [], page: 1, pageSize: 50, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/clients', () => ({
  fetchClients: vi.fn(async () => ({ items: [], page: 1, pageSize: 50, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/orders', () => ({
  fetchOrders: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/milestoneTypes', () => ({
  fetchMilestoneTypes: vi.fn(async () => ({
    items: [{ id: 'mtype-1', code: 'KICKOFF', libelle: 'Kick-off', statut: 0 }],
    page: 1,
    pageSize: 50,
    totalCount: 1,
  })),
}))

const { fetchProjectPlanningSynthesis } = vi.hoisted(() => ({
  fetchProjectPlanningSynthesis: vi.fn(),
}))
vi.mock('../../../api/endpoints/projectPlanning', () => ({
  fetchProjectPlanningSynthesis,
  fetchProjectPlanVersions: vi.fn(async () => ({
    items: [],
    page: 1,
    pageSize: 100,
    totalCount: 0,
  })),
  fetchWeeklyPlans: vi.fn(async () => []),
  createAdjustedPlanVersion: vi.fn(),
  setWeeklyPlans: vi.fn(),
}))

const { fetchProjectParticipants, removeProjectParticipant } = vi.hoisted(() => ({
  fetchProjectParticipants: vi.fn(),
  removeProjectParticipant: vi.fn(),
}))
vi.mock('../../../api/endpoints/projectParticipants', () => ({
  fetchProjectParticipants,
  removeProjectParticipant,
  createProjectParticipant: vi.fn(),
}))

const { fetchMilestones, createMilestone } = vi.hoisted(() => ({
  fetchMilestones: vi.fn(),
  createMilestone: vi.fn(),
}))
vi.mock('../../../api/endpoints/milestones', () => ({
  fetchMilestones,
  createMilestone,
  updateMilestone: vi.fn(),
}))

const { fetchBudgets } = vi.hoisted(() => ({ fetchBudgets: vi.fn() }))
vi.mock('../../../api/endpoints/budgets', () => ({ fetchBudgets }))

const { fetchTimeEntries } = vi.hoisted(() => ({ fetchTimeEntries: vi.fn() }))
vi.mock('../../../api/endpoints/timeEntries', () => ({ fetchTimeEntries }))

const { fetchProjectLinkedReferences } = vi.hoisted(() => ({
  fetchProjectLinkedReferences: vi.fn(),
}))
vi.mock('../../../api/endpoints/reporting', () => ({ fetchProjectLinkedReferences }))

vi.mock('../../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => sharedPagedResult([demoUserFixture()], 100)),
}))
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => sharedPagedResult([demoPermissionFixture()], 100)),
}))

const synthesis = {
  projectId: 'project-1',
  chargeInitiale: 30,
  chargeAjustee: 32,
  chargeConsommee: 7.75,
  chargeRestante: 24.25,
  ecartCharge: -24.25,
  deriveCharge: 2,
  atterrissageCharge: 32,
  derivePlanningJours: 0,
  risquePlanning: false,
  atterrissageFinancier: 2890,
  risqueBudget: false,
}

function emptyPage<T>(items: T[] = []) {
  return { items, page: 1, pageSize: 100, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <MemoryRouter initialEntries={['/projets/project-1']}>
        <Routes>
          <Route path="/projets/:id" element={<ProjectDetailPage />} />
        </Routes>
      </MemoryRouter>
    </DemoTestProviders>,
  )
}

describe('ProjectDetailPage', () => {
  it('displays the project name and the KPI banner from the planning synthesis', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())

    renderPage()

    expect(await screen.findByText('Migration ELM')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('7.75 h')).toBeInTheDocument())
    // Avancement (%) n'est jamais calculé côté frontend (Décision 6) : affiché "—" à côté de son
    // libellé, parmi d'autres "—" légitimes (champs optionnels non renseignés du projet).
    const avancementLabel = screen.getByText('Avancement')
    expect(
      within(avancementLabel.closest('.MuiCardContent-root') as HTMLElement).getByText('—'),
    ).toBeInTheDocument()
  })

  it('shows the Aucune alert badge when no planning or budget risk is present', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())

    renderPage()

    await screen.findByText('Migration ELM')
    await waitFor(() => expect(screen.getByText('Aucune')).toBeInTheDocument())
  })

  it('navigates to the Participants tab and lists consumed time per participant', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(
      emptyPage([
        {
          id: 'participant-1',
          projectId: 'project-1',
          resourceId: 'resource-1',
          operationalRoleId: null,
          defaultOrderId: null,
          dateDebut: '2026-01-01',
          dateFin: null,
          capacitePrevue: 10,
          statut: 0,
          financialSummary: {
            companyIdApplicable: null,
            tjmPersonneApplicable: 650,
            tjmContratApplicable: null,
          },
        },
      ]),
    )
    fetchMilestones.mockResolvedValue(emptyPage())
    fetchTimeEntries.mockResolvedValue(
      emptyPage([{ id: 'te-1', resourceId: 'resource-1', dureeHeures: 7.75, date: '2026-06-10' }]),
    )
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Migration ELM')

    await user.click(screen.getByRole('tab', { name: 'Participants' }))

    expect(await screen.findByText('Alexandre BERNARD')).toBeInTheDocument()
    const row = screen.getByText('Alexandre BERNARD').closest('tr') as HTMLElement
    expect(within(row).getByText('7.75 h')).toBeInTheDocument()
  })

  it('hides budget data without FINANCIAL_DATA_VIEW', async () => {
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())
    fetchBudgets.mockResolvedValue(emptyPage())
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Migration ELM')
    await user.click(screen.getByRole('tab', { name: 'Budget' }))

    expect(await screen.findByText('Donnée financière non accessible.')).toBeInTheDocument()
    expect(screen.queryByText('Budget ELM')).not.toBeInTheDocument()
  })

  it('shows budget data once the current identity holds FINANCIAL_DATA_VIEW', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())
    fetchBudgets.mockResolvedValue(
      emptyPage([
        {
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
          atterrissageEstime: 2890,
          risqueDepassement: false,
        },
      ]),
    )
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Migration ELM')
    await user.click(screen.getByRole('tab', { name: 'Budget' }))

    expect(await screen.findByText('Budget ELM')).toBeInTheDocument()
  })

  it('creates a milestone from the Jalons tab and refreshes the timeline', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())
    createMilestone.mockResolvedValue({
      id: 'milestone-1',
      nom: 'Kick-off',
      milestoneTypeId: 'mtype-1',
      projectId: 'project-1',
      applicationId: null,
      responsableId: 'resource-1',
      datePrevue: '2026-02-01',
      dateReelle: null,
      statut: 0,
      criticite: 1,
      commentaire: null,
      dependsOnMilestoneId: null,
      estEnRetard: false,
    })
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Migration ELM')
    await user.click(screen.getByRole('tab', { name: 'Jalons' }))

    await user.click(await screen.findByRole('button', { name: 'Créer un jalon' }))
    const dialog = await screen.findByRole('dialog')
    await user.type(within(dialog).getByLabelText('Nom'), 'Kick-off')
    await user.click(within(dialog).getByLabelText('Type de jalon'))
    await user.click(await screen.findByRole('option', { name: 'Kick-off' }))
    await user.click(within(dialog).getByLabelText('Responsable'))
    await user.click(await screen.findByRole('option', { name: 'Alexandre BERNARD' }))
    await user.type(within(dialog).getByLabelText('Date prévue'), '2026-02-01')

    fetchMilestones.mockClear()
    await user.click(within(dialog).getByRole('button', { name: 'Créer' }))

    await waitFor(() => expect(createMilestone).toHaveBeenCalled())
    await waitFor(() => expect(fetchMilestones).toHaveBeenCalled())
  })

  it('opens the project edit form and submits the update', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectById.mockResolvedValue(project)
    fetchProjectPlanningSynthesis.mockResolvedValue(synthesis)
    fetchProjectParticipants.mockResolvedValue(emptyPage())
    fetchMilestones.mockResolvedValue(emptyPage())
    updateProject.mockResolvedValue({ ...project, nom: 'Migration ELM v2' })
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Migration ELM')

    await user.click(screen.getByRole('button', { name: 'Modifier' }))
    const dialog = await screen.findByRole('dialog')
    expect(within(dialog).getByDisplayValue('Migration ELM')).toBeInTheDocument()

    await user.click(within(dialog).getByRole('button', { name: 'Enregistrer' }))

    await waitFor(() =>
      expect(updateProject).toHaveBeenCalledWith(
        'project-1',
        expect.objectContaining({ nom: 'Migration ELM' }),
      ),
    )
  })
})
