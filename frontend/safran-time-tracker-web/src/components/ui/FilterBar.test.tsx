import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { FilterBar } from './FilterBar'

describe('FilterBar', () => {
  it('renders its filter children', () => {
    render(
      <FilterBar>
        <span>Filtre statut</span>
      </FilterBar>,
    )

    expect(screen.getByText('Filtre statut')).toBeInTheDocument()
  })

  it('does not render a reset button when onReset is not provided', () => {
    render(
      <FilterBar>
        <span>Filtre</span>
      </FilterBar>,
    )

    expect(screen.queryByRole('button', { name: 'Réinitialiser' })).not.toBeInTheDocument()
  })

  it('calls onReset when the reset button is clicked', async () => {
    const onReset = vi.fn()
    const user = userEvent.setup()
    render(
      <FilterBar onReset={onReset}>
        <span>Filtre</span>
      </FilterBar>,
    )

    await user.click(screen.getByRole('button', { name: 'Réinitialiser' }))

    expect(onReset).toHaveBeenCalledOnce()
  })
})
