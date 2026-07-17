import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import { RotateCcw } from 'lucide-react'
import type { ReactNode } from 'react'

interface FilterBarProps {
  children: ReactNode
  onReset?: () => void
  resetDisabled?: boolean
}

/** Conteneur transverse des filtres d'un écran de liste (cahier des charges §8.3 : "filtres
 * visibles et réinitialisables"). Ne porte aucun état : chaque écran gère ses propres filtres et
 * fournit ses champs comme enfants. */
export function FilterBar({ children, onReset, resetDisabled }: FilterBarProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" spacing={2} useFlexGap sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
        {children}
        {onReset && (
          <Button
            startIcon={<RotateCcw size={16} />}
            onClick={onReset}
            disabled={resetDisabled}
            size="small"
          >
            Réinitialiser
          </Button>
        )}
      </Stack>
    </Paper>
  )
}
