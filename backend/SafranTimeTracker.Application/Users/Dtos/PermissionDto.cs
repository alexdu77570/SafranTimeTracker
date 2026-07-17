namespace SafranTimeTracker.Application.Users.Dtos;

/// <summary>
/// Référentiel en lecture seule des permissions complémentaires (CLAUDE.md §17, PermissionCodes).
/// Seul point de résolution GUID → Code exposé par l'API : UserDto.PermissionIds ne porte que des
/// GUIDs, et le frontend a besoin des codes pour évaluer PermissionGuard (Lot 7).
/// </summary>
public class PermissionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
}
