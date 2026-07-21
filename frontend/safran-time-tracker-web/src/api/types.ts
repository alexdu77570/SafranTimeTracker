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
export type ApplicationCriticality =
  (typeof ApplicationCriticality)[keyof typeof ApplicationCriticality]

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

// --- Commandes (Lot 1/5/6, écran Lot 11) ---

export interface OrderStatusDto {
  id: string
  code: string
  libelle: string
  ordre: number
}

/** §13.2 : consommation en jours, coûts réel/contractuel consommés, différentiel, reste financier
 * et reste en jours — absent (pas null) sans FINANCIAL_DATA_VIEW, comme ProjectDto.FinancialSummary. */
export interface OrderFinancialSummaryDto {
  consommationJours: number
  coutReelConsomme: number
  coutContractuelConsomme: number
  differentiel: number
  restFinancier: number
  restJours: number | null
}

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
  financialSummary: OrderFinancialSummaryDto | null
}

export interface OrderCreateRequest {
  reference: string
  libelle: string
  companyId: string
  projectId?: string | null
  budgetFinancierInitial: number
  budgetJoursInitial?: number | null
  dateDebut: string
  dateFinInitiale: string
  seuilAlerte?: number | null
  commentaire?: string | null
  authorizedResourceIds?: string[]
}

/** Budget/jours initiaux, date de fin initiale et statut ne sont pas modifiables ici : les
 * rallonges et les actions de transition de statut dédiées en tiennent lieu (§13.2/§13.3). */
export interface OrderUpdateRequest {
  libelle: string
  projectId?: string | null
  seuilAlerte?: number | null
  commentaire?: string | null
  authorizedResourceIds?: string[]
}

/** Réouverture d'une commande clôturée (§13.4) : motif obligatoire. */
export interface OrderReopenRequest {
  motif: string
}

/** §13.3 : rallonge, augmente le budget ajusté, conserve le budget initial, historique jamais
 * modifié. ExtensionDate/PreviousEndDate sont dérivés côté service (date du jour, date de fin
 * ajustée courante) — seuls montant/jours ajoutés, nouvelle date de fin et motif sont saisis. */
export interface OrderExtensionDto {
  id: string
  orderId: string
  extensionDate: string
  amountAdded: number
  daysAdded: number | null
  previousEndDate: string
  newEndDate: string
  reason: string
  comment: string | null
  createdAt: string
  createdBy: string
}

export interface OrderExtensionCreateRequest {
  amountAdded: number
  daysAdded?: number | null
  newEndDate: string
  reason: string
  comment?: string | null
}

/** Réception partielle (règle métier validée Lot 6) : append-only, aucun DTO de mise à jour — une
 * correction se fait par une nouvelle réception, éventuellement à montant/jours négatifs. */
export interface OrderReceiptDto {
  id: string
  orderId: string
  receiptDate: string
  receivedAmount: number | null
  receivedDays: number | null
  reason: string | null
  comment: string | null
  createdAt: string
  createdBy: string
}

/** Exactement l'un des deux (receivedAmount/receivedDays) doit être renseigné, jamais les deux. */
export interface OrderReceiptCreateRequest {
  receiptDate: string
  receivedAmount?: number | null
  receivedDays?: number | null
  reason?: string | null
  comment?: string | null
}

