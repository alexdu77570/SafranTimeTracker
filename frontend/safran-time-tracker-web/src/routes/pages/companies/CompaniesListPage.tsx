import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useNavigate } from 'react-router-dom'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchCompanies } from '../../../api/endpoints/companies'
import type { CompanyDto } from '../../../api/types'
import { ReferentialStatus } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { usePagedQuery } from '../../../hooks/usePagedQuery'
import { COMPANY_TYPE_OPTIONS } from '../../../lib/knownReferentials'

const typeLabel = (id: string) => COMPANY_TYPE_OPTIONS.find((o) => o.value === id)?.label ?? id

const columns: GridColDef<CompanyDto>[] = [
  { field: 'code', headerName: 'Code', width: 100 },
  { field: 'nom', headerName: 'Nom', flex: 1 },
  { field: 'companyTypeId', headerName: 'Type', width: 120, valueFormatter: (value: string) => typeLabel(value) },
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

/** Liste des sociétés (§12.1) — clic sur une ligne pour ouvrir la fiche avec historique des
 * contrats (confidentiel, docs/ROADMAP.md Lot 8). */
export function CompaniesListPage() {
  const navigate = useNavigate()
  const { page, setPage, pageSize, setPageSize, query } = usePagedQuery('companies', fetchCompanies)

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Sociétés</Typography>
      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        onRowClick={(params) => navigate(`/societes/${params.row.id}`)}
      />
    </Stack>
  )
}
