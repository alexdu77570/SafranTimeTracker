import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchProjectLinkedReferences } from '../../../../api/endpoints/reporting'

export function LinkedReferencesTab({ projectId }: { projectId: string }) {
  const referencesQuery = useQuery({
    queryKey: ['project-linked-references', projectId],
    queryFn: () => fetchProjectLinkedReferences(projectId),
  })

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Référence</TableCell>
          <TableCell>Type</TableCell>
          <TableCell>Nombre de saisies</TableCell>
          <TableCell>Charge</TableCell>
          <TableCell>Première date</TableCell>
          <TableCell>Dernière date</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {(referencesQuery.data ?? []).map((ref) => (
          <TableRow key={ref.reference}>
            <TableCell>{ref.reference}</TableCell>
            <TableCell>{ref.activityTypeLibelle}</TableCell>
            <TableCell>{ref.nombreSaisies}</TableCell>
            <TableCell>{ref.chargeHeures} h</TableCell>
            <TableCell>{ref.premiereDate}</TableCell>
            <TableCell>{ref.derniereDate}</TableCell>
          </TableRow>
        ))}
        {!referencesQuery.data?.length && (
          <TableRow>
            <TableCell colSpan={6}>
              <Typography variant="body2" color="text.secondary">
                Aucune référence RUN rattachée à ce projet.
              </Typography>
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}
