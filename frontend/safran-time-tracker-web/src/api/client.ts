import axios from 'axios'
import { getStoredIdentifiant } from '../auth/demoIdentityStorage'

/**
 * Client HTTP centralisé et unique de l'application (CLAUDE.md §9) : aucun appel API ne doit
 * passer par un axios/fetch isolé ailleurs dans le code. L'URL de base reste relative
 * (/api/v1 par défaut) pour ne jamais coder en dur le nom du serveur (cahier des charges §7.1).
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
})

/** Authentification de démonstration (CLAUDE.md §17) : pilote l'en-tête `X-Demo-User` existant,
 * résolu côté serveur par `DemoCurrentUserProvider`. Aucun token, aucune session — l'identifiant
 * choisi par le sélecteur d'identité (Header) est simplement rejoué sur chaque requête. */
export const DEMO_USER_HEADER = 'X-Demo-User'

apiClient.interceptors.request.use((config) => {
  const identifiant = getStoredIdentifiant()
  if (identifiant) {
    config.headers.set(DEMO_USER_HEADER, identifiant)
  }
  return config
})
