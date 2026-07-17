import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { ReactNode } from 'react'
import type { LucideIcon } from 'lucide-react'
import { Inbox } from 'lucide-react'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description?: string
  action?: ReactNode
}

/** État vide explicite (cahier des charges §8.3). Utilisé aussi bien pour "aucune donnée" que
 * pour les écrans "à venir" (routage complet du Lot 7, sans écran métier). */
export function EmptyState({ icon: Icon = Inbox, title, description, action }: EmptyStateProps) {
  return (
    <Stack
      spacing={1.5}
      sx={{
        alignItems: 'center',
        justifyContent: 'center',
        py: 8,
        px: 2,
        textAlign: 'center',
        color: 'text.secondary',
      }}
    >
      <Icon size={40} strokeWidth={1.5} />
      <Typography variant="h6" color="text.primary">
        {title}
      </Typography>
      {description && <Typography variant="body2">{description}</Typography>}
      {action}
    </Stack>
  )
}
