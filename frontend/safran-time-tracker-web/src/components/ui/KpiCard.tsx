import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { LucideIcon } from 'lucide-react'

interface KpiCardProps {
  label: string
  value: string
  icon?: LucideIcon
  trend?: string
  trendTone?: 'positive' | 'negative' | 'neutral'
}

const trendColor = {
  positive: 'success.main',
  negative: 'error.main',
  neutral: 'text.secondary',
} as const

/** Carte KPI transverse (tableau de bord, synthèses de projet, ...). Aucun calcul métier ici : le
 * consommateur fournit une valeur déjà formatée. */
export function KpiCard({ label, value, icon: Icon, trend, trendTone = 'neutral' }: KpiCardProps) {
  return (
    <Card>
      <CardContent>
        <Stack direction="row" sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Stack spacing={0.5}>
            <Typography variant="body2" color="text.secondary">
              {label}
            </Typography>
            <Typography variant="h4">{value}</Typography>
            {trend && (
              <Typography variant="caption" sx={{ color: trendColor[trendTone] }}>
                {trend}
              </Typography>
            )}
          </Stack>
          {Icon && <Icon size={22} strokeWidth={1.75} />}
        </Stack>
      </CardContent>
    </Card>
  )
}
