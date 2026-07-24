import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { demoSessionFixture, demoUserFixture, pagedResult as sharedPagedResult } from '../../../test/fixtures'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { ProjectPlanningPage } from './ProjectPlanningPage'

const { fetchProjectPlanningOverview } = vi.hoisted(() => ({
  fetchProjectPlanningOverview: vi.fn(),
}))
vi.mock('../../../api/endpoints/projectPlanning', () => ({ fetchProjectPlanningOverview }))

vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({
    items: [
      { id: 'project-1', nom: 'Migration ELM', code: 'PRJ-ELM-2026' },
      { id: 'project-2', nom: 'Refonte VTOM', code: 'PRJ-VTOM-2026' },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 2,
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
  fetchTeams: vi.fn(async () => ({
    items: [
      {
        id: 'team-1',
        code: 'A',
        nom: 'Équipe Projets A',
        serviceId: 'service-1',
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
vi.mock('../../../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

vi.mock('../../../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () => sharedPagedResult([demoUserFixture({ permissionIds: [], effectivePermissionCodes: [] })], 100)),
}))
vi.mock('../../../api/endpoints/permissions', () => ({
  fetchPermissions: vi.fn(async () => sharedPagedResult([], 100)),
}))

const row = {
  projectId: 'project-1',
  resourceId: 'resource-1',
  weekStartDate: '2026-06-08',
  chargePlanifieeInitiale: 20,
  chargePlanifieeAjustee: 24,
  chargeRealisee: 18,
  ecartPrevuRealise: -6,
  capaciteReelle: 38.75,
  surcharge: false,
}

function pagedResult(items: (typeof row)[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <ProjectPlanningPage />
    </DemoTestProviders>,
  )
}

describe('ProjectPlanningPage', () => {
  it('displays the transverse planning rows aggregated by the server', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectPlanningOverview.mockResolvedValue(pagedResult([row]))

    renderPage()

    expect(await screen.findByText('2026-06-08')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Migration ELM')).toBeInTheDocument())
    expect(screen.getByText('Alexandre BERNARD')).toBeInTheDocument()
  })

  it('re-queries with the selected projet, ressource and surcharge filters', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectPlanningOverview.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchProjectPlanningOverview).toHaveBeenCalled())
    fetchProjectPlanningOverview.mockClear()

    await user.click(screen.getByLabelText('Projet'))
    await user.click(await screen.findByRole('option', { name: 'Migration ELM' }))
    await waitFor(() =>
      expect(fetchProjectPlanningOverview).toHaveBeenCalledWith(
        expect.objectContaining({ projectId: 'project-1' }),
      ),
    )

    await user.click(screen.getByLabelText('Ressource'))
    await user.click(await screen.findByRole('option', { name: 'Alexandre BERNARD' }))
    await waitFor(() =>
      expect(fetchProjectPlanningOverview).toHaveBeenCalledWith(
        expect.objectContaining({ resourceId: 'resource-1' }),
      ),
    )

    await user.click(screen.getByLabelText('Surcharge'))
    await user.click(await screen.findByRole('option', { name: 'En surcharge' }))
    await waitFor(() =>
      expect(fetchProjectPlanningOverview).toHaveBeenCalledWith(
        expect.objectContaining({ surcharge: true }),
      ),
    )
  })

  it('shows a surcharge alert badge on rows flagged by the server', async () => {
    setStoredIdentifiant('s636140')
    fetchProjectPlanningOverview.mockResolvedValue(pagedResult([{ ...row, surcharge: true }]))

    renderPage()

    await waitFor(() => expect(screen.getAllByText('Surcharge').length).toBeGreaterThan(0))
  })
})
