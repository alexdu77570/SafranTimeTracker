namespace SafranTimeTracker.Infrastructure.Persistence;

/// <summary>
/// Providers EF Core supportés (cf. docs/DATABASE.md §1). Le provider actif est résolu
/// uniquement via la configuration ("Database:Provider"), jamais codé en dur dans le code métier.
/// </summary>
public enum DatabaseProvider
{
    Sqlite,
    PostgreSql,
    SqlServer
}
