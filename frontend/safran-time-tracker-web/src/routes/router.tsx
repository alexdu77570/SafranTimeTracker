import { createBrowserRouter, type RouteObject } from 'react-router-dom'
import { AppLayout } from '../components/ui/AppLayout'
import { navigation } from './navigation'
import { AbsencesPage } from './pages/absences/AbsencesPage'
import { AdministrationPage } from './pages/administration/AdministrationPage'
import { ApplicationDetailPage } from './pages/applications/ApplicationDetailPage'
import { ApplicationsListPage } from './pages/applications/ApplicationsListPage'
import { AvailabilityPage } from './pages/availability/AvailabilityPage'
import { BudgetsListPage } from './pages/budgets/BudgetsListPage'
import { ChargesPage } from './pages/charges/ChargesPage'
import { CompaniesListPage } from './pages/companies/CompaniesListPage'
import { CompanyDetailPage } from './pages/companies/CompanyDetailPage'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { ImportsPage } from './pages/imports/ImportsPage'
import { MilestonesListPage } from './pages/milestones/MilestonesListPage'
import { OrderDetailPage } from './pages/orders/OrderDetailPage'
import { OrdersListPage } from './pages/orders/OrdersListPage'
import { PlaceholderPage } from './pages/PlaceholderPage'
import { ReportingPage } from './pages/reporting/ReportingPage'
import { ProjectDetailPage } from './pages/projects/ProjectDetailPage'
import { ProjectPlanningPage } from './pages/projects/ProjectPlanningPage'
import { ProjectsListPage } from './pages/projects/ProjectsListPage'
import { ResourceDetailPage } from './pages/resources/ResourceDetailPage'
import { ResourcesListPage } from './pages/resources/ResourcesListPage'
import { RouteErrorBoundary } from './RouteErrorBoundary'
import { TimeEntriesPage } from './pages/timeEntries/TimeEntriesPage'

/**
 * Routage complet (cahier des charges §8.2, Lot 7) : une route par entrée de navigation.
 * Écrans métier construits lot par lot, en remplacement du PlaceholderPage par défaut, sans
 * changer le chemin de route (voir docs/ROADMAP.md). Lot 8 : Ressources, Sociétés, Applications
 * (liste + fiche détail) et Administration. Lot 9 : Temps, Mes absences, Disponibilités. Lot 10 :
 * Projets (liste + fiche détail à 7 onglets) et Planning projet (vue transverse). Lot 11 :
 * Commandes (liste + fiche détail à 5 onglets), Budgets (indicateurs + lignes) et Jalons (vue
 * transverse tableau/timeline/calendrier). Lot 12 : Tableau de bord (index route), Charges,
 * Reporting (opérationnel + financier, exports réels) et Imports (assistant + historique).
 */
const screenOverrides: Record<string, React.ReactNode> = {
  '/': <DashboardPage />,
  '/ressources': <ResourcesListPage />,
  '/societes': <CompaniesListPage />,
  '/applications': <ApplicationsListPage />,
  '/administration': <AdministrationPage />,
  '/temps': <TimeEntriesPage />,
  '/mes-absences': <AbsencesPage />,
  '/disponibilites': <AvailabilityPage />,
  '/projets': <ProjectsListPage />,
  '/planning-projet': <ProjectPlanningPage />,
  '/commandes': <OrdersListPage />,
  '/budgets': <BudgetsListPage />,
  '/jalons': <MilestonesListPage />,
  '/charges': <ChargesPage />,
  '/reporting': <ReportingPage />,
  '/imports': <ImportsPage />,
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
  { path: 'projets/:id', element: <ProjectDetailPage />, handle: { crumb: 'Détail' } },
  { path: 'commandes/:id', element: <OrderDetailPage />, handle: { crumb: 'Détail' } },
)

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorBoundary />,
    children,
  },
])
