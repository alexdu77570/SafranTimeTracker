import axios from 'axios'

/** Forme d'une réponse d'erreur `ProblemDetails` (CLAUDE.md §10/§12), avec l'extension
 * `errors` ajoutée par ASP.NET Core pour les échecs de validation FluentValidation. */
interface ProblemDetailsResponse {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

/** Point unique de traduction d'une erreur API en message affichable (CLAUDE.md §8.3 : "messages
 * d'erreur compréhensibles"). Toute vue consomme cette fonction plutôt que de lire `error.message`. */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError<ProblemDetailsResponse>(error)) {
    const problem = error.response?.data

    if (problem?.errors) {
      const firstError = Object.values(problem.errors)[0]?.[0]
      if (firstError) {
        return firstError
      }
    }

    if (problem?.detail) {
      return problem.detail
    }

    if (problem?.title) {
      return problem.title
    }

    if (error.response?.status === 404) {
      return 'Ressource introuvable.'
    }

    if (!error.response) {
      return 'Le serveur est injoignable. Vérifiez votre connexion.'
    }
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Une erreur inattendue est survenue.'
}
