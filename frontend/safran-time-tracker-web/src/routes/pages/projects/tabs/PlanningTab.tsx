import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Chip from '@mui/material/Chip'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { fetchTimeEntries } from '../../../../api/endpoints/timeEntries'
import {
  createAdjustedPlanVersion,
  fetchProjectPlanVersions,
  fetchWeeklyPlans,
  setWeeklyPlans,
} from '../../../../api/endpoints/projectPlanning'
import { ProjectPlanVersionStatus, ProjectPlanVersionType } from '../../../../api/types'
import { Modal } from '../../../../components/ui/Modal'
import { StatusBadge } from '../../../../components/ui/StatusBadge'
import { WeeklyPlanningGrid } from '../../../../components/ui/WeeklyPlanningGrid'
import { weekBounds } from '../../../../lib/dateUtils'
import type { Labels } from '../useOrganisationLabels'

export function PlanningTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const queryClient = useQueryClient()
  const [motifOpen, setMotifOpen] = useState(false)
  const [motif, setMotif] = useState('')

  const versionsQuery = useQuery({
    queryKey: ['project-plan-versions', projectId],
    queryFn: () => fetchProjectPlanVersions(projectId),
  })
  const initialVersion = versionsQuery.data?.items.find(
    (v) => v.type === ProjectPlanVersionType.Initial,
  )
  const activeAdjustedVersion = versionsQuery.data?.items.find(
    (v) => v.type === ProjectPlanVersionType.Ajuste && v.statut === ProjectPlanVersionStatus.Active,
  )

  const initialPlansQuery = useQuery({
    queryKey: ['weekly-plans', projectId, initialVersion?.id],
    queryFn: () => fetchWeeklyPlans(projectId, initialVersion?.id ?? ''),
    enabled: Boolean(initialVersion),
  })
  const adjustedPlansQuery = useQuery({
    queryKey: ['weekly-plans', projectId, activeAdjustedVersion?.id],
    queryFn: () => fetchWeeklyPlans(projectId, activeAdjustedVersion?.id ?? ''),
    enabled: Boolean(activeAdjustedVersion),
  })
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 1000 }),
  })

  const createAdjustedMutation = useMutation({
    mutationFn: () => createAdjustedPlanVersion(projectId, { motif }),
    onSuccess: () => {
      setMotifOpen(false)
      setMotif('')
      void queryClient.invalidateQueries({ queryKey: ['project-plan-versions', projectId] })
    },
  })
  const setWeeklyPlansMutation = useMutation({
    mutationFn: ({
      versionId,
      resourceId,
      weekStartDate,
      value,
    }: {
      versionId: string
      resourceId: string
      weekStartDate: string
      value: number
    }) =>
      setWeeklyPlans(projectId, versionId, [
        { resourceId, weekStartDate, chargePlanifieeHeures: value },
      ]),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['weekly-plans', projectId, activeAdjustedVersion?.id],
      })
    },
  })

  const { weekStartDates, rows } = useMemo(() => {
    const weeks = new Set<string>()
    const byResource = new Map<
      string,
      Record<string, { initial?: number; ajuste?: number; realise?: number; surcharge?: boolean }>
    >()

    for (const line of initialPlansQuery.data ?? []) {
      weeks.add(line.weekStartDate)
      const resourceWeeks = byResource.get(line.resourceId) ?? {}
      resourceWeeks[line.weekStartDate] = {
        ...resourceWeeks[line.weekStartDate],
        initial: line.chargePlanifieeHeures,
      }
      byResource.set(line.resourceId, resourceWeeks)
    }
    for (const line of adjustedPlansQuery.data ?? []) {
      weeks.add(line.weekStartDate)
      const resourceWeeks = byResource.get(line.resourceId) ?? {}
      resourceWeeks[line.weekStartDate] = {
        ...resourceWeeks[line.weekStartDate],
        ajuste: line.chargePlanifieeHeures,
      }
      byResource.set(line.resourceId, resourceWeeks)
    }
    for (const entry of timeEntriesQuery.data?.items ?? []) {
      const weekStart = weekBounds(entry.date).start
      weeks.add(weekStart)
      const resourceWeeks = byResource.get(entry.resourceId) ?? {}
      const cell = resourceWeeks[weekStart] ?? {}
      cell.realise = (cell.realise ?? 0) + entry.dureeHeures
      resourceWeeks[weekStart] = cell
      byResource.set(entry.resourceId, resourceWeeks)
    }

    const sortedWeeks = [...weeks].sort()
    const gridRows = [...byResource.entries()].map(([resourceId, weeksData]) => ({
      id: resourceId,
      label: labels.resourceLabel.get(resourceId) ?? resourceId,
      weeks: weeksData,
    }))
    return { weekStartDates: sortedWeeks, rows: gridRows }
  }, [initialPlansQuery.data, adjustedPlansQuery.data, timeEntriesQuery.data, labels.resourceLabel])

  return (
    <Stack spacing={2}>
      <Card>
        <CardHeader
          title="Versions de planning (§18.3)"
          action={
            <Button size="small" onClick={() => setMotifOpen(true)}>
              Nouvelle version Ajustée
            </Button>
          }
        />
        <CardContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Type</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell>Motif</TableCell>
                <TableCell>Créée le</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(versionsQuery.data?.items ?? []).map((version) => (
                <TableRow key={version.id}>
                  <TableCell>
                    {version.type === ProjectPlanVersionType.Initial ? 'Initiale' : 'Ajustée'}
                  </TableCell>
                  <TableCell>
                    <StatusBadge
                      label={
                        version.statut === ProjectPlanVersionStatus.Active ? 'Active' : 'Archivée'
                      }
                      tone={
                        version.statut === ProjectPlanVersionStatus.Active ? 'success' : 'neutral'
                      }
                    />
                  </TableCell>
                  <TableCell>{version.motif ?? '—'}</TableCell>
                  <TableCell>{version.createdAt}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader
          title="Grille hebdomadaire (§17.3, §18.2)"
          subheader="Le réalisé provient exclusivement des saisies de temps."
        />
        <CardContent>
          <WeeklyPlanningGrid
            weekStartDates={weekStartDates}
            rows={rows}
            onAjusteChange={
              activeAdjustedVersion
                ? (resourceId, weekStartDate, value) =>
                    setWeeklyPlansMutation.mutate({
                      versionId: activeAdjustedVersion.id,
                      resourceId,
                      weekStartDate,
                      value,
                    })
                : undefined
            }
          />
        </CardContent>
      </Card>

      <Modal open={motifOpen} title="Nouvelle version Ajustée" onClose={() => setMotifOpen(false)}>
        <Stack spacing={2}>
          <Chip label="Motif obligatoire (§18.3)" size="small" sx={{ alignSelf: 'flex-start' }} />
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
            <Button onClick={() => setMotifOpen(false)}>Annuler</Button>
            <Button
              variant="contained"
              disabled={!motif.trim()}
              loading={createAdjustedMutation.isPending}
              onClick={() => createAdjustedMutation.mutate()}
            >
              Créer
            </Button>
          </Stack>
        </Stack>
      </Modal>
    </Stack>
  )
}
