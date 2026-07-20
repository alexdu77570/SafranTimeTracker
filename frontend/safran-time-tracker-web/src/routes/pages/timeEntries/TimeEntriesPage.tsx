import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Copy, Plus, RefreshCw, Trash2 } from 'lucide-react'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { fetchActivityTypes } from '../../../api/endpoints/activityTypes'
import { fetchOrders } from '../../../api/endpoints/orders'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchResources } from '../../../api/endpoints/resources'
import {
  deleteTimeEntry,
  fetchTimeEntries,
  recalculateTimeEntry,
  type TimeEntryListParams,
} from '../../../api/endpoints/timeEntries'
import type { TimeEntryDto } from '../../../api/types'
import { PermissionCodes } from '../../../auth/permissionCodes'
import { PermissionGuard } from '../../../auth/PermissionGuard'
import { useCurrentUser } from '../../../auth/useCurrentUser'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { DataTable } from '../../../components/ui/DataTable'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FilterBar } from '../../../components/ui/FilterBar'
import { FinancialValue } from '../../../components/ui/FinancialValue'
import { KpiCard } from '../../../components/ui/KpiCard'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { ReferentialStatus } from '../../../api/types'
import { weekBounds } from '../../../lib/dateUtils'
import { TimeEntryCreateForm, TimeEntryEditForm } from './TimeEntryForm'

/** Nombre maximal de saisies additionnées pour la « totalisation automatique » (§19.4) : une
 * requête dédiée, plus large que la pagination affichée, sur le même filtre — pas une nouvelle
 * agrégation backend (aucun endpoint ajouté), juste une somme côté client sur des données déjà
 * exposées par `GET /time-entries`. Suffisant au périmètre de démonstration actuel ; à remplacer
 * par un agrégat serveur si le volume réel dépasse ce seuil. */
const TOTAL_SCAN_PAGE_SIZE = 1000

/**
 * Saisie et suivi des temps (§19, Lot 9). Ressource pré-sélectionnée = celle de l'identité de
 * démonstration courante (feuille de temps personnelle), avec un sélecteur libre pour parcourir
 * d'autres saisies (« suivi », §19 — aucune restriction serveur n'existe à ce jour, on ne simule
 * donc pas une restriction que le backend n'impose pas, décision actée à l'ouverture du lot).
 */
