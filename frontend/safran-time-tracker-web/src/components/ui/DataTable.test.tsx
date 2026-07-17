import { render, screen } from '@testing-library/react'
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
})
