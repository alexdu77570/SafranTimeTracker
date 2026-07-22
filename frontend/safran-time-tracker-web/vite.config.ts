/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // En développement, le frontend appelle toujours une URL relative (/api/v1, cf. CLAUDE.md §4) ;
    // ce proxy évite tout problème CORS local en redirigeant vers l'API ASP.NET Core.
    // Port aligné sur backend/SafranTimeTracker.Api/Properties/launchSettings.json (profil "http"),
    // seule référence de port réellement maintenue par le projet (aucun script ni doc ne référence 5297).
    proxy: {
      '/api': {
        target: 'http://localhost:5215',
        changeOrigin: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: true,
    // Filet de sécurité (Lot 13, CI/CD) : le runner GitHub Actions partagé, avec l'instrumentation
    // de couverture active, s'est montré sensiblement plus lent que les machines de développement
    // sur des tests d'interaction utilisateur denses (plusieurs user.type()/user.click()
    // successifs) — plusieurs tests différents ont dépassé le défaut Vitest (5000ms) à des
    // exécutions CI distinctes. Les cas identifiés comme réellement lents ont été corrigés
    // individuellement (fireEvent.change à la place de user.type sur les champs texte non soumis
    // à une validation par frappe) ; ce timeout élargi reste une marge de sécurité pour le reste de
    // la suite, pas un substitut à ces corrections ciblées.
    testTimeout: 10000,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'cobertura'],
      // Seuils initiaux (Lot 13), fixés en dessous de la mesure réelle au moment de leur
      // introduction (statements 72,6 % / branches 71,6 % / functions 64,3 % / lines 72,2 %) pour
      // laisser une marge de bruit de mesure — objectif de les relever progressivement, jamais de
      // les baisser sans justification écrite ici.
      thresholds: {
        statements: 65,
        branches: 60,
        functions: 55,
        lines: 65,
      },
    },
  },
})
