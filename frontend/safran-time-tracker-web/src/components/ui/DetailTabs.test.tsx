import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { DetailTabs } from './DetailTabs'

describe('DetailTabs', () => {
  it('renders one tab per label and marks the selected one', () => {
    render(<DetailTabs labels={['Synthèse', 'Participants']} value={0} onChange={vi.fn()} />)

    expect(screen.getByRole('tab', { name: 'Synthèse', selected: true })).toBeInTheDocument()
    expect(screen.getByRole('tab', { name: 'Participants', selected: false })).toBeInTheDocument()
  })

  it('calls onChange with the clicked tab index', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<DetailTabs labels={['Synthèse', 'Participants']} value={0} onChange={onChange} />)

    await user.click(screen.getByRole('tab', { name: 'Participants' }))

    expect(onChange).toHaveBeenCalledWith(1)
  })
})
