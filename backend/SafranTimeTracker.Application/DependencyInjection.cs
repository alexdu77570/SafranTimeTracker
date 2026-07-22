using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SafranTimeTracker.Application.Absences.Services;
using SafranTimeTracker.Application.Applications.Services;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Budgets.Services;
using SafranTimeTracker.Application.Capacity.Services;
using SafranTimeTracker.Application.Clients.Services;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Companies.Services;
using SafranTimeTracker.Application.Currencies.Services;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Application.Imports;
using SafranTimeTracker.Application.Imports.Adapters;
using SafranTimeTracker.Application.Imports.Services;
using SafranTimeTracker.Application.Milestones.Services;
using SafranTimeTracker.Application.Organisation.Services;
using SafranTimeTracker.Application.Orders.Services;
using SafranTimeTracker.Application.Projects.Services;
using SafranTimeTracker.Application.Reporting.Services;
using SafranTimeTracker.Application.Resources.Services;
using SafranTimeTracker.Application.Settings.Services;
using SafranTimeTracker.Application.Technologies.Services;
using SafranTimeTracker.Application.TimeTracking.Services;
using SafranTimeTracker.Application.Users.Services;

namespace SafranTimeTracker.Application;

/// <summary>Point d'entrée unique d'enregistrement des services de la couche Application dans le conteneur DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<DepartmentService>();
        services.AddScoped<ServiceService>();
        services.AddScoped<TeamService>();
        services.AddScoped<CompanyService>();
        services.AddScoped<OrderService>();
        services.AddScoped<OrderStatusService>();
        services.AddScoped<OrderReceiptService>();
        services.AddScoped<ResourceService>();
        services.AddScoped<UserService>();
        services.AddScoped<PermissionService>();
        services.AddScoped<PermissionResolutionService>();
        services.AddScoped<ApplicationReferenceService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<ResourceTjmHistoryService>();
        services.AddScoped<CompanyContractHistoryService>();
        services.AddScoped<ResourceCompanyAssignmentService>();
        services.AddScoped<FinancialCalculationService>();
        services.AddScoped<ITimeEntryRevaluationService, TimeEntryRevaluationService>();
        services.AddScoped<ActivityTypeService>();
        services.AddScoped<TimeEntryService>();
        services.AddScoped<AbsenceService>();
        services.AddScoped<ResourceCapacityPeriodService>();
        services.AddScoped<HolidayCalendarService>();
        services.AddScoped<AvailabilityService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<ProjectStatusService>();
        services.AddScoped<ProjectParticipantService>();
        services.AddScoped<ProjectPlanningService>();
        services.AddScoped<MilestoneTypeService>();
        services.AddScoped<MilestoneService>();
        services.AddScoped<OrderExtensionService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<ReportingService>();
        services.AddScoped<ExportService>();
        services.AddScoped<DashboardKpiService>();

        // Audit (Lot 6, §28.3)
        services.AddScoped<AuditService>();
        services.AddScoped<AuditLogService>();

        // Référentiels et administration (Lot 8, docs/BACKLOG_METIER.md §5-9)
        services.AddScoped<TechnologyService>();
        services.AddScoped<ClientService>();
        services.AddScoped<ProjectTypeService>();
        services.AddScoped<CostCenterService>();
        services.AddScoped<CurrencyService>();

        // Imports (Lot 6, §27) : un adaptateur par type importable, résolus collectivement par
        // ImportService via IEnumerable<IImportAdapter> — même principe d'enregistrement multiple
        // que les IEntityTypeConfiguration<T> côté Infrastructure.
        services.AddScoped<ImportService>();
        services.AddScoped<IImportAdapter, ResourceImportAdapter>();
        services.AddScoped<IImportAdapter, ApplicationReferenceImportAdapter>();
        services.AddScoped<IImportAdapter, OrganisationImportAdapter>();
        services.AddScoped<IImportAdapter, AbsenceImportAdapter>();
        services.AddScoped<IImportAdapter, CompanyImportAdapter>();
        services.AddScoped<IImportAdapter, ResourceTjmHistoryImportAdapter>();
        services.AddScoped<IImportAdapter, CompanyContractHistoryImportAdapter>();
        services.AddScoped<IImportAdapter, ResourceCompanyAssignmentImportAdapter>();
        services.AddScoped<IImportAdapter, OrderImportAdapter>();
        services.AddScoped<IImportAdapter, ProjectImportAdapter>();
        services.AddScoped<IImportAdapter, BudgetImportAdapter>();
        services.AddScoped<IImportAdapter, UserImportAdapter>();
        services.AddScoped<IImportAdapter, TimeEntryImportAdapter>();
        services.AddScoped<IImportAdapter, MilestoneImportAdapter>();
        services.AddScoped<IImportAdapter, ProjectParticipantImportAdapter>();
        services.AddScoped<IImportAdapter, PlanningImportAdapter>();

        return services;
    }
}
