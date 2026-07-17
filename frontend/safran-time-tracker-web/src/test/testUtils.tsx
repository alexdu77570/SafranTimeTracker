import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { DemoIdentityProvider } from '../auth/DemoIdentityProvider'

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
    },
  })
}

/** Enveloppe commune aux tests qui dépendent de l'identité de démonstration (useCurrentUser,
 * PermissionGuard, Sidebar) : QueryClientProvider (sans retry, pour des tests déterministes) +
 * DemoIdentityProvider. */
export function DemoTestProviders({ children }: { children: ReactNode }) {
  const client = createTestQueryClient()
  return (
    <QueryClientProvider client={client}>
      <DemoIdentityProvider>{children}</DemoIdentityProvider>
    </QueryClientProvider>
  )
}
