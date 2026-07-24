import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery } from '@tanstack/react-query'
import { fetchBudgets } from '../../../../api/endpoints/budgets'
import { PermissionCodes } from '../../../../auth/permissionCodes'
import { PermissionGuard } from '../../../../auth/PermissionGuard'
import { FinancialValue } from '../../../../components/ui/FinancialValue'
import { StatusBadge } from '../../../../components/ui/StatusBadge'

export function BudgetTab({ projectId }: { projectId: string }) {
  const budgetsQuery = useQuery({
    queryKey: ['budgets', 'byProject', projectId],
    queryFn: () => fetchBudgets({ projectId }),
  })

  return (
    <PermissionGuard
      code={PermissionCodes.FinancialDataView}
      fallback={
        <Typography variant="body2" color="text.disabled">
          Donnée financière non accessible.
        </Typography>
      }
    >
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Nom</TableCell>
            <TableCell>Initial</TableCell>
            <TableCell>Ajusté</TableCell>
            <TableCell>Coût réel</TableCell>
            <TableCell>Différentiel</TableCell>
            <TableCell>Reste à consommer</TableCell>
            <TableCell>Atterrissage estimé</TableCell>
            <TableCell>Statut</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(budgetsQuery.data?.items ?? []).map((budget) => (
            <TableRow key={budget.id}>
              <TableCell>{budget.name}</TableCell>
              <TableCell>
                <FinancialValue value={budget.initialAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.adjustedAmount} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.coutReelConsomme} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.differentiel} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.montantRestant} />
              </TableCell>
              <TableCell>
                <FinancialValue value={budget.atterrissageEstime} />
              </TableCell>
              <TableCell>
                <StatusBadge
                  label={budget.status === 0 ? 'Actif' : 'Clôturé'}
                  tone={budget.risqueDepassement ? 'error' : 'success'}
                />
              </TableCell>
            </TableRow>
          ))}
          {!budgetsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={8}>
                <Typography variant="body2" color="text.secondary">
                  Aucun budget lié à ce projet.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </PermissionGuard>
  )
}
