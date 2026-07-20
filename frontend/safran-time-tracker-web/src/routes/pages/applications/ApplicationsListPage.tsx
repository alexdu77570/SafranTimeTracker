import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchApplications } from '../../../api/endpoints/applications'
import type { ApplicationReferenceDto } from '../../../api/types'
import { ApplicationCriticality, ReferentialStatus } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { StatusBadge } from '../../../components/ui/StatusBadge'

const criticiteLabel: Record<ApplicationCriticality, string> = {
  [ApplicationCriticality.Faible]: 'Faible',
  [ApplicationCriticality.Moyenne]: 'Moyenne',
  [ApplicationCriticality.Elevee]: 'Élevée',
  [ApplicationCriticality.Critique]: 'Critique',
}

const columns: GridColDef<ApplicationReferenceDto>[] = [
  { field: 'code', headerName: 'Code', width: 120 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  { field: 'criticite', headerName: 'Criticité', width: 130, valueFormatter: (value: ApplicationCriticality) => criticiteLabel[value] },
  {
    field: 'statut',
    headerName: 'Statut',
    width: 130,
    renderCell: (params) => (
      <StatusBadge
        label={params.value === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
        tone={params.value === ReferentialStatus.Actif ? 'success' : 'neutral'}
      />
    ),
  },
]

/** Liste des applications (§15) — clic sur une ligne pour ouvrir la fiche avec détail statistique
 * et technologies rattachées (docs/ROADMAP.md, Lot 8). */
export function ApplicationsListPage() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const navigate = useNavigate()
  const query = useQuery({ queryKey: ['applications', page, pageSize], queryFn: () => fetchApplications({ page, pageSize }) })

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Applications</Typography>
      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        onRowClick={(params) => navigate(`/applications/${params.row.id}`)}
      />
    </Stack>
  )
}
