import { useContext } from 'react'
import { DemoIdentityContext } from './DemoIdentityContext'

export function useDemoIdentity() {
  const context = useContext(DemoIdentityContext)
  if (!context) {
    throw new Error('useDemoIdentity must be used within a DemoIdentityProvider')
  }
  return context
}
