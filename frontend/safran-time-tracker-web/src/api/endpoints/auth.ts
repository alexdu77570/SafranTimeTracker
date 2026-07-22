import { apiClient } from '../client'
import type { AuthSessionDto } from '../types'

/** Authentification simulée sessionnée (Lot 13) : établit/révoque la session cookie côté serveur —
 * voir `auth/DemoIdentityProvider.tsx`, seul consommateur de ce module. */
export async function createDemoSession(
  identifiant: string,
  rememberMe = false,
): Promise<AuthSessionDto> {
  const response = await apiClient.post<AuthSessionDto>('/auth/sessions', {
    identifiant,
    rememberMe,
  })
  return response.data
}

export async function revokeDemoSession(): Promise<void> {
  await apiClient.delete('/auth/sessions')
}
