/** Miroir de `PermissionCodes` (backend, Application/Common/Security) : mêmes chaînes stables,
 * dupliquées ici par nécessité technique (pas de partage de code entre les deux runtimes). */
export const PermissionCodes = {
  FinancialDataView: 'FINANCIAL_DATA_VIEW',
  TimeEntryCorrection: 'TIME_ENTRY_CORRECTION',
  TimeEntryRecalculation: 'TIME_ENTRY_RECALCULATION',
  UserAdministration: 'USER_ADMINISTRATION',
  ImportExecute: 'IMPORT_EXECUTE',
  AuditView: 'AUDIT_VIEW',
  OrderReceiptOverride: 'ORDER_RECEIPT_OVERRIDE',
} as const
