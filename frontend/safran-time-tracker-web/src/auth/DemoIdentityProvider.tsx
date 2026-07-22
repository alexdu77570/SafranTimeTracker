import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { createDemoSession, revokeDemoSession } from '../api/endpoints/auth'
import { DemoIdentityContext } from './DemoIdentityContext'
import { getStoredIdentifiant, setStoredIdentifiant } from './demoIdentityStorage'

/**
 * Identité de démonstration sessionnée (CLAUDE.md §17, Lot 13) : `setIdentifiant` établit ou révoque
 * la session cookie côté serveur (`POST`/`DELETE /auth/sessions`) en plus de la préférence
 * d'affichage locale (`localStorage`, non sensible). L'appel réseau est délibérément non attendu
 * (mise à jour optimiste de l'état local, comme avant ce lot) : un identifiant du sélecteur provient
 * toujours de la liste des utilisateurs actifs réels, l'échec reste un cas d'exception avalé plutôt
 * qu'une reprise d'état complexe pour un simple sélecteur de démonstration.
 *
 * Restauration au montage : le cookie de session ne survit pas forcément (expiration, redémarrage
 * de l'API en développement) alors que l'identifiant choisi reste mémorisé localement — sans cette
 * restauration, l'affichage resterait "connecté" alors que tous les appels gardés échoueraient
 * silencieusement en 403.
 */
export function DemoIdentityProvider({ children }: { children: ReactNode }) {
  const [identifiant, setIdentifiantState] = useState<string | null>(() => getStoredIdentifiant())
  const hasRestoredRef = useRef(false)

  useEffect(() => {
    if (hasRestoredRef.current) {
      return
    }
    hasRestoredRef.current = true
    if (identifiant) {
      createDemoSession(identifiant).catch(() => {})
    }
  }, [identifiant])

  const setIdentifiant = useCallback((next: string | null) => {
    setStoredIdentifiant(next)
    setIdentifiantState(next)
    if (next) {
      createDemoSession(next).catch(() => {})
    } else {
      revokeDemoSession().catch(() => {})
    }
  }, [])

  const value = useMemo(() => ({ identifiant, setIdentifiant }), [identifiant, setIdentifiant])

  return <DemoIdentityContext.Provider value={value}>{children}</DemoIdentityContext.Provider>
}
