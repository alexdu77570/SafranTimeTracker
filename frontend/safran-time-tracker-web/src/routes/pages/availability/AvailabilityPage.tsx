import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Grid from '@mui/material/Grid'
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
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useQueries, useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'
import 'dayjs/locale/fr'
import { useMemo, useState } from 'react'
import { fetchAbsences } from '../../../api/endpoints/absences'
import { fetchAvailability } from '../../../api/endpoints/availability'
import { fetchHolidays } from '../../../api/endpoints/holidayCalendar'
import { fetchDepartments, fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchResources } from '../../../api/endpoints/resources'
import { AbsenceStatus, AbsenceType, ReferentialStatus, type ResourceDto } from '../../../api/types'
import { useCurrentUser } from '../../../auth/useCurrentUser'
import { EmptyState } from '../../../components/ui/EmptyState'
import { FilterBar } from '../../../components/ui/FilterBar'
import { StatusBadge, type StatusTone } from '../../../components/ui/StatusBadge'
import { weekBounds } from '../../../lib/dateUtils'

const typeLabels: Record<AbsenceType, string> = {
  [AbsenceType.Conge]: 'Congé',
  [AbsenceType.Rtt]: 'RTT',
  [AbsenceType.Maladie]: 'Maladie',
  [AbsenceType.Formation]: 'Formation',
  [AbsenceType.Deplacement]: 'Déplacement',
  [AbsenceType.Indisponible]: 'Indisponible',
}

type PeriodType = 'mensuelle' | 'hebdomadaire'

/** Bornes de la période affichée (mensuelle = mois calendaire, hebdomadaire = `weekBounds`
 * partagée avec `/temps`, `lib/dateUtils.ts`). Fonction pure, testée sans dépendre de l'ordre de
 * montage de l'application. */
export function periodBounds(periodType: PeriodType, referenceDate: string): { start: string; end: string } {
  if (periodType === 'hebdomadaire') {
    return weekBounds(referenceDate)
  }
  return {
    start: dayjs(referenceDate).locale('fr').startOf('month').format('YYYY-MM-DD'),
    end: dayjs(referenceDate).locale('fr').endOf('month').format('YYYY-MM-DD'),
  }
}

/**
 * Disponibilités (§22, Lot 9). Vue équipe = N appels à GET /resources/{id}/availability (un par
 * ressource filtrée, §3.5 — aucune agrégation dupliquée côté client, chaque nombre vient du
 * backend). Calendrier coloré = composition de faits déjà exposés (jours fériés, absences) plus
 * les week-ends calculés — un classement d'affichage, pas un recalcul de capacité (CLAUDE.md §5).
 * Accès non restreint par rôle : aucun périmètre organisationnel n'existe côté serveur à ce jour
 * (écart déjà documenté, reconduit — §3.6, pas de restriction simulée côté client).
 */
