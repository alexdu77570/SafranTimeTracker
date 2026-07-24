import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { fetchBudgets } from '../../../api/endpoints/budgets'
import { fetchCompanies } from '../../../api/endpoints/companies'
import {
  activateOrder,
  closeOrder,
  fetchOrderById,
  markOrderConsumed,
  reopenOrder,
  suspendOrder,
} from '../../../api/endpoints/orders'
import { fetchOrderExtensions } from '../../../api/endpoints/orderExtensions'
import { fetchOrderReceipts, fetchOrderReceiptSummary } from '../../../api/endpoints/orderReceipts'
import { fetchOrderStatuses } from '../../../api/endpoints/orderStatuses'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchResources } from '../../../api/endpoints/resources'
import { fetchTimeEntries } from '../../../api/endpoints/timeEntries'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { DetailPageHeader } from '../../../components/ui/DetailPageHeader'
import { DetailTabs } from '../../../components/ui/DetailTabs'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { KpiBand } from '../../../components/ui/KpiBand'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { OrderExtensionCreateForm } from './OrderExtensionForm'
import { OrderEditForm } from './OrderForm'
import { OrderReceiptCreateForm } from './OrderReceiptForm'

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

function SynthesisTab({
  order,
}: {
  order: NonNullable<
    ReturnType<typeof useQuery<Awaited<ReturnType<typeof fetchOrderById>>>>['data']
  >
}) {
  return (
    <Card>
      <CardHeader title="Informations générales" />
      <CardContent>
        <Table size="small">
          <TableBody>
            <TableRow>
              <TableCell>Date de début</TableCell>
              <TableCell>{order.dateDebut}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Date de fin initiale</TableCell>
              <TableCell>{order.dateFinInitiale}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Date de fin ajustée</TableCell>
              <TableCell>{order.dateFinAjustee ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Budget en jours initial</TableCell>
              <TableCell>{order.budgetJoursInitial ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Budget en jours ajusté</TableCell>
              <TableCell>{order.budgetJoursAjuste ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Seuil d'alerte</TableCell>
              <TableCell>{order.seuilAlerte ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Commentaire</TableCell>
              <TableCell>{order.commentaire ?? '—'}</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
      <CardHeader title="Synthèse financière (§13.2)" />
      <CardContent>
        <PermissionGuard
          code={PermissionCodes.FinancialDataView}
          fallback={
            <Typography variant="body2" color="text.disabled">
              Donnée financière non accessible.
            </Typography>
          }
        >
          {order.financialSummary ? (
            <Table size="small">
              <TableBody>
                <TableRow>
                  <TableCell>Consommation en jours</TableCell>
                  <TableCell>{order.financialSummary.consommationJours} j</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Coût réel consommé</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.coutReelConsomme} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Coût contractuel consommé</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.coutContractuelConsomme} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Différentiel</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.differentiel} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Reste financier</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.restFinancier} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Reste en jours</TableCell>
                  <TableCell>{order.financialSummary.restJours ?? '—'}</TableCell>
                </TableRow>
              </TableBody>
            </Table>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucune donnée financière.
            </Typography>
          )}
        </PermissionGuard>
      </CardContent>
    </Card>
  )
}

