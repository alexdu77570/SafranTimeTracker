import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import {
  fetchBudgets,
  closeBudget,
  reactivateBudget,
  fetchBudgetVersions,
} from '../../../api/endpoints/budgets'
import { fetchOrders } from '../../../api/endpoints/orders'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchDashboard, fetchFinancialReport } from '../../../api/endpoints/reporting'
import type { BudgetDto, FinancialReportMonthlyConsumptionDto } from '../../../api/types'
import { ReportingPeriodType } from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { BudgetGauge } from '../../../components/ui/BudgetGauge'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FilterBar } from '../../../components/ui/FilterBar'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { KpiBand } from '../../../components/ui/KpiBand'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { BudgetAdjustForm, BudgetCreateForm, BudgetEditForm } from './BudgetForm'

const MONTH_LABELS = [
  'janv.',
  'févr.',
  'mars',
  'avr.',
  'mai',
  'juin',
  'juil.',
  'août',
  'sept.',
  'oct.',
  'nov.',
  'déc.',
]

/** Graphique de consommation mensuelle (§14.3), Lot 11 — palette catégorielle validée (dataviz
 * skill, run le 21/07/2026) : bleu #2a78d6 / orange #eb6834, ordre fixe, 2 séries. */
function MonthlyConsumptionChart({ data }: { data: FinancialReportMonthlyConsumptionDto[] }) {
  if (!data.length) {
    return <EmptyState title="Aucune consommation sur la période sélectionnée." />
  }
  const chartData = data.map((m) => ({
    label: `${MONTH_LABELS[m.mois - 1]} ${m.annee}`,
    'Coût réel': m.coutReel,
    'Coût contractuel': m.coutContractuel,
  }))

  return (
    <ResponsiveContainer width="100%" height={260}>
      <BarChart data={chartData} barGap={2}>
        <CartesianGrid vertical={false} stroke="#e0e0e0" />
        <XAxis
          dataKey="label"
          tick={{ fill: '#52514e', fontSize: 12 }}
          axisLine={{ stroke: '#e0e0e0' }}
          tickLine={false}
        />
        <YAxis tick={{ fill: '#52514e', fontSize: 12 }} axisLine={false} tickLine={false} />
        <Tooltip formatter={(value) => `${Number(value).toLocaleString('fr-FR')} €`} />
        <Legend />
        <Bar dataKey="Coût réel" fill="#2a78d6" radius={[4, 4, 0, 0]} maxBarSize={24} />
        <Bar dataKey="Coût contractuel" fill="#eb6834" radius={[4, 4, 0, 0]} maxBarSize={24} />
      </BarChart>
    </ResponsiveContainer>
  )
}

/** Page Budgets (§14) : indicateurs (§14.3, entièrement dérivés de GET /reporting/dashboard et
 * GET /reporting/financial existants, aucune agrégation dupliquée) + lignes de budget (CRUD,
 * versions/ajustements historisés, BudgetGauge). Écran entier gardé par FINANCIAL_DATA_VIEW. */
