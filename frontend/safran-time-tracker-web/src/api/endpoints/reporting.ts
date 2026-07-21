import { apiClient } from '../client'
import { extractFileName, triggerBrowserDownload } from '../../lib/download'
import type {
  ChargesReportDto,
  DashboardDto,
  ExportFormat,
  FinancialReportDto,
  OperationalReportDto,
  ProjectLinkedReferenceDto,
  ReportingFilterQuery,
} from '../types'

/** Réutilisé pour le détail statistique d'une application (docs/ROADMAP.md, Lot 8) : mêmes
 * agrégations que l'écran Charges (§21, Lot 5/12), filtrées par applicationId — aucune nouvelle
 * agrégation backend, seule la réutilisation d'un endpoint déjà livré. */
export async function fetchCharges(filter: ReportingFilterQuery): Promise<ChargesReportDto> {
  const response = await apiClient.get<ChargesReportDto>('/reporting/charges', { params: filter })
  return response.data
}

/** §25 : KPI opérationnels toujours renvoyés, financiers gardés par permission (champ omis, jamais
 * masqué côté client). Réutilisé pour "budget total initial/ajusté" sur la page Budgets (Lot 11). */
export async function fetchDashboard(filter?: ReportingFilterQuery): Promise<DashboardDto> {
  const response = await apiClient.get<DashboardDto>('/reporting/dashboard', { params: filter })
  return response.data
}

/** §26.1 : aucune donnée financière, accessible sans permission dédiée. */
export async function fetchOperationalReport(filter?: ReportingFilterQuery): Promise<OperationalReportDto> {
  const response = await apiClient.get<OperationalReportDto>('/reporting/operational', { params: filter })
  return response.data
}

/** §26.3 : déclenche un téléchargement réel (jamais un bouton simulé) — le nom de fichier vient de
 * l'en-tête Content-Disposition déjà posé par le serveur. */
async function downloadExport(
  path: string,
  filter: ReportingFilterQuery | undefined,
  format: ExportFormat,
  fallbackName: string,
): Promise<void> {
  const response = await apiClient.get<Blob>(path, {
    params: { ...filter, format },
    responseType: 'blob',
  })
  const fileName = extractFileName(response.headers['content-disposition'], fallbackName)
  triggerBrowserDownload(response.data, fileName)
}

export async function exportCharges(filter: ReportingFilterQuery | undefined, format: ExportFormat): Promise<void> {
  await downloadExport('/reporting/charges/export', filter, format, 'charges.csv')
}

export async function exportOperational(filter: ReportingFilterQuery | undefined, format: ExportFormat): Promise<void> {
  await downloadExport('/reporting/operational/export', filter, format, 'operationnel.csv')
}

/** §26.3 : ressource intégralement financière — 403 côté serveur sans FINANCIAL_DATA_VIEW. */
export async function exportFinancial(filter: ReportingFilterQuery | undefined, format: ExportFormat): Promise<void> {
  await downloadExport('/reporting/financial/export', filter, format, 'financier.csv')
}

/** §14.3 (Lot 11) : indicateurs de la page Budgets, ressource intégralement financière — 403 côté
 * serveur sans FINANCIAL_DATA_VIEW. */
export async function fetchFinancialReport(
  filter?: ReportingFilterQuery,
): Promise<FinancialReportDto> {
  const response = await apiClient.get<FinancialReportDto>('/reporting/financial', {
    params: filter,
  })
  return response.data
}

/** §17.7 : références RUN (INC/CHG/PRB/RITM/VABE/VSR) rattachées à un projet — dérivées de
 * TimeEntry.Reference, jamais intégrées au modèle Projet (docs/BACKLOG_METIER.md §3). */
export async function fetchProjectLinkedReferences(
  projectId: string,
): Promise<ProjectLinkedReferenceDto[]> {
  const response = await apiClient.get<ProjectLinkedReferenceDto[]>(
    `/reporting/projects/${projectId}/linked-references`,
  )
  return response.data
}
