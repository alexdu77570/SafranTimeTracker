import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import TextField from '@mui/material/TextField'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import Grid from '@mui/material/Grid'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { Ban, Check, Pencil, Plus, Send, X } from 'lucide-react'
import { useState } from 'react'
import type { GridColDef } from '@mui/x-data-grid'
import { cancelAbsence, fetchAbsences, refuseAbsence, submitAbsence, validateAbsence } from '../../../api/endpoints/absences'
import { fetchAvailability } from '../../../api/endpoints/availability'
import { fetchResources } from '../../../api/endpoints/resources'
import type { AbsenceDto } from '../../../api/types'
import { AbsenceStatus, AbsenceType } from '../../../api/types'
import { useCurrentUser } from '../../../auth/useCurrentUser'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { DataTable } from '../../../components/ui/DataTable'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FilterBar } from '../../../components/ui/FilterBar'
import { KpiCard } from '../../../components/ui/KpiCard'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { AbsenceCreateForm, AbsenceEditForm } from './AbsenceForm'

const typeLabels: Record<AbsenceType, string> = {
  [AbsenceType.Conge]: 'Congé',
  [AbsenceType.Rtt]: 'RTT',
  [AbsenceType.Maladie]: 'Maladie',
  [AbsenceType.Formation]: 'Formation',
  [AbsenceType.Deplacement]: 'Déplacement',
  [AbsenceType.Indisponible]: 'Indisponible',
}

const statutTone: Record<AbsenceStatus, { label: string; tone: 'neutral' | 'success' | 'warning' | 'error' | 'info' }> = {
  [AbsenceStatus.Brouillon]: { label: 'Brouillon', tone: 'neutral' },
  [AbsenceStatus.Soumis]: { label: 'Soumis', tone: 'info' },
  [AbsenceStatus.Valide]: { label: 'Validé', tone: 'success' },
  [AbsenceStatus.Refuse]: { label: 'Refusé', tone: 'error' },
  [AbsenceStatus.Annule]: { label: 'Annulé', tone: 'neutral' },
}

/**
 * Mes absences (§23, Lot 9). Totaux mensuel/annuel calculés via l'endpoint de disponibilité
 * existant (JoursAbsenceValidee, §29.2) plutôt que sommés côté frontend à partir des plages
 * DateDebut/DateFin brutes — la règle « jour ouvré » reste dans AvailabilityService, jamais
 * dupliquée ici (CLAUDE.md §5). Aucune permission dédiée n'existe côté serveur pour
 * valider/refuser (AbsencesController n'a pas de garde) : les actions restent donc visibles à
 * quiconque, comme l'API le permet réellement (décision actée à l'ouverture du lot, §3.6).
 */
