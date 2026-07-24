import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { fetchCompanies } from '../../../api/endpoints/companies'
import {
  activateOrder,
  closeOrder,
  fetchOrderById,
  markOrderConsumed,
  reopenOrder,
  suspendOrder,
} from '../../../api/endpoints/orders'
import { fetchOrderStatuses } from '../../../api/endpoints/orderStatuses'
import { fetchProjects } from '../../../api/endpoints/projects'
import { DetailPageHeader } from '../../../components/ui/DetailPageHeader'
import { DetailTabs } from '../../../components/ui/DetailTabs'
import { EmptyState } from '../../../components/ui/EmptyState'
import { KpiBand } from '../../../components/ui/KpiBand'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { OrderEditForm } from './OrderForm'
import { ExtensionsTab } from './tabs/ExtensionsTab'
import { LinkedBudgetsTab } from './tabs/LinkedBudgetsTab'
import { ReceiptsTab } from './tabs/ReceiptsTab'
import { SynthesisTab } from './tabs/SynthesisTab'
import { TimeTab } from './tabs/TimeTab'

const STATUS_BROUILLON = 'BROUILLON'
const STATUS_ACTIVE = 'ACTIVE'
const STATUS_SUSPENDUE = 'SUSPENDUE'
const STATUS_CONSOMMEE = 'CONSOMMEE'
const STATUS_CLOTUREE = 'CLOTUREE'

function useOrderStatuses() {
  const query = useQuery({
    queryKey: ['order-statuses', 'all'],
    queryFn: () => fetchOrderStatuses(),
  })
  const items = query.data?.items ?? []
  return {
    codeById: new Map(items.map((s) => [s.id, s.code])),
    labelById: new Map(items.map((s) => [s.id, s.libelle])),
  }
}

/** Fiche commande détaillée (§13), 5 onglets. Machine d'état pilotée par des actions dédiées
 * (jamais un statut libre) : les boutons affichés dépendent du statut courant, mêmes transitions
 * que OrdersController/OrderService (Lot 5/6). */
