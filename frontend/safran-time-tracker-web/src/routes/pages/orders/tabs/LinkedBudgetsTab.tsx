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

export function LinkedBudgetsTab({ orderId }: { orderId: string }) {
  const budgetsQuery = useQuery({
    queryKey: ['budgets', 'byOrder', orderId],
    queryFn: () => fetchBudgets({ orderId }),
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
            <TableCell>Différentiel</TableCell>
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
                <FinancialValue value={budget.differentiel} />
              </TableCell>
            </TableRow>
          ))}
          {!budgetsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={4}>
                <Typography variant="body2" color="text.secondary">
                  Aucun budget lié à cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </PermissionGuard>
  )
}
