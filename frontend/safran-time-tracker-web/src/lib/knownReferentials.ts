/**
 * Deux référentiels backend (`CompanyType`, `Role`) n'ont aucun endpoint `GET` (écarts constatés
 * au Lot 8, voir docs/IMPLEMENTATION_STATUS.md) : ils ne sont ni listés ni administrables, seules
 * les valeurs seedées au Lot 1 existent (`SeedIds.cs`, identifiants déterministes). Correction
 * propre = ajouter les endpoints en backend, hors périmètre "aucune évolution backend" de cette
 * partie du Lot 8 (ROADMAP.md) : ces valeurs sont donc portées ici en dur, en un seul endroit
 * partagé (CLAUDE.md §5), à remplacer par de vrais référentiels dès que les endpoints existeront.
 */
export const COMPANY_TYPE_OPTIONS = [
  { value: '00000000-0000-0000-0004-000000000001', label: 'Interne' },
  { value: '00000000-0000-0000-0004-000000000002', label: 'Externe' },
]

export const ROLE_LABELS: Record<string, string> = {
  '00000000-0000-0000-0001-000000000001': 'Ingénieur',
  '00000000-0000-0000-0001-000000000002': 'Responsable de service',
  '00000000-0000-0000-0001-000000000003': 'Responsable de département',
  '00000000-0000-0000-0001-000000000004': 'Administrateur',
}

export function getRoleLabel(roleId: string): string {
  return ROLE_LABELS[roleId] ?? roleId
}
