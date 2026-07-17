import { createTheme } from '@mui/material/styles'
import { frFR as coreFrFR } from '@mui/material/locale'
import { frFR as gridFrFR } from '@mui/x-data-grid/locales'
import { frFR as pickersFrFR } from '@mui/x-date-pickers/locales'

/**
 * Palette et typographie de l'identité visuelle (cahier des charges §8.1) : fond gris clair,
 * sidebar bleu marine, cartes blanches, couleur d'action bleu Safran, typographie Inter/Segoe UI,
 * sobre et corporate — inspiré ServiceNow/Azure DevOps/Jira/Power BI/Grafana, sans effet gadget.
 * Ces valeurs sont les seuls tokens de couleur/typo de l'application : tout composant réutilise
 * `theme.palette.*` plutôt que de coder une couleur en dur (CLAUDE.md §5).
 */
const safranBlue = '#0055B8'
const navySidebar = '#0B1F3D'

export const theme = createTheme(
  {
    palette: {
      mode: 'light',
      primary: {
        main: safranBlue,
      },
      secondary: {
        main: navySidebar,
      },
      background: {
        default: '#F4F5F7',
        paper: '#FFFFFF',
      },
      success: { main: '#2E7D32' },
      warning: { main: '#ED6C02' },
      error: { main: '#C62828' },
      info: { main: '#0277BD' },
    },
    shape: {
      borderRadius: 6,
    },
    typography: {
      fontFamily: '"Inter Variable", "Inter", "Segoe UI", Roboto, Arial, sans-serif',
      button: {
        textTransform: 'none',
        fontWeight: 600,
      },
    },
    components: {
      MuiButton: {
        defaultProps: { disableElevation: true },
      },
      MuiPaper: {
        defaultProps: { elevation: 0 },
        styleOverrides: {
          root: {
            backgroundImage: 'none',
          },
        },
      },
      MuiCard: {
        defaultProps: { elevation: 0 },
        styleOverrides: {
          root: {
            border: '1px solid #E3E6EA',
          },
        },
      },
      MuiAppBar: {
        defaultProps: { elevation: 0 },
      },
    },
  },
  coreFrFR,
  gridFrFR,
  pickersFrFR,
)

export const sidebarNavyColor = navySidebar
