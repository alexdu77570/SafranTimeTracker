import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { KpiCard } from './KpiCard'

describe('KpiCard', () => {
  it('renders the label and the already-formatted value', () => {
    render(<KpiCard label="Charge RUN" value="42,5 j" />)

    expect(screen.getByText('Charge RUN')).toBeInTheDocument()
    expect(screen.getByText('42,5 j')).toBeInTheDocument()
  })

  it('renders an optional trend', () => {
    render(
      <KpiCard
        label="Budget consommé"
        value="65 %"
        trend="+5 pts vs mois dernier"
        trendTone="negative"
      />,
    )

    expect(screen.getByText('+5 pts vs mois dernier')).toBeInTheDocument()
  })
})
