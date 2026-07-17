/**
 * Persistance de l'identité de démonstration choisie (localStorage, préférence non sensible —
 * CLAUDE.md §9). Module neutre séparé de `DemoIdentityContext` pour que l'intercepteur axios de
 * `api/client.ts` puisse lire l'identifiant courant sans dépendre de React ni créer d'import
 * circulaire entre la couche API et la couche auth.
 */
const STORAGE_KEY = 'safran-time-tracker.demo-identifiant'

export function getStoredIdentifiant(): string | null {
  return localStorage.getItem(STORAGE_KEY)
}

export function setStoredIdentifiant(identifiant: string | null): void {
  if (identifiant) {
    localStorage.setItem(STORAGE_KEY, identifiant)
  } else {
    localStorage.removeItem(STORAGE_KEY)
  }
}
