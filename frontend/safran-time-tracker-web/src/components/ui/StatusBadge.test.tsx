import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StatusBadge } from './StatusBadge'

describe('StatusBadge', () => {
  it('renders the provided label', () => {
    render(<StatusBadge label="Actif" tone="success" />)

    expect(screen.getByText('Actif')).toBeInTheDocument()
  })

  it('defaults to a neutral, outlined tone when none is provided', () => {
    render(<StatusBadge label="Brouillon" />)

    expect(screen.getByText('Brouillon')).toBeInTheDocument()
  })
})
