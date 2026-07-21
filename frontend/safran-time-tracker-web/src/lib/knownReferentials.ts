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

/**
 * `OperationalRole` (§10.4, rôle opérationnel d'un participant projet §17.2) n'a lui non plus
 * aucun endpoint `GET` (même écart que `CompanyType`/`Role` ci-dessus, constaté à l'ouverture du
 * Lot 10) : seules les valeurs seedées au Lot 1 existent. Même contournement, même principe — à
 * remplacer par un vrai référentiel dès qu'un endpoint existera.
 */
export const OPERATIONAL_ROLE_OPTIONS = [
  { value: '00000000-0000-0000-0003-000000000001', label: 'RUN' },
  { value: '00000000-0000-0000-0003-000000000002', label: 'Build' },
  { value: '00000000-0000-0000-0003-000000000003', label: 'Amélioration continue' },
  { value: '00000000-0000-0000-0003-000000000004', label: 'Chef de Projet' },
  { value: '00000000-0000-0000-0003-000000000005', label: 'Coordinateur IT' },
]

export function getOperationalRoleLabel(operationalRoleId: string | null): string {
  if (!operationalRoleId) {
    return '—'
  }
  return (
    OPERATIONAL_ROLE_OPTIONS.find((o) => o.value === operationalRoleId)?.label ?? operationalRoleId
  )
}
