import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchAuditLogs } from '../../../../api/endpoints/auditLogs'
import type { AuditLogDto } from '../../../../api/types'
import { PermissionCodes } from '../../../../auth/permissionCodes'
import { PermissionGuard } from '../../../../auth/PermissionGuard'
import { DataTable } from '../../../../components/ui/DataTable'
import { EmptyState } from '../../../../components/ui/EmptyState'

const columns: GridColDef<AuditLogDto>[] = [
  { field: 'timestamp', headerName: 'Date', width: 180, valueFormatter: (value: string) => new Date(value).toLocaleString('fr-FR') },
  { field: 'author', headerName: 'Auteur', width: 150 },
  { field: 'action', headerName: 'Action', width: 130 },
  { field: 'entityType', headerName: 'Type', width: 150 },
  { field: 'reason', headerName: 'Motif', flex: 1 },
]

function AuditTable() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const query = useQuery({ queryKey: ['audit-logs', page, pageSize], queryFn: () => fetchAuditLogs({ page, pageSize }) })

  return (
    <DataTable
      rows={query.data?.items ?? []}
      columns={columns}
      rowCount={query.data?.totalCount ?? 0}
      page={page}
      pageSize={pageSize}
      onPageChange={setPage}
      onPageSizeChange={setPageSize}
      loading={query.isLoading}
    />
  )
}

/** Gardé par AUDIT_VIEW côté serveur (§28.1) : sans la permission, l'API renvoie 403 — le
 * PermissionGuard adapte uniquement l'affichage (CLAUDE.md §17), il ne remplace pas la garde serveur. */
export function AuditTab() {
  return (
    <Stack spacing={2}>
      <Typography variant="h6">Journal d'audit</Typography>
      <PermissionGuard
        code={PermissionCodes.AuditView}
        fallback={<EmptyState title="Accès restreint" description="La consultation du journal d'audit nécessite la permission AUDIT_VIEW." />}
      >
        <AuditTable />
      </PermissionGuard>
    </Stack>
  )
}
