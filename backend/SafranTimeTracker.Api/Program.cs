using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using SafranTimeTracker.Api.Middleware;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application;
using SafranTimeTracker.Application.Audit.Services;
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

// Identité de démonstration sessionnée (CLAUDE.md §17, docs/ARCHITECTURE.md §4, Lot 13) : seuls ces
// deux points d'enregistrement connaissent DemoAuthenticationProvider/DemoCurrentUserProvider. Les
// remplacer par un provider LDAP/OIDC futur ne touche ni les contrôleurs, ni les services
// applicatifs, ni les règles d'autorisation, qui ne dépendent que d'ICurrentUser/IAuthenticationProvider.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, DemoCurrentUserProvider>();
builder.Services.AddScoped<IAuthenticationProvider, DemoAuthenticationProvider>();
builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection(AuthenticationOptions.SectionName));

// Contexte technique d'audit (CLAUDE.md §17, même principe que ICurrentUser) : seul ce point
// d'enregistrement connaît HttpAuditContextAccessor ; AuditService (Application) ne dépend que de
// l'abstraction IAuditContextAccessor.
builder.Services.AddScoped<IAuditContextAccessor, HttpAuditContextAccessor>();

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
// AllowCredentials (Lot 13) : le cookie de session simulée doit être envoyé/reçu par le navigateur ;
// compatible avec WithOrigins car jamais combiné à AllowAnyOrigin (le protocole CORS interdit cette
// combinaison, pas celle avec des origines explicites).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Limitation de débit native .NET (Lot 13, sécurisation API) : ciblée sur la création de session,
// aucune dépendance externe. Un dépassement répond 429 plutôt que de mettre en file d'attente une
// création de session (QueueLimit = 0 : refuser tout de suite, jamais retarder silencieusement).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
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
else
{
    // HSTS (Lot 13, sécurisation API) : jamais en Development, où le certificat est auto-signé.
    app.UseHsts();
}

app.UseHttpsRedirection();

// En-têtes de sécurité (Lot 13, cahier des charges §7 "revue de sécurité complète") : l'API ne sert
// que du JSON, jamais de HTML — CSP la plus stricte possible (default-src 'none').
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'none'");
    await next();
});

app.UseCors("Default");
app.UseRateLimiter();

// Résolution d'identité (Lot 13) : une fois par requête, avant les contrôleurs — voir
// IdentityResolutionMiddleware pour le détail (session cookie, repli Development/Test sur l'en-tête
// X-Demo-User).
app.UseMiddleware<IdentityResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Rend la classe Program accessible à WebApplicationFactory<Program> pour les tests d'intégration
// (SafranTimeTracker.Tests) sans exposer de fonctionnalité métier.
public partial class Program
{
}
