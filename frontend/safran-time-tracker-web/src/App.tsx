import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import 'dayjs/locale/fr'
import { RouterProvider } from 'react-router-dom'
import { DemoIdentityProvider } from './auth/DemoIdentityProvider'
import { router } from './routes/router'
import { theme } from './theme/theme'

/** Instance unique du client de cache serveur (CLAUDE.md §9 — TanStack Query) : gestion
 * centralisée des états de chargement/erreur (cahier des charges §8.3). Un seul essai
 * automatique : au-delà, l'écran affiche l'erreur plutôt que de faire patienter silencieusement. */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
})

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="fr">
        <QueryClientProvider client={queryClient}>
          <DemoIdentityProvider>
            <RouterProvider router={router} />
          </DemoIdentityProvider>
        </QueryClientProvider>
      </LocalizationProvider>
    </ThemeProvider>
  )
}

export default App
