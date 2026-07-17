import { describe, expect, it } from 'vitest'
import { getErrorMessage } from './errors'

/** `axios.isAxiosError` fait un simple duck-typing sur `isAxiosError === true` : inutile de
 * mocker le module axios, un objet qui porte ce marqueur suffit à traverser getErrorMessage
 * exactement comme une vraie erreur réseau. */
function makeAxiosError(overrides: { status?: number; data?: unknown; hasResponse?: boolean }) {
  return {
    isAxiosError: true,
    message: 'Request failed',
    response:
      overrides.hasResponse === false
        ? undefined
        : { status: overrides.status ?? 400, data: overrides.data },
  }
}

describe('getErrorMessage', () => {
  it('extracts the first FluentValidation error message', () => {
    const error = makeAxiosError({ data: { errors: { Code: ['Le code est obligatoire.'] } } })

    expect(getErrorMessage(error)).toBe('Le code est obligatoire.')
  })

  it('falls back to the ProblemDetails detail when there are no field errors', () => {
    const error = makeAxiosError({ data: { detail: 'Commande déjà clôturée.' } })

    expect(getErrorMessage(error)).toBe('Commande déjà clôturée.')
  })

  it('falls back to the ProblemDetails title when there is no detail', () => {
    const error = makeAxiosError({ data: { title: 'Conflit métier' } })

    expect(getErrorMessage(error)).toBe('Conflit métier')
  })

  it('returns a dedicated message for 404 responses without a ProblemDetails body', () => {
    const error = makeAxiosError({ status: 404, data: undefined })

    expect(getErrorMessage(error)).toBe('Ressource introuvable.')
  })

  it('returns a network message when the server is unreachable', () => {
    const error = makeAxiosError({ hasResponse: false })

    expect(getErrorMessage(error)).toBe('Le serveur est injoignable. Vérifiez votre connexion.')
  })

  it('falls back to a generic message for a non-axios error', () => {
    expect(getErrorMessage('unexpected')).toBe('Une erreur inattendue est survenue.')
  })
})
