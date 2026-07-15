using Microsoft.OpenApi;
using SafranTimeTracker.Api.Middleware;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Journalisation structurée (CLAUDE.md §15) : configuration lue depuis appsettings (section "Serilog").
// Pas de "bootstrap logger" à deux phases (Log.Logger statique) : ce motif entre en conflit avec
// WebApplicationFactory, qui réexécute ce fichier pour chaque instance d'hôte de test
// ("The logger is already frozen").
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHealthChecks();

// Identité de démonstration (CLAUDE.md §17, docs/ARCHITECTURE.md §4) : seul ce point
// d'enregistrement connaît DemoCurrentUserProvider. Le remplacer par un provider LDAP/OIDC futur
// ne touche ni les contrôleurs, ni les services applicatifs, ni les règles d'autorisation, qui ne
// dépendent que de l'interface ICurrentUser.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, DemoCurrentUserProvider>();

// Conflits métier (chevauchement de périodes, concurrence optimiste) -> 409 (CLAUDE.md §10, §12).
builder.Services.AddExceptionHandler<BusinessConflictExceptionHandler>();
builder.Services.AddProblemDetails();

// Documentation API (CLAUDE.md §12) : OpenAPI/Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SAFRAN TIME TRACKER API",
        Version = "v1",
        Description = "Lot 5 - Référentiels + modèle financier + temps et capacité + projets + budgets, rallonges, tableaux de bord, reporting et exports. " +
            $"Les endpoints financiers exigent la permission FINANCIAL_DATA_VIEW, résolue via l'en-tête de démonstration '{DemoCurrentUserProvider.DemoUserHeaderName}' (ex. 's636140')."
    });
});

// Origines CORS explicites par environnement (CLAUDE.md §17) : jamais de wildcard hors développement.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Rend la classe Program accessible à WebApplicationFactory<Program> pour les tests d'intégration
// (SafranTimeTracker.Tests) sans exposer de fonctionnalité métier.
public partial class Program
{
}
