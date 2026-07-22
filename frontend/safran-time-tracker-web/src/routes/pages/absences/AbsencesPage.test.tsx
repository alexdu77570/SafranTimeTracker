import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../../../test/testUtils'
import { setStoredIdentifiant } from '../../../auth/demoIdentityStorage'
import { AbsencesPage } from './AbsencesPage'

/** Le formulaire réel (DatePicker, conversion de statut) est déjà couvert par AbsenceForm.test.tsx.
 * Ce test porte sur la page : KPI (issus de l'endpoint de disponibilité, jamais sommés côté
 * client), filtres, ouverture des modals, et le workflow Brouillon → Soumis → Validé/Refusé/Annulé
 * piloté par le statut de la ligne. */
vi.mock('./AbsenceForm', () => ({
  AbsenceCreateForm: ({ onSuccess }: { onSuccess: () => void }) => (
    <div>
      <p>Formulaire de création</p>
      <button onClick={onSuccess}>Confirmer la création</button>
    </div>
  ),
  AbsenceEditForm: ({ row, onSuccess }: { row: { id: string }; onSuccess: () => void }) => (
    <div>
      <p>Formulaire d'édition ({row.id})</p>
      <button onClick={onSuccess}>Confirmer la modification</button>
    </div>
  ),
}))

const { fetchAbsences, submitAbsence, validateAbsence, refuseAbsence, cancelAbsence } = vi.hoisted(
  () => ({
    fetchAbsences: vi.fn(),
    submitAbsence: vi.fn(),
    validateAbsence: vi.fn(),
    refuseAbsence: vi.fn(),
    cancelAbsence: vi.fn(),
  }),
)
vi.mock('../../../api/endpoints/absences', () => ({
  fetchAbsences,
  submitAbsence,
  validateAbsence,
  refuseAbsence,
  cancelAbsence,
}))

const { fetchAvailability } = vi.hoisted(() => ({ fetchAvailability: vi.fn() }))
vi.mock('../../../api/endpoints/availability', () => ({ fetchAvailability }))

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
        permissionIds: [],
        effectivePermissionCodes: [],
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

function absence(
  overrides: Partial<{
    id: string
    statut: number
    dateDebut: string
    dateFin: string
    type: number
  }> = {},
) {
  return {
    id: 'ab-1',
    resourceId: 'resource-1',
    type: 0,
    dateDebut: '2026-08-01',
    dateFin: '2026-08-03',
    demiJournee: false,
    commentaire: null,
    statut: 0,
    valideParIdentifiant: null,
    dateDecision: null,
    createdAt: '2026-07-20T00:00:00Z',
    createdBy: 's636140',
    ...overrides,
  }
}

function pagedResult<T>(items: T[]) {
  return { items, page: 1, pageSize: 20, totalCount: items.length }
}

afterEach(() => {
  localStorage.clear()
  vi.clearAllMocks()
})

function renderPage() {
  render(
    <DemoTestProviders>
      <AbsencesPage />
    </DemoTestProviders>,
  )
}

