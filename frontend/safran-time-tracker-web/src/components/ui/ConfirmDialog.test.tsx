import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ConfirmDialog } from './ConfirmDialog'

describe('ConfirmDialog', () => {
  it('calls onConfirm when the confirmation button is clicked', async () => {
    const onConfirm = vi.fn()
    const user = userEvent.setup()
    render(
      <ConfirmDialog
        open
        title="Archiver le projet ?"
        description="Cette action est réversible."
        onConfirm={onConfirm}
        onCancel={vi.fn()}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Confirmer' }))

    expect(onConfirm).toHaveBeenCalledOnce()
  })

  it('calls onCancel when the cancel button is clicked', async () => {
    const onCancel = vi.fn()
    const user = userEvent.setup()
    render(
      <ConfirmDialog
        open
        title="Supprimer ?"
        description="Action destructrice."
        onConfirm={vi.fn()}
        onCancel={onCancel}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Annuler' }))

    expect(onCancel).toHaveBeenCalledOnce()
  })

  it('disables both actions while loading', () => {
    render(
      <ConfirmDialog
        open
        title="Supprimer ?"
        description="Action destructrice."
        loading
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.getByRole('button', { name: 'Annuler' })).toBeDisabled()
  })
})
