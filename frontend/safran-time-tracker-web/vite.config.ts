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
  },
})
