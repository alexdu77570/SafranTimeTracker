import Box from '@mui/material/Box'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'

export interface WorkloadHeatmapEntry {
  resourceId: string
  nom: string
  weekStartDate: string
  chargeHeures: number
}

interface WorkloadHeatmapProps {
  entries: WorkloadHeatmapEntry[]
  emptyLabel?: string
}

/** Rampe séquentielle bleue validée (dataviz skill, `references/palette.md`, un seul hue,
 * clair→foncé) — jamais une couleur catégorielle pour une magnitude continue. */
const SEQUENTIAL_STEPS = [
  '#cde2fb',
  '#9ec5f4',
  '#6da7ec',
  '#3987e5',
  '#256abf',
  '#184f95',
  '#0d366b',
]

function colorForRatio(ratio: number): string {
  const index = Math.min(SEQUENTIAL_STEPS.length - 1, Math.floor(ratio * SEQUENTIAL_STEPS.length))
  return SEQUENTIAL_STEPS[index]
}

/** Heatmap de charge (§21.3, `WorkloadHeatmap` nommé par docs/ROADMAP.md, Lot 12) : une cellule par
 * ressource × semaine, intensité proportionnelle à la charge relative au maximum observé dans le
 * jeu de données affiché. Une semaine sans saisie pour une ressource reste une cellule vide (jamais
 * confondue avec une charge nulle) — composant agnostique du domaine, le consommateur fournit déjà
 * des heures déjà calculées (CLAUDE.md §5). */
export function WorkloadHeatmap({
  entries,
  emptyLabel = 'Aucune charge à afficher.',
}: WorkloadHeatmapProps) {
  if (entries.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  const weeks = [...new Set(entries.map((e) => e.weekStartDate))].sort()
  const resourceNames = new Map(entries.map((e) => [e.resourceId, e.nom]))
  const resourceIds = [...resourceNames.keys()].sort((a, b) =>
    (resourceNames.get(a) ?? '').localeCompare(resourceNames.get(b) ?? ''),
  )
  const byCell = new Map(
    entries.map((e) => [`${e.resourceId}__${e.weekStartDate}`, e.chargeHeures]),
  )
  const maxCharge = Math.max(...entries.map((e) => e.chargeHeures), 1)

  return (
    <Box sx={{ overflowX: 'auto' }}>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: `160px repeat(${weeks.length}, 32px)`,
          gap: '2px',
          width: 'fit-content',
        }}
      >
        <Box />
        {weeks.map((week) => (
          <Typography
            key={week}
            variant="caption"
            color="text.secondary"
            sx={{ textAlign: 'center', writingMode: 'vertical-rl' }}
          >
            {week}
          </Typography>
        ))}
        {resourceIds.map((resourceId) => (
          <Box key={resourceId} sx={{ display: 'contents' }}>
            <Typography variant="caption" sx={{ alignSelf: 'center', pr: 1 }}>
              {resourceNames.get(resourceId)}
            </Typography>
            {weeks.map((week) => {
              const charge = byCell.get(`${resourceId}__${week}`)
              return (
                <Tooltip
                  key={week}
                  title={
                    charge === undefined
                      ? 'Aucune saisie'
                      : `${resourceNames.get(resourceId)} — ${week} : ${charge} h`
                  }
                >
                  <Box
                    sx={{
                      height: 24,
                      bgcolor:
                        charge === undefined ? 'action.hover' : colorForRatio(charge / maxCharge),
                      borderRadius: 0.5,
                    }}
                  />
                </Tooltip>
              )
            })}
          </Box>
        ))}
      </Box>
    </Box>
  )
}
