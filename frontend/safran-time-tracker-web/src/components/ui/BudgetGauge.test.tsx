import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { BudgetGauge } from './BudgetGauge'

describe('BudgetGauge', () => {
  it('renders the label and the consumed/total amounts', () => {
    render(<BudgetGauge label="Migration ELM" consumed={700} total={180000} />)

    expect(screen.getByText('Migration ELM')).toBeInTheDocument()
    expect(screen.getByText('700 € / 180 000 €')).toBeInTheDocument()
  })

  it('renders without error when total is zero', () => {
    render(<BudgetGauge label="Sans budget" consumed={0} total={0} />)

    expect(screen.getByText('Sans budget')).toBeInTheDocument()
  })
})
