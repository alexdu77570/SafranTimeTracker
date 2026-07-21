import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import { StatusBadge, type StatusTone } from './StatusBadge'

export interface TimelineItem {
  id: string
  date: string
  label: string
  sublabel?: string
  statusLabel: string
  statusTone: StatusTone
  highlighted?: boolean
}

interface TimelineProps {
  items: TimelineItem[]
  emptyLabel?: string
}

/**
 * Timeline graphique simple (axe chronologique, un point par élément) — pas un Gantt : aucune
 * dépendance visuelle entre éléments, aucun glisser-déposer, aucune barre de durée. Composant
 * agnostique du domaine (cahier des charges §32.2) : le consommateur fournit déjà le libellé de
 * statut et sa tonalité (même principe que StatusBadge, CLAUDE.md §5) — jamais un enum métier
 * (MilestoneStatus, ...) codé en dur ici. Premier consommateur : onglet Jalons de la fiche projet
 * (§17.6, Lot 10).
 */
export function Timeline({ items, emptyLabel = 'Aucun élément à afficher.' }: TimelineProps) {
  if (items.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  const sorted = [...items].sort((a, b) => a.date.localeCompare(b.date))

  return (
    <Stack sx={{ position: 'relative', pl: 3 }}>
      <Box
        sx={{
          position: 'absolute',
          left: 7,
          top: 8,
          bottom: 8,
          width: 2,
          bgcolor: 'divider',
        }}
      />
      <Stack spacing={2.5}>
        {sorted.map((item) => (
          <Stack key={item.id} direction="row" spacing={2} sx={{ position: 'relative' }}>
            <Box
              sx={{
                position: 'absolute',
                left: -24,
                top: 4,
                width: 12,
                height: 12,
                borderRadius: '50%',
                bgcolor: item.highlighted ? 'error.main' : 'primary.main',
                border: '2px solid',
                borderColor: 'background.paper',
              }}
            />
            <Stack spacing={0.25} sx={{ flex: 1, minWidth: 0 }}>
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {item.label}
                </Typography>
                <StatusBadge label={item.statusLabel} tone={item.statusTone} />
              </Stack>
              <Typography variant="caption" color="text.secondary">
                {item.date}
                {item.sublabel ? ` — ${item.sublabel}` : ''}
              </Typography>
            </Stack>
          </Stack>
        ))}
      </Stack>
    </Stack>
  )
}
