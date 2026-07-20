import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { DataTable } from './DataTable'

interface Row {
  id: string
  nom: string
}

const columns = [{ field: 'nom', headerName: 'Nom', width: 150 }]

describe('DataTable', () => {
  it('renders the provided rows', () => {
    const rows: Row[] = [
      { id: '1', nom: 'DSI' },
      { id: '2', nom: 'RUN' },
    ]

    render(
      <DataTable
        rows={rows}
        columns={columns}
        rowCount={2}
        page={1}
        pageSize={20}
        onPageChange={vi.fn()}
        onPageSizeChange={vi.fn()}
      />,
    )

    expect(screen.getByText('DSI')).toBeInTheDocument()
    expect(screen.getByText('RUN')).toBeInTheDocument()
  })

  it('renders the empty label when there are no rows', () => {
    render(
      <DataTable
        rows={[]}
        columns={columns}
        rowCount={0}
        page={1}
        pageSize={20}
        onPageChange={vi.fn()}
        onPageSizeChange={vi.fn()}
        emptyLabel="Aucune ressource"
      />,
    )

    expect(screen.getByText('Aucune ressource')).toBeInTheDocument()
  })

  it('calls onRowClick with the clicked row when provided', async () => {
    const onRowClick = vi.fn()
    const user = userEvent.setup()
    const rows: Row[] = [{ id: '1', nom: 'DSI' }]

    render(
      <DataTable
        rows={rows}
        columns={columns}
        rowCount={1}
        page={1}
        pageSize={20}
        onPageChange={vi.fn()}
        onPageSizeChange={vi.fn()}
        onRowClick={onRowClick}
      />,
    )

    await user.click(screen.getByText('DSI'))

    expect(onRowClick).toHaveBeenCalledOnce()
    expect(onRowClick.mock.calls[0][0].row).toEqual(rows[0])
  })

  it('does not require onRowClick (rows are not clickable by default)', async () => {
    const user = userEvent.setup()
    const rows: Row[] = [{ id: '1', nom: 'DSI' }]

    render(
      <DataTable rows={rows} columns={columns} rowCount={1} page={1} pageSize={20} onPageChange={vi.fn()} onPageSizeChange={vi.fn()} />,
    )

    // Ne doit pas lever d'exception en l'absence de handler.
    await user.click(screen.getByText('DSI'))
    expect(screen.getByText('DSI')).toBeInTheDocument()
  })
})
