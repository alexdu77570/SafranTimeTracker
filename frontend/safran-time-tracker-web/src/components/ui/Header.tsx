import AppBar from '@mui/material/AppBar'
import Avatar from '@mui/material/Avatar'
import MenuItem from '@mui/material/MenuItem'
import Select from '@mui/material/Select'
import Stack from '@mui/material/Stack'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { UserCircle } from 'lucide-react'
import { useCurrentUser } from '../../auth/useCurrentUser'

/**
 * Sélecteur d'identité de démonstration (CLAUDE.md §17) : pilote l'en-tête X-Demo-User existant.
 * Aucun mot de passe, aucune session — changer l'identité ici change immédiatement les données et
 * permissions renvoyées par l'API, dès la requête suivante.
 */
export function Header() {
  const { identifiant, setIdentifiant, availableUsers, user } = useCurrentUser()

  return (
    <AppBar
      position="sticky"
      color="inherit"
      sx={{ backgroundColor: 'background.paper', borderBottom: '1px solid #E3E6EA' }}
    >
      <Toolbar sx={{ justifyContent: 'flex-end', gap: 2 }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main' }}>
            <UserCircle size={20} />
          </Avatar>
          <Select
            value={identifiant ?? ''}
            onChange={(event) => setIdentifiant(event.target.value || null)}
            displayEmpty
            size="small"
            sx={{ minWidth: 240 }}
            renderValue={() =>
              user ? (
                <Typography variant="body2">
                  {user.prenom} {user.nom}
                </Typography>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  Choisir une identité de démonstration
                </Typography>
              )
            }
          >
            {availableUsers.map((candidate) => (
              <MenuItem key={candidate.id} value={candidate.identifiant}>
                {candidate.prenom} {candidate.nom} ({candidate.identifiant})
              </MenuItem>
            ))}
          </Select>
        </Stack>
      </Toolbar>
    </AppBar>
  )
}
