import { createBrowserRouter, type RouteObject } from 'react-router-dom'
import { AppLayout } from '../components/ui/AppLayout'
import { navigation } from './navigation'
import { AdministrationPage } from './pages/administration/AdministrationPage'
import { ApplicationDetailPage } from './pages/applications/ApplicationDetailPage'
import { ApplicationsListPage } from './pages/applications/ApplicationsListPage'
import { CompaniesListPage } from './pages/companies/CompaniesListPage'
import { CompanyDetailPage } from './pages/companies/CompanyDetailPage'
import { PlaceholderPage } from './pages/PlaceholderPage'
import { ResourceDetailPage } from './pages/resources/ResourceDetailPage'
import { ResourcesListPage } from './pages/resources/ResourcesListPage'
import { RouteErrorBoundary } from './RouteErrorBoundary'

/**
 * Routage complet (cahier des charges §8.2, Lot 7) : une route par entrée de navigation.
 * Écrans métier construits lot par lot, en remplacement du PlaceholderPage par défaut, sans
 * changer le chemin de route (voir docs/ROADMAP.md). Lot 8 : Ressources, Sociétés, Applications
 * (liste + fiche détail) et Administration.
 */
const screenOverrides: Record<string, React.ReactNode> = {
  '/ressources': <ResourcesListPage />,
  '/societes': <CompaniesListPage />,
  '/applications': <ApplicationsListPage />,
  '/administration': <AdministrationPage />,
}

const children: RouteObject[] = navigation.map((entry) => ({
  path: entry.path === '/' ? undefined : entry.path.slice(1),
  index: entry.path === '/',
  element: screenOverrides[entry.path] ?? <PlaceholderPage title={entry.label} />,
  handle: { crumb: entry.label },
}))

children.push(
  { path: 'ressources/:id', element: <ResourceDetailPage />, handle: { crumb: 'Détail' } },
  { path: 'societes/:id', element: <CompanyDetailPage />, handle: { crumb: 'Détail' } },
  { path: 'applications/:id', element: <ApplicationDetailPage />, handle: { crumb: 'Détail' } },
)

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorBoundary />,
    children,
  },
])
