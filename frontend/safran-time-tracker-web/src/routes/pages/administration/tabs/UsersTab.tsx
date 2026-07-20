import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { deactivateUser, fetchUsers, reactivateUser } from '../../../../api/endpoints/users'
import type { UserDto } from '../../../../api/types'
import { ReferentialStatus } from '../../../../api/types'
import { ConfirmDialog } from '../../../../components/ui/ConfirmDialog'
import { DataTable } from '../../../../components/ui/DataTable'
import { StatusBadge } from '../../../../components/ui/StatusBadge'

const columns: GridColDef<UserDto>[] = [
  { field: 'identifiant', headerName: 'Identifiant', width: 120 },
  { field: 'nom', headerName: 'Nom', width: 150 },
  { field: 'prenom', headerName: 'Prénom', width: 150 },
  { field: 'email', headerName: 'Email', flex: 1 },
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

/** Gestion des utilisateurs (§28.3) limitée à activer/désactiver, gardée côté serveur par
 * USER_ADMINISTRATION (Lot 6) — la gestion complète des rôles/permissions par utilisateur reste un
 * écart documenté (docs/IMPLEMENTATION_STATUS.md), l'API le permet déjà mais l'écran dédié n'est
 * pas construit dans ce lot, pour ne pas gonfler le périmètre au-delà de ce qui est demandé. */
export function UsersTab() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [target, setTarget] = useState<UserDto | null>(null)
  const queryClient = useQueryClient()

  const query = useQuery({ queryKey: ['users', 'admin', page, pageSize], queryFn: () => fetchUsers(pageSize) })

  const handleConfirm = async () => {
    if (!target) return
    if (target.statut === ReferentialStatus.Actif) {
      await deactivateUser(target.id)
    } else {
      await reactivateUser(target.id)
    }
    setTarget(null)
    void queryClient.invalidateQueries({ queryKey: ['users'] })
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Utilisateurs</Typography>
      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        onRowClick={(params) => setTarget(params.row)}
      />
      <ConfirmDialog
        open={Boolean(target)}
        title={target?.statut === ReferentialStatus.Actif ? 'Désactiver cet utilisateur ?' : 'Réactiver cet utilisateur ?'}
        description={`${target?.prenom ?? ''} ${target?.nom ?? ''}`.trim()}
        destructive={target?.statut === ReferentialStatus.Actif}
        onConfirm={() => void handleConfirm()}
        onCancel={() => setTarget(null)}
      />
    </Stack>
  )
}
