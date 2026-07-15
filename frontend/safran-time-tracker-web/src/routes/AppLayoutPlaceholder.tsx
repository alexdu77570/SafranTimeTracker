/**
 * Page technique de vérification du Lot 0 : prouve que le routage, le build et le style de base
 * fonctionnent. Ce n'est pas un écran fonctionnel — aucune page métier n'est créée avant le Lot 1
 * (voir docs/ROADMAP.md). Elle sera remplacée par la mise en page réelle (Sidebar, Header, ...).
 */
export function AppLayoutPlaceholder() {
  return (
    <main style={{ fontFamily: 'Inter, "Segoe UI", sans-serif', padding: '2rem' }}>
      <h1>SAFRAN TIME TRACKER</h1>
      <p>Lot 0 — Fondations techniques. Aucune fonctionnalité métier n'est encore implémentée.</p>
    </main>
  )
}
