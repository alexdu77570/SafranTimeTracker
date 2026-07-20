import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemText from '@mui/material/ListItemText'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

/** AbsenceType est un enum C# (docs/IMPLEMENTATION_STATUS.md, Lot 3), pas une entité : contrairement
 * à MilestoneType, il n'existe aucune administrabilité prévue par le cahier des charges (§23) — cet
 * onglet reste donc volontairement une liste de consultation, jamais un formulaire de création qui
 * appellerait un endpoint inexistant (CLAUDE.md §5, §12). */
const ABSENCE_TYPES = ['Congé', 'RTT', 'Maladie', 'Formation', 'Déplacement', 'Indisponible']

export function AbsenceTypesTab() {
  return (
    <Stack spacing={2}>
      <Typography variant="h6">Types d'absences</Typography>
      <Typography variant="body2" color="text.secondary">
        Liste fixe, non administrable (cahier des charges §23) — à la différence des types d'activité
        et de jalon, les types d'absence ne sont pas un référentiel modifiable.
      </Typography>
      <List dense>
        {ABSENCE_TYPES.map((type) => (
          <ListItem key={type} divider>
            <ListItemText primary={type} />
          </ListItem>
        ))}
      </List>
    </Stack>
  )
}
