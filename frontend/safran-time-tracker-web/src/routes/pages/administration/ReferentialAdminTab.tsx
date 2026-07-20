import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import type { GridColDef, GridValidRowModel } from '@mui/x-data-grid'
import { DataTable } from '../../../components/ui/DataTable'
import { Modal } from '../../../components/ui/Modal'
import type { PagedResult, PaginationQuery } from '../../../api/types'

interface ReferentialAdminTabProps<T extends GridValidRowModel> {
  title: string
  description?: string
  queryKey: string
  fetchList: (params: PaginationQuery) => Promise<PagedResult<T>>
  columns: GridColDef<T>[]
  createLabel?: string
  /** Rendu du formulaire de création dans une Modal ; absent si l'API ne supporte pas la création
   * pour ce référentiel (CLAUDE.md §5 : ne jamais construire une action que le serveur ne peut pas honorer). */
  renderCreateForm?: (props: { onSuccess: () => void; onCancel: () => void }) => ReactNode
  /** Rendu du formulaire d'édition, déclenché par un clic sur une ligne ; absent si l'API ne
   * supporte pas la modification pour ce référentiel. */
  renderEditForm?: (props: { row: T; onSuccess: () => void; onCancel: () => void }) => ReactNode
}

/**
 * Coquille partagée par la majorité des onglets du panneau Administration (Lot 8) : liste
 * paginée serveur + dialog de création/édition. Ne porte aucune règle métier — chaque formulaire
 * (renderCreateForm/renderEditForm) reste spécifique à son référentiel (CLAUDE.md §5, pas de
 * duplication mais pas de généralisation forcée d'une forme qui varie réellement d'un domaine à l'autre).
 */
export function ReferentialAdminTab<T extends GridValidRowModel & { id: string }>({
  title,
  description,
  queryKey,
  fetchList,
  columns,
  createLabel = 'Créer',
  renderCreateForm,
  renderEditForm,
}: ReferentialAdminTabProps<T>) {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [createOpen, setCreateOpen] = useState(false)
  const [editRow, setEditRow] = useState<T | null>(null)

  const query = useQuery({
    queryKey: [queryKey, page, pageSize],
    queryFn: () => fetchList({ page, pageSize }),
  })

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Stack spacing={0.5}>
          <Typography variant="h6">{title}</Typography>
          {description && (
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          )}
        </Stack>
        {renderCreateForm && (
          <Button variant="contained" startIcon={<Plus size={16} />} onClick={() => setCreateOpen(true)}>
            {createLabel}
          </Button>
        )}
      </Stack>

      <DataTable
        rows={query.data?.items ?? []}
        columns={columns}
        rowCount={query.data?.totalCount ?? 0}
        page={page}
        pageSize={pageSize}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        loading={query.isLoading}
        onRowClick={renderEditForm ? (params) => setEditRow(params.row as T) : undefined}
      />

      {renderCreateForm && (
        <Modal open={createOpen} title={createLabel} onClose={() => setCreateOpen(false)}>
          {renderCreateForm({
            onSuccess: () => {
              setCreateOpen(false)
              void query.refetch()
            },
            onCancel: () => setCreateOpen(false),
          })}
        </Modal>
      )}

      {renderEditForm && editRow && (
        <Modal open={Boolean(editRow)} title="Modifier" onClose={() => setEditRow(null)}>
          {renderEditForm({
            row: editRow,
            onSuccess: () => {
              setEditRow(null)
              void query.refetch()
            },
            onCancel: () => setEditRow(null),
          })}
        </Modal>
      )}
    </Stack>
  )
}
