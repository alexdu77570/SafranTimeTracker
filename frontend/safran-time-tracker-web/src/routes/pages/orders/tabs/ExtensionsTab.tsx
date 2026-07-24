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
import { fetchOrderExtensions } from '../../../../api/endpoints/orderExtensions'
import { FinancialValue } from '../../../../components/ui/FinancialValue'
import { Modal } from '../../../../components/ui/Modal'
import { OrderExtensionCreateForm } from '../OrderExtensionForm'

export function ExtensionsTab({ orderId }: { orderId: string }) {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const extensionsQuery = useQuery({
    queryKey: ['order-extensions', orderId],
    queryFn: () => fetchOrderExtensions(orderId),
  })

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <IconButton
          color="primary"
          onClick={() => setCreateOpen(true)}
          aria-label="Créer une rallonge"
        >
          <Plus size={20} />
        </IconButton>
      </Stack>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Montant ajouté</TableCell>
            <TableCell>Jours ajoutés</TableCell>
            <TableCell>Nouvelle date de fin</TableCell>
            <TableCell>Motif</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(extensionsQuery.data?.items ?? []).map((extension) => (
            <TableRow key={extension.id}>
              <TableCell>{extension.extensionDate}</TableCell>
              <TableCell>
                <FinancialValue value={extension.amountAdded} />
              </TableCell>
              <TableCell>{extension.daysAdded ?? '—'}</TableCell>
              <TableCell>{extension.newEndDate}</TableCell>
              <TableCell>{extension.reason}</TableCell>
            </TableRow>
          ))}
          {!extensionsQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={5}>
                <Typography variant="body2" color="text.secondary">
                  Aucune rallonge sur cette commande.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal open={createOpen} title="Nouvelle rallonge" onClose={() => setCreateOpen(false)}>
        <OrderExtensionCreateForm
          orderId={orderId}
          onSuccess={() => {
            setCreateOpen(false)
            void queryClient.invalidateQueries({ queryKey: ['order-extensions', orderId] })
            void queryClient.invalidateQueries({ queryKey: ['orders', orderId] })
          }}
          onCancel={() => setCreateOpen(false)}
        />
      </Modal>
    </Stack>
  )
}
