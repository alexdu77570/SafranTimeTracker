namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Identifiants fixes des données de démonstration (cahier des charges §5.4). HasData exige des
/// clés déterministes pour rester idempotent d'une génération de migration à l'autre
/// (docs/DATABASE.md §7).
/// </summary>
internal static class SeedIds
{
    // Rôles applicatifs (§5.2)
    public static readonly Guid RoleIngenieur = new("00000000-0000-0000-0001-000000000001");
    public static readonly Guid RoleResponsableService = new("00000000-0000-0000-0001-000000000002");
    public static readonly Guid RoleResponsableDepartement = new("00000000-0000-0000-0001-000000000003");
    public static readonly Guid RoleAdministrateur = new("00000000-0000-0000-0001-000000000004");

    // Permissions (§6.2)
    public static readonly Guid PermissionFinancialDataView = new("00000000-0000-0000-0002-000000000001");

    // Rôles opérationnels (§10.4)
    public static readonly Guid OpRoleRun = new("00000000-0000-0000-0003-000000000001");
    public static readonly Guid OpRoleBuild = new("00000000-0000-0000-0003-000000000002");
    public static readonly Guid OpRoleAmeliorationContinue = new("00000000-0000-0000-0003-000000000003");
    public static readonly Guid OpRoleChefDeProjet = new("00000000-0000-0000-0003-000000000004");
    public static readonly Guid OpRoleCoordinateurIt = new("00000000-0000-0000-0003-000000000005");

    // Types de société (§12.1)
    public static readonly Guid CompanyTypeInterne = new("00000000-0000-0000-0004-000000000001");
    public static readonly Guid CompanyTypeExterne = new("00000000-0000-0000-0004-000000000002");

    // Statuts de commande (§13.2)
    public static readonly Guid OrderStatusBrouillon = new("00000000-0000-0000-0005-000000000001");
    public static readonly Guid OrderStatusActive = new("00000000-0000-0000-0005-000000000002");
    public static readonly Guid OrderStatusSuspendue = new("00000000-0000-0000-0005-000000000003");
    public static readonly Guid OrderStatusConsommee = new("00000000-0000-0000-0005-000000000004");
    public static readonly Guid OrderStatusCloturee = new("00000000-0000-0000-0005-000000000005");

    // Organisation (§9)
    public static readonly Guid DepartmentDsi = new("00000000-0000-0000-0010-000000000001");

    public static readonly Guid ServiceProductionApplicative = new("00000000-0000-0000-0011-000000000001");
    public static readonly Guid ServiceRunMco = new("00000000-0000-0000-0011-000000000002");
    public static readonly Guid ServiceSupport = new("00000000-0000-0000-0011-000000000003");
    public static readonly Guid ServiceProjets = new("00000000-0000-0000-0011-000000000004");

    public static readonly Guid TeamRunA = new("00000000-0000-0000-0012-000000000001");
    public static readonly Guid TeamProjetsA = new("00000000-0000-0000-0012-000000000002");

    // Société (§12.1)
    public static readonly Guid CompanySafran = new("00000000-0000-0000-0013-000000000001");

    // Utilisateurs (ordre du §5.4)
    public static readonly Guid UserBernard = new("00000000-0000-0000-0020-000000000001");
    public static readonly Guid UserLegrand = new("00000000-0000-0000-0020-000000000002");
    public static readonly Guid UserGeorges = new("00000000-0000-0000-0020-000000000003");
    public static readonly Guid UserManceron = new("00000000-0000-0000-0020-000000000004");
    public static readonly Guid UserFocquenoey = new("00000000-0000-0000-0020-000000000005");
    public static readonly Guid UserReau = new("00000000-0000-0000-0020-000000000006");
    public static readonly Guid UserMishra = new("00000000-0000-0000-0020-000000000007");
    public static readonly Guid UserDurand = new("00000000-0000-0000-0020-000000000008");
    public static readonly Guid UserNguyen = new("00000000-0000-0000-0020-000000000009");
    public static readonly Guid UserPatel = new("00000000-0000-0000-0020-000000000010");
    public static readonly Guid UserLefevre = new("00000000-0000-0000-0020-000000000011");
    public static readonly Guid UserCosta = new("00000000-0000-0000-0020-000000000012");
    public static readonly Guid UserVerma = new("00000000-0000-0000-0020-000000000013");

