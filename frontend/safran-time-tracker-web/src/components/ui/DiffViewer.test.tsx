import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { DiffViewer } from './DiffViewer'
import { ImportDiffType } from '../../api/types'

describe('DiffViewer', () => {
  it('renders one row per diff with its type and field change', () => {
    render(
      <DiffViewer
        diffs={[
          {
            id: 'diff-1',
            importBatchId: 'batch-1',
            entityType: 'Resource',
            entityId: 'r1',
            diffType: ImportDiffType.Modification,
            fieldName: 'Nom',
            oldValue: 'Ancien',
            newValue: 'Nouveau',
          },
        ]}
      />,
    )

    expect(screen.getByText('Resource')).toBeInTheDocument()
    expect(screen.getByText('Modification')).toBeInTheDocument()
    expect(screen.getByText('Ancien')).toBeInTheDocument()
    expect(screen.getByText('Nouveau')).toBeInTheDocument()
  })

  it('renders the empty label when there are no diffs', () => {
    render(<DiffViewer diffs={[]} emptyLabel="Aucun écart pour ce lot." />)

    expect(screen.getByText('Aucun écart pour ce lot.')).toBeInTheDocument()
  })
})
