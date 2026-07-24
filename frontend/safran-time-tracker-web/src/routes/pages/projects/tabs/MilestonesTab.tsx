import Button from '@mui/material/Button'
import Grid from '@mui/material/Grid'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { fetchMilestones } from '../../../../api/endpoints/milestones'
import { MilestoneStatus } from '../../../../api/types'
import { KpiCard } from '../../../../components/ui/KpiCard'
import { Modal } from '../../../../components/ui/Modal'
import { StatusBadge } from '../../../../components/ui/StatusBadge'
import { Timeline } from '../../../../components/ui/Timeline'
import { MilestoneCreateForm, MilestoneEditForm } from '../MilestoneForm'
import type { Labels } from '../useOrganisationLabels'

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

export function MilestonesTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<string | null>(null)
  const milestonesQuery = useQuery({
    queryKey: ['milestones', 'byProject', projectId],
    queryFn: () => fetchMilestones({ projectId, pageSize: 100 }),
  })
  const items = milestonesQuery.data?.items ?? []
  const editing = items.find((m) => m.id === editTarget)

  const enRetard = items.filter((m) => m.estEnRetard).length
  const aVenir = items.filter((m) => m.statut === MilestoneStatus.AVenir).length
  const termines = items.filter((m) => m.statut === MilestoneStatus.Termine).length

  return (
    <Stack spacing={2}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="En retard" value={String(enRetard)} />
        </Grid>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="À venir" value={String(aVenir)} />
        </Grid>
        <Grid size={{ xs: 4 }}>
          <KpiCard label="Terminés" value={String(termines)} />
        </Grid>
      </Grid>

      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton color="primary" onClick={() => setCreateOpen(true)} aria-label="Créer un jalon">
          <Plus size={20} />
        </IconButton>
      </Stack>

      <Timeline
        items={items.map((m) => ({
          id: m.id,
          date: m.datePrevue,
          label: m.nom,
          sublabel: labels.resourceLabel.get(m.responsableId),
          statusLabel: milestoneStatusLabel[m.statut],
          statusTone: m.estEnRetard ? 'error' : milestoneStatusTone[m.statut],
          highlighted: m.estEnRetard,
        }))}
        emptyLabel="Aucun jalon pour ce projet."
      />

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Nom</TableCell>
            <TableCell>Date prévue</TableCell>
            <TableCell>Statut</TableCell>
            <TableCell>Dépend de</TableCell>
            <TableCell>Actions</TableCell>
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
              <TableCell>{m.datePrevue}</TableCell>
              <TableCell>
                <StatusBadge
                  label={milestoneStatusLabel[m.statut]}
                  tone={m.estEnRetard ? 'error' : milestoneStatusTone[m.statut]}
                />
              </TableCell>
              <TableCell>
                {m.dependsOnMilestoneId
                  ? (items.find((o) => o.id === m.dependsOnMilestoneId)?.nom ?? '—')
                  : '—'}
              </TableCell>
              <TableCell>
                <Button
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation()
                    setEditTarget(m.id)
                  }}
                >
                  Modifier
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Créer un jalon" onClose={() => setCreateOpen(false)}>
        <MilestoneCreateForm
          projectId={projectId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['milestones', 'byProject', projectId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
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
              void queryClient.invalidateQueries({
                queryKey: ['milestones', 'byProject', projectId],
              })
            }}
            onCancel={() => setEditTarget(null)}
          />
        </Modal>
      )}
    </Stack>
  )
}
