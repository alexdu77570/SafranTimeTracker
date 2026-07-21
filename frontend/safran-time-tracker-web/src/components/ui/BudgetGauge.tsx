import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { ProgressBar } from './ProgressBar'

interface BudgetGaugeProps {
  label: string
  consumed: number
  total: number
  /** Risque de dépassement déjà calculé côté serveur (BudgetDto.risqueDepassement) — jamais
   * recalculé ici, seule la représentation visuelle en dépend (CLAUDE.md §5). */
  atRisk?: boolean
}

/** Jauge de consommation budgétaire (docs/ROADMAP.md, Lot 11), enveloppe de `ProgressBar` (Lot 7,
 * anticipée pour cet usage) avec la sémantique montant consommé/ajusté. Un ratio > 1 (dépassement)
 * reste affiché tel quel, la barre se sature visuellement à 100 %. */
export function BudgetGauge({ label, consumed, total, atRisk = false }: BudgetGaugeProps) {
  const ratio = total > 0 ? consumed / total : 0
  const tone = atRisk || ratio > 1 ? 'error' : ratio > 0.8 ? 'warning' : 'success'

  return (
    <Stack spacing={0.5} sx={{ width: '100%' }}>
      <ProgressBar value={ratio} label={label} tone={tone} />
      <Typography variant="caption" color="text.secondary">
        {consumed.toLocaleString('fr-FR')} € / {total.toLocaleString('fr-FR')} €
      </Typography>
    </Stack>
  )
}
