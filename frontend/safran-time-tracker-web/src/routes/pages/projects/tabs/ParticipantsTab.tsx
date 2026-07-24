import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import {
  removeProjectParticipant,
  fetchProjectParticipants,
} from '../../../../api/endpoints/projectParticipants'
import { fetchResources } from '../../../../api/endpoints/resources'
import { fetchTimeEntries } from '../../../../api/endpoints/timeEntries'
import { ConfirmDialog } from '../../../../components/ui/ConfirmDialog'
import { FinancialValue } from '../../../../components/ui/FinancialValue'
import { Modal } from '../../../../components/ui/Modal'
import { getOperationalRoleLabel } from '../../../../lib/knownReferentials'
import { ProjectParticipantCreateForm } from '../ProjectParticipantForm'

export function ParticipantsTab({ projectId }: { projectId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [removeTarget, setRemoveTarget] = useState<string | null>(null)
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const participantsQuery = useQuery({
    queryKey: ['project-participants', projectId],
    queryFn: () => fetchProjectParticipants(projectId),
  })
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 1000 }),
  })
  const removeMutation = useMutation({
    mutationFn: (participantId: string) => removeProjectParticipant(projectId, participantId),
    onSuccess: () => {
      setRemoveTarget(null)
      void queryClient.invalidateQueries({ queryKey: ['project-participants', projectId] })
    },
  })

  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )

  /** Temps consommé/reste à faire par participant (§17.2) : agrégation frontend sur des saisies
   * déjà filtrées par permission (financialSnapshot absent sans FINANCIAL_DATA_VIEW, même principe
   * que la totalisation automatique du Lot 9) — aucune nouvelle règle métier serveur. */
  const consumedByResource = new Map<string, number>()
  for (const entry of timeEntriesQuery.data?.items ?? []) {
    consumedByResource.set(
      entry.resourceId,
      (consumedByResource.get(entry.resourceId) ?? 0) + entry.dureeHeures,
    )
  }

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Ajouter un participant"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Ressource</TableCell>
            <TableCell>Rôle opérationnel</TableCell>
            <TableCell>Période</TableCell>
            <TableCell>Capacité prévue</TableCell>
            <TableCell>Temps consommé</TableCell>
            <TableCell>Reste à faire</TableCell>
            <TableCell>TJM personne</TableCell>
            <TableCell>TJM contrat</TableCell>
            <TableCell>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(participantsQuery.data?.items ?? []).map((participant) => {
            const consomme = consumedByResource.get(participant.resourceId) ?? 0
            const resteAFaire =
              participant.capacitePrevue !== null
                ? Math.max(participant.capacitePrevue - consomme, 0)
                : null
            return (
              <TableRow key={participant.id}>
                <TableCell>
                  {resourceLabel.get(participant.resourceId) ?? participant.resourceId}
                </TableCell>
                <TableCell>{getOperationalRoleLabel(participant.operationalRoleId)}</TableCell>
                <TableCell>
                  {participant.dateDebut} — {participant.dateFin ?? 'en cours'}
                </TableCell>
                <TableCell>{participant.capacitePrevue ?? '—'}</TableCell>
                <TableCell>{consomme} h</TableCell>
                <TableCell>{resteAFaire !== null ? `${resteAFaire} h` : '—'}</TableCell>
                <TableCell>
                  <FinancialValue value={participant.financialSummary?.tjmPersonneApplicable} />
                </TableCell>
                <TableCell>
                  <FinancialValue value={participant.financialSummary?.tjmContratApplicable} />
                </TableCell>
                <TableCell>
                  <Button
                    size="small"
                    color="error"
                    onClick={() => setRemoveTarget(participant.id)}
                  >
                    Retirer
                  </Button>
                </TableCell>
              </TableRow>
            )
          })}
          {!participantsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={9}>
                <Typography variant="body2" color="text.secondary">
                  Aucun participant.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Ajouter un participant" onClose={() => setCreateOpen(false)}>
        <ProjectParticipantCreateForm
          projectId={projectId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['project-participants', projectId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>

      <ConfirmDialog
        open={Boolean(removeTarget)}
        title="Retirer ce participant ?"
        description="Le participant sera désactivé (retrait logique), jamais supprimé physiquement."
        destructive
        loading={removeMutation.isPending}
        onConfirm={() => removeTarget && removeMutation.mutate(removeTarget)}
        onCancel={() => setRemoveTarget(null)}
      />
    </Stack>
  )
}
