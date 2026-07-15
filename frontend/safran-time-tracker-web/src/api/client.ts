import axios from 'axios'

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