function ExtensionsTab({ orderId }: { orderId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const extensionsQuery = useQuery({
    queryKey: ['order-extensions', orderId],
    queryFn: () => fetchOrderExtensions(orderId),
  })

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer une rallonge"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Montant ajouté</TableCell>
            <TableCell>Jours ajoutés</TableCell>
            <TableCell>Nouvelle date de fin</TableCell>
            <TableCell>Motif</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(extensionsQuery.data?.items ?? []).map((extension) => (
            <TableRow key={extension.id}>
              <TableCell>{extension.extensionDate}</TableCell>
              <TableCell>
                <FinancialValue value={extension.amountAdded} />
              </TableCell>
              <TableCell>{extension.daysAdded ?? '—'}</TableCell>
              <TableCell>{extension.newEndDate}</TableCell>
              <TableCell>{extension.reason}</TableCell>
            </TableRow>
          ))}
          {!extensionsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={5}>
                <Typography variant="body2" color="text.secondary">
                  Aucune rallonge sur cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Nouvelle rallonge" onClose={() => setCreateOpen(false)}>
        <OrderExtensionCreateForm
          orderId={orderId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['order-extensions', orderId] })
            void queryClient.invalidateQueries({ queryKey: ['orders', orderId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}

function ReceiptsTab({ orderId }: { orderId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const receiptsQuery = useQuery({
    queryKey: ['order-receipts', orderId],
    queryFn: () => fetchOrderReceipts(orderId),
  })
  const summaryQuery = useQuery({
    queryKey: ['order-receipts-summary', orderId],
    queryFn: () => fetchOrderReceiptSummary(orderId),
  })

  return (
    <Stack spacing={2}>
      {summaryQuery.data && (
        <KpiBand
          items={[
            { label: 'Total réceptionné (€)', value: `${summaryQuery.data.totalReceivedAmount} €` },
            {
              label: 'Total réceptionné (jours)',
              value: String(summaryQuery.data.totalReceivedDays),
            },
            {
              label: 'Reste réceptionnable (€)',
              value: `${summaryQuery.data.remainingReceivableAmount} €`,
            },
            {
              label: 'Reste réceptionnable (jours)',
              value:
                summaryQuery.data.remainingReceivableDays !== null
                  ? String(summaryQuery.data.remainingReceivableDays)
                  : '—',
            },
          ]}
        />
      )}
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer une réception"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Montant reçu</TableCell>
            <TableCell>Jours reçus</TableCell>
            <TableCell>Motif</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(receiptsQuery.data?.items ?? []).map((receipt) => (
            <TableRow key={receipt.id}>
              <TableCell>{receipt.receiptDate}</TableCell>
              <TableCell>
                {receipt.receivedAmount !== null ? (
                  <FinancialValue value={receipt.receivedAmount} />
                ) : (
                  '—'
                )}
              </TableCell>
              <TableCell>{receipt.receivedDays ?? '—'}</TableCell>
              <TableCell>{receipt.reason ?? '—'}</TableCell>
            </TableRow>
          ))}
          {!receiptsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={4}>
                <Typography variant="body2" color="text.secondary">
                  Aucune réception sur cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Nouvelle réception" onClose={() => setCreateOpen(false)}>
        <OrderReceiptCreateForm
          orderId={orderId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['order-receipts', orderId] })
            void queryClient.invalidateQueries({ queryKey: ['order-receipts-summary', orderId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}

function TimeTab({ orderId }: { orderId: string }) {
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byOrder', orderId],
    queryFn: () => fetchTimeEntries({ orderId, pageSize: 200 }),
  })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )
  const entries = timeEntriesQuery.data?.items ?? []

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Date</TableCell>
          <TableCell>Ressource</TableCell>
          <TableCell>Durée</TableCell>
          <TableCell>Référence</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {entries.map((entry) => (
          <TableRow key={entry.id}>
            <TableCell>{entry.date}</TableCell>
            <TableCell>{resourceLabel.get(entry.resourceId) ?? entry.resourceId}</TableCell>
            <TableCell>{entry.dureeHeures} h</TableCell>
            <TableCell>{entry.reference ?? '—'}</TableCell>
          </TableRow>
        ))}
        {!entries.length && (
          <TableRow>
            <TableCell colSpan={4}>
              <Typography variant="body2" color="text.secondary">
                Aucune saisie de temps sur cette commande.
              </Typography>
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}

function LinkedBudgetsTab({ orderId }: { orderId: string }) {
  const budgetsQuery = useQuery({
    queryKey: ['budgets', 'byOrder', orderId],
    queryFn: () => fetchBudgets({ orderId }),
  })

  return (
    <PermissionGuard
      code={PermissionCodes.FinancialDataView}
      fallback={
        <Typography variant="body2" color="text.disabled">
          Donnée financière non accessible.
        </Typography>
      }
    >
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Nom</TableCell>
            <TableCell>Initial</TableCell>
            <TableCell>Ajusté</TableCell>
            <TableCell>Différentiel</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(budgetsQuery.data?.items ?? []).map((budget) => (
            <TableRow key={budget.id}>
              <TableCell>{budget.name}</TableCell>
              <TableCell>
                <FinancialValue value={budget.initialAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.adjustedAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.differentiel} />
              </TableCell>
            </TableRow>
          ))}
          {!budgetsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={4}>
                <Typography variant="body2" color="text.secondary">
                  Aucun budget lié à cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </PermissionGuard>
  )
}
