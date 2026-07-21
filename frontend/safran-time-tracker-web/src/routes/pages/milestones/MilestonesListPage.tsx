import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import ToggleButton from '@mui/material/ToggleButton'
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { fetchApplications } from '../../../api/endpoints/applications'
import { fetchMilestones, type MilestoneListParams } from '../../../api/endpoints/milestones'
import { fetchProjects } from '../../../api/endpoints/projects'
import { fetchResources } from '../../../api/endpoints/resources'
import type { MilestoneDto } from '../../../api/types'
import { MilestoneStatus } from '../../../api/types'
import { FilterBar } from '../../../components/ui/FilterBar'
import { KpiBand } from '../../../components/ui/KpiBand'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge } from '../../../components/ui/StatusBadge'
import { Timeline } from '../../../components/ui/Timeline'
import { MilestoneCreateForm, MilestoneEditForm } from '../projects/MilestoneForm'

const milestoneStatusLabel: Record<MilestoneStatus, string> = {
  0: 'À venir',
  1: 'En cours',
  2: 'Terminé',
  3: 'Annulé',
}
const milestoneStatusTone: Record<MilestoneStatus, 'neutral' | 'info' | 'success' | 'error'> = {
  0: 'neutral',
  1: 'info',
  2: 'success',
  3: 'error',
}
const WEEKDAY_LABELS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim']

type ViewMode = 'tableau' | 'timeline' | 'calendrier'

/** Écran Jalons (§24), vue transverse tous projets — timeline/calendrier/tableau (§24.3), filtres
 * projet/application/responsable/statut, compteur à 30 jours et alerte de dérive planning calculés
 * côté frontend (agrégations sur des données déjà exposées, aucune nouvelle règle serveur, même
 * principe que la totalisation automatique du Lot 9). */