export function BudgetsListPage() {
  const queryClient = useQueryClient()
  const today = new Date().toISOString().slice(0, 10)
  const [customFrom, setCustomFrom] = useState('2024-01-01')
  const [customTo, setCustomTo] = useState(today)
  const [projectId, setProjectId] = useState('')
  const [orderId, setOrderId] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<string | null>(null)
  const [adjustTarget, setAdjustTarget] = useState<string | null>(null)
  const [historyTarget, setHistoryTarget] = useState<string | null>(null)

  const reportFilter = { periodType: ReportingPeriodType.Personnalisee, customFrom, customTo }
  const dashboardQuery = useQuery({
    queryKey: ['reporting-dashboard', reportFilter],
    queryFn: () => fetchDashboard(reportFilter),
  })
  const financialQuery = useQuery({
    queryKey: ['reporting-financial', reportFilter],
    queryFn: () => fetchFinancialReport(reportFilter),
  })

  const projectsQuery = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const ordersQuery = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })
  const projectLabel = new Map((projectsQuery.data?.items ?? []).map((p) => [p.id, p.nom]))
  const orderLabel = new Map((ordersQuery.data?.items ?? []).map((o) => [o.id, o.reference]))

  const budgetsQuery = useQuery({
    queryKey: ['budgets', projectId, orderId],
    queryFn: () =>
      fetchBudgets({ projectId: projectId || undefined, orderId: orderId || undefined }),
  })
  const versionsQuery = useQuery({
    queryKey: ['budget-versions', historyTarget],
    queryFn: () => fetchBudgetVersions(historyTarget ?? ''),
    enabled: Boolean(historyTarget),
  })
  const editing = (budgetsQuery.data?.items ?? []).find((b) => b.id === editTarget)
  const adjusting = (budgetsQuery.data?.items ?? []).find((b) => b.id === adjustTarget)

  const invalidateBudgets = () => void queryClient.invalidateQueries({ queryKey: ['budgets'] })

  return (
    <PermissionGuard
      code={PermissionCodes.FinancialDataView}
      fallback={
        <Typography variant="body2" color="text.disabled">
          Donnée financière non accessible.
        </Typography>
      }
    >
      <Stack spacing={2}>
        <Typography variant="h5">Budgets</Typography>

        <FilterBar onReset={() => {}}>
          <TextField
            size="small"
            type="date"
            label="Du"
            value={customFrom}
            onChange={(e) => setCustomFrom(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            size="small"
            type="date"
            label="Au"
            value={customTo}
            onChange={(e) => setCustomTo(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </FilterBar>

        {dashboardQuery.data?.financial && financialQuery.data && (
          <KpiBand
            items={[
              {
                label: 'Budget initial total',
                value: `${dashboardQuery.data.financial.budgetInitialTotal} €`,
              },
              {
                label: 'Budget ajusté total',
                value: `${dashboardQuery.data.financial.budgetAjusteTotal} €`,
              },
              {
                label: 'Coût réel total',
                value: `${dashboardQuery.data.financial.coutReelTotal} €`,
              },
              {
                label: 'Coût contractuel total',
                value: `${dashboardQuery.data.financial.coutContractuelTotal} €`,
              },
              {
                label: 'Différentiel global',
                value: `${financialQuery.data.differentielGlobal} €`,
              },
              { label: 'Budget restant', value: `${financialQuery.data.budgetRestant} €` },
              {
                label: 'Atterrissage estimé',
                value: `${financialQuery.data.atterrissageEstime} €`,
              },
              {
                label: 'Commandes à risque',
                value: String(dashboardQuery.data.financial.commandesARisque),
              },
              {
                label: 'Projets sous-financés',
                value: String(dashboardQuery.data.financial.projetsSousFinances),
              },
            ]}
          />
        )}

        <Stack spacing={1}>
          <Typography variant="h6">Consommation mensuelle</Typography>
          <MonthlyConsumptionChart data={financialQuery.data?.consommationMensuelle ?? []} />
        </Stack>

        <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap' }}>
          <DifferentialTable
            title="Par projet"
            rows={financialQuery.data?.differentielParProjet ?? []}
          />
          <DifferentialTable
            title="Par commande"
            rows={financialQuery.data?.differentielParCommande ?? []}
          />
          <DifferentialTable
            title="Par société"
            rows={financialQuery.data?.differentielParSociete ?? []}
          />
          <DifferentialTable
            title="Par ressource"
            rows={financialQuery.data?.differentielParRessource ?? []}
          />
        </Stack>

        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
          <Typography variant="h6">Lignes de budget</Typography>
          <IconButton
            color="primary"
            onClick={() => setCreateOpen(true)}
            aria-label="Créer un budget"
          >
            <Plus size={20} />
          </IconButton>
        </Stack>

        <FilterBar
          onReset={() => {
            setProjectId('')
            setOrderId('')
          }}
        >
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
          <TextField
            select
            size="small"
            label="Commande"
            value={orderId}
            onChange={(e) => setOrderId(e.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">(toutes)</MenuItem>
            {(ordersQuery.data?.items ?? []).map((o) => (
              <MenuItem key={o.id} value={o.id}>
                {o.reference}
              </MenuItem>
            ))}
          </TextField>
        </FilterBar>

        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Nom</TableCell>
              <TableCell>Projet</TableCell>
              <TableCell>Commande</TableCell>
              <TableCell>Consommation</TableCell>
              <TableCell>Statut</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(budgetsQuery.data?.items ?? []).map((budget) => (
              <TableRow key={budget.id}>
                <TableCell>{budget.name}</TableCell>
                <TableCell>
                  {budget.projectId ? (projectLabel.get(budget.projectId) ?? '—') : '—'}
                </TableCell>
                <TableCell>
                  {budget.orderId ? (orderLabel.get(budget.orderId) ?? '—') : '—'}
                </TableCell>
                <TableCell sx={{ minWidth: 200 }}>
                  <BudgetGauge
                    label=""
                    consumed={budget.coutReelConsomme}
                    total={budget.adjustedAmount}
                    atRisk={budget.risqueDepassement}
                  />
                </TableCell>
                <TableCell>
                  <StatusBadge
                    label={budget.status === 0 ? 'Actif' : 'Clôturé'}
                    tone={budget.status === 0 ? 'success' : 'neutral'}
                  />
                </TableCell>
                <TableCell>
                  <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap' }}>
                    <StatusBadgeButton label="Modifier" onClick={() => setEditTarget(budget.id)} />
                    <StatusBadgeButton label="Ajuster" onClick={() => setAdjustTarget(budget.id)} />
                    <StatusBadgeButton
                      label="Historique"
                      onClick={() => setHistoryTarget(budget.id)}
                    />
                    {budget.status === 0 ? (
                      <CloseReactivateButton
                        budget={budget}
                        action="close"
                        onDone={invalidateBudgets}
                      />
                    ) : (
                      <CloseReactivateButton
                        budget={budget}
                        action="reactivate"
                        onDone={invalidateBudgets}
                      />
                    )}
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
            {!budgetsQuery.data?.items.length && (
              <TableRow>
                <TableCell colSpan={6}>
                  <Typography variant="body2" color="text.secondary">
                    Aucun budget.
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>

        <Modal
          open={createOpen}
          title="Créer un budget"
          onClose={() => setCreateOpen(false)}
          maxWidth="md"
        >
          <BudgetCreateForm
            onSuccess={() => {
              setCreateOpen(false)
              invalidateBudgets()
            }}
            onCancel={() => setCreateOpen(false)}
          />
        </Modal>

        {editing && (
          <Modal
            open={Boolean(editing)}
            title="Modifier le budget"
            onClose={() => setEditTarget(null)}
            maxWidth="md"
          >
            <BudgetEditForm
              budget={editing}
              onSuccess={() => {
                setEditTarget(null)
                invalidateBudgets()
              }}
              onCancel={() => setEditTarget(null)}
            />
          </Modal>
        )}

        {adjusting && (
          <Modal
            open={Boolean(adjusting)}
            title="Ajuster le budget"
            onClose={() => setAdjustTarget(null)}
          >
            <BudgetAdjustForm
              budget={adjusting}
              onSuccess={() => {
                setAdjustTarget(null)
                invalidateBudgets()
                void queryClient.invalidateQueries({ queryKey: ['budget-versions'] })
              }}
              onCancel={() => setAdjustTarget(null)}
            />
          </Modal>
        )}

        <Modal
          open={Boolean(historyTarget)}
          title="Historique des ajustements"
          onClose={() => setHistoryTarget(null)}
        >
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Ancienne valeur</TableCell>
                <TableCell>Nouvelle valeur</TableCell>
                <TableCell>Motif</TableCell>
                <TableCell>Auteur</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(versionsQuery.data?.items ?? []).map((version) => (
                <TableRow key={version.id}>
                  <TableCell>{version.createdAt}</TableCell>
                  <TableCell>
                    <FinancialValue value={version.oldValue} />
                  </TableCell>
                  <TableCell>
                    <FinancialValue value={version.newValue} />
                  </TableCell>
                  <TableCell>{version.reason}</TableCell>
                  <TableCell>{version.createdBy}</TableCell>
                </TableRow>
              ))}
              {!versionsQuery.data?.items.length && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Typography variant="body2" color="text.secondary">
                      Aucun ajustement.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Modal>
      </Stack>
    </PermissionGuard>
  )
}

function DifferentialTable({
  title,
  rows,
}: {
  title: string
  rows: {
    id: string
    nom: string
    coutReel: number
    coutContractuel: number
    differentiel: number
  }[]
}) {
  return (
    <Stack spacing={1} sx={{ minWidth: 260, flex: 1 }}>
      <Typography variant="subtitle2">{title}</Typography>
      <Table size="small">
        <TableBody>
          {rows.slice(0, 5).map((row) => (
            <TableRow key={row.id}>
              <TableCell>{row.nom}</TableCell>
              <TableCell>
                <FinancialValue value={row.differentiel} />
              </TableCell>
            </TableRow>
          ))}
          {!rows.length && (
            <TableRow>
              <TableCell>
                <Typography variant="body2" color="text.secondary">
                  Aucune donnée.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </Stack>
  )
}

function StatusBadgeButton({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <Button size="small" onClick={onClick}>
      {label}
    </Button>
  )
}

function CloseReactivateButton({
  budget,
  action,
  onDone,
}: {
  budget: BudgetDto
  action: 'close' | 'reactivate'
  onDone: () => void
}) {
  const mutationFn = action === 'close' ? closeBudget : reactivateBudget
  const label = action === 'close' ? 'Clôturer' : 'Réactiver'
  const [loading, setLoading] = useState(false)

  return (
    <Button
      size="small"
      color={action === 'close' ? 'error' : 'primary'}
      loading={loading}
      onClick={async () => {
        setLoading(true)
        try {
          await mutationFn(budget.id)
          onDone()
        } finally {
          setLoading(false)
        }
      }}
    >
      {label}
    </Button>
  )
}
