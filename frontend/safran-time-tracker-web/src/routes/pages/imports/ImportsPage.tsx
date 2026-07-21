import MenuItem from '@mui/material/MenuItem'
import Stack from '@mui/material/Stack'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import {
  fetchImportBatches,
  fetchImportDiffs,
  fetchImportTypes,
} from '../../../api/endpoints/imports'
import type { ImportBatchDto, ImportEntityType } from '../../../api/types'
import { ImportBatchStatus } from '../../../api/types'
import { DiffViewer } from '../../../components/ui/DiffViewer'
import { ImportWizard } from '../../../components/ui/ImportWizard'
import { Modal } from '../../../components/ui/Modal'
import { StatusBadge, type StatusTone } from '../../../components/ui/StatusBadge'

const statusLabel: Record<ImportBatchStatus, string> = {
  [ImportBatchStatus.Previsualise]: 'Prévisualisé',
  [ImportBatchStatus.Simule]: 'Simulé',
  [ImportBatchStatus.Confirme]: 'Confirmé',
  [ImportBatchStatus.Echoue]: 'Échoué',
}
const statusTone: Record<ImportBatchStatus, StatusTone> = {
  [ImportBatchStatus.Previsualise]: 'neutral',
  [ImportBatchStatus.Simule]: 'info',
  [ImportBatchStatus.Confirme]: 'success',
  [ImportBatchStatus.Echoue]: 'error',
}

/** Écran Imports (§27), sous PermissionGuard IMPORT_EXECUTE (garde serveur, Lot 6/7) : assistant
 * (`ImportWizard`) + historique des lots + `DiffViewer` pour un lot passé — moteur d'import
 * entièrement backend (Lot 6), aucune nouvelle logique ici. */
export function ImportsPage() {
  const queryClient = useQueryClient()
  const [typeFilter, setTypeFilter] = useState('')
  const [selectedBatch, setSelectedBatch] = useState<ImportBatchDto | null>(null)

  const typesQuery = useQuery({ queryKey: ['import-types'], queryFn: () => fetchImportTypes() })
  const batchesQuery = useQuery({
    queryKey: ['import-batches', typeFilter],
    queryFn: () =>
      fetchImportBatches({
        type: typeFilter === '' ? undefined : (Number(typeFilter) as ImportEntityType),
        pageSize: 20,
      }),
  })
  const diffsQuery = useQuery({
    queryKey: ['import-diffs', selectedBatch?.id],
    queryFn: () => fetchImportDiffs(selectedBatch?.id ?? ''),
    enabled: Boolean(selectedBatch),
  })

  const typeLabel = new Map(
    (typesQuery.data ?? []).map((t) => [t.type, t.expectedHeaders.join(', ')]),
  )

  return (
    <Stack spacing={2}>
      <Typography variant="h5">Imports</Typography>

      <ImportWizard
        onCompleted={() => void queryClient.invalidateQueries({ queryKey: ['import-batches'] })}
      />

      <Typography variant="h6">Historique des lots</Typography>
      <TextField
        select
        size="small"
        label="Type"
        value={typeFilter}
        onChange={(e) => setTypeFilter(e.target.value)}
        sx={{ maxWidth: 250 }}
      >
        <MenuItem value="">(tous)</MenuItem>
        {(typesQuery.data ?? []).map((t) => (
          <MenuItem key={t.type} value={String(t.type)}>
            {t.expectedHeaders.join(', ')}
          </MenuItem>
        ))}
      </TextField>

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Type</TableCell>
            <TableCell>Source</TableCell>
            <TableCell>Fichier</TableCell>
            <TableCell>Statut</TableCell>
            <TableCell>Ajouts</TableCell>
            <TableCell>Mises à jour</TableCell>
            <TableCell>Erreurs</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(batchesQuery.data?.items ?? []).map((batch) => (
            <TableRow
              key={batch.id}
              hover
              onClick={() => setSelectedBatch(batch)}
              sx={{ cursor: 'pointer' }}
            >
              <TableCell>{batch.importDate}</TableCell>
              <TableCell>{typeLabel.get(batch.type) ?? batch.type}</TableCell>
              <TableCell>{batch.source}</TableCell>
              <TableCell>{batch.fileName}</TableCell>
              <TableCell>
                <StatusBadge label={statusLabel[batch.status]} tone={statusTone[batch.status]} />
              </TableCell>
              <TableCell>{batch.addCount}</TableCell>
              <TableCell>{batch.updateCount}</TableCell>
              <TableCell>{batch.errorCount}</TableCell>
            </TableRow>
          ))}
          {!batchesQuery.data?.items.length && (
            <TableRow>
              <TableCell colSpan={8}>
                <Typography variant="body2" color="text.secondary">
                  Aucun lot d'import.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Modal
        open={Boolean(selectedBatch)}
        title={`Diffs — ${selectedBatch?.fileName ?? ''}`}
        onClose={() => setSelectedBatch(null)}
        maxWidth="md"
      >
        <DiffViewer diffs={diffsQuery.data?.items ?? []} />
      </Modal>
    </Stack>
  )
}
