import TextField from '@mui/material/TextField'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Controller, type Control, type FieldPath, type FieldValues } from 'react-hook-form'
import { describe, expect, it, vi } from 'vitest'
import { TestProviders } from '../../../test/testUtils'
import { AbsenceStatus, AbsenceType } from '../../../api/types'
import { AbsenceCreateForm, AbsenceEditForm } from './AbsenceForm'

const { createAbsence, updateAbsence } = vi.hoisted(() => ({ createAbsence: vi.fn(), updateAbsence: vi.fn() }))
vi.mock('../../../api/endpoints/absences', () => ({ createAbsence, updateAbsence }))

// Le DatePicker MUI (sections segmentées) n'est pas pilotable de façon fiable via userEvent.type ;
// ces tests portent sur la logique du formulaire, pas sur le composant DatePicker lui-même
// (couvert par ailleurs). Remplacé par un simple champ texte relié au même Controller RHF.
vi.mock('../../../components/ui/FormDatePicker', () => ({
  FormDatePicker: <TFieldValues extends FieldValues>({
    control,
    name,
    label,
  }: {
    control: Control<TFieldValues>
    name: FieldPath<TFieldValues>
    label: string
  }) => (
    <Controller
      control={control}
      name={name}
      render={({ field }) => (
        <TextField label={label} value={field.value ?? ''} onChange={(e) => field.onChange(e.target.value)} size="small" />
      )}
    />
  ),
}))

function renderWithProviders(node: React.ReactNode) {
  render(<TestProviders>{node}</TestProviders>)
}

describe('AbsenceCreateForm', () => {
  it('creates a Brouillon absence for the given resource', async () => {
    createAbsence.mockResolvedValueOnce({
      id: '1', resourceId: 'resource-1', type: AbsenceType.Conge, dateDebut: '2026-08-01', dateFin: '2026-08-01',
      demiJournee: false, commentaire: null, statut: AbsenceStatus.Brouillon, valideParIdentifiant: null, dateDecision: null,
      createdAt: '2026-07-20T00:00:00Z', createdBy: 'test',
    })
    const user = userEvent.setup()
    const onSuccess = vi.fn()
    renderWithProviders(<AbsenceCreateForm resourceId="resource-1" onSuccess={onSuccess} onCancel={vi.fn()} />)

    await user.type(screen.getByLabelText('Date de début'), '2026-08-01')
    await user.type(screen.getByLabelText('Date de fin'), '2026-08-01')
    await user.click(screen.getByRole('button', { name: 'Créer' }))

    await waitFor(() => expect(createAbsence).toHaveBeenCalled())
    expect(createAbsence.mock.calls[0][0]).toEqual(
      expect.objectContaining({ resourceId: 'resource-1', type: AbsenceType.Conge, dateDebut: '2026-08-01', dateFin: '2026-08-01' }),
    )
    await waitFor(() => expect(onSuccess).toHaveBeenCalledOnce())
  })
})

describe('AbsenceEditForm', () => {
  const brouillon = {
    id: '1', resourceId: 'resource-1', type: AbsenceType.Conge, dateDebut: '2026-08-01', dateFin: '2026-08-01',
    demiJournee: false, commentaire: null, statut: AbsenceStatus.Brouillon, valideParIdentifiant: null, dateDecision: null,
    createdAt: '2026-07-20T00:00:00Z', createdBy: 'test',
  }

  it('sends the numeric type back to the API despite FormSelect using string values', async () => {
    updateAbsence.mockResolvedValueOnce({ ...brouillon, type: AbsenceType.Rtt })
    const user = userEvent.setup()
    renderWithProviders(<AbsenceEditForm row={brouillon} onSuccess={vi.fn()} onCancel={vi.fn()} />)

    await user.click(screen.getByLabelText("Type d'absence"))
    await user.click(await screen.findByRole('option', { name: 'RTT' }))
    await user.click(screen.getByRole('button', { name: 'Enregistrer' }))

    await waitFor(() => expect(updateAbsence).toHaveBeenCalledWith('1', expect.objectContaining({ type: AbsenceType.Rtt })))
  })
})
