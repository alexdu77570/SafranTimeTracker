import MuiBreadcrumbs from '@mui/material/Breadcrumbs'
import Link from '@mui/material/Link'
import Typography from '@mui/material/Typography'
import { Link as RouterLink, useMatches } from 'react-router-dom'

interface RouteHandle {
  crumb?: string
}

/** Fil d'Ariane (cahier des charges §8.3), construit à partir de `route.handle.crumb` (React
 * Router `useMatches`) — chaque route fournit son propre libellé plutôt qu'un mapping centralisé
 * ici, pour ne pas dupliquer la déclaration des routes. */
export function Breadcrumb() {
  const matches = useMatches()
  const crumbs = matches
    .filter((match) => Boolean((match.handle as RouteHandle | undefined)?.crumb))
    .map((match) => ({
      pathname: match.pathname,
      title: (match.handle as RouteHandle).crumb as string,
    }))

  if (crumbs.length <= 1) {
    return null
  }

  return (
    <MuiBreadcrumbs aria-label="fil d'Ariane">
      {crumbs.map((crumb, index) =>
        index === crumbs.length - 1 ? (
          <Typography key={crumb.pathname} color="text.primary" variant="body2">
            {crumb.title}
          </Typography>
        ) : (
          <Link
            key={crumb.pathname}
            component={RouterLink}
            to={crumb.pathname}
            underline="hover"
            color="inherit"
            variant="body2"
          >
            {crumb.title}
          </Link>
        ),
      )}
    </MuiBreadcrumbs>
  )
}