export function TimeEntriesPage() {
  const { user } = useCurrentUser()
  const queryClient = useQueryClient()

  const [resourceId, setResourceId] = useState('')
  const [activityTypeId, setActivityTypeId] = useState('')
  const [projectId, setProjectId] = useState('')
  const [orderId, setOrderId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [createOpen, setCreateOpen] = useState(false)
  const [duplicateSeed, setDuplicateSeed] = useState<Partial<TimeEntryDto> | null>(null)
  const [editRow, setEditRow] = useState<TimeEntryDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<TimeEntryDto | null>(null)
  const [recalculateTarget, setRecalculateTarget] = useState<TimeEntryDto | null>(null)
  const [recalculateReason, setRecalculateReason] = useState('')

  const effectiveResourceId = resourceId || user?.resourceId || ''

  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const activityTypesQuery = useQuery({
    queryKey: ['activity-types', 'all'],
    queryFn: () => fetchActivityTypes({ pageSize: 100 }),
  })
  const activityTypeLabel = new Map(
    (activityTypesQuery.data?.items ?? []).map((a) => [a.id, a.libelle]),
  )
  const projectsQuery = useQuery({ queryKey: ['projects', 'all'], queryFn: () => fetchProjects() })
  const ordersQuery = useQuery({
    queryKey: ['orders', 'all'],
    queryFn: () => fetchOrders({ pageSize: 100 }),
  })

  const baseFilters = {
    resourceId: effectiveResourceId || undefined,
    activityTypeId: activityTypeId || undefined,
    projectId: projectId || undefined,
    orderId: orderId || undefined,
    from: from || undefined,
    to: to || undefined,
  }
  const filters: TimeEntryListParams = { page, pageSize, ...baseFilters }
  const query = useQuery({
    queryKey: ['time-entries', filters],
    queryFn: () => fetchTimeEntries(filters),
  })

  /** Totalisation automatique (§19.4) : somme des heures sur l'ensemble des saisies correspondant
   * au filtre courant, pas seulement la page affichée — requête dédiée sur le même endpoint,
   * jamais un calcul dupliqué depuis une autre source. */
  const totalsQuery = useQuery({
    queryKey: ['time-entries', 'totals', baseFilters],
    queryFn: () => fetchTimeEntries({ page: 1, pageSize: TOTAL_SCAN_PAGE_SIZE, ...baseFilters }),
    enabled: Boolean(effectiveResourceId),
  })
  const totalHeures = totalsQuery.data?.items.reduce((sum, item) => sum + item.dureeHeures, 0)

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['time-entries'] })
  }

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteTimeEntry(id),
    onSuccess: () => {
      setDeleteTarget(null)
      invalidate()
    },
  })
  const recalculateMutation = useMutation({
    mutationFn: (id: string) => recalculateTimeEntry(id, { reason: recalculateReason }),
    onSuccess: () => {
      setRecalculateTarget(null)
      setRecalculateReason('')
      invalidate()
    },
  })

  const columns: GridColDef<TimeEntryDto>[] = [
    { field: 'date', headerName: 'Date', width: 110 },
    {
      field: 'activityTypeId',
      headerName: "Type d'activité",
      width: 160,
      valueFormatter: (value: string) => activityTypeLabel.get(value) ?? value,
    },
    { field: 'dureeHeures', headerName: 'Durée (h)', width: 100 },
    {
      field: 'reference',
      headerName: 'Référence',
      width: 140,
      valueFormatter: (value: string | null) => value ?? '—',
    },
    {
      field: 'statut',
      headerName: 'Statut',
      width: 110,
      renderCell: (params) => (
        <StatusBadge
          label={params.value === ReferentialStatus.Actif ? 'Actif' : 'Supprimé'}
          tone={params.value === ReferentialStatus.Actif ? 'success' : 'neutral'}
        />
      ),
    },
    {
      field: 'financialSnapshot',
      headerName: 'Coût réel',
      width: 130,
      renderCell: (params) => <FinancialValue value={params.value?.coutReelCalcule} />,
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 150,
      sortable: false,
      renderCell: (params) => (
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Dupliquer">
            <IconButton
              size="small"
              onClick={(event) => {
                event.stopPropagation()
                setDuplicateSeed(params.row)
                setCreateOpen(true)
              }}
            >
              <Copy size={16} />
            </IconButton>
          </Tooltip>
          <PermissionGuard code={PermissionCodes.TimeEntryRecalculation}>
            <Tooltip title="Recalculer">
              <IconButton
                size="small"
                onClick={(event) => {
                  event.stopPropagation()
                  setRecalculateTarget(params.row)
                }}
              >
                <RefreshCw size={16} />
              </IconButton>
            </Tooltip>
          </PermissionGuard>
          <Tooltip title="Supprimer">
            <IconButton
              size="small"
              onClick={(event) => {
                event.stopPropagation()
                setDeleteTarget(params.row)
              }}
            >
              <Trash2 size={16} />
            </IconButton>
          </Tooltip>
        </Stack>
      ),
    },
  ]

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5">Temps</Typography>
        <IconButton
          color="primary"
          onClick={() => {
            setDuplicateSeed(null)
            setCreateOpen(true)
          }}
          aria-label="Créer une saisie"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>

      <FilterBar
        onReset={() => {
          setResourceId('')
          setActivityTypeId('')
          setProjectId('')
          setOrderId('')
          setFrom('')
          setTo('')
        }}
      >
        <TextField
          select
          size="small"
          label="Ressource"
          value={effectiveResourceId}
          onChange={(e) => setResourceId(e.target.value)}
          sx={{ minWidth: 200 }}
        >
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Type d'activité"
          value={activityTypeId}
          onChange={(e) => setActivityTypeId(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(activityTypesQuery.data?.items ?? []).map((a) => (
            <MenuItem key={a.id} value={a.id}>
              {a.libelle}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Projet"
          value={projectId}
          onChange={(e) => setProjectId(e.target.value)}
          sx={{ minWidth: 180 }}
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
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(ordersQuery.data?.items ?? []).map((o) => (
            <MenuItem key={o.id} value={o.id}>
              {o.reference}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="date"
          label="Du"
          value={from}
          onChange={(e) => setFrom(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          size="small"
          type="date"
          label="Au"
          value={to}
          onChange={(e) => setTo(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          size="small"
          type="date"
          label="Semaine (jour contenu)"
          value=""
          onChange={(e) => {
            if (!e.target.value) {
              return
            }
            const bounds = weekBounds(e.target.value)
            setFrom(bounds.start)
            setTo(bounds.end)
          }}
          slotProps={{ inputLabel: { shrink: true } }}
          sx={{ minWidth: 190 }}
        />
      </FilterBar>

      {effectiveResourceId && (
        <KpiCard
          label="Total (filtre courant)"
          value={totalHeures !== undefined ? `${totalHeures} h` : '—'}
        />
      )}

      {!effectiveResourceId ? (
        <EmptyState
          title="Sélectionnez une ressource"
          description="Choisissez une ressource dans le filtre ci-dessus pour afficher ses saisies."
        />
      ) : (
        <DataTable
          rows={query.data?.items ?? []}
          columns={columns}
          rowCount={query.data?.totalCount ?? 0}
          page={page}
          pageSize={pageSize}
          onPageChange={setPage}
          onPageSizeChange={setPageSize}
          loading={query.isLoading}
          onRowClick={(params) => setEditRow(params.row)}
        />
      )}

      <Modal
        open={createOpen}
        title="Créer une saisie de temps"
        onClose={() => setCreateOpen(false)}
      >
        {effectiveResourceId && (
          <TimeEntryCreateForm
            resourceId={effectiveResourceId}
            seed={
              duplicateSeed
                ? {
                    activityTypeId: duplicateSeed.activityTypeId,
                    projectId: duplicateSeed.projectId ?? '',
                    orderId: duplicateSeed.orderId ?? '',
                    reference: duplicateSeed.reference ?? '',
                    commentaire: duplicateSeed.commentaire ?? '',
                    dureeHeures: duplicateSeed.dureeHeures,
                  }
                : undefined
            }
            onSuccess={() => {
              setCreateOpen(false)
              setDuplicateSeed(null)
              invalidate()
            }}
            onCancel={() => setCreateOpen(false)}
          />
        )}
      </Modal>

      {editRow && (
        <Modal open={Boolean(editRow)} title="Modifier la saisie" onClose={() => setEditRow(null)}>
          <TimeEntryEditForm
            row={editRow}
            onSuccess={() => {
              setEditRow(null)
              invalidate()
            }}
            onCancel={() => setEditRow(null)}
          />
        </Modal>
      )}

      <ConfirmDialog
        open={Boolean(deleteTarget)}
        title="Supprimer cette saisie ?"
        description={deleteTarget ? `${deleteTarget.date} — ${deleteTarget.dureeHeures} h` : ''}
        destructive
        loading={deleteMutation.isPending}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />

      <Modal
        open={Boolean(recalculateTarget)}
        title="Recalculer cette saisie"
        onClose={() => setRecalculateTarget(null)}
      >
        <Stack spacing={2}>
          <Chip label="Motif obligatoire (§19.6)" size="small" sx={{ alignSelf: 'flex-start' }} />
          <Typography variant="body2" color="text.secondary">
            Le recalcul explicite est audité et nécessite un motif (cahier des charges §19.6) :
            l'ancienne valeur du snapshot financier est conservée dans le journal d'audit.
          </Typography>
          <TextField
            label="Motif"
            value={recalculateReason}
            onChange={(e) => setRecalculateReason(e.target.value)}
            multiline
            rows={2}
            fullWidth
            size="small"
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setRecalculateTarget(null)}>Annuler</Button>
            <Button
              variant="contained"
              disabled={!recalculateReason.trim()}
              loading={recalculateMutation.isPending}
              onClick={() => recalculateTarget && recalculateMutation.mutate(recalculateTarget.id)}
            >
              Recalculer
            </Button>
          </Stack>
        </Stack>
      </Modal>
    </Stack>
  )
}
