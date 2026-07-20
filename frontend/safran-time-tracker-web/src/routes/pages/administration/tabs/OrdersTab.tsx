import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchOrders } from '../../../../api/endpoints/orders'
import type { OrderDto } from '../../../../api/types'
import { DataTable } from '../../../../components/ui/DataTable'

const columns: GridColDef<OrderDto>[] = [
  { field: 'reference', headerName: 'Référence', width: 160 },
  { field: 'libelle', headerName: 'Libellé', flex: 1 },
  { field: 'dateDebut', headerName: 'Début', width: 120 },
  { field: 'dateFinInitiale', headerName: 'Fin initiale', width: 120 },
]

/** Liste de consultation (référence, libellé, dates) : la gestion complète (machine d'état,
 * rallonges, réceptions) relève du Lot 11 — Commandes, Budgets et Jalons (docs/ROADMAP.md), pas
 * anticipée ici. Les montants sont volontairement absents : cet onglet n'est pas un écran
 * financier, la fiche Commande détaillée est hors périmètre du Lot 8. */
export function OrdersTab() {
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const query = useQuery({ queryKey: ['orders', 'admin', page, pageSize], queryFn: () => fetchOrders({ page, pageSize }) })

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Commandes</Typography>
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
    </Stack>
  )
}
