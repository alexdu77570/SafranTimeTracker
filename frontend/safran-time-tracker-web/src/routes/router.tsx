import { createBrowserRouter } from 'react-router-dom'
import { AppLayoutPlaceholder } from './AppLayoutPlaceholder'

/**
 * Squelette de routage (CLAUDE.md §9). Une seule route technique de vérification en Lot 0 ;
 * les routes fonctionnelles (Tableau de bord, Temps, Projets, ...) arrivent à partir du Lot 1.
 */
export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayoutPlaceholder />,
  },
])
