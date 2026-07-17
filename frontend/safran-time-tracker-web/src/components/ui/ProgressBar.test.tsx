import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ProgressBar } from './ProgressBar'

describe('ProgressBar', () => {
  it('renders the percentage rounded from the ratio', () => {
    render(<ProgressBar value={0.42} label="Consommation budget" />)

    expect(screen.getByText('42%')).toBeInTheDocument()
    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '42')
  })

  it('clamps a ratio above 1 (dépassement) to 100 for the visual bar', () => {
    render(<ProgressBar value={1.3} />)

    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '100')
  })

  it('clamps a negative ratio to 0', () => {
    render(<ProgressBar value={-0.2} />)

    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '0')
  })
})
