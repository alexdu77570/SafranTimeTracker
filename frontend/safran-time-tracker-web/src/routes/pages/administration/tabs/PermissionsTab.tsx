import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchPermissions } from '../../../../api/endpoints/permissions'
import type { PermissionDto } from '../../../../api/types'
import { DataTable } from '../../../../components/ui/DataTable'

const columns: GridColDef<PermissionDto>[] = [
  { field: 'code', headerName: 'Code', width: 220 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  { field: 'description', headerName: 'Description', flex: 2 },
]

/** Référentiel en lecture seule (Lot 7) : les permissions ne sont ni créées ni modifiées depuis
 * l'interface, seul leur octroi/retrait par utilisateur l'est (UsersController, hors périmètre de
 * cet onglet — voir UsersTab). */
export function PermissionsTab() {
  const query = useQuery({ queryKey: ['permissions', 'admin'], queryFn: () => fetchPermissions(100) })

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Permissions</Typography>
      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={1}
        pageSize={100}
        onPageChange={() => {}}
        onPageSizeChange={() => {}}
        loading={query.isLoading}
      />
    </Stack>
  )
}
