import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchOrderById } from '../../../../api/endpoints/orders'
import { PermissionCodes } from '../../../../auth/permissionCodes'
import { PermissionGuard } from '../../../../auth/PermissionGuard'
import { FinancialValue } from '../../../../components/ui/FinancialValue'

export function SynthesisTab({
  order,
}: {
  order: NonNullable<
    ReturnType<typeof useQuery<Awaited<ReturnType<typeof fetchOrderById>>>>['data']
  >
}) {
  return (
    <Card>
      <CardHeader title="Informations générales" />
      <CardContent>
        <Table size="small">
          <TableBody>
            <TableRow>
              <TableCell>Date de début</TableCell>
              <TableCell>{order.dateDebut}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Date de fin initiale</TableCell>
              <TableCell>{order.dateFinInitiale}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Date de fin ajustée</TableCell>
              <TableCell>{order.dateFinAjustee ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Budget en jours initial</TableCell>
              <TableCell>{order.budgetJoursInitial ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Budget en jours ajusté</TableCell>
              <TableCell>{order.budgetJoursAjuste ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Seuil d'alerte</TableCell>
              <TableCell>{order.seuilAlerte ?? '—'}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Commentaire</TableCell>
              <TableCell>{order.commentaire ?? '—'}</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
      <CardHeader title="Synthèse financière (§13.2)" />
      <CardContent>
        <PermissionGuard
          code={PermissionCodes.FinancialDataView}
          fallback={
            <Typography variant="body2" color="text.disabled">
              Donnée financière non accessible.
            </Typography>
          }
        >
          {order.financialSummary ? (
            <Table size="small">
              <TableBody>
                <TableRow>
                  <TableCell>Consommation en jours</TableCell>
                  <TableCell>{order.financialSummary.consommationJours} j</TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Coût réel consommé</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.coutReelConsomme} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Coût contractuel consommé</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.coutContractuelConsomme} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Différentiel</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.differentiel} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Reste financier</TableCell>
                  <TableCell>
                    <FinancialValue value={order.financialSummary.restFinancier} />
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell>Reste en jours</TableCell>
                  <TableCell>{order.financialSummary.restJours ?? '—'}</TableCell>
                </TableRow>
              </TableBody>
            </Table>
          ) : (
            <Typography variant="body2" color="text.secondary">
              Aucune donnée financière.
            </Typography>
          )}
        </PermissionGuard>
      </CardContent>
    </Card>
  )
}
