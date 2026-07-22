import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import type { ImportDiffDto } from '../../api/types'
import { ImportDiffType } from '../../api/types'
import { StatusBadge, type StatusTone } from './StatusBadge'

const diffTypeLabel: Record<ImportDiffType, string> = {
  [ImportDiffType.Ajout]: 'Ajout',
  [ImportDiffType.Modification]: 'Modification',
  [ImportDiffType.Suppression]: 'Suppression',
  [ImportDiffType.Inchange]: 'Inchangé',
  [ImportDiffType.Erreur]: 'Erreur',
}
const diffTypeTone: Record<ImportDiffType, StatusTone> = {
  [ImportDiffType.Ajout]: 'success',
  [ImportDiffType.Modification]: 'info',
  [ImportDiffType.Suppression]: 'error',
  [ImportDiffType.Inchange]: 'neutral',
  [ImportDiffType.Erreur]: 'error',
}

interface DiffViewerProps {
  diffs: ImportDiffDto[]
  emptyLabel?: string
}

/** `DiffViewer` (§27.4 "afficher les ajouts/modifications/suppressions/champs modifiés"), anticipé
 * par docs/ARCHITECTURE.md §3, construit au Lot 12 — agnostique du domaine (composant transverse,
 * même principe que StatusBadge/Timeline) : le consommateur fournit déjà les diffs calculés côté
 * serveur (ImportDiffDto, Lot 6), jamais recalculés ici. */
export function DiffViewer({ diffs, emptyLabel = 'Aucun écart.' }: DiffViewerProps) {
  if (diffs.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Entité</TableCell>
          <TableCell>Type</TableCell>
          <TableCell>Champ</TableCell>
          <TableCell>Ancienne valeur</TableCell>
          <TableCell>Nouvelle valeur</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {diffs.map((diff) => (
          <TableRow key={diff.id}>
            <TableCell>{diff.entityType}</TableCell>
            <TableCell>
              <StatusBadge label={diffTypeLabel[diff.diffType]} tone={diffTypeTone[diff.diffType]} />
            </TableCell>
            <TableCell>{diff.fieldName ?? '—'}</TableCell>
            <TableCell>{diff.oldValue ?? '—'}</TableCell>
            <TableCell>{diff.newValue ?? '—'}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
