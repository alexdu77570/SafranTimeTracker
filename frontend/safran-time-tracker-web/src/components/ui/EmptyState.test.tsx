import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { EmptyState } from './EmptyState'

describe('EmptyState', () => {
  it('renders the title and description', () => {
    render(
      <EmptyState title="Aucun jalon" description="Aucun jalon n'est planifié pour ce projet." />,
    )

    expect(screen.getByText('Aucun jalon')).toBeInTheDocument()
    expect(screen.getByText("Aucun jalon n'est planifié pour ce projet.")).toBeInTheDocument()
  })

  it('renders without a description', () => {
    render(<EmptyState title="Tableau de bord" />)

    expect(screen.getByText('Tableau de bord')).toBeInTheDocument()
  })
})
