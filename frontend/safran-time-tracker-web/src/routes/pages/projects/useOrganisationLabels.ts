import { useQuery } from '@tanstack/react-query'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchClients } from '../../../api/endpoints/clients'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchProjectStatuses } from '../../../api/endpoints/projectStatuses'
import { fetchProjectTypes } from '../../../api/endpoints/projectTypes'
import { fetchResources } from '../../../api/endpoints/resources'

/** Libellés organisationnels partagés par ProjectDetailPage et ses onglets (Synthèse, Planning,
 * Temps, Jalons) — un seul point de récupération pour ne pas dupliquer ces 8 requêtes (CLAUDE.md §5). */
export function useOrganisationLabels() {
  const applications = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const resources = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const departments = useQuery({
    queryKey: ['departments', 'all'],
    queryFn: () => fetchDepartments(),
  })
  const services = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teams = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const statuses = useQuery({
    queryKey: ['project-statuses', 'all'],
    queryFn: () => fetchProjectStatuses(),
  })
  const projectTypes = useQuery({
    queryKey: ['project-types', 'all'],
    queryFn: () => fetchProjectTypes(),
  })
  const clients = useQuery({ queryKey: ['clients', 'all'], queryFn: () => fetchClients() })

  return {
    applicationLabel: new Map((applications.data?.items ?? []).map((a) => [a.id, a.nom])),
    resourceLabel: new Map(
      (resources.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
    ),
    departmentLabel: new Map((departments.data?.items ?? []).map((d) => [d.id, d.nom])),
    serviceLabel: new Map((services.data?.items ?? []).map((s) => [s.id, s.nom])),
    teamLabel: new Map((teams.data?.items ?? []).map((t) => [t.id, t.nom])),
    statusLabel: new Map((statuses.data?.items ?? []).map((s) => [s.id, s.libelle])),
    projectTypeLabel: new Map((projectTypes.data?.items ?? []).map((t) => [t.id, t.libelle])),
    clientLabel: new Map((clients.data?.items ?? []).map((c) => [c.id, c.nom])),
  }
}

export type Labels = ReturnType<typeof useOrganisationLabels>