describe('AbsencesPage', () => {
  it('displays the monthly/annual KPI from the availability endpoint, never summed client-side', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence()]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
      startDate: '2026-07-01',
      endDate: '2026-07-31',
      joursOuvres: 23,
      joursFeries: 0,
      joursAbsenceValidee: 3,
      capaciteTheorique: 178.25,
      capaciteReelle: 155,
      tauxDisponibilite: 87,
      chargeRunHeures: 0,
      chargeHorsRunHeures: 0,
    })

    renderPage()

    expect(await screen.findByText('2026-08-01')).toBeInTheDocument()
    await waitFor(() => expect(screen.getAllByText('3').length).toBeGreaterThan(0))
    expect(screen.getByText('87%')).toBeInTheDocument()
    expect(fetchAvailability).toHaveBeenCalledWith(
      'resource-1',
      expect.any(String),
      expect.any(String),
    )
  })

  it('re-queries when the statut filter changes', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchAbsences).toHaveBeenCalled())
    fetchAbsences.mockClear()

    await user.click(screen.getByLabelText('Statut'))
    await user.click(await screen.findByRole('option', { name: 'Validé' }))

    await waitFor(() =>
      expect(fetchAbsences).toHaveBeenCalledWith(expect.objectContaining({ statut: 2 })),
    )
  })

  it('creates an absence and refreshes the list', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    const user = userEvent.setup()

    renderPage()
    await waitFor(() => expect(fetchAbsences).toHaveBeenCalled())
    fetchAbsences.mockClear()

    await user.click(screen.getByRole('button', { name: 'Créer une absence' }))
    await user.click(screen.getByText('Confirmer la création'))

    await waitFor(() => expect(fetchAbsences).toHaveBeenCalled())
  })

  it('shows Modifier/Soumettre on a Brouillon and opens the edit form', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence({ statut: 0 })]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-08-01')

    expect(screen.getByRole('button', { name: 'Modifier' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Soumettre' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Supprimer le brouillon' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Modifier' }))
    expect(screen.getByText("Formulaire d'édition (ab-1)")).toBeInTheDocument()
  })

  it('submits a Brouillon and invalidates the queries', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence({ statut: 0 })]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    submitAbsence.mockResolvedValue(absence({ statut: 1 }))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-08-01')

    await user.click(screen.getByRole('button', { name: 'Soumettre' }))

    await waitFor(() => expect(submitAbsence).toHaveBeenCalled())
    expect(submitAbsence.mock.calls[0][0]).toBe('ab-1')
  })

  it('shows Valider/Refuser on a Soumis absence, and refuses with an optional motif', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence({ statut: 1 })]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    refuseAbsence.mockResolvedValue(absence({ statut: 3 }))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-08-01')

    expect(screen.getByRole('button', { name: 'Valider' })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Refuser' }))
    // fireEvent.change plutôt que user.type (CI, Lot 13) : évite de simuler 20 frappes clavier sur
    // un runner GitHub partagé plus lent, sans changer le comportement testé (champ facultatif
    // sans validation par frappe).
    fireEvent.change(screen.getByLabelText('Motif (facultatif)'), {
      target: { value: 'Charge insuffisante' },
    })
    await user.click(screen.getByRole('button', { name: 'Confirmer le refus' }))

    await waitFor(() =>
      expect(refuseAbsence).toHaveBeenCalledWith('ab-1', { commentaire: 'Charge insuffisante' }),
    )
  })

  it('validates a Soumis absence', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence({ statut: 1 })]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
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
    })
    validateAbsence.mockResolvedValue(absence({ statut: 2 }))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-08-01')

    await user.click(screen.getByRole('button', { name: 'Valider' }))

    await waitFor(() => expect(validateAbsence).toHaveBeenCalled())
    expect(validateAbsence.mock.calls[0][0]).toBe('ab-1')
  })

  it('cancels a validated absence after confirmation', async () => {
    setStoredIdentifiant('s636140')
    fetchAbsences.mockResolvedValue(pagedResult([absence({ statut: 2 })]))
    fetchAvailability.mockResolvedValue({
      resourceId: 'resource-1',
      startDate: '2026-07-01',
      endDate: '2026-07-31',
      joursOuvres: 23,
      joursFeries: 0,
      joursAbsenceValidee: 1,
      capaciteTheorique: 178.25,
      capaciteReelle: 170.5,
      tauxDisponibilite: 96,
      chargeRunHeures: 0,
      chargeHorsRunHeures: 0,
    })
    cancelAbsence.mockResolvedValue(absence({ statut: 4 }))
    const user = userEvent.setup()

    renderPage()
    await screen.findByText('2026-08-01')

    await user.click(screen.getByRole('button', { name: 'Annuler' }))
    await user.click(screen.getByRole('button', { name: 'Confirmer' }))

    await waitFor(() => expect(cancelAbsence).toHaveBeenCalledWith('ab-1'))
  })
})