export function MilestonesListPage() {
  const queryClient = useQueryClient()
  const [view, setView] = useState<ViewMode>('tableau')
  const [projectId, setProjectId] = useState('')
  const [applicationId, setApplicationId] = useState('')
  const [responsableId, setResponsableId] = useState('')
  const [statut, setStatut] = useState('')
  const [enRetardOnly, setEnRetardOnly] = useState(false)
  const [referenceMonth, setReferenceMonth] = useState(dayjs().format('YYYY-MM'))
  const [createOpen, setCreateOpen] = useState(false)
  const [createProjectId, setCreateProjectId] = useState('')
  const [editTarget, setEditTarget] = useState<string | null>(null)

  const projectsQuery = useQuery({
    queryKey: ['projects', 'all'],
    queryFn: () => fetchProjects({ pageSize: 100 }),
  })
  const applicationsQuery = useQuery({
    queryKey: ['applications', 'all'],
    queryFn: () => fetchApplications(),
  })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })

  const projectLabel = new Map((projectsQuery.data?.items ?? []).map((p) => [p.id, p.nom]))
  const applicationLabel = new Map((applicationsQuery.data?.items ?? []).map((a) => [a.id, a.nom]))
  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )
  /** §29.5 (Lot 10) : même formule que le filtre serveur alertePlanning de GET /projects, recalculée
   * ici pour l'afficher au niveau du jalon plutôt que de dupliquer un appel par jalon. */
  const projectRisk = new Map(
    (projectsQuery.data?.items ?? []).map((p) => [
      p.id,
      Boolean(p.dateFinAjustee && p.dateFinAjustee > p.dateFinPrevueInitiale),
    ]),
  )

  const filters: MilestoneListParams = {
    pageSize: 200,
    projectId: projectId || undefined,
    applicationId: applicationId || undefined,
    responsableId: responsableId || undefined,
    statut: statut === '' ? undefined : (Number(statut) as MilestoneStatus),
    enRetard: enRetardOnly || undefined,
  }
  const milestonesQuery = useQuery({
    queryKey: ['milestones', 'all', filters],
    queryFn: () => fetchMilestones(filters),
  })
  const items = milestonesQuery.data?.items ?? []
  const editing = items.find((m) => m.id === editTarget)

  const today = dayjs().format('YYYY-MM-DD')
  const in30Days = dayjs().add(30, 'day').format('YYYY-MM-DD')
  const upcoming30 = items.filter(
    (m) =>
      m.datePrevue >= today &&
      m.datePrevue <= in30Days &&
      m.statut !== MilestoneStatus.Termine &&
      m.statut !== MilestoneStatus.Annule,
  ).length
  const enRetardCount = items.filter((m) => m.estEnRetard).length

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: ['milestones', 'all'] })

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5">Jalons</Typography>
        <IconButton
          color="primary"
          onClick={() => {
            setCreateProjectId('')
            setCreateOpen(true)
          }}
          aria-label="Créer un jalon"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>

      <KpiBand
        items={[
          { label: 'Total', value: String(items.length) },
          { label: 'En retard', value: String(enRetardCount) },
          { label: 'À venir sous 30 jours', value: String(upcoming30) },
        ]}
      />

      <FilterBar
        onReset={() => {
          setProjectId('')
          setApplicationId('')
          setResponsableId('')
          setStatut('')
          setEnRetardOnly(false)
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
          label="Application"
          value={applicationId}
          onChange={(e) => setApplicationId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(toutes)</MenuItem>
          {(applicationsQuery.data?.items ?? []).map((a) => (
            <MenuItem key={a.id} value={a.id}>
              {a.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Responsable"
          value={responsableId}
          onChange={(e) => setResponsableId(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          {(resourcesQuery.data?.items ?? []).map((r) => (
            <MenuItem key={r.id} value={r.id}>
              {r.prenom} {r.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Statut"
          value={statut}
          onChange={(e) => setStatut(e.target.value)}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(tous)</MenuItem>
          <MenuItem value={String(MilestoneStatus.AVenir)}>À venir</MenuItem>
          <MenuItem value={String(MilestoneStatus.EnCours)}>En cours</MenuItem>
          <MenuItem value={String(MilestoneStatus.Termine)}>Terminé</MenuItem>
          <MenuItem value={String(MilestoneStatus.Annule)}>Annulé</MenuItem>
        </TextField>
        <TextField
          select
          size="small"
          label="Retard"
          value={enRetardOnly ? 'true' : ''}
          onChange={(e) => setEnRetardOnly(e.target.value === 'true')}
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">(indifférent)</MenuItem>
          <MenuItem value="true">En retard uniquement</MenuItem>
        </TextField>
      </FilterBar>

      <ToggleButtonGroup
        size="small"
        exclusive
        value={view}
        onChange={(_, value: ViewMode | null) => value && setView(value)}
      >
        <ToggleButton value="tableau">Tableau</ToggleButton>
        <ToggleButton value="timeline">Timeline</ToggleButton>
        <ToggleButton value="calendrier">Calendrier</ToggleButton>
      </ToggleButtonGroup>

      {view === 'tableau' && (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Nom</TableCell>
              <TableCell>Projet</TableCell>
              <TableCell>Application</TableCell>
              <TableCell>Responsable</TableCell>
              <TableCell>Date prévue</TableCell>
              <TableCell>Statut</TableCell>
              <TableCell>Alerte planning projet</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((m) => (
              <TableRow
                key={m.id}
                hover
                onClick={() => setEditTarget(m.id)}
                sx={{ cursor: 'pointer' }}
              >
                <TableCell>{m.nom}</TableCell>
                <TableCell>{projectLabel.get(m.projectId) ?? '—'}</TableCell>
                <TableCell>
                  {m.applicationId ? (applicationLabel.get(m.applicationId) ?? '—') : '—'}
                </TableCell>
                <TableCell>{resourceLabel.get(m.responsableId) ?? '—'}</TableCell>
                <TableCell>{m.datePrevue}</TableCell>
                <TableCell>
                  <StatusBadge
                    label={milestoneStatusLabel[m.statut]}
                    tone={m.estEnRetard ? 'error' : milestoneStatusTone[m.statut]}
                  />
                </TableCell>
                <TableCell>
                  {projectRisk.get(m.projectId) ? (
                    <StatusBadge label="Risque planning" tone="error" />
                  ) : (
                    '—'
                  )}
                </TableCell>
              </TableRow>
            ))}
            {!items.length && (
              <TableRow>
                <TableCell colSpan={7}>
                  <Typography variant="body2" color="text.secondary">
                    Aucun jalon pour ces filtres.
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}

      {view === 'timeline' && (
        <Timeline
          items={items.map((m) => ({
            id: m.id,
            date: m.datePrevue,
            label: m.nom,
            sublabel: `${projectLabel.get(m.projectId) ?? m.projectId} — ${resourceLabel.get(m.responsableId) ?? m.responsableId}${projectRisk.get(m.projectId) ? ' — risque planning projet' : ''}`,
            statusLabel: milestoneStatusLabel[m.statut],
            statusTone: m.estEnRetard ? 'error' : milestoneStatusTone[m.statut],
            highlighted: m.estEnRetard,
          }))}
          emptyLabel="Aucun jalon pour ces filtres."
        />
      )}

      {view === 'calendrier' && (
        <Stack spacing={2}>
          <TextField
            size="small"
            type="month"
            label="Mois"
            value={referenceMonth}
            onChange={(e) => setReferenceMonth(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
            sx={{ maxWidth: 200 }}
          />
          <MilestonesCalendarView
            milestones={items}
            referenceMonth={referenceMonth}
            onSelect={setEditTarget}
          />
        </Stack>
      )}

      <Modal open={createOpen} title="Créer un jalon" onClose={() => setCreateOpen(false)}>
        {createProjectId ? (
          <MilestoneCreateForm
            projectId={createProjectId}
            onSuccess={() => {
              setCreateOpen(false)
              invalidate()
            }}
            onCancel={() => setCreateOpen(false)}
          />
        ) : (
          <Stack spacing={2}>
            <TextField
              select
              size="small"
              label="Projet"
              value={createProjectId}
              onChange={(e) => setCreateProjectId(e.target.value)}
              fullWidth
            >
              {(projectsQuery.data?.items ?? []).map((p) => (
                <MenuItem key={p.id} value={p.id}>
                  {p.nom}
                </MenuItem>
              ))}
            </TextField>
            <Typography variant="caption" color="text.secondary">
              Un jalon appartient toujours à un projet (§24.2) : sélectionnez-le d'abord.
            </Typography>
          </Stack>
        )}
      </Modal>

      {editing && (
        <Modal
          open={Boolean(editing)}
          title="Modifier le jalon"
          onClose={() => setEditTarget(null)}
        >
          <MilestoneEditForm
            milestone={editing}
            onSuccess={() => {
              setEditTarget(null)
              invalidate()
            }}
            onCancel={() => setEditTarget(null)}
          />
        </Modal>
      )}
    </Stack>
  )
}

function MilestonesCalendarView({
  milestones,
  referenceMonth,
  onSelect,
}: {
  milestones: MilestoneDto[]
  referenceMonth: string
  onSelect: (id: string) => void
}) {
  const start = dayjs(`${referenceMonth}-01`)
  const daysInMonth = start.daysInMonth()
  const firstWeekday = (start.day() + 6) % 7 // 0 = lundi
  const cells: (number | null)[] = [
    ...Array.from({ length: firstWeekday }, () => null),
    ...Array.from({ length: daysInMonth }, (_, i) => i + 1),
  ]
  while (cells.length % 7 !== 0) {
    cells.push(null)
  }

  const byDay = new Map<number, MilestoneDto[]>()
  for (const m of milestones) {
    const date = dayjs(m.datePrevue)
    if (date.format('YYYY-MM') === referenceMonth) {
      const day = date.date()
      byDay.set(day, [...(byDay.get(day) ?? []), m])
    }
  }

  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 1 }}>
      {WEEKDAY_LABELS.map((label) => (
        <Typography
          key={label}
          variant="caption"
          color="text.secondary"
          sx={{ textAlign: 'center', fontWeight: 600 }}
        >
          {label}
        </Typography>
      ))}
      {cells.map((day, index) => (
        <Box
          key={index}
          sx={{
            minHeight: 90,
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 1,
            p: 0.5,
          }}
        >
          {day && (
            <Stack spacing={0.5}>
              <Typography variant="caption" color="text.secondary">
                {day}
              </Typography>
              {(byDay.get(day) ?? []).map((m) => (
                <Box key={m.id} onClick={() => onSelect(m.id)} sx={{ cursor: 'pointer' }}>
                  <StatusBadge
                    label={m.nom}
                    tone={m.estEnRetard ? 'error' : milestoneStatusTone[m.statut]}
                  />
                </Box>
              ))}
            </Stack>
          )}
        </Box>
      ))}
    </Box>
  )
}
