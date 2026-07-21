import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import TextField from '@mui/material/TextField'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'

export interface WeeklyPlanningCell {
  initial?: number
  ajuste?: number
  realise?: number
  surcharge?: boolean
}

export interface WeeklyPlanningGridRow {
  id: string
  label: string
  weeks: Record<string, WeeklyPlanningCell>
}

interface WeeklyPlanningGridProps {
  weekStartDates: string[]
  rows: WeeklyPlanningGridRow[]
  /** Édition de la charge Ajustée d'une cellule (semaine, ligne) — omis pour un rendu lecture seule. */
  onAjusteChange?: (rowId: string, weekStartDate: string, value: number) => void
  emptyLabel?: string
}

/**
 * Grille hebdomadaire par ressource (cahier des charges §17.3, §18.2 : "les charges peuvent varier
 * chaque semaine"). Composant transverse de présentation uniquement (cahier des charges §32.2) :
 * ne calcule aucun écart ni aucune agrégation — le consommateur fournit des cellules déjà
 * calculées (ProjectPlanningService, jamais recalculé côté frontend, CLAUDE.md §5). Premiers
 * consommateurs : onglet Planning de la fiche projet (Lot 10, édition de la charge Ajustée) et la
 * vue transverse "Planning projet" (Lot 10, lecture seule).
 */
export function WeeklyPlanningGrid({
  weekStartDates,
  rows,
  onAjusteChange,
  emptyLabel = 'Aucune donnée de planning.',
}: WeeklyPlanningGridProps) {
  if (weekStartDates.length === 0 || rows.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyLabel}
      </Typography>
    )
  }

  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell sx={{ fontWeight: 600 }}>Ressource</TableCell>
            {weekStartDates.map((week) => (
              <TableCell key={week} align="center" sx={{ fontWeight: 600 }}>
                {week}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.id} hover>
              <TableCell>{row.label}</TableCell>
              {weekStartDates.map((week) => {
                const cell = row.weeks[week]
                return (
                  <TableCell
                    key={week}
                    align="center"
                    sx={{ bgcolor: cell?.surcharge ? 'error.50' : undefined }}
                  >
                    <Stack spacing={0.25} sx={{ alignItems: 'center' }}>
                      <Typography variant="caption" color="text.secondary">
                        Initial : {cell?.initial ?? '—'} h
                      </Typography>
                      {onAjusteChange ? (
                        <TextField
                          size="small"
                          type="number"
                          value={cell?.ajuste ?? ''}
                          onChange={(e) => onAjusteChange(row.id, week, Number(e.target.value))}
                          sx={{ width: 80 }}
                          slotProps={{
                            htmlInput: {
                              min: 0,
                              step: 0.25,
                              'aria-label': `Ajusté ${row.label} ${week}`,
                            },
                          }}
                        />
                      ) : (
                        <Typography variant="body2">Ajusté : {cell?.ajuste ?? '—'} h</Typography>
                      )}
                      <Typography
                        variant="caption"
                        color={cell?.surcharge ? 'error.main' : 'text.secondary'}
                      >
                        Réalisé : {cell?.realise ?? '—'} h
                      </Typography>
                    </Stack>
                  </TableCell>
                )
              })}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
