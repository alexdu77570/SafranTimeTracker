import Chip from '@mui/material/Chip'

export type StatusTone = 'neutral' | 'success' | 'warning' | 'error' | 'info'

interface StatusBadgeProps {
  label: string
  tone?: StatusTone
}

const toneColor: Record<StatusTone, 'default' | 'success' | 'warning' | 'error' | 'info'> = {
  neutral: 'default',
  success: 'success',
  warning: 'warning',
  error: 'error',
  info: 'info',
}

/**
 * Badge de statut générique (cahier des charges §8.1 : "badges de statut cohérents"). Ne connaît
 * aucun enum métier (OrderStatus, TimeEntryStatus, ...) : chaque écran fournit son propre mapping
 * statut → { label, tone }, pour ne pas dupliquer les règles métier dans un composant transverse
 * (CLAUDE.md §5).
 */
export function StatusBadge({ label, tone = 'neutral' }: StatusBadgeProps) {
  return (
    <Chip
      label={label}
      color={toneColor[tone]}
      size="small"
      variant={tone === 'neutral' ? 'outlined' : 'filled'}
    />
  )
}
