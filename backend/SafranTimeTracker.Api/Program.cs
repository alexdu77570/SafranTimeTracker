using Microsoft.OpenApi;
using SafranTimeTracker.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Journalisation structurée (CLAUDE.md §15) : configuration lue depuis appsettings (section "Serilog"),
// avec repli console garanti même si la configuration est absente ou invalide au tout premier démarrage.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

// Documentation API (CLAUDE.md §12) : OpenAPI/Swagger, aucun endpoint métier documenté en Lot 0.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SAFRAN TIME TRACKER API",
        Version = "v1",
        Description = "Lot 0 - Fondations techniques. Aucun endpoint métier n'est encore exposé."
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
