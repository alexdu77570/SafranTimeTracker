import { createContext } from 'react'

export interface DemoIdentityContextValue {
  /** Identifiant (User.Identifiant) actuellement rejoué dans l'en-tête X-Demo-User, ou null tant
   * qu'aucune identité de démonstration n'a été choisie. */
  identifiant: string | null
  setIdentifiant: (identifiant: string | null) => void
}

export const DemoIdentityContext = createContext<DemoIdentityContextValue | undefined>(undefined)
