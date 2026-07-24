import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchResources } from '../../../../api/endpoints/resources'
import { fetchTimeEntries } from '../../../../api/endpoints/timeEntries'

export function TimeTab({ orderId }: { orderId: string }) {
  const timeEntriesQuery = useQuery({
    queryKey: ['time-entries', 'byOrder', orderId],
    queryFn: () => fetchTimeEntries({ orderId, pageSize: 200 }),
  })
  const resourcesQuery = useQuery({
    queryKey: ['resources', 'all'],
    queryFn: () => fetchResources({ pageSize: 100 }),
  })
  const resourceLabel = new Map(
    (resourcesQuery.data?.items ?? []).map((r) => [r.id, `${r.prenom} ${r.nom}`]),
  )
  const entries = timeEntriesQuery.data?.items ?? []

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Date</TableCell>
          <TableCell>Ressource</TableCell>
          <TableCell>Durée</TableCell>
          <TableCell>Référence</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {entries.map((entry) => (
          <TableRow key={entry.id}>
            <TableCell>{entry.date}</TableCell>
            <TableCell>{resourceLabel.get(entry.resourceId) ?? entry.resourceId}</TableCell>
            <TableCell>{entry.dureeHeures} h</TableCell>
            <TableCell>{entry.reference ?? '—'}</TableCell>
          </TableRow>
        ))}
        {!entries.length && (
          <TableRow>
            <TableCell colSpan={4}>
              <Typography variant="body2" color="text.secondary">
                Aucune saisie de temps sur cette commande.
              </Typography>
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}
