import { apiClient } from '../client'
import type { SettingsDto, SettingsUpdateRequest } from '../types'

export async function fetchSettings(): Promise<SettingsDto> {
  const response = await apiClient.get<SettingsDto>('/settings')
  return response.data
}
export async function updateSettings(payload: SettingsUpdateRequest): Promise<SettingsDto> {
  const response = await apiClient.put<SettingsDto>('/settings', payload)
  return response.data
}