export function AbsencesPage() {
  const { user } = useCurrentUser()
  const queryClient = useQueryClient()

  const [resourceId, setResourceId] = useState('')
  const [statut, setStatut] = useState<string>('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [createOpen, setCreateOpen] = useState(false)
  const [editRow, setEditRow] = useState<AbsenceDto | null>(null)
  const [cancelTarget, setCancelTarget] = useState<AbsenceDto | null>(null)
  const [refuseTarget, setRefuseTarget] = useState<AbsenceDto | null>(null)
  const [refuseReason, setRefuseReason] = useState('')

  const effectiveResourceId = resourceId || user?.resourceId || ''

  const resourcesQuery = useQuery({ queryKey: ['resources', 'all'], queryFn: () => fetchResources({ pageSize: 100 }) })

  const filters = { page, pageSize, resourceId: effectiveResourceId || undefined, statut: statut ? (Number(statut) as AbsenceStatus) : undefined }
  const query = useQuery({ queryKey: ['absences', filters], queryFn: () => fetchAbsences(filters), enabled: Boolean(effectiveResourceId) })

  const monthStart = dayjs().startOf('month').format('YYYY-MM-DD')
  const monthEnd = dayjs().endOf('month').format('YYYY-MM-DD')
  const yearStart = dayjs().startOf('year').format('YYYY-MM-DD')
  const yearEnd = dayjs().endOf('year').format('YYYY-MM-DD')
  const monthlyAvailability = useQuery({
    queryKey: ['availability', effectiveResourceId, monthStart, monthEnd],
    queryFn: () => fetchAvailability(effectiveResourceId, monthStart, monthEnd),
    enabled: Boolean(effectiveResourceId),
  })
  const annualAvailability = useQuery({
    queryKey: ['availability', effectiveResourceId, yearStart, yearEnd],
    queryFn: () => fetchAvailability(effectiveResourceId, yearStart, yearEnd),
    enabled: Boolean(effectiveResourceId),
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['absences'] })
    void queryClient.invalidateQueries({ queryKey: ['availability'] })
  }

  const submitMutation = useMutation({ mutationFn: submitAbsence, onSuccess: invalidate })
  const validateMutation = useMutation({ mutationFn: validateAbsence, onSuccess: invalidate })
  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelAbsence(id),
    onSuccess: () => {
      setCancelTarget(null)
      invalidate()
    },
  })
  const refuseMutation = useMutation({
    mutationFn: (id: string) => refuseAbsence(id, { commentaire: refuseReason }),
    onSuccess: () => {
      setRefuseTarget(null)
      setRefuseReason('')
      invalidate()
    },
  })

  const columns: GridColDef<AbsenceDto>[] = [
    { field: 'dateDebut', headerName: 'Début', width: 110 },
    { field: 'dateFin', headerName: 'Fin', width: 110 },
    { field: 'type', headerName: 'Type', width: 130, valueFormatter: (value: AbsenceType) => typeLabels[value] },
    { field: 'demiJournee', headerName: 'Demi-journée', width: 120, valueFormatter: (value: boolean) => (value ? 'Oui' : 'Non') },
    {
      field: 'statut',
      headerName: 'Statut',
      width: 120,
      renderCell: (params) => <StatusBadge label={statutTone[params.value as AbsenceStatus].label} tone={statutTone[params.value as AbsenceStatus].tone} />,
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 180,
      sortable: false,
      renderCell: (params) => {
        const row = params.row
        return (
          <Stack direction="row" spacing={0.5}>
            {row.statut === AbsenceStatus.Brouillon && (
              <>
                <Tooltip title="Modifier">
                  <IconButton size="small" onClick={(e) => { e.stopPropagation(); setEditRow(row) }}>
                    <Pencil size={16} />
                  </IconButton>
                </Tooltip>
                <Tooltip title="Soumettre">
                  <IconButton size="small" onClick={(e) => { e.stopPropagation(); submitMutation.mutate(row.id) }}>
                    <Send size={16} />
                  </IconButton>
                </Tooltip>
              </>
            )}
            {row.statut === AbsenceStatus.Soumis && (
              <>
                <Tooltip title="Valider">
                  <IconButton size="small" onClick={(e) => { e.stopPropagation(); validateMutation.mutate(row.id) }}>
                    <Check size={16} />
                  </IconButton>
                </Tooltip>
                <Tooltip title="Refuser">
                  <IconButton size="small" onClick={(e) => { e.stopPropagation(); setRefuseTarget(row) }}>
                    <X size={16} />
                  </IconButton>
                </Tooltip>
              </>
            )}
            {row.statut !== AbsenceStatus.Refuse && row.statut !== AbsenceStatus.Annule && (
              <Tooltip title={row.statut === AbsenceStatus.Brouillon ? 'Supprimer le brouillon' : 'Annuler'}>
                <IconButton size="small" onClick={(e) => { e.stopPropagation(); setCancelTarget(row) }}>
                  <Ban size={16} />
                </IconButton>
              </Tooltip>
            )}
          </Stack>
        )
      },
    },
  ]

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5">Mes absences</Typography>
        <IconButton color="primary" onClick={() => setCreateOpen(true)} aria-label="Créer une absence">
          <Plus size={20} />
        </IconButton>
      </Stack>

      <Grid container spacing={2}>
        <Grid size={{ xs: 6, sm: 3 }}>
          <KpiCard label="Jours validés (mois)" value={monthlyAvailability.data ? `${monthlyAvailability.data.joursAbsenceValidee}` : '—'} />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <KpiCard label="Jours validés (année)" value={annualAvailability.data ? `${annualAvailability.data.joursAbsenceValidee}` : '—'} />
        </Grid>
        <Grid size={{ xs: 6, sm: 3 }}>
          <KpiCard label="Taux de disponibilité (mois)" value={monthlyAvailability.data ? `${monthlyAvailability.data.tauxDisponibilite}%` : '—'} />
        </Grid>
      </Grid>

      <FilterBar
        onReset={() => {
          setResourceId('')
          setStatut('')
        }}
      >
        <TextField select size="small" label="Ressource" value={effectiveResourceId} onChange={(e) => setResourceId(e.target.value)} sx={{ minWidth: 200 }}>
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField select size="small" label="Statut" value={statut} onChange={(e) => setStatut(e.target.value)} sx={{ minWidth: 160 }}>
          <MenuItem value="">(tous)</MenuItem>
          <MenuItem value={String(AbsenceStatus.Brouillon)}>Brouillon</MenuItem>
          <MenuItem value={String(AbsenceStatus.Soumis)}>Soumis</MenuItem>
          <MenuItem value={String(AbsenceStatus.Valide)}>Validé</MenuItem>
          <MenuItem value={String(AbsenceStatus.Refuse)}>Refusé</MenuItem>
          <MenuItem value={String(AbsenceStatus.Annule)}>Annulé</MenuItem>
        </TextField>
      </FilterBar>

      {!effectiveResourceId ? (
        <EmptyState title="Sélectionnez une ressource" description="Choisissez une ressource dans le filtre ci-dessus pour afficher ses absences." />
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
        />
      )}

      <Modal open={createOpen} title="Créer une absence" onClose={() => setCreateOpen(false)}>
        {effectiveResourceId && (
          <AbsenceCreateForm
            resourceId={effectiveResourceId}
            onSuccess={() => {
              setCreateOpen(false)
              invalidate()
            }}
            onCancel={() => setCreateOpen(false)}
          />
        )}
      </Modal>

      {editRow && (
        <Modal open={Boolean(editRow)} title="Modifier le brouillon" onClose={() => setEditRow(null)}>
          <AbsenceEditForm
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
        open={Boolean(cancelTarget)}
        title={cancelTarget?.statut === AbsenceStatus.Brouillon ? 'Supprimer ce brouillon ?' : 'Annuler cette absence ?'}
        description={cancelTarget ? `${typeLabels[cancelTarget.type]} — ${cancelTarget.dateDebut} au ${cancelTarget.dateFin}` : ''}
        destructive
        loading={cancelMutation.isPending}
        onConfirm={() => cancelTarget && cancelMutation.mutate(cancelTarget.id)}
        onCancel={() => setCancelTarget(null)}
      />

      <Modal open={Boolean(refuseTarget)} title="Refuser cette absence" onClose={() => setRefuseTarget(null)}>
        <Stack spacing={2}>
          <TextField
            label="Motif (facultatif)"
            value={refuseReason}
            onChange={(e) => setRefuseReason(e.target.value)}
            multiline
            rows={2}
            fullWidth
            size="small"
          />
          <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
            <Button onClick={() => setRefuseTarget(null)}>Annuler</Button>
            <Button
              variant="contained"
              color="error"
              loading={refuseMutation.isPending}
              onClick={() => refuseTarget && refuseMutation.mutate(refuseTarget.id)}
            >
              Confirmer le refus
            </Button>
          </Stack>
        </Stack>
      </Modal>
    </Stack>
  )
}
