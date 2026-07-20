import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AbsenceTypesTab } from './AbsenceTypesTab'

/** AbsenceType est un enum C#, pas une entité administrable (docs/IMPLEMENTATION_STATUS.md, Lot
 * 3) : cet onglet doit rester une liste fixe, jamais un formulaire de création. */
describe('AbsenceTypesTab', () => {
  it('renders the fixed list of absence types without any create action', () => {
    render(<AbsenceTypesTab />)

    expect(screen.getByText('Congé')).toBeInTheDocument()
    expect(screen.getByText('RTT')).toBeInTheDocument()
    expect(screen.getByText('Maladie')).toBeInTheDocument()
    expect(screen.getByText('Formation')).toBeInTheDocument()
    expect(screen.getByText('Déplacement')).toBeInTheDocument()
    expect(screen.getByText('Indisponible')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Créer' })).not.toBeInTheDocument()
  })
})
