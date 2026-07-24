import { useQuery, type UseQueryResult } from '@tanstack/react-query'
import { useState } from 'react'
import type { PagedResult, PaginationQuery } from '../api/types'

/** Pagination page/pageSize + `useQuery` associé (page 1, taille 20 par défaut) : partagé par les
 * écrans de liste dont la seule variable de requête est la pagination (pas de filtre), pour ne pas
 * dupliquer ce triptyque `useState`/`useState`/`useQuery` (CLAUDE.md §5). */
export function usePagedQuery<T>(
  queryKey: string,
  queryFn: (pagination: Required<PaginationQuery>) => Promise<PagedResult<T>>,
): {
  page: number
  setPage: (page: number) => void
  pageSize: number
  setPageSize: (pageSize: number) => void
  query: UseQueryResult<PagedResult<T>>
} {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const query = useQuery({
    queryKey: [queryKey, page, pageSize],
    queryFn: () => queryFn({ page, pageSize }),
  })

  return { page, setPage, pageSize, setPageSize, query }
}
