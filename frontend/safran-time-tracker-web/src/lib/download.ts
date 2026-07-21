/** §26.3 : premier besoin de téléchargement de fichier du frontend (Lot 12) — déclenche un
 * téléchargement navigateur à partir d'un `Blob` déjà reçu (jamais un lien direct vers l'API, pour
 * rester compatible avec l'intercepteur d'authentification de démonstration sur `apiClient`). */
export function triggerBrowserDownload(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/** Le serveur nomme déjà le fichier via l'en-tête `Content-Disposition` (ASP.NET Core `File(...)`,
 * §26.3) — repli uniquement si l'en-tête est absent ou non exposé. */
export function extractFileName(contentDisposition: string | undefined, fallback: string): string {
  if (!contentDisposition) {
    return fallback
  }
  const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(contentDisposition)
  return match ? decodeURIComponent(match[1]) : fallback
}
