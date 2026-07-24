import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { TestProviders } from '../test/testUtils'
import { pagedResult } from '../test/fixtures'
import { usePagedQuery } from './usePagedQuery'

describe('usePagedQuery', () => {
  it('starts at page 1 with a default page size of 20 and exposes the query result', async () => {
    const queryFn = vi.fn(async () => pagedResult([{ id: '1' }], 100))

    const { result } = renderHook(() => usePagedQuery('items', queryFn), { wrapper: TestProviders })

    expect(result.current.page).toBe(1)
    expect(result.current.pageSize).toBe(20)
    await waitFor(() => expect(result.current.query.isSuccess).toBe(true))
    expect(queryFn).toHaveBeenCalledWith({ page: 1, pageSize: 20 })
    expect(result.current.query.data?.items).toEqual([{ id: '1' }])
  })

  it('re-fetches with the new page/pageSize when setPage/setPageSize are called', async () => {
    const queryFn = vi.fn(async () => pagedResult([{ id: '1' }], 100))

    const { result } = renderHook(() => usePagedQuery('items', queryFn), { wrapper: TestProviders })
    await waitFor(() => expect(result.current.query.isSuccess).toBe(true))

    act(() => {
      result.current.setPage(2)
      result.current.setPageSize(50)
    })

    await waitFor(() => expect(queryFn).toHaveBeenCalledWith({ page: 2, pageSize: 50 }))
  })
})
