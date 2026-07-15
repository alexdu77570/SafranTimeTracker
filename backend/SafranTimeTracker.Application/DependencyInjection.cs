using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SafranTimeTracker.Application.Applications.Services;
using SafranTimeTracker.Application.Companies.Services;
using SafranTimeTracker.Application.Financial.Services;
using SafranTimeTracker.Application.Organisation.Services;
using SafranTimeTracker.Application.Orders.Services;
using SafranTimeTracker.Application.Resources.Services;
using SafranTimeTracker.Application.Settings.Services;
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
        services.AddScoped<ResourceService>();
        services.AddScoped<UserService>();
        services.AddScoped<ApplicationReferenceService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<ResourceTjmHistoryService>();
        services.AddScoped<CompanyContractHistoryService>();
        services.AddScoped<ResourceCompanyAssignmentService>();
        services.AddScoped<FinancialCalculationService>();

        return services;
    }
}
