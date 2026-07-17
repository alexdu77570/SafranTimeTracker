import { createBrowserRouter, type RouteObject } from 'react-router-dom'
import { AppLayout } from '../components/ui/AppLayout'
import { navigation } from './navigation'
import { PlaceholderPage } from './pages/PlaceholderPage'
import { RouteErrorBoundary } from './RouteErrorBoundary'

/**
 * Routage complet (cahier des charges §8.2, Lot 7) : une route par entrée de navigation, rendant
 * un écran neutre "à venir" — aucun écran métier n'est construit dans ce lot (voir
 * docs/ROADMAP.md). Générées depuis `navigation` pour ne jamais faire diverger sidebar et routes.
 */
const children: RouteObject[] = navigation.map((entry) => ({
  path: entry.path === '/' ? undefined : entry.path.slice(1),
  index: entry.path === '/',
  element: <PlaceholderPage title={entry.label} />,
  handle: { crumb: entry.label },
}))

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorBoundary />,
    children,
  },
])
