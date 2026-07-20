import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { TestProviders } from '../../../test/testUtils'
import type { ActivityTypeDto } from '../../../api/types'
import { ReferentialStatus } from '../../../api/types'
import { TimeEntryCreateForm, validateReference } from './TimeEntryForm'

const runType: ActivityTypeDto = {
  id: 'run', code: 'RUN', libelle: 'RUN', isRun: true, referenceRequired: false,
  referenceFormatRegex: null, referenceExample: null, statut: ReferentialStatus.Actif,
}
const incidentType: ActivityTypeDto = {
  id: 'incident', code: 'INCIDENT', libelle: 'Incident', isRun: true, referenceRequired: true,
  referenceFormatRegex: '^INC\\d{7}$', referenceExample: 'INC0001234', statut: ReferentialStatus.Actif,
}

/** Réplique côté client la même donnée que ActivityType.ReferenceRequired/ReferenceFormatRegex
 * (§19.3) — jamais une seconde règle, la même métadonnée est simplement lue (CLAUDE.md §5). */
describe('validateReference', () => {
  it('allows an empty reference when the activity type does not require one', () => {
    expect(validateReference(runType, undefined)).toBeNull()
    expect(validateReference(runType, '')).toBeNull()
  })

  it('rejects a missing reference when required', () => {
    expect(validateReference(incidentType, undefined)).toContain('obligatoire')
  })

  it('rejects a reference that does not match the format', () => {
    expect(validateReference(incidentType, 'TICKET-123')).toContain('Format');
  })

  it('accepts a reference matching the format', () => {
    expect(validateReference(incidentType, 'INC0001234')).toBeNull()
  })

  it('returns null when the activity type is not yet resolved', () => {
    expect(validateReference(undefined, 'anything')).toBeNull()
  })
})

const { fetchActivityTypes } = vi.hoisted(() => ({ fetchActivityTypes: vi.fn() }))
vi.mock('../../../api/endpoints/activityTypes', () => ({ fetchActivityTypes }))
vi.mock('../../../api/endpoints/projects', () => ({ fetchProjects: vi.fn().mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 }) }))
vi.mock('../../../api/endpoints/orders', () => ({ fetchOrders: vi.fn().mockResolvedValue({ items: [], page: 1, pageSize: 100, totalCount: 0 }) }))
const { createTimeEntry } = vi.hoisted(() => ({ createTimeEntry: vi.fn() }))
vi.mock('../../../api/endpoints/timeEntries', () => ({ createTimeEntry, updateTimeEntry: vi.fn() }))

describe('TimeEntryCreateForm', () => {
  it('blocks submission client-side when the reference is missing for a type that requires one', async () => {
    fetchActivityTypes.mockResolvedValue({ items: [incidentType], page: 1, pageSize: 100, totalCount: 1 })
    const user = userEvent.setup()
    const onSuccess = vi.fn()

    render(
      <TestProviders>
        {/* `seed` pré-remplit la date (fonctionnalité réelle de duplication) : évite d'avoir à
         * piloter le DatePicker MUI segmenté dans ce test, qui porte sur la validation de
         * référence, pas sur le composant DatePicker lui-même. */}
        <TimeEntryCreateForm resourceId="resource-1" seed={{ date: '2026-07-20' }} onSuccess={onSuccess} onCancel={vi.fn()} />
      </TestProviders>,
    )

    await user.click(await screen.findByLabelText("Type d'activité"))
    await user.click(await screen.findByRole('option', { name: 'Incident' }))
    await user.click(screen.getByRole('button', { name: 'Créer' }))

    expect(await screen.findByText(/référence est obligatoire/i)).toBeInTheDocument()
    expect(createTimeEntry).not.toHaveBeenCalled()
    await waitFor(() => expect(onSuccess).not.toHaveBeenCalled())
  })
})
