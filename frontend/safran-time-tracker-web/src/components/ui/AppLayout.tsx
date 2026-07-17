import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import { Outlet } from 'react-router-dom'
import { Breadcrumb } from './Breadcrumb'
import { Header } from './Header'
import { Sidebar } from './Sidebar'

/** Mise en page applicative (cahier des charges §8.1/§8.2) : Sidebar fixe + Header + fil
 * d'Ariane + contenu de route. Ne porte aucune logique métier — uniquement l'assemblage des
 * fondations transverses du Lot 7. */
export function AppLayout() {
  return (
    <Stack direction="row" sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <Sidebar />
      <Box component="main" sx={{ flexGrow: 1, minWidth: 0 }}>
        <Header />
        <Container maxWidth="xl" sx={{ py: 3 }}>
          <Box sx={{ mb: 2 }}>
            <Breadcrumb />
          </Box>
          <Outlet />
        </Container>
      </Box>
    </Stack>
  )
}
