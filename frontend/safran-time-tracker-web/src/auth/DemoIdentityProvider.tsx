import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { DemoIdentityContext } from './DemoIdentityContext'
import { getStoredIdentifiant, setStoredIdentifiant } from './demoIdentityStorage'

export function DemoIdentityProvider({ children }: { children: ReactNode }) {
  const [identifiant, setIdentifiantState] = useState<string | null>(() => getStoredIdentifiant())

  const setIdentifiant = useCallback((next: string | null) => {
    setStoredIdentifiant(next)
    setIdentifiantState(next)
  }, [])

  const value = useMemo(() => ({ identifiant, setIdentifiant }), [identifiant, setIdentifiant])

  return <DemoIdentityContext.Provider value={value}>{children}</DemoIdentityContext.Provider>
}
