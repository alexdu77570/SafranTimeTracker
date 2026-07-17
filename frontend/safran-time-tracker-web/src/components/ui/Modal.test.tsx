import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { Modal } from './Modal'

describe('Modal', () => {
  it('renders the title and children when open', () => {
    render(
      <Modal open title="Détail du jalon" onClose={vi.fn()}>
        <p>Contenu du jalon</p>
      </Modal>,
    )

    expect(screen.getByText('Détail du jalon')).toBeInTheDocument()
    expect(screen.getByText('Contenu du jalon')).toBeInTheDocument()
  })

  it('does not render its content when closed', () => {
    render(
      <Modal open={false} title="Détail du jalon" onClose={vi.fn()}>
        <p>Contenu du jalon</p>
      </Modal>,
    )

    expect(screen.queryByText('Contenu du jalon')).not.toBeInTheDocument()
  })

  it('calls onClose when the close button is clicked', async () => {
    const onClose = vi.fn()
    const user = userEvent.setup()
    render(
      <Modal open title="Détail du jalon" onClose={onClose}>
        <p>Contenu</p>
      </Modal>,
    )

    await user.click(screen.getByRole('button', { name: 'Fermer' }))

    expect(onClose).toHaveBeenCalledOnce()
  })
})