    // Ressources (même ordre, une par utilisateur)
    public static readonly Guid ResourceBernard = new("00000000-0000-0000-0021-000000000001");
    public static readonly Guid ResourceLegrand = new("00000000-0000-0000-0021-000000000002");
    public static readonly Guid ResourceGeorges = new("00000000-0000-0000-0021-000000000003");
    public static readonly Guid ResourceManceron = new("00000000-0000-0000-0021-000000000004");
    public static readonly Guid ResourceFocquenoey = new("00000000-0000-0000-0021-000000000005");
    public static readonly Guid ResourceReau = new("00000000-0000-0000-0021-000000000006");
    public static readonly Guid ResourceMishra = new("00000000-0000-0000-0021-000000000007");
    public static readonly Guid ResourceDurand = new("00000000-0000-0000-0021-000000000008");
    public static readonly Guid ResourceNguyen = new("00000000-0000-0000-0021-000000000009");
    public static readonly Guid ResourcePatel = new("00000000-0000-0000-0021-000000000010");
    public static readonly Guid ResourceLefevre = new("00000000-0000-0000-0021-000000000011");
    public static readonly Guid ResourceCosta = new("00000000-0000-0000-0021-000000000012");
    public static readonly Guid ResourceVerma = new("00000000-0000-0000-0021-000000000013");

    // Applications (exemples §15.2)
    public static readonly Guid AppIbmElm = new("00000000-0000-0000-0022-000000000001");
    public static readonly Guid AppVtom = new("00000000-0000-0000-0022-000000000002");
    public static readonly Guid AppServiceNow = new("00000000-0000-0000-0022-000000000003");

    // Commande de démonstration
    public static readonly Guid OrderDemo = new("00000000-0000-0000-0023-000000000001");

    // Paramètres (ligne singleton)
    public static readonly Guid SettingsSingleton = new("00000000-0000-0000-0024-000000000001");

    // Lot 2 — Société externe de démonstration (§12.1)
    public static readonly Guid CompanyExterneConseil = new("00000000-0000-0000-0013-000000000002");

    // Lot 2 — Historique TJM (§11.2)
    public static readonly Guid TjmBernard = new("00000000-0000-0000-0030-000000000001");
    public static readonly Guid TjmLegrand = new("00000000-0000-0000-0030-000000000002");
    public static readonly Guid TjmGeorges1 = new("00000000-0000-0000-0030-000000000003");
    public static readonly Guid TjmGeorges2 = new("00000000-0000-0000-0030-000000000004");

    // Lot 2 — Historique contrats (§12.3)
    public static readonly Guid ContractExterneConseil = new("00000000-0000-0000-0031-000000000001");

    // Lot 2 — Rattachements ressource/société (§12.2)
    public static readonly Guid AssignmentBernard = new("00000000-0000-0000-0032-000000000001");
    public static readonly Guid AssignmentLegrand = new("00000000-0000-0000-0032-000000000002");
    public static readonly Guid AssignmentGeorges = new("00000000-0000-0000-0032-000000000003");

    // Lot 3 — Types d'activité (§19.2, §29.4)
    public static readonly Guid ActivityTypeRun = new("00000000-0000-0000-0040-000000000001");
    public static readonly Guid ActivityTypeIncident = new("00000000-0000-0000-0040-000000000002");
    public static readonly Guid ActivityTypeChange = new("00000000-0000-0000-0040-000000000003");
    public static readonly Guid ActivityTypeProblem = new("00000000-0000-0000-0040-000000000004");
    public static readonly Guid ActivityTypeRitm = new("00000000-0000-0000-0040-000000000005");
    public static readonly Guid ActivityTypeProjet = new("00000000-0000-0000-0040-000000000006");
    public static readonly Guid ActivityTypeAmeliorationContinue = new("00000000-0000-0000-0040-000000000007");
    public static readonly Guid ActivityTypeSupport = new("00000000-0000-0000-0040-000000000008");
    public static readonly Guid ActivityTypeAstreinte = new("00000000-0000-0000-0040-000000000009");
    public static readonly Guid ActivityTypeReunion = new("00000000-0000-0000-0040-000000000010");
    public static readonly Guid ActivityTypeFormation = new("00000000-0000-0000-0040-000000000011");
    public static readonly Guid ActivityTypeVabe = new("00000000-0000-0000-0040-000000000012");
    public static readonly Guid ActivityTypeVsr = new("00000000-0000-0000-0040-000000000013");

