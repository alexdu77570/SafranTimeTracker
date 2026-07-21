import Grid from '@mui/material/Grid'
import type { ReactNode } from 'react'
import { KpiCard } from './KpiCard'

interface KpiBandItem {
  label: string
  value: string
}

interface KpiBandProps {
  items: KpiBandItem[]
  /** Emplacement additionnel (ex. bloc d'alertes), déjà enveloppé dans un `Grid item` par
   * l'appelant pour rester libre de sa propre mise en forme. */
  children?: ReactNode
}

/** Bandeau de KPI générique en tête de fiche détail, factorisé à l'ouverture du Lot 11 depuis
 * ProjectDetailPage (même grille `xs=6 sm=4 md=2` que le bandeau Synthèse d'origine). */
export function KpiBand({ items, children }: KpiBandProps) {
  return (
    <Grid container spacing={2}>
      {items.map((item) => (
        <Grid key={item.label} size={{ xs: 6, sm: 4, md: 2 }}>
          <KpiCard label={item.label} value={item.value} />
        </Grid>
      ))}
      {children}
    </Grid>
  )
}
