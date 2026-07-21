import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { WeeklyPlanningGrid } from './WeeklyPlanningGrid'

describe('WeeklyPlanningGrid', () => {
  it('renders an empty message when there is no data', () => {
    render(<WeeklyPlanningGrid weekStartDates={[]} rows={[]} />)

    expect(screen.getByText('Aucune donnée de planning.')).toBeInTheDocument()
  })

  it('renders one column per week and the Initial/Ajusté/Réalisé values per row, read-only by default', () => {
    render(
      <WeeklyPlanningGrid
        weekStartDates={['2026-06-08', '2026-06-15']}
        rows={[
          {
            id: 'r1',
            label: 'Alexandre BERNARD',
            weeks: { '2026-06-08': { initial: 20, ajuste: 24, realise: 18, surcharge: false } },
          },
        ]}
      />,
    )

    expect(screen.getByText('2026-06-08')).toBeInTheDocument()
    expect(screen.getByText('2026-06-15')).toBeInTheDocument()
    expect(screen.getByText('Alexandre BERNARD')).toBeInTheDocument()
    expect(screen.getByText('Initial : 20 h')).toBeInTheDocument()
    expect(screen.getByText('Ajusté : 24 h')).toBeInTheDocument()
    expect(screen.getByText('Réalisé : 18 h')).toBeInTheDocument()
  })

  it('renders an editable field for Ajusté and calls onAjusteChange when edited', () => {
    const onAjusteChange = vi.fn()

    render(
      <WeeklyPlanningGrid
        weekStartDates={['2026-06-08']}
        rows={[
          {
            id: 'r1',
            label: 'Alexandre BERNARD',
            weeks: { '2026-06-08': { initial: 20, ajuste: 24 } },
          },
        ]}
        onAjusteChange={onAjusteChange}
      />,
    )

    const input = screen.getByLabelText('Ajusté Alexandre BERNARD 2026-06-08')
    fireEvent.change(input, { target: { value: '30' } })

    expect(onAjusteChange).toHaveBeenCalledWith('r1', '2026-06-08', 30)
  })
})
