import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { createTestQueryClient } from '../../../../test/testUtils'
import { ReferentialStatus } from '../../../../api/types'
import { ClientCreateForm, ClientEditForm } from './ClientForm'

const { createClient, updateClient } = vi.hoisted(() => ({
  createClient: vi.fn(),
  updateClient: vi.fn(),
}))
vi.mock('../../../../api/endpoints/clients', () => ({ createClient, updateClient }))

function renderWithClient(node: React.ReactNode) {
  const client = createTestQueryClient()
  render(<QueryClientProvider client={client}>{node}</QueryClientProvider>)
}

describe('ClientCreateForm', () => {
  it('rejects submission when required fields are empty', async () => {
    const user = userEvent.setup()
    const onSuccess = vi.fn()
    renderWithClient(<ClientCreateForm onSuccess={onSuccess} onCancel={vi.fn()} />)

    await user.click(screen.getByRole('button', { name: 'Créer' }))

    expect(await screen.findByText('Code obligatoire')).toBeInTheDocument()
    expect(createClient).not.toHaveBeenCalled()
    expect(onSuccess).not.toHaveBeenCalled()
  })

  it('submits the client and calls onSuccess', async () => {
    createClient.mockResolvedValueOnce({ id: '1', code: 'ACME', nom: 'Acme', statut: ReferentialStatus.Actif, commentaire: null })
    const user = userEvent.setup()
    const onSuccess = vi.fn()
    renderWithClient(<ClientCreateForm onSuccess={onSuccess} onCancel={vi.fn()} />)

    await user.type(screen.getByLabelText('Code'), 'ACME')
    await user.type(screen.getByLabelText('Nom'), 'Acme')
    await user.click(screen.getByRole('button', { name: 'Créer' }))

    // `mutationFn: createClient` est appelé directement par TanStack Query, qui lui transmet un
    // second argument de contexte (mutationKey/meta) — on vérifie donc uniquement le premier
    // argument plutôt que la liste complète des arguments de l'appel.
    await waitFor(() => expect(createClient).toHaveBeenCalled())
    expect(createClient.mock.calls[0][0]).toEqual(expect.objectContaining({ code: 'ACME', nom: 'Acme' }))
    await waitFor(() => expect(onSuccess).toHaveBeenCalledOnce())
  })
})

describe('ClientEditForm', () => {
  const existing = { id: '1', code: 'ACME', nom: 'Acme', statut: ReferentialStatus.Actif, commentaire: null }

  it('sends the numeric statut back to the API despite FormSelect using string values', async () => {
    updateClient.mockResolvedValueOnce({ ...existing, statut: ReferentialStatus.Inactif })
    const user = userEvent.setup()
    const onSuccess = vi.fn()
    renderWithClient(<ClientEditForm row={existing} onSuccess={onSuccess} onCancel={vi.fn()} />)

    await user.click(screen.getByLabelText('Statut'))
    await user.click(await screen.findByRole('option', { name: 'Inactif' }))
    await user.click(screen.getByRole('button', { name: 'Enregistrer' }))

    await waitFor(() =>
      expect(updateClient).toHaveBeenCalledWith('1', expect.objectContaining({ statut: ReferentialStatus.Inactif })),
    )
  })
})