    // Lot 3 — Jours fériés de démonstration (§22.2, §29.2)
    public static readonly Guid HolidayNouvelAn2024 = new("00000000-0000-0000-0041-000000000001");
    public static readonly Guid HolidayFeteTravail2024 = new("00000000-0000-0000-0041-000000000002");
    public static readonly Guid HolidayFeteNationale2024 = new("00000000-0000-0000-0041-000000000003");
    public static readonly Guid HolidayNoel2024 = new("00000000-0000-0000-0041-000000000004");
    public static readonly Guid HolidayNouvelAn2025 = new("00000000-0000-0000-0041-000000000005");

    // Lot 3 — Variation de capacité de démonstration (§10.5)
    public static readonly Guid CapacityPeriodPatel = new("00000000-0000-0000-0042-000000000001");

    // Lot 3 — Saisies de temps de démonstration (§19.1)
    public static readonly Guid TimeEntryBernardRun = new("00000000-0000-0000-0043-000000000001");
    public static readonly Guid TimeEntryBernardIncident = new("00000000-0000-0000-0043-000000000002");
    public static readonly Guid TimeEntryLegrandProjet = new("00000000-0000-0000-0043-000000000003");
    public static readonly Guid TimeEntryGeorgesChange2024 = new("00000000-0000-0000-0043-000000000004");
    public static readonly Guid TimeEntryGeorgesChange2025 = new("00000000-0000-0000-0043-000000000005");
    public static readonly Guid TimeEntryMishraFormation = new("00000000-0000-0000-0043-000000000006");

    // TimeEntryFinancialSnapshot réutilise l'Id de sa TimeEntry (clé partagée, cf.
    // TimeEntryService.ValorizeAsync) : pas de Guid de seed distinct.

    // Lot 3 — Absences de démonstration (§23)
    public static readonly Guid AbsenceBernardConge = new("00000000-0000-0000-0045-000000000001");
    public static readonly Guid AbsenceLegrandMaladie = new("00000000-0000-0000-0045-000000000002");
    public static readonly Guid AbsenceGeorgesRtt = new("00000000-0000-0000-0045-000000000003");
    public static readonly Guid AbsenceMishraFormationRefusee = new("00000000-0000-0000-0045-000000000004");

    // Lot 3 — Ressource inactive de démonstration (test de blocage de saisie, §19.4)
    public static readonly Guid ResourceInactiveDemo = new("00000000-0000-0000-0046-000000000001");

    // Lot 4 — Statuts de projet (§16.2, §30)
    public static readonly Guid ProjectStatusActif = new("00000000-0000-0000-0050-000000000001");
    public static readonly Guid ProjectStatusSuspendu = new("00000000-0000-0000-0050-000000000002");
    public static readonly Guid ProjectStatusTermine = new("00000000-0000-0000-0050-000000000003");
    public static readonly Guid ProjectStatusArchive = new("00000000-0000-0000-0050-000000000004");

    // Lot 4 — Types de jalon (§24.1)
    public static readonly Guid MilestoneTypeKickOff = new("00000000-0000-0000-0051-000000000001");
    public static readonly Guid MilestoneTypeArchitecture = new("00000000-0000-0000-0051-000000000002");
    public static readonly Guid MilestoneTypeVabe = new("00000000-0000-0000-0051-000000000003");
    public static readonly Guid MilestoneTypeVsr = new("00000000-0000-0000-0051-000000000004");
    public static readonly Guid MilestoneTypeGoDev = new("00000000-0000-0000-0051-000000000005");
    public static readonly Guid MilestoneTypeGoQual = new("00000000-0000-0000-0051-000000000006");
    public static readonly Guid MilestoneTypeGoVal = new("00000000-0000-0000-0051-000000000007");
    public static readonly Guid MilestoneTypeGoPprod = new("00000000-0000-0000-0051-000000000008");
    public static readonly Guid MilestoneTypeGoProd = new("00000000-0000-0000-0051-000000000009");
    public static readonly Guid MilestoneTypeCab = new("00000000-0000-0000-0051-000000000010");
    public static readonly Guid MilestoneTypeHypercare = new("00000000-0000-0000-0051-000000000011");

