import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { ReactNode } from 'react'

interface DetailPageHeaderProps {
  title: string
  subtitle?: string
  actions?: ReactNode
}

/** En-tête générique d'une fiche détail (titre + sous-titre + actions), factorisé à l'ouverture du
 * Lot 11 depuis ProjectDetailPage — premier consommateur d'un second écran à onglets
 * (OrderDetailPage). Aucune logique métier : le consommateur fournit titre/sous-titre/actions. */
export function DetailPageHeader({ title, subtitle, actions }: DetailPageHeaderProps) {
  return (
    <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
      <Stack>
        <Typography variant="h5">{title}</Typography>
        {subtitle && (
          <Typography variant="body2" color="text.secondary">
            {subtitle}
          </Typography>
        )}
      </Stack>
      {actions}
    </Stack>
  )
}
