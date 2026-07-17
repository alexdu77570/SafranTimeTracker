import { isRouteErrorResponse, useRouteError } from 'react-router-dom'
import { AlertTriangle } from 'lucide-react'
import { EmptyState } from '../components/ui/EmptyState'

/** Gestion centralisée des erreurs de routage (cahier des charges §8.3 : "messages d'erreur
 * compréhensibles"), utilisée comme `errorElement` par le routeur — évite qu'une exception de
 * rendu dans un écran ne fasse disparaître l'application entière. */
export function RouteErrorBoundary() {
  const error = useRouteError()

  const description = isRouteErrorResponse(error)
    ? `Erreur ${error.status} : ${error.statusText}`
    : "Une erreur inattendue est survenue lors de l'affichage de cette page."

  return (
    <EmptyState
      icon={AlertTriangle}
      title="Impossible d'afficher cette page"
      description={description}
    />
  )
}
