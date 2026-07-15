using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Domain.Applications;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Organisation;
using SafranTimeTracker.Domain.Orders;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Users;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Infrastructure.Persistence.Seed;

/// <summary>
/// Jeu de données de démonstration du Lot 1 : organisation minimale + les 13 utilisateurs et
/// ressources nommés au cahier des charges §5.4, une société interne, quelques applications et
/// une commande d'exemple, et les paramètres par défaut. Idempotent (HasData), sans secret ni
/// mot de passe réel (§5.4). Ne couvre pas le jeu de données volumétrique complet du §35, qui
/// dépend d'entités non encore créées (Project, TimeEntry, ...).
///
/// Toutes les dates/heures sont des constantes fixes : HasData doit rester strictement
/// déterministe d'une génération de migration à l'autre.
/// </summary>
internal static class Lot1Seed
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const string SeedAuthor = "system-seed";

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedReferenceTables(modelBuilder);
        SeedOrganisation(modelBuilder);
        SeedCompanyAndOrder(modelBuilder);
        SeedResourcesAndUsers(modelBuilder);
        SeedApplications(modelBuilder);
        SeedJoins(modelBuilder);
        SeedSettings(modelBuilder);
    }

    private static void SeedReferenceTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = SeedIds.RoleIngenieur, Code = "INGENIEUR", Libelle = "Ingénieur", Ordre = 1 },
            new Role { Id = SeedIds.RoleResponsableService, Code = "RESPONSABLE_SERVICE", Libelle = "Responsable Service", Ordre = 2 },
            new Role { Id = SeedIds.RoleResponsableDepartement, Code = "RESPONSABLE_DEPARTEMENT", Libelle = "Responsable Département", Ordre = 3 },
            new Role { Id = SeedIds.RoleAdministrateur, Code = "ADMINISTRATEUR", Libelle = "Administrateur", Ordre = 4 });

        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                Id = SeedIds.PermissionFinancialDataView,
                Code = "FINANCIAL_DATA_VIEW",
                Libelle = "Accès aux données financières",
                Description = "Autorise l'accès aux TJM, contrats, budgets, commandes, coûts et différentiels (cahier des charges §6.2)."
            });

        modelBuilder.Entity<OperationalRole>().HasData(
            new OperationalRole { Id = SeedIds.OpRoleRun, Code = "RUN", Libelle = "RUN" },
            new OperationalRole { Id = SeedIds.OpRoleBuild, Code = "BUILD", Libelle = "Build" },
            new OperationalRole { Id = SeedIds.OpRoleAmeliorationContinue, Code = "AMELIORATION_CONTINUE", Libelle = "Amélioration continue" },
            new OperationalRole { Id = SeedIds.OpRoleChefDeProjet, Code = "CHEF_DE_PROJET", Libelle = "Chef de Projet" },
            new OperationalRole { Id = SeedIds.OpRoleCoordinateurIt, Code = "COORDINATEUR_IT", Libelle = "Coordinateur IT" });

        modelBuilder.Entity<CompanyType>().HasData(
            new CompanyType { Id = SeedIds.CompanyTypeInterne, Code = "INTERNE", Libelle = "Interne" },
            new CompanyType { Id = SeedIds.CompanyTypeExterne, Code = "EXTERNE", Libelle = "Externe" });

        modelBuilder.Entity<OrderStatus>().HasData(
            new OrderStatus { Id = SeedIds.OrderStatusBrouillon, Code = "BROUILLON", Libelle = "Brouillon", Ordre = 1 },
            new OrderStatus { Id = SeedIds.OrderStatusActive, Code = "ACTIVE", Libelle = "Active", Ordre = 2 },
            new OrderStatus { Id = SeedIds.OrderStatusSuspendue, Code = "SUSPENDUE", Libelle = "Suspendue", Ordre = 3 },
            new OrderStatus { Id = SeedIds.OrderStatusConsommee, Code = "CONSOMMEE", Libelle = "Consommée", Ordre = 4 },
            new OrderStatus { Id = SeedIds.OrderStatusCloturee, Code = "CLOTUREE", Libelle = "Clôturée", Ordre = 5 });
    }

    private static void SeedOrganisation(ModelBuilder modelBuilder)
    {
        // Les responsables (Department/Service/Team) restent volontairement non renseignés dans
        // le seed : ils pointent vers Resource, qui est seedée après pour éviter toute dépendance
        // circulaire entre les tables lors de la génération de la migration.
        modelBuilder.Entity<Department>().HasData(new Department
        {
            Id = SeedIds.DepartmentDsi,
            Code = "DSI",
            Nom = "Direction des Systèmes d'Information",
            Statut = ReferentialStatus.Actif,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });

        modelBuilder.Entity<Service>().HasData(
            new Service { Id = SeedIds.ServiceProductionApplicative, Code = "PRODAPP", Nom = "Production Applicative", DepartmentId = SeedIds.DepartmentDsi, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Service { Id = SeedIds.ServiceRunMco, Code = "RUNMCO", Nom = "RUN / MCO", DepartmentId = SeedIds.DepartmentDsi, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Service { Id = SeedIds.ServiceSupport, Code = "SUPPORT", Nom = "Support", DepartmentId = SeedIds.DepartmentDsi, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Service { Id = SeedIds.ServiceProjets, Code = "PROJETS", Nom = "Projets", DepartmentId = SeedIds.DepartmentDsi, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });

        modelBuilder.Entity<Team>().HasData(
            new Team { Id = SeedIds.TeamRunA, Code = "RUN-A", Nom = "Équipe RUN A", ServiceId = SeedIds.ServiceRunMco, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Team { Id = SeedIds.TeamProjetsA, Code = "PROJ-A", Nom = "Équipe Projets A", ServiceId = SeedIds.ServiceProjets, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }

    private static void SeedCompanyAndOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().HasData(new Company
        {
            Id = SeedIds.CompanySafran,
            Nom = "SAFRAN",
            Code = "SAFRAN",
            CompanyTypeId = SeedIds.CompanyTypeInterne,
            Statut = ReferentialStatus.Actif,
            ContactPrincipal = "Direction DSI",
            EmailContact = "contact@safrantimetracker.local",
            Commentaire = "Société interne de référence (données de démonstration).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });

        modelBuilder.Entity<Order>().HasData(new Order
        {
            Id = SeedIds.OrderDemo,
            Reference = "CMD-2026-0001",
            Libelle = "Prestation cadre SAFRAN TIME TRACKER 2026",
            CompanyId = SeedIds.CompanySafran,
            BudgetFinancierInitial = 150000.00m,
            // Budget/jours/date "ajusté" reflètent la rallonge de démonstration du Lot 5
            // (Lot5Seed.OrderExtensionDemo, §13.3) : +20 000 €, +30 jours, échéance repoussée.
            BudgetFinancierAjuste = 170000.00m,
            BudgetJoursInitial = 200m,
            BudgetJoursAjuste = 230m,
            DateDebut = new DateOnly(2026, 1, 1),
            DateFinInitiale = new DateOnly(2026, 12, 31),
            DateFinAjustee = new DateOnly(2027, 3, 31),
            StatusId = SeedIds.OrderStatusActive,
            SeuilAlerte = 80m,
            Commentaire = "Commande de démonstration (Lot 1).",
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        });
    }

    private static void SeedResourcesAndUsers(ModelBuilder modelBuilder)
    {
        const decimal dailyCapacity = 7.75m;
        const decimal weeklyCapacity = 38.75m;
        var dateArrivee = new DateOnly(2021, 9, 1);

        // Ordre volontaire : la hiérarchie (ResponsableHierarchiqueId) référence uniquement des
        // ressources déjà listées plus haut dans ce tableau, pour rester insérable en une passe.
        modelBuilder.Entity<Resource>().HasData(
            new Resource { Id = SeedIds.ResourceBernard, Nom = "BERNARD", Prenom = "Alexandre", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceLegrand, Nom = "LEGRAND", Prenom = "Fabien", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceGeorges, Nom = "GEORGES", Prenom = "Thierry", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, ResponsableHierarchiqueId = SeedIds.ResourceLegrand, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceManceron, Nom = "MANCERON", Prenom = "Emmanuel", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceRunMco, ResponsableHierarchiqueId = SeedIds.ResourceLegrand, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceFocquenoey, Nom = "FOCQUENOEY", Prenom = "Thomas", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceSupport, ResponsableHierarchiqueId = SeedIds.ResourceLegrand, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceReau, Nom = "REAU", Prenom = "Alexandre", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProjets, ResponsableHierarchiqueId = SeedIds.ResourceLegrand, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceMishra, Nom = "MISHRA", Prenom = "Reena", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, ResponsableHierarchiqueId = SeedIds.ResourceGeorges, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceDurand, Nom = "DURAND", Prenom = "Camille", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceRunMco, TeamId = SeedIds.TeamRunA, ResponsableHierarchiqueId = SeedIds.ResourceManceron, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceNguyen, Nom = "NGUYEN", Prenom = "Minh", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceRunMco, TeamId = SeedIds.TeamRunA, ResponsableHierarchiqueId = SeedIds.ResourceManceron, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourcePatel, Nom = "PATEL", Prenom = "Aarav", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceSupport, ResponsableHierarchiqueId = SeedIds.ResourceFocquenoey, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceLefevre, Nom = "LEFEVRE", Prenom = "Julie", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProjets, TeamId = SeedIds.TeamProjetsA, ResponsableHierarchiqueId = SeedIds.ResourceReau, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceCosta, Nom = "COSTA", Prenom = "Marco", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProjets, TeamId = SeedIds.TeamProjetsA, ResponsableHierarchiqueId = SeedIds.ResourceReau, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new Resource { Id = SeedIds.ResourceVerma, Nom = "VERMA", Prenom = "Priya", DepartmentId = SeedIds.DepartmentDsi, ServiceId = SeedIds.ServiceProductionApplicative, ResponsableHierarchiqueId = SeedIds.ResourceGeorges, CompanyId = SeedIds.CompanySafran, DailyCapacity = dailyCapacity, WeeklyCapacity = weeklyCapacity, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });

        modelBuilder.Entity<User>().HasData(
            User(SeedIds.UserBernard, "BERNARD", "Alexandre", "s636140", SeedIds.RoleAdministrateur, SeedIds.ResourceBernard, dateArrivee, accesGlobal: true),
            User(SeedIds.UserLegrand, "LEGRAND", "Fabien", "flegrand", SeedIds.RoleResponsableDepartement, SeedIds.ResourceLegrand, dateArrivee, accesGlobal: true),
            User(SeedIds.UserGeorges, "GEORGES", "Thierry", "tgeorges", SeedIds.RoleResponsableService, SeedIds.ResourceGeorges, dateArrivee),
            User(SeedIds.UserManceron, "MANCERON", "Emmanuel", "emanceron", SeedIds.RoleResponsableService, SeedIds.ResourceManceron, dateArrivee),
            User(SeedIds.UserFocquenoey, "FOCQUENOEY", "Thomas", "tfocquenoey", SeedIds.RoleResponsableService, SeedIds.ResourceFocquenoey, dateArrivee),
            User(SeedIds.UserReau, "REAU", "Alexandre", "areau", SeedIds.RoleResponsableService, SeedIds.ResourceReau, dateArrivee),
            User(SeedIds.UserMishra, "MISHRA", "Reena", "rmishra", SeedIds.RoleIngenieur, SeedIds.ResourceMishra, dateArrivee),
            User(SeedIds.UserDurand, "DURAND", "Camille", "cdurand", SeedIds.RoleIngenieur, SeedIds.ResourceDurand, dateArrivee),
            User(SeedIds.UserNguyen, "NGUYEN", "Minh", "mnguyen", SeedIds.RoleIngenieur, SeedIds.ResourceNguyen, dateArrivee),
            User(SeedIds.UserPatel, "PATEL", "Aarav", "apatel", SeedIds.RoleIngenieur, SeedIds.ResourcePatel, dateArrivee),
            User(SeedIds.UserLefevre, "LEFEVRE", "Julie", "jlefevre", SeedIds.RoleIngenieur, SeedIds.ResourceLefevre, dateArrivee),
            User(SeedIds.UserCosta, "COSTA", "Marco", "mcosta", SeedIds.RoleIngenieur, SeedIds.ResourceCosta, dateArrivee),
            User(SeedIds.UserVerma, "VERMA", "Priya", "pverma", SeedIds.RoleIngenieur, SeedIds.ResourceVerma, dateArrivee));

        User User(Guid id, string nom, string prenom, string identifiant, Guid roleId, Guid resourceId, DateOnly arrivee, bool accesGlobal = false) => new()
        {
            Id = id,
            Nom = nom,
            Prenom = prenom,
            Identifiant = identifiant,
            Email = $"{identifiant}@safrantimetracker.local",
            Statut = ReferentialStatus.Actif,
            DateArrivee = arrivee,
            ResourceId = resourceId,
            RoleId = roleId,
            AccesGlobal = accesGlobal,
            CreatedAt = SeedTimestamp,
            CreatedBy = SeedAuthor
        };
    }

    private static void SeedApplications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationReference>().HasData(
            new ApplicationReference { Id = SeedIds.AppIbmElm, Nom = "IBM ELM", Code = "IBMELM", ServiceId = SeedIds.ServiceProductionApplicative, Criticite = ApplicationCriticality.Elevee, ResponsableId = SeedIds.ResourceGeorges, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new ApplicationReference { Id = SeedIds.AppVtom, Nom = "VTOM", Code = "VTOM", ServiceId = SeedIds.ServiceRunMco, TeamId = SeedIds.TeamRunA, Criticite = ApplicationCriticality.Moyenne, ResponsableId = SeedIds.ResourceManceron, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor },
            new ApplicationReference { Id = SeedIds.AppServiceNow, Nom = "ServiceNow", Code = "SNOW", ServiceId = SeedIds.ServiceSupport, Criticite = ApplicationCriticality.Critique, ResponsableId = SeedIds.ResourceFocquenoey, Statut = ReferentialStatus.Actif, CreatedAt = SeedTimestamp, CreatedBy = SeedAuthor });
    }

    private static void SeedJoins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermission>().HasData(
            new UserPermission { UserId = SeedIds.UserBernard, PermissionId = SeedIds.PermissionFinancialDataView, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor },
            new UserPermission { UserId = SeedIds.UserLegrand, PermissionId = SeedIds.PermissionFinancialDataView, GrantedAt = SeedTimestamp, GrantedBy = SeedAuthor });

        modelBuilder.Entity<OrderAuthorizedResource>().HasData(
            new OrderAuthorizedResource { OrderId = SeedIds.OrderDemo, ResourceId = SeedIds.ResourceGeorges },
            new OrderAuthorizedResource { OrderId = SeedIds.OrderDemo, ResourceId = SeedIds.ResourceMishra });

        modelBuilder.Entity<ResourceOperationalRole>().HasData(
            new ResourceOperationalRole { ResourceId = SeedIds.ResourceGeorges, OperationalRoleId = SeedIds.OpRoleChefDeProjet },
            new ResourceOperationalRole { ResourceId = SeedIds.ResourceMishra, OperationalRoleId = SeedIds.OpRoleRun },
            new ResourceOperationalRole { ResourceId = SeedIds.ResourceDurand, OperationalRoleId = SeedIds.OpRoleRun },
            new ResourceOperationalRole { ResourceId = SeedIds.ResourceLefevre, OperationalRoleId = SeedIds.OpRoleChefDeProjet });
    }

    private static void SeedSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SettingsEntity>().HasData(new SettingsEntity
        {
            Id = SeedIds.SettingsSingleton,
            HeuresParJour = 7.75m,
            JoursOuvresParSemaine = 5,
            PaysParDefaut = "France",
            DeviseParDefaut = "EUR",
            SeuilSurcharge = 100m,
            SeuilSousCharge = 50m,
            SeuilAlerteBudget = 80m,
            SeuilAlerteCommande = 80m,
            DelaiModificationTempsJours = 5,
            ActivationValidationAbsences = true,
            AutorisationSaisieSansValorisation = false,
            UpdatedAt = SeedTimestamp,
            UpdatedBy = SeedAuthor
        });
    }
}
