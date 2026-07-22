import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ChartCard } from './ChartCard'

describe('ChartCard', () => {
  it('renders the title, subheader and children', () => {
    render(
      <ChartCard title="Répartition RUN/hors RUN" subheader="Du 01/01/2026 au 31/12/2026">
        <div>Contenu du graphique</div>
      </ChartCard>,
    )

    expect(screen.getByText('Répartition RUN/hors RUN')).toBeInTheDocument()
    expect(screen.getByText('Du 01/01/2026 au 31/12/2026')).toBeInTheDocument()
    expect(screen.getByText('Contenu du graphique')).toBeInTheDocument()
  })
})
