import Typography from '@mui/material/Typography'
import Tooltip from '@mui/material/Tooltip'

const euroFormatter = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'EUR' })

interface FinancialValueProps {
  /** Absent (undefined/null) quand le serveur a omis le champ faute de permission
   * FINANCIAL_DATA_VIEW (CLAUDE.md §13) — jamais un masquage visuel côté client. */
  value: number | null | undefined
}

/**
 * N'effectue aucune vérification de permission : elle se contente de refléter la présence ou
 * l'absence du champ dans la réponse API (ARCHITECTURE.md §3). Une donnée financière non
 * autorisée n'atteint jamais ce composant, elle n'existe simplement pas dans le DTO reçu.
 */
export function FinancialValue({ value }: FinancialValueProps) {
  if (value === null || value === undefined) {
    return (
      <Tooltip title="Donnée financière non accessible">
        <Typography component="span" color="text.disabled">
          —
        </Typography>
      </Tooltip>
    )
  }

  return <Typography component="span">{euroFormatter.format(value)}</Typography>
}
