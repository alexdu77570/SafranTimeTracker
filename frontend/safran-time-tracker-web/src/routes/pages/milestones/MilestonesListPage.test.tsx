import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import dayjs from 'dayjs'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { MilestonesListPage } from './MilestonesListPage'

const { fetchMilestones, createMilestone, updateMilestone } = vi.hoisted(() => ({
  fetchMilestones: vi.fn(),
  createMilestone: vi.fn(),
  updateMilestone: vi.fn(),
}))
vi.mock('../../../api/endpoints/milestones', () => ({
  fetchMilestones,
  createMilestone,
  updateMilestone,
}))

vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({
    items: [
      {
        id: 'project-1',
        nom: 'Migration ELM',
        dateFinPrevueInitiale: '2026-12-31',
        dateFinAjustee: '2027-03-31',
      },
      {
        id: 'project-2',
        nom: 'Refonte VTOM',
        dateFinPrevueInitiale: '2026-06-30',
        dateFinAjustee: null,
      },
    ],
    page: 1,
    pageSize: 100,
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
    items: [{ id: 'resource-1', nom: 'GEORGES', prenom: 'Thierry' }],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/milestoneTypes', () => ({
  fetchMilestoneTypes: vi.fn(async () => ({
    items: [{ id: 'type-1', code: 'KICKOFF', libelle: 'Kick-off', statut: 0 }],
    page: 1,
    pageSize: 50,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))

const today = dayjs()
const milestoneElm = {
  id: 'milestone-1',
  nom: 'GO PROD Migration ELM',
  milestoneTypeId: 'type-1',
  projectId: 'project-1',
  applicationId: 'app-1',
  responsableId: 'resource-1',
  datePrevue: today.add(10, 'day').format('YYYY-MM-DD'),
  dateReelle: null,
  statut: 1,
  criticite: 3,
  commentaire: null,
  dependsOnMilestoneId: null,
  estEnRetard: false,
}
const milestoneVtom = {
  id: 'milestone-2',
  nom: 'Kick-off Refonte VTOM',
  milestoneTypeId: 'type-1',
  projectId: 'project-2',
  applicationId: null,
  responsableId: 'resource-1',
  datePrevue: '2024-02-05',
  dateReelle: '2024-02-05',
  statut: 2,
  criticite: 1,
  commentaire: null,
  dependsOnMilestoneId: null,
  estEnRetard: false,
}

function pagedResult<T>(items: T[]) {
  return { items, page: 1, pageSize: 200, totalCount: items.length }
}

afterEach(() => {
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <MilestonesListPage />
    </DemoTestProviders>,
  )
}

describe('MilestonesListPage', () => {
  it('displays the milestones in table view with the planning risk badge and the 30-day counter', async () => {
    fetchMilestones.mockResolvedValue(pagedResult([milestoneElm, milestoneVtom]))

    renderPage()

    expect(await screen.findByText('GO PROD Migration ELM')).toBeInTheDocument()
    expect(screen.getByText('Kick-off Refonte VTOM')).toBeInTheDocument()
    // Migration ELM (dateFinAjustee > dateFinPrevueInitiale) porte l'alerte planning projet.
    expect(screen.getByText('Risque planning')).toBeInTheDocument()
    // Un seul jalon (GO PROD, dans 10 jours, statut En cours) tombe dans la fenêtre des 30 jours.
    const upcomingCard = screen.getByText('À venir sous 30 jours').closest('div')
    if (!upcomingCard?.parentElement) {
      throw new Error('KPI card not found')
    }
    expect(within(upcomingCard.parentElement).getByText('1')).toBeInTheDocument()
  })

  it('re-queries with the selected application filter', async () => {
    fetchMilestones.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchMilestones).toHaveBeenCalled())
    fetchMilestones.mockClear()

    await user.click(screen.getByLabelText('Application'))
    await user.click(await screen.findByRole('option', { name: 'IBM ELM' }))

    await waitFor(() =>
      expect(fetchMilestones).toHaveBeenCalledWith(
        expect.objectContaining({ applicationId: 'app-1' }),
      ),
    )
  })

  it('switches to the timeline view', async () => {
    fetchMilestones.mockResolvedValue(pagedResult([milestoneElm]))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('GO PROD Migration ELM')

    await user.click(screen.getByRole('button', { name: 'Timeline' }))

    expect(await screen.findByText('GO PROD Migration ELM')).toBeInTheDocument()
    expect(screen.getByText(/Migration ELM.*Thierry GEORGES/)).toBeInTheDocument()
  })

  it('switches to the calendar view', async () => {
    fetchMilestones.mockResolvedValue(pagedResult([milestoneVtom]))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Kick-off Refonte VTOM')

    await user.click(screen.getByRole('button', { name: 'Calendrier' }))
    const monthInput = screen.getByLabelText('Mois')
    await user.clear(monthInput)
    await user.type(monthInput, '2024-02')

    expect(await screen.findByText('Lun')).toBeInTheDocument()
  })

  it('creates a milestone after picking a project first', async () => {
    fetchMilestones.mockResolvedValue(pagedResult([]))
    createMilestone.mockResolvedValue(milestoneElm)
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchMilestones).toHaveBeenCalled())

    await user.click(screen.getByRole('button', { name: 'Créer un jalon' }))
    const dialog = await screen.findByRole('dialog')

    await user.click(within(dialog).getByLabelText('Projet'))
    await user.click(await screen.findByRole('option', { name: 'Migration ELM' }))

    await user.type(within(dialog).getByLabelText('Nom'), 'Nouveau jalon')
    await user.click(within(dialog).getByLabelText('Type de jalon'))
    await user.click(await screen.findByRole('option', { name: 'Kick-off' }))
    await user.click(within(dialog).getByLabelText('Responsable'))
    await user.click(await screen.findByRole('option', { name: 'Thierry GEORGES' }))
    await user.type(within(dialog).getByLabelText('Date prévue'), '2027-01-01')

    await user.click(within(dialog).getByRole('button', { name: 'Créer' }))

    await waitFor(() =>
      expect(createMilestone).toHaveBeenCalledWith(
        expect.objectContaining({ projectId: 'project-1' }),
      ),
    )
  })
})
