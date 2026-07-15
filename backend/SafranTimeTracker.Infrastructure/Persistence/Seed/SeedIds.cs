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
}
