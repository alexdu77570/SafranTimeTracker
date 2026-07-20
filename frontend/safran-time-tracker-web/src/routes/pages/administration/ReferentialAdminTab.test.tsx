import { QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { createTestQueryClient } from '../../../test/testUtils'
import { ReferentialAdminTab } from './ReferentialAdminTab'

interface Row {
  id: string
  libelle: string
}

const columns = [{ field: 'libelle', headerName: 'Libellé', flex: 1 }]

function renderTab(props: Partial<Parameters<typeof ReferentialAdminTab<Row>>[0]> = {}) {
  const client = createTestQueryClient()
  const fetchList = vi.fn().mockResolvedValue({ items: [{ id: '1', libelle: 'Existant' }], page: 1, pageSize: 20, totalCount: 1 })

  render(
    <QueryClientProvider client={client}>
      <ReferentialAdminTab<Row> title="Référentiel de test" queryKey="test-referential" fetchList={fetchList} columns={columns} {...props} />
    </QueryClientProvider>,
  )
  return { fetchList }
}

describe('ReferentialAdminTab', () => {
  it('renders the seeded list', async () => {
    renderTab()

    expect(await screen.findByText('Existant')).toBeInTheDocument()
  })

  it('does not show a create button when renderCreateForm is absent (API sans création)', async () => {
    renderTab()
    await screen.findByText('Existant')

    expect(screen.queryByRole('button', { name: 'Créer' })).not.toBeInTheDocument()
  })

  it('opens the create dialog and lets the form call onSuccess to refresh the list', async () => {
    const user = userEvent.setup()
    const { fetchList } = renderTab({
      renderCreateForm: ({ onSuccess }) => <button onClick={onSuccess}>Confirmer la création</button>,
    })
    await screen.findByText('Existant')
    fetchList.mockClear()

    await user.click(screen.getByRole('button', { name: 'Créer' }))
    expect(screen.getByText('Confirmer la création')).toBeInTheDocument()

    await user.click(screen.getByText('Confirmer la création'))

    await waitFor(() => expect(fetchList).toHaveBeenCalled())
  })

  it('opens the edit dialog on row click only when renderEditForm is provided', async () => {
    const user = userEvent.setup()
    renderTab({ renderEditForm: () => <p>Formulaire d'édition</p> })
    await screen.findByText('Existant')

    await user.click(screen.getByText('Existant'))

    expect(screen.getByText("Formulaire d'édition")).toBeInTheDocument()
  })
})
