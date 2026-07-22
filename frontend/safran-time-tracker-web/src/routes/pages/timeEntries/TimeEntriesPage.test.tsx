import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { weekBounds } from '../../../lib/dateUtils'
import { TimeEntriesPage } from './TimeEntriesPage'

/** Le formulaire réel (validation de référence, DatePicker MUI segmenté) est déjà couvert par
 * TimeEntryForm.test.tsx. Ce test porte sur la page elle-même : filtres, câblage
 * ouverture/fermeture des modals, rafraîchissement de la liste après succès, duplication,
 * recalcul gardé par permission — jamais retesté ici pour ne pas dupliquer TimeEntryForm.test.tsx. */
vi.mock('./TimeEntryForm', () => ({
  TimeEntryCreateForm: ({
    seed,
    onSuccess,
  }: {
    seed?: { reference?: string }
    onSuccess: () => void
  }) => (
    <div>
      <p>Formulaire de création{seed?.reference ? ` (dupliqué depuis ${seed.reference})` : ''}</p>
      <button onClick={onSuccess}>Confirmer la création</button>
    </div>
  ),
  TimeEntryEditForm: ({ row, onSuccess }: { row: { id: string }; onSuccess: () => void }) => (
    <div>
      <p>Formulaire d'édition ({row.id})</p>
      <button onClick={onSuccess}>Confirmer la modification</button>
    </div>
  ),
}))

const { fetchTimeEntries, deleteTimeEntry, recalculateTimeEntry } = vi.hoisted(() => ({
  fetchTimeEntries: vi.fn(),
  deleteTimeEntry: vi.fn(),
  recalculateTimeEntry: vi.fn(),
}))
vi.mock('../../../api/endpoints/timeEntries', () => ({
  fetchTimeEntries,
  deleteTimeEntry,
  recalculateTimeEntry,
}))

