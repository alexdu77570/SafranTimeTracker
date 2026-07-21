import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { DetailPageHeader } from './DetailPageHeader'

describe('DetailPageHeader', () => {
  it('renders the title, subtitle and actions', () => {
    render(
      <DetailPageHeader
        title="Migration ELM"
        subtitle="PRJ-ELM-2026"
        actions={<button type="button">Modifier</button>}
      />,
    )

    expect(screen.getByText('Migration ELM')).toBeInTheDocument()
    expect(screen.getByText('PRJ-ELM-2026')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Modifier' })).toBeInTheDocument()
  })

  it('renders without a subtitle when none is provided', () => {
    render(<DetailPageHeader title="Commande CMD-001" />)

    expect(screen.getByText('Commande CMD-001')).toBeInTheDocument()
  })
})
