/**
 * Types partagés miroir des DTO backend (CLAUDE.md §13 : l'API n'expose jamais une entité EF Core
 * directement, ces types reflètent donc les DTO, pas le modèle de données). Un enum C# sans
 * `JsonStringEnumConverter` explicite (voir Program.cs) sérialise en entier côté API : les enums
 * ci-dessous sont donc représentés en constantes numériques, pas en union de chaînes.
 */

export interface PaginationQuery {
  page?: number
  pageSize?: number
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export const ReferentialStatus = {
  Actif: 0,
  Inactif: 1,
} as const
export type ReferentialStatus = (typeof ReferentialStatus)[keyof typeof ReferentialStatus]

export interface UserDto {
  id: string
  nom: string
  prenom: string
  identifiant: string
  email: string
  telephone: string | null
  statut: ReferentialStatus
  dateArrivee: string
  dateSortie: string | null
  commentaire: string | null
  resourceId: string | null
  roleId: string
  accesGlobal: boolean
  permissionIds: string[]
}

export interface PermissionDto {
  id: string
  code: string
  libelle: string
  description: string | null
}

export const ApplicationCriticality = {
  Faible: 0,
  Moyenne: 1,
  Elevee: 2,
  Critique: 3,
} as const
export type ApplicationCriticality = (typeof ApplicationCriticality)[keyof typeof ApplicationCriticality]

// --- Organisation (Lot 1) ---

export interface DepartmentDto {
  id: string
  code: string
  nom: string
  responsableId: string | null
  statut: ReferentialStatus
  commentaire: string | null
}
export interface DepartmentCreateRequest {
  code: string
  nom: string
  responsableId?: string | null
  commentaire?: string | null
}

export interface ServiceDto {
  id: string
  code: string
  nom: string
  departmentId: string
  responsableId: string | null
  statut: ReferentialStatus
  commentaire: string | null
}
export interface ServiceCreateRequest {
  code: string
  nom: string
  departmentId: string
  responsableId?: string | null
  commentaire?: string | null
}

export interface TeamDto {
  id: string
  code: string
  nom: string
  serviceId: string
  responsableFonctionnelId: string | null
  statut: ReferentialStatus
  commentaire: string | null
}
export interface TeamCreateRequest {
  code: string
  nom: string
  serviceId: string
  responsableFonctionnelId?: string | null
  commentaire?: string | null
}

// --- Ressources (Lot 1) ---

export interface ResourceDto {
  id: string
  nom: string
  prenom: string
  departmentId: string
  serviceId: string
  teamId: string | null
  responsableHierarchiqueId: string | null
  companyId: string | null
  defaultOrderId: string | null
  dailyCapacity: number
  weeklyCapacity: number
  statut: ReferentialStatus
  commentaire: string | null
  operationalRoleIds: string[]
}

// --- Applications (Lot 1) ---

export interface ApplicationReferenceDto {
  id: string
  nom: string
  code: string
  serviceId: string
  teamId: string | null
  criticite: ApplicationCriticality
  responsableId: string | null
  statut: ReferentialStatus
  commentaire: string | null
}
export interface ApplicationReferenceCreateRequest {
  nom: string
  code: string
  serviceId: string
  teamId?: string | null
  criticite: ApplicationCriticality
  responsableId?: string | null
  commentaire?: string | null
}

// --- Sociétés (Lot 1/2/6) ---

export interface CompanyDto {
  id: string
  nom: string
  code: string
  companyTypeId: string
  statut: ReferentialStatus
  contactPrincipal: string
  emailContact: string
  telephone: string | null
  adresse: string | null
  commentaire: string | null
}
export interface CompanyCreateRequest {
  nom: string
  code: string
  companyTypeId: string
  contactPrincipal: string
  emailContact: string
  telephone?: string | null
  adresse?: string | null
  commentaire?: string | null
}
export interface CompanyUpdateRequest {
  nom: string
  companyTypeId: string
  contactPrincipal: string
  emailContact: string
  telephone?: string | null
  adresse?: string | null
  commentaire?: string | null
}

export interface CompanyContractHistoryDto {
  id: string
  companyId: string
  contractNumber: string | null
  startDate: string
  endDate: string | null
  contractDailyRate: number
  currency: string
  comment: string | null
  status: ReferentialStatus
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
}

// --- Commandes (Lot 1/5/6) ---

export interface OrderDto {
  id: string
  reference: string
  libelle: string
  companyId: string
  projectId: string | null
  budgetFinancierInitial: number
  budgetFinancierAjuste: number
  budgetJoursInitial: number | null
  budgetJoursAjuste: number | null
  dateDebut: string
  dateFinInitiale: string
  dateFinAjustee: string | null
  statusId: string
  seuilAlerte: number | null
  commentaire: string | null
  authorizedResourceIds: string[]
}

// --- Temps et capacité (Lot 3) ---

export interface ActivityTypeDto {
  id: string
  code: string
  libelle: string
  isRun: boolean
  referenceRequired: boolean
  referenceFormatRegex: string | null
  referenceExample: string | null
  statut: ReferentialStatus
}
export interface ActivityTypeCreateRequest {
  code: string
  libelle: string
  isRun: boolean
  referenceRequired: boolean
  referenceFormatRegex?: string | null
  referenceExample?: string | null
}

// --- Projets (Lot 4) ---

export interface MilestoneTypeDto {
  id: string
  code: string
  libelle: string
  statut: ReferentialStatus
}
export interface MilestoneTypeCreateRequest {
  code: string
  libelle: string
}

// --- Paramètres (Lot 1) ---

export interface SettingsDto {
  heuresParJour: number
  joursOuvresParSemaine: number
  paysParDefaut: string
  deviseParDefaut: string
  seuilSurcharge: number | null
  seuilSousCharge: number | null
  seuilAlerteBudget: number | null
  seuilAlerteCommande: number | null
  delaiModificationTempsJours: number
  activationValidationAbsences: boolean
  autorisationSaisieSansValorisation: boolean
}
export type SettingsUpdateRequest = SettingsDto

// --- Audit (Lot 6) ---

export interface AuditLogDto {
  id: string
  author: string
  timestamp: string
  action: string
  entityType: string
  entityId: string | null
  oldValue: string | null
  newValue: string | null
  reason: string | null
  technicalContext: string | null
}

// --- Financier (Lot 2, sous FINANCIAL_DATA_VIEW) ---

export interface ResourceTjmHistoryDto {
  id: string
  resourceId: string
  startDate: string
  endDate: string | null
  dailyRate: number
  reason: string | null
  comment: string | null
  status: ReferentialStatus
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
}

// --- Référentiels et administration (Lot 8, docs/BACKLOG_METIER.md §5-9) ---

export interface TechnologyDto {
  id: string
  code: string
  libelle: string
  statut: ReferentialStatus
  applicationIds: string[]
  resourceIds: string[]
}
export interface TechnologyCreateRequest {
  code: string
  libelle: string
  applicationIds: string[]
  resourceIds: string[]
}
export interface TechnologyUpdateRequest {
  libelle: string
  statut: ReferentialStatus
  applicationIds: string[]
  resourceIds: string[]
}

export interface ClientDto {
  id: string
  code: string
  nom: string
  statut: ReferentialStatus
  commentaire: string | null
}
export interface ClientCreateRequest {
  code: string
  nom: string
  commentaire?: string | null
}
export interface ClientUpdateRequest {
  nom: string
  statut: ReferentialStatus
  commentaire?: string | null
}

export interface ProjectTypeDto {
  id: string
  code: string
  libelle: string
  statut: ReferentialStatus
}
export interface ProjectTypeCreateRequest {
  code: string
  libelle: string
}
export interface ProjectTypeUpdateRequest {
  libelle: string
  statut: ReferentialStatus
}

export interface CostCenterDto {
  id: string
  code: string
  libelle: string
  departmentId: string | null
  serviceId: string | null
  statut: ReferentialStatus
}
export interface CostCenterCreateRequest {
  code: string
  libelle: string
  departmentId?: string | null
  serviceId?: string | null
}
export interface CostCenterUpdateRequest {
  libelle: string
  departmentId?: string | null
  serviceId?: string | null
  statut: ReferentialStatus
}

export interface CurrencyDto {
  id: string
  codeIso: string
  libelle: string
  symbole: string
  statut: ReferentialStatus
}
export interface CurrencyCreateRequest {
  codeIso: string
  libelle: string
  symbole: string
}
export interface CurrencyUpdateRequest {
  libelle: string
  symbole: string
  statut: ReferentialStatus
}

// --- Reporting (Lot 5, réutilisé pour le détail statistique d'une application, Lot 8) ---

export const ReportingPeriodType = {
  Jour: 0,
  Semaine: 1,
  Mois: 2,
  Annee: 3,
  Personnalisee: 4,
} as const
export type ReportingPeriodType = (typeof ReportingPeriodType)[keyof typeof ReportingPeriodType]

export interface ReportingFilterQuery {
  periodType?: ReportingPeriodType
  referenceDate?: string
  customFrom?: string
  customTo?: string
  applicationId?: string
  projectId?: string
  orderId?: string
  departmentId?: string
  serviceId?: string
  teamId?: string
  resourceId?: string
  activityTypeId?: string
  operationalRoleId?: string
}

export interface ChargesReportDto {
  periodFrom: string
  periodTo: string
  chargeTotaleHeures: number
  chargeRunHeures: number
  chargeHorsRunHeures: number
  nombreIncidents: number
  nombreChanges: number
  nombreProblems: number
  nombreRitm: number
  nombreVabe: number
  nombreVsr: number
}