vi.mock('../../../api/endpoints/activityTypes', () => ({
  fetchActivityTypes: vi.fn(async () => ({
    items: [
      {
        id: 'run',
        code: 'RUN',
        libelle: 'RUN',
        isRun: true,
        referenceRequired: false,
        referenceFormatRegex: null,
        referenceExample: null,
        statut: 0,
      },
      {
        id: 'incident',
        code: 'INCIDENT',
        libelle: 'Incident',
        isRun: true,
        referenceRequired: true,
        referenceFormatRegex: '^INC\\d{7}$',
        referenceExample: 'INC0001234',
        statut: 0,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 2,
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
vi.mock('../../../api/endpoints/projects', () => ({
  fetchProjects: vi.fn(async () => ({
    items: [
      {
        id: 'project-1',
        nom: 'Migration ELM',
        code: 'PRJ-ELM-2026',
        applicationId: 'a',
        piloteId: 'p',
        departmentId: 'd',
        serviceId: 's',
        teamId: null,
        statusId: 'st',
        projectTypeId: null,
        clientId: null,
        dateDebut: '2026-01-01',
        dateFinPrevueInitiale: '2026-12-31',
        dateFinAjustee: null,
        dateFinReelle: null,
        niveauRisque: 0,
        commentaire: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))
vi.mock('../../../api/endpoints/orders', () => ({
  fetchOrders: vi.fn(async () => ({
    items: [
      {
        id: 'order-1',
        reference: 'CMD-2026-0001',
        libelle: 'Commande démo',
        companyId: 'c',
        projectId: null,
        budgetFinancierInitial: 0,
        budgetFinancierAjuste: 0,
        budgetJoursInitial: null,
        budgetJoursAjuste: null,
        dateDebut: '2026-01-01',
        dateFinInitiale: '2026-12-31',
        dateFinAjustee: null,
        statusId: 'st',
        seuilAlerte: null,
        commentaire: null,
      },
    ],
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
        permissionIds: ['perm-recalc'],
        effectivePermissionCodes: ['TIME_ENTRY_RECALCULATION'],
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
        id: 'perm-recalc',
        code: 'TIME_ENTRY_RECALCULATION',
        libelle: 'Recalcul',
        description: null,
      },
    ],
    page: 1,
    pageSize: 100,
    totalCount: 1,
  })),
}))

const entry = {
  id: 'te-1',
  resourceId: 'resource-1',
  activityTypeId: 'run',
  projectId: null,
  orderId: null,
  date: '2026-07-15',
  dureeHeures: 7.75,
  reference: null,
  commentaire: null,
  statut: 0,
  createdAt: '2026-07-15T00:00:00Z',
  createdBy: 's636140',
  updatedAt: null,
  updatedBy: null,
  financialSnapshot: {
    tjmPersonneSnapshot: 500,
    sourceTjmPersonne: 'ResourceTjmHistory',
    resourceTjmHistoryId: 'h1',
    tjmContratSnapshot: null,
    sourceContrat: null,
    companyContractHistoryId: null,
    companyIdSnapshot: 'c1',
    coutReelCalcule: 500,
    coutContratCalcule: null,
    differentielCalcule: null,
    calculationDate: '2026-07-15T00:00:00Z',
    calculationStatus: 0,
  },
}

function pagedResult(items: (typeof entry)[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <TimeEntriesPage />
    </DemoTestProviders>,
  )
}

describe('TimeEntriesPage', () => {
  it('displays the current identity resource entries with the financial value visible', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))

    renderPage()

    expect(await screen.findByText('2026-07-15')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('500,00 €')).toBeInTheDocument())
  })

  it('re-queries with the selected activity type, project and order filters', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([]))
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchTimeEntries).toHaveBeenCalled())
    fetchTimeEntries.mockClear()

    await user.click(screen.getByLabelText("Type d'activité"))
    await user.click(await screen.findByRole('option', { name: 'Incident' }))
    await waitFor(() =>
      expect(fetchTimeEntries).toHaveBeenCalledWith(
        expect.objectContaining({ activityTypeId: 'incident' }),
      ),
    )

    await user.click(screen.getByLabelText('Projet'))
    await user.click(await screen.findByRole('option', { name: 'Migration ELM' }))
    await waitFor(() =>
      expect(fetchTimeEntries).toHaveBeenCalledWith(
        expect.objectContaining({ projectId: 'project-1' }),
      ),
    )

    await user.click(screen.getByLabelText('Commande'))
    await user.click(await screen.findByRole('option', { name: 'CMD-2026-0001' }))
    await waitFor(() =>
      expect(fetchTimeEntries).toHaveBeenCalledWith(
        expect.objectContaining({ orderId: 'order-1' }),
      ),
    )
  })

  it('sets Du/Au to the ISO week bounds when a day is picked in the "Semaine" filter (§19.4)', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([]))

    renderPage()
    await waitFor(() => expect(fetchTimeEntries).toHaveBeenCalled())
    fetchTimeEntries.mockClear()

    fireEvent.change(screen.getByLabelText('Semaine (jour contenu)'), {
      target: { value: '2026-07-15' },
    })

    const bounds = weekBounds('2026-07-15')
    await waitFor(() =>
      expect(fetchTimeEntries).toHaveBeenCalledWith(
        expect.objectContaining({ from: bounds.start, to: bounds.end }),
      ),
    )
    expect(screen.getByLabelText('Du')).toHaveValue(bounds.start)
    expect(screen.getByLabelText('Au')).toHaveValue(bounds.end)
  })

  it('creates a duplicated entry from an existing row and refreshes the list', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-07-15')
    fetchTimeEntries.mockClear()

    await user.click(screen.getByRole('button', { name: 'Dupliquer' }))
    expect(screen.getByText('Formulaire de création')).toBeInTheDocument()

    await user.click(screen.getByText('Confirmer la création'))

    await waitFor(() => expect(fetchTimeEntries).toHaveBeenCalled())
    await waitFor(() =>
      expect(screen.queryByText('Formulaire de création')).not.toBeInTheDocument(),
    )
  })

  it('opens the edit form on row click and refreshes the list on success', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-07-15')
    fetchTimeEntries.mockClear()

    await user.click(screen.getByText('2026-07-15'))
    expect(screen.getByText("Formulaire d'édition (te-1)")).toBeInTheDocument()

    await user.click(screen.getByText('Confirmer la modification'))

    await waitFor(() => expect(fetchTimeEntries).toHaveBeenCalled())
  })

  it('deletes an entry after confirmation', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))
    deleteTimeEntry.mockResolvedValue(undefined)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-07-15')

    await user.click(screen.getByRole('button', { name: 'Supprimer' }))
    await user.click(screen.getByRole('button', { name: 'Confirmer' }))

    await waitFor(() => expect(deleteTimeEntry).toHaveBeenCalledWith('te-1'))
  })

  it('recalculates an entry with a mandatory reason, gated by TIME_ENTRY_RECALCULATION', async () => {
    setStoredIdentifiant('s636140')
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))
    recalculateTimeEntry.mockResolvedValue(entry)
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-07-15')

    const recalculateButton = screen.getByRole('button', { name: 'Recalculer' })
    await user.click(recalculateButton)

    const confirmButton = screen.getByRole('button', { name: 'Recalculer' })
    expect(confirmButton).toBeDisabled()

    await user.type(screen.getByLabelText('Motif'), 'Correction du TJM')
    await waitFor(() => expect(confirmButton).not.toBeDisabled())
    await user.click(confirmButton)

    await waitFor(() =>
      expect(recalculateTimeEntry).toHaveBeenCalledWith('te-1', { reason: 'Correction du TJM' }),
    )
  })

  it('hides the recalculate action when the current identity lacks TIME_ENTRY_RECALCULATION', async () => {
    fetchTimeEntries.mockResolvedValue(pagedResult([entry]))
    const user = userEvent.setup()

    renderPage()

    await user.click(screen.getByLabelText('Ressource'))
    await user.click(await screen.findByRole('option', { name: 'Alexandre BERNARD' }))

    await screen.findByText('2026-07-15')
    expect(screen.queryByRole('button', { name: 'Recalculer' })).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Dupliquer' })).toBeInTheDocument()
  })
})
