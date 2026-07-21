import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchProjects, type ProjectListParams } from '../../../api/endpoints/projects'
import { fetchProjectStatuses } from '../../../api/endpoints/projectStatuses'
import { fetchResources } from '../../../api/endpoints/resources'
import type { ProjectDto } from '../../../api/types'
import { ProjectRiskLevel } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { FilterBar } from '../../../components/ui/FilterBar'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { ProjectCreateForm } from './ProjectForm'

const riskLevelLabel: Record<ProjectRiskLevel, string> = { 0: 'Faible', 1: 'Moyen', 2: 'Élevé' }
const riskLevelTone: Record<ProjectRiskLevel, 'success' | 'warning' | 'error'> = {
  0: 'success',
  1: 'warning',
  2: 'error',
}

/** Liste des projets (§16.1) — vue liste professionnelle, tous les filtres du cahier des charges. */
export function ProjectsListPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [statusId, setStatusId] = useState('')
  const [applicationId, setApplicationId] = useState('')
  const [piloteId, setPiloteId] = useState('')
  const [departmentId, setDepartmentId] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [teamId, setTeamId] = useState('')
  const [niveauRisque, setNiveauRisque] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [alertePlanning, setAlertePlanning] = useState('')
  const [alerteBudget, setAlerteBudget] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [createOpen, setCreateOpen] = useState(false)

  const statusesQuery = useQuery({
    queryKey: ['project-statuses', 'all'],
    queryFn: () => fetchProjectStatuses(),
  })
  const applicationsQuery = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
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

  const statusLabel = new Map((statusesQuery.data?.items ?? []).map((s) => [s.id, s.libelle]))
  const applicationLabel = new Map((applicationsQuery.data?.items ?? []).map((a) => [a.id, a.nom]))
  const piloteLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )

  const filters: ProjectListParams = {
    page,
    pageSize,
    statusId: statusId || undefined,
    applicationId: applicationId || undefined,
    piloteId: piloteId || undefined,
    departmentId: departmentId || undefined,
    serviceId: serviceId || undefined,
    teamId: teamId || undefined,
    niveauRisque: niveauRisque === '' ? undefined : (Number(niveauRisque) as ProjectRiskLevel),
    from: from || undefined,
    to: to || undefined,
    alertePlanning: alertePlanning === '' ? undefined : alertePlanning === 'true',
    alerteBudget: alerteBudget === '' ? undefined : alerteBudget === 'true',
  }
  const query = useQuery({ queryKey: ['projects', filters], queryFn: () => fetchProjects(filters) })

  const columns: GridColDef<ProjectDto>[] = [
    { field: 'nom', headerName: 'Nom', width: 200 },
    { field: 'code', headerName: 'Code', width: 150 },
    {
      field: 'applicationId',
      headerName: 'Application',
      width: 160,
      valueFormatter: (value: string) => applicationLabel.get(value) ?? value,
    },
    {
      field: 'piloteId',
      headerName: 'Pilote',
      width: 160,
      valueFormatter: (value: string) => piloteLabel.get(value) ?? value,
    },
    {
      field: 'statusId',
      headerName: 'Statut',
      width: 130,
      renderCell: (params) => (
        <StatusBadge label={statusLabel.get(params.value) ?? '—'} tone="info" />
      ),
    },
    {
      field: 'niveauRisque',
      headerName: 'Niveau de risque',
      width: 150,
      renderCell: (params) => (
        <StatusBadge
          label={riskLevelLabel[params.value as ProjectRiskLevel]}
          tone={riskLevelTone[params.value as ProjectRiskLevel]}
        />
      ),
    },
    { field: 'dateDebut', headerName: 'Début', width: 110 },
    { field: 'dateFinPrevueInitiale', headerName: 'Fin prévue', width: 110 },
    {
      field: 'financialSummary',
      headerName: 'Budget initial',
      width: 140,
      renderCell: (params) => <FinancialValue value={params.value?.budgetInitial} />,
    },
  ]

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5">Projets</Typography>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer un projet"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>

      <FilterBar
        onReset={() => {
          setStatusId('')
          setApplicationId('')
          setPiloteId('')
          setDepartmentId('')
          setServiceId('')
          setTeamId('')
          setNiveauRisque('')
          setFrom('')
          setTo('')
          setAlertePlanning('')
          setAlerteBudget('')
        }}
      >
        <TextField
          select
          size="small"
          label="Statut"
          value={statusId}
          onChange={(e) => setStatusId(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(statusesQuery.data?.items ?? []).map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.libelle}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Application"
          value={applicationId}
          onChange={(e) => setApplicationId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(applicationsQuery.data?.items ?? []).map((a) => (
            <MenuItem key={a.id} value={a.id}>
              {a.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Pilote"
          value={piloteId}
          onChange={(e) => setPiloteId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
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
          select
          size="small"
          label="Niveau de risque"
          value={niveauRisque}
          onChange={(e) => setNiveauRisque(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          <MenuItem value={String(ProjectRiskLevel.Faible)}>Faible</MenuItem>
          <MenuItem value={String(ProjectRiskLevel.Moyen)}>Moyen</MenuItem>
          <MenuItem value={String(ProjectRiskLevel.Eleve)}>Élevé</MenuItem>
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
          label="Alerte planning"
          value={alertePlanning}
          onChange={(e) => setAlertePlanning(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(indifférent)</MenuItem>
          <MenuItem value="true">Avec alerte</MenuItem>
          <MenuItem value="false">Sans alerte</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label="Alerte budget"
          value={alerteBudget}
          onChange={(e) => setAlerteBudget(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(indifférent)</MenuItem>
          <MenuItem value="true">Avec alerte</MenuItem>
          <MenuItem value="false">Sans alerte</MenuItem>
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
        onRowClick={(params) => navigate(`/projets/${params.row.id}`)}
      />

      <Modal
        open={createOpen}
        title="Créer un projet"
        onClose={() => setCreateOpen(false)}
        maxWidth="md"
      >
        <ProjectCreateForm
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['projects'] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}
