import Drawer from '@mui/material/Drawer'
import List from '@mui/material/List'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { NavLink } from 'react-router-dom'
import { sidebarNavyColor } from '../../theme/theme'
import { useCurrentUser } from '../../auth/useCurrentUser'
import { navigation } from '../../routes/navigation'

export const SIDEBAR_WIDTH = 260

/** Sidebar bleu marine (cahier des charges §8.1), filtrée par droits (§8.2) : une entrée dont
 * `requiredPermission` n'est pas détenue par l'identité de démonstration courante disparaît
 * entièrement plutôt que d'être affichée désactivée. */
export function Sidebar() {
  const { hasPermission } = useCurrentUser()
  const entries = navigation.filter(
    (entry) => !entry.requiredPermission || hasPermission(entry.requiredPermission),
  )

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: SIDEBAR_WIDTH,
        flexShrink: 0,
        [`& .MuiDrawer-paper`]: {
          width: SIDEBAR_WIDTH,
          boxSizing: 'border-box',
          backgroundColor: sidebarNavyColor,
          color: '#E5E9F0',
          borderRight: 'none',
        },
      }}
    >
      <Toolbar>
        <Typography variant="subtitle1" noWrap sx={{ fontWeight: 700, color: '#FFFFFF' }}>
          SAFRAN TIME TRACKER
        </Typography>
      </Toolbar>
      <List sx={{ px: 1 }}>
        {entries.map((entry) => {
          const Icon = entry.icon
          return (
            <ListItemButton
              key={entry.path}
              component={NavLink}
              to={entry.path}
              end={entry.path === '/'}
              sx={{
                borderRadius: 1,
                mb: 0.25,
                color: 'inherit',
                '&.active': {
                  backgroundColor: 'rgba(255, 255, 255, 0.12)',
                },
                '&:hover': {
                  backgroundColor: 'rgba(255, 255, 255, 0.08)',
                },
              }}
            >
              <ListItemIcon sx={{ color: 'inherit', minWidth: 36 }}>
                <Icon size={20} strokeWidth={1.75} />
              </ListItemIcon>
              <ListItemText slotProps={{ primary: { sx: { fontSize: 14 } } }}>
                {entry.label}
              </ListItemText>
            </ListItemButton>
          )
        })}
      </List>
    </Drawer>
  )
}
