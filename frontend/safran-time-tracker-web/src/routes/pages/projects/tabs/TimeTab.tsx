import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchTimeEntries } from '../../../../api/endpoints/timeEntries'
import { weekBounds } from '../../../../lib/dateUtils'
import type { Labels } from '../useOrganisationLabels'

export function TimeTab({ projectId, labels }: { projectId: string; labels: Labels }) {
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byProject', 'detail', projectId],
    queryFn: () => fetchTimeEntries({ projectId, pageSize: 200 }),
  })
  const entries = timeEntriesQuery.data?.items ?? []

  const byPersonne = new Map<string, number>()
  const bySemaine = new Map<string, number>()
  for (const entry of entries) {
    byPersonne.set(entry.resourceId, (byPersonne.get(entry.resourceId) ?? 0) + entry.dureeHeures)
    const weekStart = weekBounds(entry.date).start
    bySemaine.set(weekStart, (bySemaine.get(weekStart) ?? 0) + entry.dureeHeures)
  }

  return (
    <Stack spacing={2}>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Temps par personne" />
            <CardContent>
              <Table size="small">
                <TableBody>
                  {[...byPersonne.entries()].map(([resourceId, heures]) => (
                    <TableRow key={resourceId}>
                      <TableCell>{labels.resourceLabel.get(resourceId) ?? resourceId}</TableCell>
                      <TableCell>{heures} h</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Card>
            <CardHeader title="Temps par semaine" />
            <CardContent>
              <Table size="small">
                <TableBody>
                  {[...bySemaine.entries()].sort().map(([week, heures]) => (
                    <TableRow key={week}>
                      <TableCell>{week}</TableCell>
                      <TableCell>{heures} h</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Card>
        <CardHeader title="Temps détaillés" />
        <CardContent>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Ressource</TableCell>
                <TableCell>Durée</TableCell>
                <TableCell>Référence</TableCell>
                <TableCell>Modifié le</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {entries.map((entry) => (
                <TableRow key={entry.id}>
                  <TableCell>{entry.date}</TableCell>
                  <TableCell>
                    {labels.resourceLabel.get(entry.resourceId) ?? entry.resourceId}
                  </TableCell>
                  <TableCell>{entry.dureeHeures} h</TableCell>
                  <TableCell>{entry.reference ?? '—'}</TableCell>
                  <TableCell>{entry.updatedAt ?? entry.createdAt}</TableCell>
                </TableRow>
              ))}
              {!entries.length && (
                <TableRow>
                  <TableCell colSpan={5}>
                    <Typography variant="body2" color="text.secondary">
                      Aucune saisie de temps sur ce projet.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </Stack>
  )
}
