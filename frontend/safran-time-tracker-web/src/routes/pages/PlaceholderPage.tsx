import { EmptyState } from '../../components/ui/EmptyState'

interface PlaceholderPageProps {
  title: string
}

/** Écran "à venir" (Lot 7 : routage complet sans écran métier — voir docs/ROADMAP.md). Remplacé
 * lot par lot par l'écran fonctionnel réel, sans changer le chemin de route. */
export function PlaceholderPage({ title }: PlaceholderPageProps) {
  return <EmptyState title={title} description="Cet écran sera développé dans un lot ultérieur." />
}
