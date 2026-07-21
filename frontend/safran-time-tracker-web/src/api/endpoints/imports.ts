import { apiClient } from '../client'
import type {
  ImportBatchDto,
  ImportDiffDto,
  ImportEntityType,
  ImportMode,
  ImportPreviewDto,
  ImportSimulationDto,
  ImportTypeMetadataDto,
  PagedResult,
  PaginationQuery,
} from '../types'

export async function fetchImportTypes(): Promise<ImportTypeMetadataDto[]> {
  const response = await apiClient.get<ImportTypeMetadataDto[]>('/imports/types')
  return response.data
}

export interface ImportBatchListParams extends PaginationQuery {
  type?: ImportEntityType
}

export async function fetchImportBatches(params?: ImportBatchListParams): Promise<PagedResult<ImportBatchDto>> {
  const response = await apiClient.get<PagedResult<ImportBatchDto>>('/imports', { params: { pageSize: 20, ...params } })
  return response.data
}

export async function fetchImportBatchById(id: string): Promise<ImportBatchDto> {
  const response = await apiClient.get<ImportBatchDto>(`/imports/${id}`)
  return response.data
}

export async function fetchImportDiffs(id: string, params?: PaginationQuery): Promise<PagedResult<ImportDiffDto>> {
  const response = await apiClient.get<PagedResult<ImportDiffDto>>(`/imports/${id}/diffs`, {
    params: { pageSize: 100, ...params },
  })
  return response.data
}

/** `apiClient` fixe un Content-Type JSON par défaut (client.ts) : il faut l'effacer explicitement
 * pour un envoi multipart, afin que le navigateur pose lui-même l'en-tête avec sa frontière. */
const multipartHeaders = { headers: { 'Content-Type': undefined } }

/** §27.3 étapes 1-4 : aucune écriture, détection encodage/séparateur déjà faite côté serveur. */
export async function previewImport(type: ImportEntityType, file: File): Promise<ImportPreviewDto> {
  const form = new FormData()
  form.append('Type', String(type))
  form.append('File', file)
  const response = await apiClient.post<ImportPreviewDto>('/imports/preview', form, multipartHeaders)
  return response.data
}

/** §27.3 étapes 5-9 : aucune écriture (§27.4 "ne pas modifier les données avant confirmation"). */
export async function simulateImport(type: ImportEntityType, mode: ImportMode, file: File): Promise<ImportSimulationDto> {
  const form = new FormData()
  form.append('Type', String(type))
  form.append('Mode', String(mode))
  form.append('File', file)
  const response = await apiClient.post<ImportSimulationDto>('/imports/simulate', form, multipartHeaders)
  return response.data
}

/** §27.3 étapes 10-12 : écrit ImportBatch/ImportDiff et audite. */
export async function executeImport(type: ImportEntityType, mode: ImportMode, file: File): Promise<ImportBatchDto> {
  const form = new FormData()
  form.append('Type', String(type))
  form.append('Mode', String(mode))
  form.append('File', file)
  const response = await apiClient.post<ImportBatchDto>('/imports/execute', form, multipartHeaders)
  return response.data
}

/** §27.4 : import SharePoint simulé — même pipeline, source figée à "SharePoint" côté serveur. */
export async function executeSharePointImport(type: ImportEntityType, mode: ImportMode, file: File): Promise<ImportBatchDto> {
  const form = new FormData()
  form.append('Type', String(type))
  form.append('Mode', String(mode))
  form.append('File', file)
  const response = await apiClient.post<ImportBatchDto>('/imports/sharepoint/execute', form, multipartHeaders)
  return response.data
}
