import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
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

/** QueryClientProvider (sans retry, pour des tests déterministes) + LocalizationProvider (requis
 * par tout composant utilisant `FormDatePicker`/`DatePicker` MUI, sous peine d'exception "Can not
 * find the date and time pickers localization context") — même arbre que `App.tsx`, sans
 * l'identité de démonstration. */
export function TestProviders({ children }: { children: ReactNode }) {
  const client = createTestQueryClient()
  return (
    <QueryClientProvider client={client}>
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="fr">
        {children}
      </LocalizationProvider>
    </QueryClientProvider>
  )
}

/** Enveloppe commune aux tests qui dépendent en plus de l'identité de démonstration
 * (useCurrentUser, PermissionGuard, Sidebar). */
export function DemoTestProviders({ children }: { children: ReactNode }) {
  const client = createTestQueryClient()
  return (
    <QueryClientProvider client={client}>
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="fr">
        <DemoIdentityProvider>{children}</DemoIdentityProvider>
      </LocalizationProvider>
    </QueryClientProvider>
  )
}
