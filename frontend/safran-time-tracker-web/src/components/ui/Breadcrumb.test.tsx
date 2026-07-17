import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { createMemoryRouter, RouterProvider } from 'react-router-dom'
import { Breadcrumb } from './Breadcrumb'

function renderAt(initialPath: string) {
  const router = createMemoryRouter(
    [
      {
        path: '/',
        children: [
          {
            path: 'projets',
            handle: { crumb: 'Projets' },
            children: [
              { index: true, element: <Breadcrumb /> },
              { path: ':id', element: <Breadcrumb />, handle: { crumb: 'PRJ-VTOM-2026' } },
            ],
          },
        ],
      },
    ],
    { initialEntries: [initialPath] },
  )

  return render(<RouterProvider router={router} />)
}

describe('Breadcrumb', () => {
  it('renders nothing when there is only one crumb (list page)', () => {
    const { container } = renderAt('/projets')

    expect(container).toBeEmptyDOMElement()
  })

  it('renders the full trail on a detail page, with the last crumb non-clickable', () => {
    renderAt('/projets/PRJ-VTOM-2026')

    expect(screen.getByRole('link', { name: 'Projets' })).toHaveAttribute('href', '/projets')
    expect(screen.getByText('PRJ-VTOM-2026')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'PRJ-VTOM-2026' })).not.toBeInTheDocument()
  })
})
