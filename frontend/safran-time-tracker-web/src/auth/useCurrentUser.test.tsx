import { renderHook, waitFor } from '@testing-library/react'
import { act } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemoTestProviders } from '../test/testUtils'
import { demoSessionFixture, demoUserFixture, pagedResult } from '../test/fixtures'
import { setStoredIdentifiant } from './demoIdentityStorage'
import { useCurrentUser } from './useCurrentUser'

vi.mock('../api/endpoints/users', () => ({
  fetchUsers: vi.fn(async () =>
    pagedResult(
      [
        demoUserFixture({
          nom: 'Dupont',
          prenom: 'Alice',
          email: 'alice.dupont@safran.com',
          dateArrivee: '2024-01-01',
          resourceId: null,
          accesGlobal: false,
          permissionIds: ['perm-audit'],
          effectivePermissionCodes: ['AUDIT_VIEW'],
        }),
      ],
      100,
    ),
  ),
}))

vi.mock('../api/endpoints/auth', () => ({
  createDemoSession: vi.fn(async () => demoSessionFixture()),
  revokeDemoSession: vi.fn(async () => undefined),
}))

afterEach(() => {
  localStorage.clear()
})

describe('useCurrentUser', () => {
  it('resolves no user and no permissions when no identity is chosen', async () => {
    const { result } = renderHook(() => useCurrentUser(), { wrapper: DemoTestProviders })

    await waitFor(() => expect(result.current.isLoading).toBe(false))

    expect(result.current.user).toBeUndefined()
    expect(result.current.permissionCodes).toEqual([])
    expect(result.current.hasPermission('AUDIT_VIEW')).toBe(false)
  })

  it('resolves the matching user and reads its effective permission codes', async () => {
    setStoredIdentifiant('s636140')

    const { result } = renderHook(() => useCurrentUser(), { wrapper: DemoTestProviders })

    await waitFor(() => expect(result.current.user).toBeDefined())

    expect(result.current.user?.identifiant).toBe('s636140')
    expect(result.current.permissionCodes).toEqual(['AUDIT_VIEW'])
    expect(result.current.hasPermission('AUDIT_VIEW')).toBe(true)
    expect(result.current.hasPermission('FINANCIAL_DATA_VIEW')).toBe(false)
  })

  it('updates the resolved user when setIdentifiant is called', async () => {
    const { result } = renderHook(() => useCurrentUser(), { wrapper: DemoTestProviders })

    await waitFor(() => expect(result.current.isLoading).toBe(false))

    act(() => {
      result.current.setIdentifiant('s636140')
    })

    await waitFor(() => expect(result.current.user?.identifiant).toBe('s636140'))
  })
})
