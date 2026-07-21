import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { ProjectsListPage } from './ProjectsListPage'

const { fetchProjects, createProject } = vi.hoisted(() => ({
  fetchProjects: vi.fn(),
  createProject: vi.fn(),
}))
vi.mock('../../../api/endpoints/projects', () => ({ fetchProjects, createProject }))

vi.mock('../../../api/endpoints/projectStatuses', () => ({
  fetchProjectStatuses: vi.fn(async () => ({
    items: [
      { id: 'status-actif', code: 'ACTIF', libelle: 'Actif', ordre: 1 },
      { id: 'status-archive', code: 'ARCHIVE', libelle: 'Archivé', ordre: 4 },
    ],
    page: 1,
    pageSize: 50,
    totalCount: 2,
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
        departmentId: 'd',
        serviceId: 's',
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
  fetchProjectTypes: vi.fn(async () => ({
    items: [{ id: 'type-1', code: 'FORFAIT', libelle: 'Forfait', statut: 0 }],
    page: 1,
    pageSize: 50,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/clients', () => ({
  fetchClients: vi.fn(async () => ({ items: [], page: 1, pageSize: 50, totalCount: 0 })),
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

const project = {
  id: 'project-1',
  nom: 'Migration ELM',
  code: 'PRJ-ELM-2026',
  applicationId: 'app-1',
  descriptionCourte: null,
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

function pagedResult(items: (typeof project)[]) {
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
          <Route path="/" element={<ProjectsListPage />} />
        </Routes>
      </MemoryRouter>
    </DemoTestProviders>,
  )
}

describe('ProjectsListPage', () => {
  it('displays the seeded projects with their status and budget', async () => {
    setStoredIdentifiant('s636140')
    fetchProjects.mockResolvedValue(pagedResult([project]))

    renderPage()

    expect(await screen.findByText('Migration ELM')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Actif')).toBeInTheDocument())
    await waitFor(() => expect(screen.getByText(/150\s?000,00\s?€/)).toBeInTheDocument())
  })

  it('re-queries with the selected statut and niveau de risque filters', async () => {
    setStoredIdentifiant('s636140')
    fetchProjects.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchProjects).toHaveBeenCalled())
    fetchProjects.mockClear()

    await user.click(screen.getByLabelText('Statut'))
    await user.click(await screen.findByRole('option', { name: 'Actif' }))
    await waitFor(() =>
      expect(fetchProjects).toHaveBeenCalledWith(
        expect.objectContaining({ statusId: 'status-actif' }),
      ),
    )

    await user.click(screen.getByLabelText('Niveau de risque'))
    await user.click(await screen.findByRole('option', { name: 'Élevé' }))
    await waitFor(() =>
      expect(fetchProjects).toHaveBeenCalledWith(expect.objectContaining({ niveauRisque: 2 })),
    )
  })

  it('re-queries with the alerte planning and alerte budget filters', async () => {
    setStoredIdentifiant('s636140')
    fetchProjects.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchProjects).toHaveBeenCalled())
    fetchProjects.mockClear()

    await user.click(screen.getByLabelText('Alerte planning'))
    await user.click(await screen.findByRole('option', { name: 'Avec alerte' }))
    await waitFor(() =>
      expect(fetchProjects).toHaveBeenCalledWith(expect.objectContaining({ alertePlanning: true })),
    )

    await user.click(screen.getByLabelText('Alerte budget'))
    await user.click(await screen.findByRole('option', { name: 'Sans alerte' }))
    await waitFor(() =>
      expect(fetchProjects).toHaveBeenCalledWith(expect.objectContaining({ alerteBudget: false })),
    )
  })

  it('creates a project from the create form and refreshes the list', async () => {
    setStoredIdentifiant('s636140')
    fetchProjects.mockResolvedValue(pagedResult([]))
    createProject.mockResolvedValue(project)
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchProjects).toHaveBeenCalled())

    await user.click(screen.getByRole('button', { name: 'Créer un projet' }))
    const dialog = await screen.findByRole('dialog')

    await user.type(within(dialog).getByLabelText('Nom'), 'Nouveau projet')
    await user.type(within(dialog).getByLabelText('Code'), 'PRJ-TEST-2026')
    await user.click(within(dialog).getByLabelText('Application'))
    await user.click(await screen.findByRole('option', { name: 'IBM ELM' }))
    await user.click(within(dialog).getByLabelText('Pilote'))
    await user.click(await screen.findByRole('option', { name: 'Alexandre BERNARD' }))
    await user.click(within(dialog).getByLabelText('Département'))
    await user.click(await screen.findByRole('option', { name: 'DSI' }))
    await user.click(within(dialog).getByLabelText('Service'))
    await user.click(await screen.findByRole('option', { name: 'Production applicative' }))
    await user.type(within(dialog).getByLabelText('Date de début'), '2026-01-01')
    await user.type(within(dialog).getByLabelText('Date de fin prévue initiale'), '2026-12-31')

    fetchProjects.mockClear()
    await user.click(within(dialog).getByRole('button', { name: 'Créer' }))

    await waitFor(() => expect(createProject).toHaveBeenCalled())
    await waitFor(() => expect(fetchProjects).toHaveBeenCalled())
  })
})
