import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Timeline } from './Timeline'

describe('Timeline', () => {
  it('renders an empty message when there are no items', () => {
    render(<Timeline items={[]} />)

    expect(screen.getByText('Aucun élément à afficher.')).toBeInTheDocument()
  })

  it('renders items sorted chronologically with their status', () => {
    render(
      <Timeline
        items={[
          {
            id: '1',
            date: '2026-06-01',
            label: 'GO PROD',
            statusLabel: 'En cours',
            statusTone: 'info',
          },
          {
            id: '2',
            date: '2026-01-15',
            label: 'Kick-off',
            statusLabel: 'Terminé',
            statusTone: 'success',
          },
        ]}
      />,
    )

    const labels = screen.getAllByText(/Kick-off|GO PROD/).map((el) => el.textContent)
    expect(labels).toEqual(['Kick-off', 'GO PROD'])
    expect(screen.getByText('Terminé')).toBeInTheDocument()
    expect(screen.getByText('En cours')).toBeInTheDocument()
  })

  it('renders the sublabel when provided', () => {
    render(
      <Timeline
        items={[
          {
            id: '1',
            date: '2026-06-01',
            label: 'GO PROD',
            sublabel: 'Migration ELM',
            statusLabel: 'En cours',
            statusTone: 'info',
          },
        ]}
      />,
    )

    expect(screen.getByText(/Migration ELM/)).toBeInTheDocument()
  })
})
