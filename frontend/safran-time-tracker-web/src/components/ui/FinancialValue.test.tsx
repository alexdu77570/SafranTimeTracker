import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { FinancialValue } from './FinancialValue'

describe('FinancialValue', () => {
  it('formats a numeric value in French euros', () => {
    render(<FinancialValue value={1234.5} />)

    // Intl.NumberFormat('fr-FR') uses a narrow no-break space between amount and symbol.
    expect(screen.getByText(/1\s?234,50\s?€/)).toBeInTheDocument()
  })

  it('renders a placeholder instead of null when the field is absent (unauthorized)', () => {
    render(<FinancialValue value={null} />)

    expect(screen.getByText('—')).toBeInTheDocument()
  })

  it('renders a placeholder when the field is undefined', () => {
    render(<FinancialValue value={undefined} />)

    expect(screen.getByText('—')).toBeInTheDocument()
  })
})