    // Lot 4 — Projets de démonstration (§16.2)
    public static readonly Guid ProjectMigrationElm = new("00000000-0000-0000-0052-000000000001");
    public static readonly Guid ProjectRefonteVtom = new("00000000-0000-0000-0052-000000000002");

    // Lot 4 — Participants (§17.2)
    public static readonly Guid ParticipantGeorgesOnElm = new("00000000-0000-0000-0053-000000000001");
    public static readonly Guid ParticipantLegrandOnElm = new("00000000-0000-0000-0053-000000000002");

    // Lot 4 — Versions de planning (§18.3)
    public static readonly Guid PlanVersionElmInitial = new("00000000-0000-0000-0054-000000000001");
    public static readonly Guid PlanVersionElmAjuste = new("00000000-0000-0000-0054-000000000002");

    // Lot 4 — Lignes hebdomadaires (§17.3)
    public static readonly Guid WeeklyPlanElmInitialGeorgesW1 = new("00000000-0000-0000-0055-000000000001");
    public static readonly Guid WeeklyPlanElmInitialLegrandW1 = new("00000000-0000-0000-0055-000000000002");
    public static readonly Guid WeeklyPlanElmAjusteGeorgesW1 = new("00000000-0000-0000-0055-000000000003");
    public static readonly Guid WeeklyPlanElmAjusteLegrandW1 = new("00000000-0000-0000-0055-000000000004");

    // Lot 4 — Jalons de démonstration (§24)
    public static readonly Guid MilestoneElmKickoff = new("00000000-0000-0000-0056-000000000001");
    public static readonly Guid MilestoneElmGoProd = new("00000000-0000-0000-0056-000000000002");
    public static readonly Guid MilestoneElmCab = new("00000000-0000-0000-0056-000000000003");

    // Lot 5 — Permission (§6.2)
    public static readonly Guid PermissionTimeEntryCorrection = new("00000000-0000-0000-0002-000000000002");

    // Lot 5 — Rallonge de commande de démonstration (§13.3)
    public static readonly Guid OrderExtensionDemo = new("00000000-0000-0000-0060-000000000001");

    // Lot 5 — Budget et version de démonstration (§14)
    public static readonly Guid BudgetMigrationElm = new("00000000-0000-0000-0061-000000000001");
    public static readonly Guid BudgetVersionMigrationElm = new("00000000-0000-0000-0062-000000000001");

    // Lot 5 — KPI de tableau de bord (§25.1, §25.2)
    public static readonly Guid DashboardKpiTempsSaisis = new("00000000-0000-0000-0063-000000000001");
    public static readonly Guid DashboardKpiCapaciteReelle = new("00000000-0000-0000-0063-000000000002");
    public static readonly Guid DashboardKpiTauxDisponibilite = new("00000000-0000-0000-0063-000000000003");
    public static readonly Guid DashboardKpiChargeRunHorsRun = new("00000000-0000-0000-0063-000000000004");
    public static readonly Guid DashboardKpiProjetsActifs = new("00000000-0000-0000-0063-000000000005");
    public static readonly Guid DashboardKpiJalonsEnRetard = new("00000000-0000-0000-0063-000000000006");
    public static readonly Guid DashboardKpiBudgetRestant = new("00000000-0000-0000-0063-000000000007");
    public static readonly Guid DashboardKpiDifferentielGlobal = new("00000000-0000-0000-0063-000000000008");

    // Lot 6 — Permissions (§6.2, §28.3)
    public static readonly Guid PermissionUserAdministration = new("00000000-0000-0000-0002-000000000003");
    public static readonly Guid PermissionTimeEntryRecalculation = new("00000000-0000-0000-0002-000000000004");
    public static readonly Guid PermissionImportExecute = new("00000000-0000-0000-0002-000000000005");
    public static readonly Guid PermissionAuditView = new("00000000-0000-0000-0002-000000000006");
    public static readonly Guid PermissionOrderReceiptOverride = new("00000000-0000-0000-0002-000000000007");

