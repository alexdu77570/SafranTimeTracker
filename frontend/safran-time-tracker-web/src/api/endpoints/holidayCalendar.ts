import { apiClient } from '../client'
import type { HolidayCalendarDto, PagedResult } from '../types'

export async function fetchHolidays(year?: number): Promise<PagedResult<HolidayCalendarDto>> {
  const response = await apiClient.get<PagedResult<HolidayCalendarDto>>('/holiday-calendar', { params: { pageSize: 100, year } })
  return response.data
}