export function AvailabilityPage() {
  const { user } = useCurrentUser()
  const [departmentId, setDepartmentId] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [teamId, setTeamId] = useState('')
  const [periodType, setPeriodType] = useState<PeriodType>('mensuelle')
  const [referenceDate, setReferenceDate] = useState(dayjs().format('YYYY-MM-DD'))
  const [calendarResourceId, setCalendarResourceId] = useState('')

  const { start, end } = periodBounds(periodType, referenceDate)

  const departmentsQuery = useQuery({ queryKey: ['departments', 'all'], queryFn: () => fetchDepartments() })
  const servicesQuery = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teamsQuery = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'availability-scope', departmentId, serviceId],
    queryFn: () => fetchResources({ pageSize: 100, departmentId: departmentId || undefined, serviceId: serviceId || undefined }),
  })
  const resources = useMemo(
    () => (resourcesQuery.data?.items ?? []).filter((r) => !teamId || r.teamId === teamId),
    [resourcesQuery.data, teamId],
  )

  const availabilityQueries = useQueries({
    queries: resources.map((r) => ({
      queryKey: ['availability', r.id, start, end],
      queryFn: () => fetchAvailability(r.id, start, end),
      enabled: resources.length > 0,
    })),
  })

  const effectiveCalendarResourceId = calendarResourceId || user?.resourceId || resources[0]?.id || ''
  const yearsInPeriod = Array.from(new Set([dayjs(start).year(), dayjs(end).year()]))
  const holidaysQueries = useQueries({ queries: yearsInPeriod.map((year) => ({ queryKey: ['holidays', year], queryFn: () => fetchHolidays(year) })) })
  const holidayDates = new Set(
    holidaysQueries.flatMap((q) => q.data?.items.filter((h) => h.statut === ReferentialStatus.Actif).map((h) => h.date) ?? []),
  )

  const calendarAbsencesQuery = useQuery({
    queryKey: ['absences', 'calendar', effectiveCalendarResourceId, start, end],
    queryFn: () => fetchAbsences({ resourceId: effectiveCalendarResourceId, statut: AbsenceStatus.Valide, pageSize: 100 }),
    enabled: Boolean(effectiveCalendarResourceId),
  })

  const days: string[] = []
  for (let d = dayjs(start); d.isBefore(dayjs(end).add(1, 'day')); d = d.add(1, 'day')) {
    days.push(d.format('YYYY-MM-DD'))
  }

  function classifyDay(date: string): { label: string; tone: StatusTone } {
    const day = dayjs(date)
    if (day.day() === 0 || day.day() === 6) {
      return { label: 'Week-end', tone: 'neutral' }
    }
    if (holidayDates.has(date)) {
      return { label: 'Férié', tone: 'info' }
    }
    const absence = calendarAbsencesQuery.data?.items.find((a) => a.dateDebut <= date && a.dateFin >= date)
    if (absence) {
      return { label: typeLabels[absence.type], tone: 'warning' }
    }
    return { label: 'Disponible', tone: 'success' }
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Disponibilités</Typography>

      <FilterBar
        onReset={() => {
          setDepartmentId('')
          setServiceId('')
          setTeamId('')
        }}
      >
        <TextField select size="small" label="Département" value={departmentId} onChange={(e) => setDepartmentId(e.target.value)} sx={{ minWidth: 180 }}>
          <MenuItem value="">(tous)</MenuItem>
          {(departmentsQuery.data?.items ?? []).map((d) => (
            <MenuItem key={d.id} value={d.id}>
              {d.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField select size="small" label="Service" value={serviceId} onChange={(e) => setServiceId(e.target.value)} sx={{ minWidth: 180 }}>
          <MenuItem value="">(tous)</MenuItem>
          {(servicesQuery.data?.items ?? []).map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField select size="small" label="Équipe" value={teamId} onChange={(e) => setTeamId(e.target.value)} sx={{ minWidth: 180 }}>
          <MenuItem value="">(toutes)</MenuItem>
          {(teamsQuery.data?.items ?? []).map((t) => (
            <MenuItem key={t.id} value={t.id}>
              {t.nom}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="date"
          label="Période de référence"
          value={referenceDate}
          onChange={(e) => setReferenceDate(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <ToggleButtonGroup
          size="small"
          exclusive
          value={periodType}
          onChange={(_, value: PeriodType | null) => value && setPeriodType(value)}
        >
          <ToggleButton value="mensuelle">Mensuelle</ToggleButton>
          <ToggleButton value="hebdomadaire">Hebdomadaire</ToggleButton>
        </ToggleButtonGroup>
      </FilterBar>

      <Card>
        <CardHeader title="Capacité par ressource" subheader={`Du ${start} au ${end}`} />
        <CardContent>
          {resources.length === 0 ? (
            <EmptyState title="Aucune ressource" description="Ajustez les filtres département/service/équipe." />
          ) : (
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Ressource</TableCell>
                  <TableCell>Capacité théorique</TableCell>
                  <TableCell>Capacité réelle</TableCell>
                  <TableCell>Taux de disponibilité</TableCell>
                  <TableCell>Jours fériés</TableCell>
                  <TableCell>Jours d'absence</TableCell>
                  <TableCell>Charge RUN</TableCell>
                  <TableCell>Charge hors RUN</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {resources.map((resource, index) => {
                  const result = availabilityQueries[index]?.data
                  return (
                    <TableRow
                      key={resource.id}
                      hover
                      selected={resource.id === effectiveCalendarResourceId}
                      onClick={() => setCalendarResourceId(resource.id)}
                      sx={{ cursor: 'pointer' }}
                    >
                      <TableCell>{resource.prenom} {resource.nom}</TableCell>
                      <TableCell>{result ? `${result.capaciteTheorique} h` : '…'}</TableCell>
                      <TableCell>{result ? `${result.capaciteReelle} h` : '…'}</TableCell>
                      <TableCell>{result ? `${result.tauxDisponibilite}%` : '…'}</TableCell>
                      <TableCell>{result?.joursFeries ?? '…'}</TableCell>
                      <TableCell>{result?.joursAbsenceValidee ?? '…'}</TableCell>
                      <TableCell>{result ? `${result.chargeRunHeures} h` : '…'}</TableCell>
                      <TableCell>{result ? `${result.chargeHorsRunHeures} h` : '…'}</TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader
          title="Calendrier coloré"
          subheader={resourceLabel(resources, effectiveCalendarResourceId) ?? 'Sélectionnez une ressource dans le tableau ci-dessus'}
        />
        <CardContent>
          {effectiveCalendarResourceId ? (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
              {days.map((date) => {
                const { label, tone } = classifyDay(date)
                return (
                  <Tooltip key={date} title={`${dayjs(date).format('DD/MM/YYYY')} — ${label}`}>
                    <Box>
                      <StatusBadge label={dayjs(date).format('DD')} tone={tone} />
                    </Box>
                  </Tooltip>
                )
              })}
            </Box>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucune ressource sélectionnée.
            </Typography>
          )}
        </CardContent>
      </Card>

      <Grid container spacing={1}>
        {(['success', 'info', 'warning', 'neutral'] as StatusTone[]).map((tone) => (
          <Grid key={tone}>
            <StatusBadge
              tone={tone}
              label={tone === 'success' ? 'Disponible' : tone === 'info' ? 'Férié' : tone === 'warning' ? 'Absence' : 'Week-end'}
            />
          </Grid>
        ))}
      </Grid>
    </Stack>
  )
}

function resourceLabel(resources: ResourceDto[], resourceId: string): string | null {
  const resource = resources.find((r) => r.id === resourceId)
  return resource ? `${resource.prenom} ${resource.nom}` : null
}
