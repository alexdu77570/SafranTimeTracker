import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import {
  fetchProjectPlanningOverview,
  type ProjectPlanningOverviewParams,
} from '../../../api/endpoints/projectPlanning'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchResources } from '../../../api/endpoints/resources'
import type { ProjectPlanningRowDto } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { FilterBar } from '../../../components/ui/FilterBar'
import { StatusBadge } from '../../../components/ui/StatusBadge'

/**
 * Planning projet (§18) : vue transverse tous projets, tableau semaine par semaine. Entièrement
 * agrégée côté serveur (GET /api/v1/project-planning) — décision actée avec l'utilisateur à
 * l'ouverture du Lot 10 pour éviter les N appels frontend, incompatibles avec une pagination/un
 * tri serveur corrects.
 */
export function ProjectPlanningPage() {
  const [projectId, setProjectId] = useState('')
  const [resourceId, setResourceId] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [departmentId, setDepartmentId] = useState('')
  const [teamId, setTeamId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [surcharge, setSurcharge] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const projectsQuery = useQuery({ queryKey: ['projects', 'all'], queryFn: () => fetchProjects() })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const departmentsQuery = useQuery({
    queryKey: ['departments', 'all'],
    queryFn: () => fetchDepartments(),
  })
  const servicesQuery = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teamsQuery = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })

  const projectLabel = new Map((projectsQuery.data?.items ?? []).map((p) => [p.id, p.nom]))
  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )

  const filters: ProjectPlanningOverviewParams = {
    page,
    pageSize,
    projectId: projectId || undefined,
    resourceId: resourceId || undefined,
    serviceId: serviceId || undefined,
    departmentId: departmentId || undefined,
    teamId: teamId || undefined,
    from: from || undefined,
    to: to || undefined,
    surcharge: surcharge === '' ? undefined : surcharge === 'true',
  }
  const query = useQuery({
    queryKey: ['project-planning', filters],
    queryFn: () => fetchProjectPlanningOverview(filters),
  })

  const columns: GridColDef<ProjectPlanningRowDto>[] = [
    { field: 'weekStartDate', headerName: 'Semaine', width: 110 },
    {
      field: 'projectId',
      headerName: 'Projet',
      width: 180,
      valueFormatter: (value: string) => projectLabel.get(value) ?? value,
    },
    {
      field: 'resourceId',
      headerName: 'Ressource',
      width: 180,
      valueFormatter: (value: string) => resourceLabel.get(value) ?? value,
    },
    {
      field: 'chargePlanifieeInitiale',
      headerName: 'Prévu (initial)',
      width: 130,
      valueFormatter: (value: number) => `${value} h`,
    },
    {
      field: 'chargePlanifieeAjustee',
      headerName: 'Prévu (ajusté)',
      width: 130,
      valueFormatter: (value: number | null) => (value !== null ? `${value} h` : '—'),
    },
    {
      field: 'chargeRealisee',
      headerName: 'Réalisé',
      width: 110,
      valueFormatter: (value: number) => `${value} h`,
    },
    {
      field: 'ecartPrevuRealise',
      headerName: 'Écart',
      width: 100,
      valueFormatter: (value: number) => `${value} h`,
    },
    {
      field: 'capaciteReelle',
      headerName: 'Capacité disponible',
      width: 150,
      valueFormatter: (value: number) => `${value} h`,
    },
    {
      field: 'surcharge',
      headerName: 'Alerte',
      width: 120,
      renderCell: (params) =>
        params.value ? (
          <StatusBadge label="Surcharge" tone="error" />
        ) : (
          <StatusBadge label="OK" tone="success" />
        ),
    },
  ]

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Planning projet</Typography>

      <FilterBar
        onReset={() => {
          setProjectId('')
          setResourceId('')
          setServiceId('')
          setDepartmentId('')
          setTeamId('')
          setFrom('')
          setTo('')
          setSurcharge('')
        }}
      >
        <TextField
          select
          size="small"
          label="Projet"
          value={projectId}
          onChange={(e) => setProjectId(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(projectsQuery.data?.items ?? []).map((p) => (
            <MenuItem key={p.id} value={p.id}>
              {p.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Ressource"
          value={resourceId}
          onChange={(e) => setResourceId(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Service"
          value={serviceId}
          onChange={(e) => setServiceId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(servicesQuery.data?.items ?? []).map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Département"
          value={departmentId}
          onChange={(e) => setDepartmentId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(departmentsQuery.data?.items ?? []).map((d) => (
            <MenuItem key={d.id} value={d.id}>
              {d.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Équipe"
          value={teamId}
          onChange={(e) => setTeamId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(teamsQuery.data?.items ?? []).map((t) => (
            <MenuItem key={t.id} value={t.id}>
              {t.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="date"
          label="Du"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          size="small"
          type="date"
          label="Au"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          select
          size="small"
          label="Surcharge"
          value={surcharge}
          onChange={(e) => setSurcharge(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(indifférent)</MenuItem>
          <MenuItem value="true">En surcharge</MenuItem>
          <MenuItem value="false">Sans surcharge</MenuItem>
        </TextField>
      </FilterBar>

      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        getRowId={(row) => `${row.projectId}-${row.resourceId}-${row.weekStartDate}`}
        emptyLabel="Aucune ligne de planning pour ce filtre."
      />
    </Stack>
  )
}
