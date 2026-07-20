import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { AvailabilityPage, periodBounds } from './AvailabilityPage'

describe('periodBounds', () => {
  it('resolves monthly bounds around the reference date', () => {
    expect(periodBounds('mensuelle', '2026-07-20')).toEqual({
      start: '2026-07-01',
      end: '2026-07-31',
    })
  })

  it('resolves weekly bounds around the reference date (locale française, semaine du lundi)', () => {
    // 2026-07-20 est un lundi.
    expect(periodBounds('hebdomadaire', '2026-07-20')).toEqual({
      start: '2026-07-20',
      end: '2026-07-26',
    })
  })
})

const { fetchAvailability } = vi.hoisted(() => ({ fetchAvailability: vi.fn() }))
vi.mock('../../../api/endpoints/availability', () => ({ fetchAvailability }))
const { fetchAbsences } = vi.hoisted(() => ({ fetchAbsences: vi.fn() }))
vi.mock('../../../api/endpoints/absences', () => ({ fetchAbsences }))
const { fetchHolidays } = vi.hoisted(() => ({ fetchHolidays: vi.fn() }))
vi.mock('../../../api/endpoints/holidayCalendar', () => ({ fetchHolidays }))
const { fetchResources } = vi.hoisted(() => ({ fetchResources: vi.fn() }))
vi.mock('../../../api/endpoints/resources', () => ({ fetchResources }))

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
        permissionIds: [],
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => ({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
}))

function resource(id: string, prenom: string, nom: string) {
  return {
    id,
    nom,
    prenom,
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
  }
}

function availability(resourceId: string, overrides: Partial<{ tauxDisponibilite: number }> = {}) {
  return {
    resourceId,
    startDate: '2026-07-01',
    endDate: '2026-07-31',
    joursOuvres: 23,
    joursFeries: 0,
    joursAbsenceValidee: 0,
    capaciteTheorique: 178.25,
    capaciteReelle: 178.25,
    tauxDisponibilite: 100,
    chargeRunHeures: 0,
    chargeHorsRunHeures: 0,
    ...overrides,
  }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <AvailabilityPage />
    </DemoTestProviders>,
  )
}

describe('AvailabilityPage', () => {
  it('displays the capacity-per-resource table and the colored calendar for the current identity', async () => {
    setStoredIdentifiant('s636140')
    fetchResources.mockResolvedValue({
      items: [resource('resource-1', 'Alexandre', 'BERNARD')],
      page: 1,
      pageSize: 100,
      totalCount: 1,
    })
    fetchAvailability.mockImplementation(async (resourceId: string) => availability(resourceId))
    fetchHolidays.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    fetchAbsences.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })

    renderPage()

    await waitFor(() => expect(screen.getAllByText('Alexandre BERNARD').length).toBeGreaterThan(0))
    await waitFor(() => expect(screen.getAllByText('178.25 h').length).toBe(2))
    expect(screen.getByText('100%')).toBeInTheDocument()
    await waitFor(() =>
      expect(screen.getAllByText('Disponible', { selector: 'span' }).length).toBeGreaterThan(0),
    )
  })

  it('re-queries resources when the département filter changes', async () => {
    setStoredIdentifiant('s636140')
    fetchResources.mockResolvedValue({
      items: [resource('resource-1', 'Alexandre', 'BERNARD')],
      page: 1,
      pageSize: 100,
      totalCount: 1,
    })
    fetchAvailability.mockImplementation(async (resourceId: string) => availability(resourceId))
    fetchHolidays.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    fetchAbsences.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    const user = userEvent.setup()

    renderPage()
    await screen.findAllByText('Alexandre BERNARD')
    fetchResources.mockClear()

    await user.click(screen.getByLabelText('Département'))
    await user.click(await screen.findByRole('option', { name: 'DSI' }))

    await waitFor(() =>
      expect(fetchResources).toHaveBeenCalledWith(
        expect.objectContaining({ departmentId: 'dept-1' }),
      ),
    )
  })

  it('refetches with weekly bounds when switching to the Hebdomadaire view', async () => {
    setStoredIdentifiant('s636140')
    fetchResources.mockResolvedValue({
      items: [resource('resource-1', 'Alexandre', 'BERNARD')],
      page: 1,
      pageSize: 100,
      totalCount: 1,
    })
    fetchAvailability.mockImplementation(async (resourceId: string) => availability(resourceId))
    fetchHolidays.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    fetchAbsences.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    const user = userEvent.setup()

    renderPage()
    await screen.findAllByText('Alexandre BERNARD')
    fireEvent.change(screen.getByLabelText('Période de référence'), {
      target: { value: '2026-07-20' },
    })
    await screen.findByText('Du 2026-07-01 au 2026-07-31')

    await user.click(screen.getByRole('button', { name: 'Hebdomadaire' }))

    await screen.findByText('Du 2026-07-20 au 2026-07-26')
  })

  it('switches the colored calendar to the resource clicked in the table', async () => {
    setStoredIdentifiant('s636140')
    fetchResources.mockResolvedValue({
      items: [
        resource('resource-1', 'Alexandre', 'BERNARD'),
        resource('resource-2', 'Camille', 'DURAND'),
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
    })
    fetchAvailability.mockImplementation(async (resourceId: string) => availability(resourceId))
    fetchHolidays.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    fetchAbsences.mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 })
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('Camille DURAND')
    // Par défaut, le calendrier suit la ressource de l'identité courante (resource-1).
    await waitFor(() =>
      expect(fetchAbsences).toHaveBeenCalledWith(
        expect.objectContaining({ resourceId: 'resource-1' }),
      ),
    )
    fetchAbsences.mockClear()

    await user.click(screen.getByText('Camille DURAND'))

    await waitFor(() =>
      expect(fetchAbsences).toHaveBeenCalledWith(
        expect.objectContaining({ resourceId: 'resource-2' }),
      ),
    )
  })
})