    // Lot 6 — Réception partielle de démonstration (§13, règle métier validée)
    public static readonly Guid OrderReceiptDemo = new("00000000-0000-0000-0070-000000000001");

    // Lot 6 — Import de démonstration (§27)
    public static readonly Guid ImportBatchDemo = new("00000000-0000-0000-0071-000000000001");
    public static readonly Guid ImportDiffDemoAdd = new("00000000-0000-0000-0072-000000000001");
    public static readonly Guid ImportDiffDemoUpdate = new("00000000-0000-0000-0072-000000000002");

    // Lot 6 — Journal d'audit de démonstration (§28.3)
    public static readonly Guid AuditLogDemoOrderExtension = new("00000000-0000-0000-0073-000000000001");
    public static readonly Guid AuditLogDemoImport = new("00000000-0000-0000-0073-000000000002");

    // Lot 8 — Technologies (docs/BACKLOG_METIER.md §5)
    public static readonly Guid TechnologyDotNet = new("00000000-0000-0000-0080-000000000001");
    public static readonly Guid TechnologyReact = new("00000000-0000-0000-0080-000000000002");
    public static readonly Guid TechnologyPostgresql = new("00000000-0000-0000-0080-000000000003");

    // Lot 8 — Clients (docs/BACKLOG_METIER.md §6)
    public static readonly Guid ClientDirectionProduction = new("00000000-0000-0000-0081-000000000001");
    public static readonly Guid ClientDirectionSupport = new("00000000-0000-0000-0081-000000000002");

    // Lot 8 — Types de projet (docs/BACKLOG_METIER.md §7)
    public static readonly Guid ProjectTypeForfait = new("00000000-0000-0000-0082-000000000001");
    public static readonly Guid ProjectTypeRegie = new("00000000-0000-0000-0082-000000000002");
    public static readonly Guid ProjectTypeInterne = new("00000000-0000-0000-0082-000000000003");

    // Lot 8 — Centres de coûts (docs/BACKLOG_METIER.md §8)
    public static readonly Guid CostCenterDsi = new("00000000-0000-0000-0083-000000000001");
    public static readonly Guid CostCenterProductionApplicative = new("00000000-0000-0000-0083-000000000002");

    // Lot 8 — Devises (docs/BACKLOG_METIER.md §9)
    public static readonly Guid CurrencyEur = new("00000000-0000-0000-0084-000000000001");
    public static readonly Guid CurrencyUsd = new("00000000-0000-0000-0084-000000000002");

    // Lot 10 — Projets supplémentaires (§16.2, §35 : jeu de démonstration enrichi ~8 projets)
    public static readonly Guid ProjectPortailRunServiceNow = new("00000000-0000-0000-0090-000000000001");
    public static readonly Guid ProjectSupportServiceNowN2 = new("00000000-0000-0000-0090-000000000002");
    public static readonly Guid ProjectObservabiliteVtom = new("00000000-0000-0000-0090-000000000003");
    public static readonly Guid ProjectRefontePortailElm = new("00000000-0000-0000-0090-000000000004");
    public static readonly Guid ProjectConsolidationReferentiels = new("00000000-0000-0000-0090-000000000005");
    public static readonly Guid ProjectArchiveLegacyVtom = new("00000000-0000-0000-0090-000000000006");

    // Lot 10 — Participants supplémentaires (§17.2)
    public static readonly Guid ParticipantLefevreOnVtomRefonte = new("00000000-0000-0000-0091-000000000001");
    public static readonly Guid ParticipantNguyenOnVtomRefonte = new("00000000-0000-0000-0091-000000000002");
    public static readonly Guid ParticipantManceronOnPortailRun = new("00000000-0000-0000-0091-000000000003");
    public static readonly Guid ParticipantNguyenOnPortailRun = new("00000000-0000-0000-0091-000000000004");
    public static readonly Guid ParticipantFocquenoeyOnSupportN2 = new("00000000-0000-0000-0091-000000000005");
    public static readonly Guid ParticipantPatelOnSupportN2 = new("00000000-0000-0000-0091-000000000006");
    public static readonly Guid ParticipantReauOnObservabilite = new("00000000-0000-0000-0091-000000000007");
    public static readonly Guid ParticipantDurandOnObservabilite = new("00000000-0000-0000-0091-000000000008");
    public static readonly Guid ParticipantLefevreOnRefontePortailElm = new("00000000-0000-0000-0091-000000000009");
    public static readonly Guid ParticipantCostaOnRefontePortailElm = new("00000000-0000-0000-0091-000000000010");
    public static readonly Guid ParticipantCostaOnConsolidation = new("00000000-0000-0000-0091-000000000011");
    public static readonly Guid ParticipantVermaOnConsolidation = new("00000000-0000-0000-0091-000000000012");
    public static readonly Guid ParticipantVermaOnArchiveLegacy = new("00000000-0000-0000-0091-000000000013");

