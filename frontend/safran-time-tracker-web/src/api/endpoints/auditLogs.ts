import { apiClient } from '../client'
import type { AuditLogDto, PagedResult, PaginationQuery } from '../types'

/** Gardé par AUDIT_VIEW côté serveur (§28.1) : un appelant sans la permission reçoit un 403. */
export async function fetchAuditLogs(params?: PaginationQuery): Promise<PagedResult<AuditLogDto>> {
  const response = await apiClient.get<PagedResult<AuditLogDto>>('/audit-logs', { params: { pageSize: 20, ...params } })
  return response.data
}