export interface OrderReceiptSummaryDto {
  totalReceivedAmount: number
  totalReceivedDays: number
  remainingReceivableAmount: number
  remainingReceivableDays: number | null
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

export const MilestoneStatus = {
  AVenir: 0,
  EnCours: 1,
  Termine: 2,
  Annule: 3,
} as const
export type MilestoneStatus = (typeof MilestoneStatus)[keyof typeof MilestoneStatus]

export const MilestoneCriticality = {
  Faible: 0,
  Moyenne: 1,
  Elevee: 2,
  Critique: 3,
} as const
export type MilestoneCriticality = (typeof MilestoneCriticality)[keyof typeof MilestoneCriticality]

export interface MilestoneDto {
  id: string
  nom: string
  milestoneTypeId: string
  projectId: string
  applicationId: string | null
  responsableId: string
  datePrevue: string
  dateReelle: string | null
  statut: MilestoneStatus
  criticite: MilestoneCriticality
  commentaire: string | null
  dependsOnMilestoneId: string | null
  /** Dérivé (§24.2), jamais stocké : DatePrevue dépassée et statut ni Terminé ni Annulé. */
  estEnRetard: boolean
}

export interface MilestoneCreateRequest {
  nom: string
  milestoneTypeId: string
  projectId: string
  applicationId?: string | null
  responsableId: string
  datePrevue: string
  criticite?: MilestoneCriticality
  commentaire?: string | null
  dependsOnMilestoneId?: string | null
}

/** Ne permet pas de changer le type ni le projet, qui définissent l'identité du jalon (§24.2). */
export interface MilestoneUpdateRequest {
  nom: string
  responsableId: string
  datePrevue: string
  dateReelle?: string | null
  statut: MilestoneStatus
  criticite: MilestoneCriticality
  commentaire?: string | null
  dependsOnMilestoneId?: string | null
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

/** §26.3 : exports réels CSV/Excel/PDF, jamais de simples boutons simulés. */
export const ExportFormat = {
  Csv: 0,
  Excel: 1,
  Pdf: 2,
} as const
export type ExportFormat = (typeof ExportFormat)[keyof typeof ExportFormat]

export interface ChargesTopEntryDto {
  id: string
  nom: string
  chargeHeures: number
}

export interface ChargesResourceAlertDto {
  resourceId: string
  nom: string
  chargeHeures: number
  capaciteReelle: number
}

/** §21.3 "courbe mensuelle" / §25.3 "évolution mensuelle" (Lot 12, décision 1). */
export interface ChargesMonthlyEvolutionDto {
  annee: number
  mois: number
  chargeTotaleHeures: number
  chargeRunHeures: number
  chargeHorsRunHeures: number
}

/** §21.3 "heatmap de charge" (`WorkloadHeatmap`, Lot 12, décision 1). */
export interface ChargesHeatmapEntryDto {
  resourceId: string
  nom: string
  weekStartDate: string
  chargeHeures: number
}

/** §21.2/§21.3/§25.3 "prévu vs réalisé" à l'échelle du portefeuille filtré (Lot 12, décision 3). */
export interface ChargesPlanComparisonDto {
  /** Null si le filtre porte sur une commande ou un type d'activité — non calculable à ce niveau
   * de granularité (la planification ne connaît pas ces dimensions), jamais approché à zéro. */
  chargePrevue: number | null
  chargeRealisee: number
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
  topApplications: ChargesTopEntryDto[]
  topUtilisateurs: ChargesTopEntryDto[]
  topProjets: ChargesTopEntryDto[]
  topCommandes: ChargesTopEntryDto[]
  ressourcesSurchargees: ChargesResourceAlertDto[]
  ressourcesSousChargees: ChargesResourceAlertDto[]
  evolutionMensuelle: ChargesMonthlyEvolutionDto[]
  heatmap: ChargesHeatmapEntryDto[]
  prevuVsRealise: ChargesPlanComparisonDto
}

/** §26.1 : aucune donnée financière, accessible sans permission dédiée. */
export interface OperationalReportGroupDto {
  id: string
  nom: string
  chargeHeures: number
}

export interface OperationalReportMilestoneDto {
  id: string
  nom: string
  projectId: string
  datePrevue: string
}

export interface OperationalReportCapacityDto {
  resourceId: string
  nom: string
  capaciteTheorique: number
  capaciteReelle: number
  tauxDisponibilite: number
}

export interface OperationalReportDto {
  periodFrom: string
  periodTo: string
  chargeParEquipe: OperationalReportGroupDto[]
  chargeParService: OperationalReportGroupDto[]
  chargeParDepartement: OperationalReportGroupDto[]
  consommationParProjet: ChargesTopEntryDto[]
  consommationParCommande: ChargesTopEntryDto[]
  jalonsEnRetard: OperationalReportMilestoneDto[]
  ressourcesSurchargees: ChargesResourceAlertDto[]
  ressourcesSousUtilisees: ChargesResourceAlertDto[]
  capaciteEtDisponibilite: OperationalReportCapacityDto[]
}

// --- Imports (Lot 6, écran Lot 12) ---

/** §27.1 : les 16 types importables, dans l'ordre exact du backend (enum non stringifié — valeurs
 * numériques par défaut System.Text.Json, même convention que MilestoneStatus/ProjectRiskLevel). */
export const ImportEntityType = {
  Resources: 0,
  Users: 1,
  Companies: 2,
  ResourceCompanyAssignments: 3,
  ResourceTjmHistories: 4,
  CompanyContractHistories: 5,
  Projects: 6,
  Budgets: 7,
  Orders: 8,
  ProjectParticipants: 9,
  Plannings: 10,
  TimeEntries: 11,
  Absences: 12,
  Milestones: 13,
  Applications: 14,
  Organisation: 15,
} as const
export type ImportEntityType = (typeof ImportEntityType)[keyof typeof ImportEntityType]

export const ImportMode = {
  Ajout: 0,
  MiseAJour: 1,
  Complet: 2,
} as const
export type ImportMode = (typeof ImportMode)[keyof typeof ImportMode]

export const ImportBatchStatus = {
  Previsualise: 0,
  Simule: 1,
  Confirme: 2,
  Echoue: 3,
} as const
export type ImportBatchStatus = (typeof ImportBatchStatus)[keyof typeof ImportBatchStatus]

export const ImportDiffType = {
  Ajout: 0,
  Modification: 1,
  Suppression: 2,
  Inchange: 3,
  Erreur: 4,
} as const
export type ImportDiffType = (typeof ImportDiffType)[keyof typeof ImportDiffType]

export interface ImportTypeMetadataDto {
  type: ImportEntityType
  expectedHeaders: string[]
  supportedModes: ImportMode[]
}

/** §27.3 étapes 1-4 : détection encodage/séparateur déjà faite côté serveur (CsvFileParser), avant
 * tout mapping. `expectedHeaders` sert uniquement à un affichage lecture seule (docs/BACKLOG_METIER.md
 * §16, décision 4 — pas de réassociation interactive des colonnes). */
export interface ImportPreviewDto {
  detectedHeaders: string[]
  expectedHeaders: string[]
  lineCount: number
  sampleRows: Record<string, string>[]
}

export interface FieldChangeDto {
  fieldName: string
  oldValue: string | null
  newValue: string | null
}

export interface ImportRowResultDto {
  rowNumber: number
  entityId: string | null
  diffType: ImportDiffType
  errorMessage: string | null
  changes: FieldChangeDto[]
}

/** §27.3 étapes 5-9 : aucune persistance (§27.4 "ne pas modifier les données avant confirmation"). */
export interface ImportSimulationDto {
  lineCount: number
  addCount: number
  updateCount: number
  unchangedCount: number
  deleteCount: number
  errorCount: number
  rows: ImportRowResultDto[]
}

export interface ImportBatchDto {
  id: string
  type: ImportEntityType
  source: string
  importDate: string
  userId: string
  mode: ImportMode
  fileName: string
  lineCount: number
  addCount: number
  updateCount: number
  deleteCount: number
  errorCount: number
  status: ImportBatchStatus
  errors: string | null
  checksum: string
  previousBatchId: string | null
}

export interface ImportDiffDto {
  id: string
  importBatchId: string
  entityType: string
  entityId: string | null
  diffType: ImportDiffType
  fieldName: string | null
  oldValue: string | null
  newValue: string | null
}

/** §25.1 : toujours renvoyés, aucune donnée financière. */
export interface DashboardOperationalKpisDto {
  tempsSaisisHeures: number
  capaciteTheorique: number
  capaciteReelle: number
  tauxDisponibilite: number
  chargeRunHeures: number
  chargeHorsRunHeures: number
  incidentsOuverts: number
  changesEnCours: number
  problemsOuverts: number
  ritmEnCours: number
  projetsActifs: number
  jalonsEnRetard: number
  ressourcesSurchargees: number
  ressourcesSousChargees: number
}

/** §25.2 : visible uniquement avec FINANCIAL_DATA_VIEW. */
export interface DashboardFinancialKpisDto {
  budgetInitialTotal: number
  budgetAjusteTotal: number
  coutReelTotal: number
  coutContractuelTotal: number
  differentielGlobal: number
  budgetRestant: number
  commandesARisque: number
  projetsSousFinances: number
  atterrissageEstime: number
}

export interface DashboardDto {
  periodFrom: string
  periodTo: string
  operational: DashboardOperationalKpisDto
  financial: DashboardFinancialKpisDto | null
}

/** §26.2 : ressource intégralement financière (gardée par FINANCIAL_DATA_VIEW au niveau action).
 * "Commandes à renouveler" et "besoins de rallonge" sont des simplifications documentées côté
 * backend (fenêtre fixe de 30 jours, reste financier négatif ou sous le seuil d'alerte). */
export interface FinancialReportDifferentialDto {
  id: string
  nom: string
  coutReel: number
  coutContractuel: number
  differentiel: number
}

export interface FinancialReportOrderAlertDto {
  orderId: string
  reference: string
  budgetFinancierAjuste: number
  coutReelConsomme: number
  dateFinAjustee: string | null
}

/** §14.3 "consommation mensuelle" (Lot 11, décision 3) : un mois n'est présent que s'il porte au
 * moins une saisie valorisée sur la période demandée. */
export interface FinancialReportMonthlyConsumptionDto {
  annee: number
  mois: number
  coutReel: number
  coutContractuel: number
  differentiel: number
}

export interface FinancialReportDto {
  periodFrom: string
  periodTo: string
  differentielGlobal: number
  budgetRestant: number
  atterrissageEstime: number
  differentielParProjet: FinancialReportDifferentialDto[]
  differentielParCommande: FinancialReportDifferentialDto[]
  differentielParSociete: FinancialReportDifferentialDto[]
  differentielParRessource: FinancialReportDifferentialDto[]
  consommationMensuelle: FinancialReportMonthlyConsumptionDto[]
  besoinsRallonge: FinancialReportOrderAlertDto[]
  commandesARenouveler: FinancialReportOrderAlertDto[]
  sourcesMontants: string[]
}

// --- Projets (Lot 4, écrans Lot 10) ---

export const ProjectRiskLevel = {
  Faible: 0,
  Moyen: 1,
  Eleve: 2,
} as const
export type ProjectRiskLevel = (typeof ProjectRiskLevel)[keyof typeof ProjectRiskLevel]

/** Absent (pas null) sans FINANCIAL_DATA_VIEW — projection faite par ProjectService (CLAUDE.md §13). */
export interface ProjectFinancialSummaryDto {
  budgetInitial: number | null
  coutReelConsomme: number
  coutContractuelConsomme: number
  differentiel: number
  budgetRestant: number | null
}

export interface ProjectDto {
  id: string
  nom: string
  code: string
  applicationId: string
  descriptionCourte: string | null
  piloteId: string
  departmentId: string
  serviceId: string
  teamId: string | null
  statusId: string
  projectTypeId: string | null
  clientId: string | null
  dateDebut: string
  dateFinPrevueInitiale: string
  dateFinAjustee: string | null
  dateFinReelle: string | null
  niveauRisque: ProjectRiskLevel
  commentaire: string | null
  financialSummary: ProjectFinancialSummaryDto | null
}

export interface ProjectCreateRequest {
  nom: string
  code: string
  applicationId: string
  descriptionCourte?: string | null
  piloteId: string
  departmentId: string
  serviceId: string
  teamId?: string | null
  projectTypeId?: string | null
  clientId?: string | null
  dateDebut: string
  dateFinPrevueInitiale: string
  budgetInitial?: number | null
  niveauRisque?: ProjectRiskLevel
  commentaire?: string | null
}

/** Statut non modifiable ici : archiver/réactiver sont des actions dédiées (§16.3). */
export interface ProjectUpdateRequest {
  nom: string
  descriptionCourte?: string | null
  piloteId: string
  teamId?: string | null
  projectTypeId?: string | null
  clientId?: string | null
  dateFinAjustee: string
  dateFinReelle?: string | null
  budgetInitial?: number | null
  niveauRisque: ProjectRiskLevel
  commentaire?: string | null
}

/** Référentiel de statuts de projet (§16.2, §30) — écart corrigé au Lot 10 (aucun endpoint GET n'existait). */
export interface ProjectStatusDto {
  id: string
  code: string
  libelle: string
  ordre: number
}

/** Cahier des charges §17.2. Null sans FINANCIAL_DATA_VIEW. */
export interface ProjectParticipantFinancialSummaryDto {
  companyIdApplicable: string | null
  tjmPersonneApplicable: number | null
  tjmContratApplicable: number | null
}

export interface ProjectParticipantDto {
  id: string
  projectId: string
  resourceId: string
  operationalRoleId: string | null
  defaultOrderId: string | null
  dateDebut: string
  dateFin: string | null
  capacitePrevue: number | null
  statut: ReferentialStatus
  financialSummary: ProjectParticipantFinancialSummaryDto | null
}

export interface ProjectParticipantCreateRequest {
  resourceId: string
  operationalRoleId?: string | null
  defaultOrderId?: string | null
  dateDebut: string
  dateFin?: string | null
  capacitePrevue?: number | null
}

export const ProjectPlanVersionType = {
  Initial: 0,
  Ajuste: 1,
} as const
export type ProjectPlanVersionType =
  (typeof ProjectPlanVersionType)[keyof typeof ProjectPlanVersionType]

export const ProjectPlanVersionStatus = {
  Active: 0,
  Archivee: 1,
} as const
export type ProjectPlanVersionStatus =
  (typeof ProjectPlanVersionStatus)[keyof typeof ProjectPlanVersionStatus]

export interface ProjectPlanVersionDto {
  id: string
  projectId: string
  type: ProjectPlanVersionType
  statut: ProjectPlanVersionStatus
  motif: string | null
  createdAt: string
  createdBy: string
}

export interface ProjectPlanVersionCreateRequest {
  motif?: string | null
}

/** Motif obligatoire pour une version Ajustée (§18.3). */
export interface ProjectPlanVersionAdjustmentRequest {
  motif: string
}

export interface ProjectWeeklyPlanDto {
  id: string
  projectPlanVersionId: string
  resourceId: string
  weekStartDate: string
  chargePlanifieeHeures: number
}

export interface ProjectWeeklyPlanLineRequest {
  resourceId: string
  weekStartDate: string
  chargePlanifieeHeures: number
}

/** Cahier des charges §18.1, §29.5 — seul point de calcul des écarts/risques, jamais recalculé côté frontend. */
export interface ProjectPlanningSynthesisDto {
  projectId: string
  chargeInitiale: number
  chargeAjustee: number | null
  chargeConsommee: number
  chargeRestante: number
  ecartCharge: number
  deriveCharge: number
  atterrissageCharge: number
  derivePlanningJours: number
  risquePlanning: boolean
  /** Null sans FINANCIAL_DATA_VIEW. */
  atterrissageFinancier: number | null
  /** Null sans FINANCIAL_DATA_VIEW ou si aucun budget n'est disponible. */
  risqueBudget: boolean | null
}

/** Vue transverse "Planning projet" (§18.2, GET /api/v1/project-planning) — une ligne par
 * (Projet, Ressource, Semaine), entièrement agrégée côté serveur. */
export interface ProjectPlanningRowDto {
  projectId: string
  resourceId: string
  weekStartDate: string
  chargePlanifieeInitiale: number
  chargePlanifieeAjustee: number | null
  chargeRealisee: number
  ecartPrevuRealise: number
  capaciteReelle: number
  surcharge: boolean
}

/** Cahier des charges §17.7 : références RUN (INC/CHG/PRB/RITM/VABE/VSR) rattachées à un projet,
 * jamais intégrées au modèle Projet lui-même (docs/BACKLOG_METIER.md §3). */
export interface ProjectLinkedReferenceDto {
  reference: string
  activityTypeCode: string
  activityTypeLibelle: string
  nombreSaisies: number
  chargeHeures: number
  premiereDate: string
  derniereDate: string
}

// --- Temps et capacité (Lot 3, écrans Lot 9) ---

export const FinancialValuationStatus = {
  Complete: 0,
  Incomplete: 1,
} as const
export type FinancialValuationStatus =
  (typeof FinancialValuationStatus)[keyof typeof FinancialValuationStatus]

export interface TimeEntryFinancialSnapshotDto {
  tjmPersonneSnapshot: number | null
  sourceTjmPersonne: string | null
  resourceTjmHistoryId: string | null
  tjmContratSnapshot: number | null
  sourceContrat: string | null
  companyContractHistoryId: string | null
  companyIdSnapshot: string | null
  coutReelCalcule: number | null
  coutContratCalcule: number | null
  differentielCalcule: number | null
  calculationDate: string
  calculationStatus: FinancialValuationStatus
}

export interface TimeEntryDto {
  id: string
  resourceId: string
  activityTypeId: string
  projectId: string | null
  orderId: string | null
  date: string
  dureeHeures: number
  reference: string | null
  commentaire: string | null
  statut: ReferentialStatus
  createdAt: string
  createdBy: string
  updatedAt: string | null
  updatedBy: string | null
  /** Absent (pas null) sans FINANCIAL_DATA_VIEW — projection faite par le service backend (CLAUDE.md §13). */
  financialSnapshot: TimeEntryFinancialSnapshotDto | null
}

export interface TimeEntryCreateRequest {
  resourceId: string
  activityTypeId: string
  projectId?: string | null
  orderId?: string | null
  date: string
  dureeHeures: number
  reference?: string | null
  commentaire?: string | null
}

/** ResourceId volontairement non modifiable (même convention que le backend) — voir TimeEntryUpdateRequest côté API. */
export interface TimeEntryUpdateRequest {
  activityTypeId: string
  projectId?: string | null
  orderId?: string | null
  date: string
  dureeHeures: number
  reference?: string | null
  commentaire?: string | null
}

export interface TimeEntryRecalculationRequest {
  reason: string
}

// --- Absences (Lot 3, écrans Lot 9) ---

export const AbsenceType = {
  Conge: 0,
  Rtt: 1,
  Maladie: 2,
  Formation: 3,
  Deplacement: 4,
  Indisponible: 5,
} as const
export type AbsenceType = (typeof AbsenceType)[keyof typeof AbsenceType]

export const AbsenceStatus = {
  Brouillon: 0,
  Soumis: 1,
  Valide: 2,
  Refuse: 3,
  Annule: 4,
} as const
export type AbsenceStatus = (typeof AbsenceStatus)[keyof typeof AbsenceStatus]

export interface AbsenceDto {
  id: string
  resourceId: string
  type: AbsenceType
  dateDebut: string
  dateFin: string
  demiJournee: boolean
  commentaire: string | null
  statut: AbsenceStatus
  valideParIdentifiant: string | null
  dateDecision: string | null
  createdAt: string
  createdBy: string
}

export interface AbsenceCreateRequest {
  resourceId: string
  type: AbsenceType
  dateDebut: string
  dateFin: string
  demiJournee?: boolean
  commentaire?: string | null
}

/** Restreint au statut Brouillon côté serveur (409 sinon, docs/BACKLOG_METIER.md §12). */
export interface AbsenceUpdateRequest {
  type: AbsenceType
  dateDebut: string
  dateFin: string
  demiJournee?: boolean
  commentaire?: string | null
}

export interface AbsenceDecisionRequest {
  commentaire?: string | null
}

// --- Capacité et disponibilité (Lot 3, écran Disponibilités Lot 9) ---

export interface AvailabilityResultDto {
  resourceId: string
  startDate: string
  endDate: string
  joursOuvres: number
  joursFeries: number
  joursAbsenceValidee: number
  capaciteTheorique: number
  capaciteReelle: number
  tauxDisponibilite: number
  chargeRunHeures: number
  chargeHorsRunHeures: number
}

export interface HolidayCalendarDto {
  id: string
  date: string
  libelle: string
  pays: string
  statut: ReferentialStatus
}

// --- Budgets (Lot 5, onglet Budget Lot 10) ---

export const BudgetStatus = {
  Actif: 0,
  Cloture: 1,
} as const
export type BudgetStatus = (typeof BudgetStatus)[keyof typeof BudgetStatus]

/** Ressource intégralement financière (§14) : endpoint entièrement gardé par FINANCIAL_DATA_VIEW
 * au niveau contrôleur, aucun champ omis à la pièce. */
export interface BudgetDto {
  id: string
  name: string
  projectId: string | null
  orderId: string | null
  initialAmount: number
  adjustedAmount: number
  status: BudgetStatus
  alertThreshold: number | null
  startDate: string
  endDate: string | null
  comment: string | null
  coutReelConsomme: number
  coutContractuelConsomme: number
  differentiel: number
  montantRestant: number
  atterrissageEstime: number
  risqueDepassement: boolean
}

export interface BudgetCreateRequest {
  name: string
  projectId?: string | null
  orderId?: string | null
  initialAmount: number
  alertThreshold?: number | null
  startDate: string
  endDate?: string | null
  comment?: string | null
}

export interface BudgetUpdateRequest {
  name: string
  alertThreshold?: number | null
  endDate?: string | null
  comment?: string | null
}

export interface BudgetVersionDto {
  id: string
  budgetId: string
  oldValue: number
  newValue: number
  reason: string
  referencePiece: string | null
  createdAt: string
  createdBy: string
}

/** OldValue est dérivé côté service (AdjustedAmount courant), seul NewValue est saisi (§14.2). */
export interface BudgetAdjustRequest {
  newValue: number
  reason: string
  referencePiece?: string | null
}