    // Lot 10 — Versions de planning supplémentaires (§18.3)
    public static readonly Guid PlanVersionVtomRefonteInitial = new("00000000-0000-0000-0092-000000000001");
    public static readonly Guid PlanVersionPortailRunInitial = new("00000000-0000-0000-0092-000000000002");
    public static readonly Guid PlanVersionPortailRunAjuste = new("00000000-0000-0000-0092-000000000003");
    public static readonly Guid PlanVersionObservabiliteInitial = new("00000000-0000-0000-0092-000000000004");

    // Lot 10 — Lignes hebdomadaires supplémentaires (§17.3, §18.2)
    public static readonly Guid WeeklyPlanVtomRefonteInitialLefevreW1 = new("00000000-0000-0000-0093-000000000001");
    public static readonly Guid WeeklyPlanVtomRefonteInitialNguyenW1 = new("00000000-0000-0000-0093-000000000002");
    public static readonly Guid WeeklyPlanVtomRefonteInitialLefevreW2 = new("00000000-0000-0000-0093-000000000003");
    public static readonly Guid WeeklyPlanPortailRunInitialManceronW1 = new("00000000-0000-0000-0093-000000000004");
    public static readonly Guid WeeklyPlanPortailRunAjusteManceronW1 = new("00000000-0000-0000-0093-000000000005");
    public static readonly Guid WeeklyPlanObservabiliteInitialReauW1 = new("00000000-0000-0000-0093-000000000006");
    public static readonly Guid WeeklyPlanPortailRunInitialNguyenW1 = new("00000000-0000-0000-0093-000000000007");

    // Lot 10 — Jalons supplémentaires (§24)
    public static readonly Guid MilestoneVtomRefonteKickoff = new("00000000-0000-0000-0094-000000000001");
    public static readonly Guid MilestoneVtomRefonteGoProd = new("00000000-0000-0000-0094-000000000002");
    public static readonly Guid MilestonePortailRunKickoff = new("00000000-0000-0000-0094-000000000003");
    public static readonly Guid MilestonePortailRunGoQual = new("00000000-0000-0000-0094-000000000004");
    public static readonly Guid MilestoneSupportN2Kickoff = new("00000000-0000-0000-0094-000000000005");
    public static readonly Guid MilestoneObservabiliteArchitecture = new("00000000-0000-0000-0094-000000000006");
    public static readonly Guid MilestoneObservabiliteGoProd = new("00000000-0000-0000-0094-000000000007");
    public static readonly Guid MilestoneRefontePortailElmKickoff = new("00000000-0000-0000-0094-000000000008");
    public static readonly Guid MilestoneRefontePortailElmVabe = new("00000000-0000-0000-0094-000000000009");
    public static readonly Guid MilestoneConsolidationKickoff = new("00000000-0000-0000-0094-000000000010");
    public static readonly Guid MilestoneConsolidationGoProd = new("00000000-0000-0000-0094-000000000011");
    public static readonly Guid MilestoneArchiveLegacyCab = new("00000000-0000-0000-0094-000000000012");

    // Lot 10 — Budgets supplémentaires (§14, §17.4)
    public static readonly Guid BudgetVtomRefonte = new("00000000-0000-0000-0095-000000000001");
    public static readonly Guid BudgetPortailRunServiceNow = new("00000000-0000-0000-0095-000000000002");
    public static readonly Guid BudgetRefontePortailElm = new("00000000-0000-0000-0095-000000000003");
}
