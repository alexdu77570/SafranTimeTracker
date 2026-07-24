import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import {
  fetchOrderReceipts,
  fetchOrderReceiptSummary,
} from '../../../../api/endpoints/orderReceipts'
import { FinancialValue } from '../../../../components/ui/FinancialValue'
import { KpiBand } from '../../../../components/ui/KpiBand'
import { Modal } from '../../../../components/ui/Modal'
import { OrderReceiptCreateForm } from '../OrderReceiptForm'

export function ReceiptsTab({ orderId }: { orderId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const receiptsQuery = useQuery({
    queryKey: ['order-receipts', orderId],
    queryFn: () => fetchOrderReceipts(orderId),
  })
  const summaryQuery = useQuery({
    queryKey: ['order-receipts-summary', orderId],
    queryFn: () => fetchOrderReceiptSummary(orderId),
  })

  return (
    <Stack spacing={2}>
      {summaryQuery.data && (
        <KpiBand
          items={[
            { label: 'Total réceptionné (€)', value: `${summaryQuery.data.totalReceivedAmount} €` },
            {
              label: 'Total réceptionné (jours)',
              value: String(summaryQuery.data.totalReceivedDays),
            },
            {
              label: 'Reste réceptionnable (€)',
              value: `${summaryQuery.data.remainingReceivableAmount} €`,
            },
            {
              label: 'Reste réceptionnable (jours)',
              value:
                summaryQuery.data.remainingReceivableDays !== null
                  ? String(summaryQuery.data.remainingReceivableDays)
                  : '—',
            },
          ]}
        />
      )}
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer une réception"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Montant reçu</TableCell>
            <TableCell>Jours reçus</TableCell>
            <TableCell>Motif</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(receiptsQuery.data?.items ?? []).map((receipt) => (
            <TableRow key={receipt.id}>
              <TableCell>{receipt.receiptDate}</TableCell>
              <TableCell>
                {receipt.receivedAmount !== null ? (
                  <FinancialValue value={receipt.receivedAmount} />
                ) : (
                  '—'
                )}
              </TableCell>
              <TableCell>{receipt.receivedDays ?? '—'}</TableCell>
              <TableCell>{receipt.reason ?? '—'}</TableCell>
            </TableRow>
          ))}
          {!receiptsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={4}>
                <Typography variant="body2" color="text.secondary">
                  Aucune réception sur cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Nouvelle réception" onClose={() => setCreateOpen(false)}>
        <OrderReceiptCreateForm
          orderId={orderId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['order-receipts', orderId] })
            void queryClient.invalidateQueries({ queryKey: ['order-receipts-summary', orderId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}
