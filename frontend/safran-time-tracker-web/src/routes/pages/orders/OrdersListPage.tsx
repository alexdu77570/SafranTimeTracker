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
import { fetchCompanies } from '../../../api/endpoints/companies'
import { fetchOrders, type OrderListParams } from '../../../api/endpoints/orders'
import { fetchOrderStatuses } from '../../../api/endpoints/orderStatuses'
import { fetchProjects } from '../../../api/endpoints/projects'
import type { OrderDto } from '../../../api/types'
import { DataTable } from '../../../components/ui/DataTable'
import { FilterBar } from '../../../components/ui/FilterBar'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { OrderCreateForm } from './OrderForm'

/** Liste des commandes (§13) — filtres société/statut/projet, colonnes non financières + colonnes
 * financières via FinancialValue (OrderDto.financialSummary, absent sans FINANCIAL_DATA_VIEW). */
export function OrdersListPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [companyId, setCompanyId] = useState('')
  const [statusId, setStatusId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [createOpen, setCreateOpen] = useState(false)

  const companiesQuery = useQuery({
    queryKey: ['companies', 'all'],
    queryFn: () => fetchCompanies({ pageSize: 100 }),
  })
  const statusesQuery = useQuery({
    queryKey: ['order-statuses', 'all'],
    queryFn: () => fetchOrderStatuses(),
  })
  const projectsQuery = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })

  const companyLabel = new Map((companiesQuery.data?.items ?? []).map((c) => [c.id, c.nom]))
  const statusLabel = new Map((statusesQuery.data?.items ?? []).map((s) => [s.id, s.libelle]))
  const projectLabel = new Map((projectsQuery.data?.items ?? []).map((p) => [p.id, p.nom]))

  const filters: OrderListParams = {
    page,
    pageSize,
    companyId: companyId || undefined,
    statusId: statusId || undefined,
    projectId: projectId || undefined,
  }
  const query = useQuery({ queryKey: ['orders', filters], queryFn: () => fetchOrders(filters) })

  const columns: GridColDef<OrderDto>[] = [
    { field: 'reference', headerName: 'Référence', width: 160 },
    { field: 'libelle', headerName: 'Libellé', flex: 1 },
    {
      field: 'companyId',
      headerName: 'Société',
      width: 160,
      valueFormatter: (value: string) => companyLabel.get(value) ?? value,
    },
    {
      field: 'projectId',
      headerName: 'Projet',
      width: 160,
      valueFormatter: (value: string | null) => (value ? (projectLabel.get(value) ?? value) : '—'),
    },
    {
      field: 'statusId',
      headerName: 'Statut',
      width: 130,
      renderCell: (params) => (
        <StatusBadge label={statusLabel.get(params.value) ?? '—'} tone="info" />
      ),
    },
    { field: 'dateDebut', headerName: 'Début', width: 110 },
    { field: 'dateFinAjustee', headerName: 'Fin ajustée', width: 110 },
    {
      field: 'budgetFinancierAjuste',
      headerName: 'Budget ajusté',
      width: 140,
      renderCell: (params) => <FinancialValue value={params.value} />,
    },
    {
      field: 'financialSummary',
      headerName: 'Différentiel',
      width: 130,
      renderCell: (params) => <FinancialValue value={params.value?.differentiel} />,
    },
  ]

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5">Commandes</Typography>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer une commande"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>

      <FilterBar
        onReset={() => {
          setCompanyId('')
          setStatusId('')
          setProjectId('')
        }}
      >
        <TextField
          select
          size="small"
          label="Société"
          value={companyId}
          onChange={(e) => setCompanyId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(companiesQuery.data?.items ?? []).map((c) => (
            <MenuItem key={c.id} value={c.id}>
              {c.nom}
            </MenuItem>
          ))}
        </TextField>
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
          label="Projet"
          value={projectId}
          onChange={(e) => setProjectId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(projectsQuery.data?.items ?? []).map((p) => (
            <MenuItem key={p.id} value={p.id}>
              {p.nom}
            </MenuItem>
          ))}
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
        onRowClick={(params) => navigate(`/commandes/${params.row.id}`)}
      />

      <Modal
        open={createOpen}
        title="Créer une commande"
        onClose={() => setCreateOpen(false)}
        maxWidth="md"
      >
        <OrderCreateForm
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['orders'] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}
