import Box from '@mui/material/Box'
import Tab from '@mui/material/Tab'
import Tabs from '@mui/material/Tabs'
import type { SyntheticEvent } from 'react'

interface DetailTabsProps {
  labels: string[]
  value: number
  onChange: (value: number) => void
}

/** Barre d'onglets générique d'une fiche détail, factorisée à l'ouverture du Lot 11 depuis
 * ProjectDetailPage. Ne rend que la barre : le contenu de chaque onglet reste à la charge du
 * consommateur (les onglets diffèrent entièrement d'un écran à l'autre). */
export function DetailTabs({ labels, value, onChange }: DetailTabsProps) {
  const handleChange = (_: SyntheticEvent, newValue: number) => onChange(newValue)

  return (
    <Box>
      <Tabs
        value={value}
        onChange={handleChange}
        variant="scrollable"
        scrollButtons="auto"
        sx={{ mb: 2 }}
      >
        {labels.map((label) => (
          <Tab key={label} label={label} />
        ))}
      </Tabs>
    </Box>
  )
}
