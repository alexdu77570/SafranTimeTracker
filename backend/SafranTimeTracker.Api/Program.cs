using Microsoft.OpenApi;
using SafranTimeTracker.Application;
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

// Documentation API (CLAUDE.md §12) : OpenAPI/Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SAFRAN TIME TRACKER API",
        Version = "v1",
        Description = "Lot 1 - Référentiels (organisation, utilisateurs, ressources, applications, sociétés, commandes, paramètres)."
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
