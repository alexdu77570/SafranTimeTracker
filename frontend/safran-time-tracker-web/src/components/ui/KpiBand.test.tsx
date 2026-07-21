import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { KpiBand } from './KpiBand'

describe('KpiBand', () => {
  it('renders one KpiCard per item', () => {
    render(
      <KpiBand
        items={[
          { label: 'Budget initial', value: '10 000 €' },
          { label: 'Participants', value: '2' },
        ]}
      />,
    )

    expect(screen.getByText('Budget initial')).toBeInTheDocument()
    expect(screen.getByText('10 000 €')).toBeInTheDocument()
    expect(screen.getByText('Participants')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('renders the additional slot alongside the KPI items', () => {
    render(
      <KpiBand items={[{ label: 'Jalons', value: '3' }]}>
        <div>Bloc additionnel</div>
      </KpiBand>,
    )

    expect(screen.getByText('Jalons')).toBeInTheDocument()
    expect(screen.getByText('Bloc additionnel')).toBeInTheDocument()
  })
})
