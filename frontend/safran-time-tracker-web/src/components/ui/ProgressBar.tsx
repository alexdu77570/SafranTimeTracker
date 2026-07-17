import LinearProgress from '@mui/material/LinearProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

interface ProgressBarProps {
  /** Ratio entre 0 et 1 (ex. consommé / budget). Non borné en entrée : une valeur > 1 (dépassement)
   * reste affichée telle quelle par l'appelant (ex. BudgetGauge, Lot 11), ce composant se contente
   * de saturer la barre visuelle à 100 %. */
  value: number
  label?: string
  tone?: 'primary' | 'success' | 'warning' | 'error'
}

export function ProgressBar({ value, label, tone = 'primary' }: ProgressBarProps) {
  const percent = Math.round(value * 100)
  const clamped = Math.min(Math.max(percent, 0), 100)

  return (
    <Stack spacing={0.5} sx={{ width: '100%' }}>
      {label && (
        <Stack direction="row" sx={{ justifyContent: 'space-between' }}>
          <Typography variant="caption" color="text.secondary">
            {label}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {percent}%
          </Typography>
        </Stack>
      )}
      <LinearProgress variant="determinate" value={clamped} color={tone} />
    </Stack>
  )
}