export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const orderId = id ?? ''
  const queryClient = useQueryClient()
  const [tab, setTab] = useState(0)
  const [editOpen, setEditOpen] = useState(false)
  const [reopenOpen, setReopenOpen] = useState(false)
  const [motif, setMotif] = useState('')

  const statuses = useOrderStatuses()
  const companiesQuery = useQuery({
    queryKey: ['companies', 'all'],
    queryFn: () => fetchCompanies({ pageSize: 100 }),
  })
  const projectsQuery = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const orderQuery = useQuery({
    queryKey: ['orders', orderId],
    queryFn: () => fetchOrderById(orderId),
    enabled: Boolean(orderId),
  })

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ['orders', orderId] })
  const activateMutation = useMutation({
    mutationFn: () => activateOrder(orderId),
    onSuccess: invalidate,
  })
  const suspendMutation = useMutation({
    mutationFn: () => suspendOrder(orderId),
    onSuccess: invalidate,
  })
  const markConsumedMutation = useMutation({
    mutationFn: () => markOrderConsumed(orderId),
    onSuccess: invalidate,
  })
  const closeMutation = useMutation({
    mutationFn: () => closeOrder(orderId),
    onSuccess: invalidate,
  })
  const reopenMutation = useMutation({
    mutationFn: () => reopenOrder(orderId, { motif }),
    onSuccess: () => {
      setReopenOpen(false)
      setMotif('')
      invalidate()
    },
  })

  if (orderQuery.isLoading) {
    return <EmptyState title="Chargement de la commande…" />
  }
  const order = orderQuery.data
  if (!order) {
    return <EmptyState title="Commande introuvable" />
  }

  const statusCode = statuses.codeById.get(order.statusId)
  const statusLabel = statuses.labelById.get(order.statusId) ?? '—'
  const companyLabel = new Map((companiesQuery.data?.items ?? []).map((c) => [c.id, c.nom])).get(
    order.companyId,
  )
  const projectLabel = order.projectId
    ? new Map((projectsQuery.data?.items ?? []).map((p) => [p.id, p.nom])).get(order.projectId)
    : null

  const tabs = ['Synthèse', 'Rallonges', 'Réceptions', 'Temps', 'Budgets liés']

  return (
    <Stack spacing={2}>
      <DetailPageHeader
        title={order.reference}
        subtitle={order.libelle}
        actions={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={() => setEditOpen(true)}>
              Modifier
            </Button>
            {(statusCode === STATUS_BROUILLON ||
              statusCode === STATUS_SUSPENDUE ||
              statusCode === STATUS_CONSOMMEE) && (
              <Button
                variant="contained"
                loading={activateMutation.isPending}
                onClick={() => activateMutation.mutate()}
              >
                Activer
              </Button>
            )}
            {statusCode === STATUS_ACTIVE && (
              <Button loading={suspendMutation.isPending} onClick={() => suspendMutation.mutate()}>
                Suspendre
              </Button>
            )}
            {statusCode === STATUS_ACTIVE && (
              <Button
                loading={markConsumedMutation.isPending}
                onClick={() => markConsumedMutation.mutate()}
              >
                Marquer consommée
              </Button>
            )}
            {statusCode !== STATUS_CLOTUREE && (
              <Button
                color="error"
                loading={closeMutation.isPending}
                onClick={() => closeMutation.mutate()}
              >
                Clôturer
              </Button>
            )}
            {statusCode === STATUS_CLOTUREE && (
              <Button variant="contained" onClick={() => setReopenOpen(true)}>
                Réouvrir
              </Button>
            )}
          </Stack>
        }
      />

      <KpiBand
        items={[
          {
            label: 'Budget initial',
            value:
              order.budgetFinancierInitial === null ? '—' : `${order.budgetFinancierInitial} €`,
          },
          {
            label: 'Budget ajusté',
            value: order.budgetFinancierAjuste === null ? '—' : `${order.budgetFinancierAjuste} €`,
          },
          { label: 'Société', value: companyLabel ?? '—' },
          { label: 'Projet lié', value: projectLabel ?? '—' },
        ]}
      >
        <div>
          <Typography variant="body2" color="text.secondary">
            Statut
          </Typography>
          <StatusBadge
            label={statusLabel}
            tone={statusCode === STATUS_CLOTUREE ? 'neutral' : 'info'}
          />
        </div>
      </KpiBand>

      <DetailTabs labels={tabs} value={tab} onChange={setTab} />
      {tab === 0 && <SynthesisTab order={order} />}
      {tab === 1 && <ExtensionsTab orderId={orderId} />}
      {tab === 2 && <ReceiptsTab orderId={orderId} />}
      {tab === 3 && <TimeTab orderId={orderId} />}
      {tab === 4 && <LinkedBudgetsTab orderId={orderId} />}

      <Modal
        open={editOpen}
        title="Modifier la commande"
        onClose={() => setEditOpen(false)}
        maxWidth="md"
      >
        <OrderEditForm
          order={order}
          onSuccess={() => {
            setEditOpen(false)
            invalidate()
          }}
          onCancel={() => setEditOpen(false)}
        />
      </Modal>

      <Modal open={reopenOpen} title="Réouvrir la commande" onClose={() => setReopenOpen(false)}>
        <Stack spacing={2}>
          <TextField
            label="Motif"
            value={motif}
            onChange={(e) => setMotif(e.target.value)}
            multiline
            rows={2}
            fullWidth
            size="small"
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setReopenOpen(false)}>Annuler</Button>
            <Button
              variant="contained"
              disabled={!motif.trim()}
              loading={reopenMutation.isPending}
              onClick={() => reopenMutation.mutate()}
            >
              Réouvrir
            </Button>
          </Stack>
        </Stack>
      </Modal>
    </Stack>
  )
}
