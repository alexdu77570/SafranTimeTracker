import axios from 'axios'

/**
 * Client HTTP centralisé et unique de l'application (CLAUDE.md §9) : aucun appel API ne doit
 * passer par un axios/fetch isolé ailleurs dans le code. L'URL de base reste relative
 * (/api/v1 par défaut) pour ne jamais coder en dur le nom du serveur (cahier des charges §7.1).
 *
 * Authentification de démonstration sessionnée (CLAUDE.md §17, Lot 13) : l'identité est portée par
 * un cookie de session serveur (posé par `POST /auth/sessions`, voir `api/endpoints/auth.ts` et
 * `auth/DemoIdentityProvider.tsx`), plus rejouée manuellement sur chaque requête. Aucun
 * `withCredentials` nécessaire : le frontend et l'API sont toujours servis sous la même origine
 * (proxy Vite en développement, même nom DNS derrière IIS en production — CLAUDE.md §18), donc le
 * cookie circule comme tout cookie same-origin.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
})
