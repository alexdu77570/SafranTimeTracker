import { afterEach, describe, expect, it } from 'vitest'
import { getStoredIdentifiant, setStoredIdentifiant } from './demoIdentityStorage'

afterEach(() => {
  localStorage.clear()
})

describe('demoIdentityStorage', () => {
  it('returns null when no identity has been chosen yet', () => {
    expect(getStoredIdentifiant()).toBeNull()
  })

  it('persists the chosen identifiant to localStorage', () => {
    setStoredIdentifiant('s636140')

    expect(getStoredIdentifiant()).toBe('s636140')
  })

  it('clears the stored identifiant when set to null', () => {
    setStoredIdentifiant('s636140')

    setStoredIdentifiant(null)

    expect(getStoredIdentifiant()).toBeNull()
  })
})
