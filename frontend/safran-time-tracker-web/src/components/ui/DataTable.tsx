import { DataGrid, type GridColDef, type GridRowId, type GridRowParams, type GridValidRowModel } from '@mui/x-data-grid'
import Box from '@mui/material/Box'
import { EmptyState } from './EmptyState'

interface DataTableProps<T extends GridValidRowModel> {
  rows: T[]
  columns: GridColDef<T>[]
  /** Nombre total de lignes côté serveur (PagedResult.totalCount) — distinct de rows.length,
   * qui ne contient qu'une page. */
  rowCount: number
  /** Page 1-based, alignée sur PaginationQuery.page (backend) plutôt que sur l'index 0-based du
   * DataGrid — la conversion se fait à l'intérieur du composant. */
  page: number
  pageSize: number
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
  loading?: boolean
  getRowId?: (row: T) => GridRowId
  emptyLabel?: string
  /** Absent par défaut : une ligne n'est cliquable que si l'écran en a explicitement besoin
   * (navigation vers une fiche détail, ouverture d'un dialog d'édition). */
  onRowClick?: (params: GridRowParams<T>) => void
}

/** Tableau de données transverse (cahier des charges §8.3 : recherche, pagination, tri). Toujours
 * en pagination serveur, cohérente avec PagedResult<T>/PaginationQuery (CLAUDE.md §12) : le
 * DataGrid n'effectue jamais de tri/pagination client sur des données déjà paginées côté API. */
export function DataTable<T extends GridValidRowModel>({
  rows,
  columns,
  rowCount,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
  loading = false,
  getRowId,
  emptyLabel = 'Aucune donnée',
  onRowClick,
}: DataTableProps<T>) {
  return (
    <Box sx={{ width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        getRowId={getRowId}
        rowCount={rowCount}
        loading={loading}
        paginationMode="server"
        sortingMode="server"
        paginationModel={{ page: page - 1, pageSize }}
        onPaginationModelChange={(model) => {
          if (model.pageSize !== pageSize) {
            onPageSizeChange(model.pageSize)
          } else if (model.page !== page - 1) {
            onPageChange(model.page + 1)
          }
        }}
        onRowClick={onRowClick}
        pageSizeOptions={[10, 20, 50, 100]}
        disableRowSelectionOnClick
        density="compact"
        autoHeight
        sx={onRowClick ? { '& .MuiDataGrid-row': { cursor: 'pointer' } } : undefined}
        slots={{
          noRowsOverlay: () => <EmptyState title={emptyLabel} />,
        }}
      />
    </Box>
  )
}
