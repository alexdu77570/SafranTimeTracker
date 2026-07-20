import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Chip from '@mui/material/Chip'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { fetchApplicationById } from '../../../api/endpoints/applications'
import { fetchCharges } from '../../../api/endpoints/reporting'
import { fetchServices, fetchTeams } from '../../../api/endpoints/organisation'
import { fetchTechnologies } from '../../../api/endpoints/technologies'
import type { ApplicationCriticality } from '../../../api/types'
import { ReferentialStatus, ReportingPeriodType } from '../../../api/types'
import { EmptyState } from '../../../components/ui/EmptyState'
import { KpiCard } from '../../../components/ui/KpiCard'
import { StatusBadge } from '../../../components/ui/StatusBadge'

const criticiteLabel: Record<ApplicationCriticality, string> = { 0: 'Faible', 1: 'Moyenne', 2: 'Élevée', 3: 'Critique' }

/** Fiche Application (§15) + détail statistique (charges RUN/hors RUN, décomptes de références —
 * réutilise GET /api/v1/reporting/charges du Lot 5, filtré par applicationId, aucune nouvelle
 * agrégation backend) + technologies rattachées (docs/BACKLOG_METIER.md §5, Lot 8). */
export function ApplicationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const applicationId = id ?? ''

  const appQuery = useQuery({
    queryKey: ['applications', applicationId],
    queryFn: () => fetchApplicationById(applicationId),
    enabled: Boolean(applicationId),
  })
  const servicesQuery = useQuery({ queryKey: ['services', 'all'], queryFn: () => fetchServices() })
  const teamsQuery = useQuery({ queryKey: ['teams', 'all'], queryFn: () => fetchTeams() })
  const chargesQuery = useQuery({
    queryKey: ['reporting', 'charges', applicationId],
    queryFn: () => fetchCharges({ applicationId, periodType: ReportingPeriodType.Annee }),
    enabled: Boolean(applicationId),
  })
  const technologiesQuery = useQuery({
    queryKey: ['technologies', 'byApplication', applicationId],
    queryFn: () => fetchTechnologies({ applicationId }),
    enabled: Boolean(applicationId),
  })

  if (appQuery.isLoading) {
    return <EmptyState title="Chargement de la fiche…" />
  }
  const application = appQuery.data
  if (!application) {
    return <EmptyState title="Application introuvable" />
  }

  const service = servicesQuery.data?.items.find((s) => s.id === application.serviceId)
  const team = teamsQuery.data?.items.find((t) => t.id === application.teamId)
  const charges = chargesQuery.data

  return (
    <Stack spacing={3}>
      <Typography variant="h5">{application.nom}</Typography>

      <Card>
        <CardHeader title="Informations générales" />
        <CardContent>
          <Table size="small">
            <TableBody>
              <TableRow>
                <TableCell>Code</TableCell>
                <TableCell>{application.code}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Service</TableCell>
                <TableCell>{service?.nom ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Équipe</TableCell>
                <TableCell>{team?.nom ?? '—'}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Criticité</TableCell>
                <TableCell>{criticiteLabel[application.criticite]}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Statut</TableCell>
                <TableCell>
                  <StatusBadge
                    label={application.statut === ReferentialStatus.Actif ? 'Actif' : 'Inactif'}
                    tone={application.statut === ReferentialStatus.Actif ? 'success' : 'neutral'}
                  />
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell>Commentaire</TableCell>
                <TableCell>{application.commentaire ?? '—'}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Détail statistique" subheader="Année en cours" />
        <CardContent>
          {charges ? (
            <Grid container spacing={2}>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Charge totale" value={`${charges.chargeTotaleHeures} h`} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Charge RUN" value={`${charges.chargeRunHeures} h`} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Charge hors RUN" value={`${charges.chargeHorsRunHeures} h`} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Incidents" value={String(charges.nombreIncidents)} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Changes" value={String(charges.nombreChanges)} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="Problems" value={String(charges.nombreProblems)} />
              </Grid>
              <Grid size={{ xs: 6, sm: 3 }}>
                <KpiCard label="RITM" value={String(charges.nombreRitm)} />
              </Grid>
            </Grid>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucune donnée statistique disponible.
            </Typography>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader title="Technologies" />
        <CardContent>
          {technologiesQuery.data?.items.length ? (
            <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
              {technologiesQuery.data.items.map((technology) => (
                <Chip key={technology.id} label={technology.libelle} size="small" />
              ))}
            </Stack>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucune technologie rattachée.
            </Typography>
          )}
        </CardContent>
      </Card>
    </Stack>
  )
}
