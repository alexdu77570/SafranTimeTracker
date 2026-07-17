namespace SafranTimeTracker.Application.Audit;

/// <summary>
/// Codes d'action stables du journal d'audit (cahier des charges §28.3). Chaîne extensible plutôt
/// qu'un enum fermé (CLAUDE.md §5 : la liste du §28.3 est un minimum, pas un catalogue clos) — même
/// principe que <see cref="Common.Security.PermissionCodes"/>.
/// </summary>
public static class AuditActions
{
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string LogicalDelete = "LOGICAL_DELETE";
    public const string Archive = "ARCHIVE";
    public const string Reactivate = "REACTIVATE";
    public const string StatusChange = "STATUS_CHANGE";
    public const string RoleChange = "ROLE_CHANGE";
    public const string PermissionGranted = "PERMISSION_GRANTED";
    public const string PermissionRevoked = "PERMISSION_REVOKED";
    public const string AdminGranted = "ADMIN_GRANTED";
    public const string AdminRevoked = "ADMIN_REVOKED";
    public const string Extension = "EXTENSION";
    public const string Receipt = "RECEIPT";
    public const string Recalculation = "RECALCULATION";
    public const string Import = "IMPORT";
    public const string ImportComparison = "IMPORT_COMPARISON";
    public const string ExportFinancial = "EXPORT_FINANCIAL";
}
