import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

// L'auto-cleanup intégré de @testing-library/react ne s'active que si `afterEach` est global
// (ex. Jest, ou Vitest en mode `globals: true`) — ce projet garde `globals: false`
// (CLAUDE.md §9 : imports explicites) et déclare donc ce nettoyage lui-même.
afterEach(() => {
  cleanup()
})
