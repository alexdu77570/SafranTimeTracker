import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardHeader from '@mui/material/CardHeader'
import type { ReactNode } from 'react'

interface ChartCardProps {
  title: string
  subheader?: string
  children: ReactNode
}

/** Enveloppe Card + titre pour un graphique (docs/ARCHITECTURE.md §3, anticipé depuis le Lot 7,
 * construit ici — premier lot portant plusieurs graphiques par écran, Charges/Tableau de bord). */
export function ChartCard({ title, subheader, children }: ChartCardProps) {
  return (
    <Card>
      <CardHeader title={title} subheader={subheader} />
      <CardContent>{children}</CardContent>
    </Card>
  )
}
