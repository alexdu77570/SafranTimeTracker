import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchResources } from '../../../api/endpoints/resources'
import type { ResourceDto } from '../../../api/types'
import { ReferentialStatus } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { StatusBadge } from '../../../components/ui/StatusBadge'

const columns: GridColDef<ResourceDto>[] = [
  { field: 'nom', headerName: 'Nom', width: 180 },
  { field: 'prenom', headerName: 'Prénom', width: 180 },
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

/** Liste des ressources (§10.1) — clic sur une ligne pour ouvrir la fiche à 4 sections
 * (docs/ROADMAP.md, Lot 8). */
export function ResourcesListPage() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const navigate = useNavigate()
  const query = useQuery({ queryKey: ['resources', page, pageSize], queryFn: () => fetchResources({ page, pageSize }) })

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Ressources</Typography>
      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        onRowClick={(params) => navigate(`/ressources/${params.row.id}`)}
      />
    </Stack>
  )
}
