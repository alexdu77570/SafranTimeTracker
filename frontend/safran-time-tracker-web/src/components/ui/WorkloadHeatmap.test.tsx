import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { WorkloadHeatmap } from './WorkloadHeatmap'

describe('WorkloadHeatmap', () => {
  it('renders one row per resource and one column per week', () => {
    render(
      <WorkloadHeatmap
        entries={[
          {
            resourceId: 'r1',
            nom: 'Alexandre BERNARD',
            weekStartDate: '2026-06-01',
            chargeHeures: 35,
          },
          {
            resourceId: 'r1',
            nom: 'Alexandre BERNARD',
            weekStartDate: '2026-06-08',
            chargeHeures: 10,
          },
          {
            resourceId: 'r2',
            nom: 'Fabien LEGRAND',
            weekStartDate: '2026-06-01',
            chargeHeures: 20,
          },
        ]}
      />,
    )

    expect(screen.getByText('Alexandre BERNARD')).toBeInTheDocument()
    expect(screen.getByText('Fabien LEGRAND')).toBeInTheDocument()
    expect(screen.getAllByText('2026-06-01')).toHaveLength(1)
  })

  it('renders the empty label when there are no entries', () => {
    render(<WorkloadHeatmap entries={[]} emptyLabel="Aucune charge sur cette période." />)

    expect(screen.getByText('Aucune charge sur cette période.')).toBeInTheDocument()
  })
})
