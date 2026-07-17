using System.Text.Json;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Domain.Auditing;

namespace SafranTimeTracker.Application.Audit.Services;

/// <summary>
/// Point d'écriture unique du journal d'audit (cahier des charges §28.3, CLAUDE.md §16).
/// <see cref="RecordAsync"/> ajoute l'entrée au suivi EF Core mais n'appelle jamais
/// <c>SaveChangesAsync</c> lui-même : c'est l'appel de <c>SaveChangesAsync</c> déjà fait par le
/// service métier appelant (sur le changement qu'il décrit) qui persiste les deux ensembles de
/// modifications dans une seule transaction EF Core — "même transaction que le changement métier"
/// (§28.3) sans introduire de nouvelle abstraction de type Unit of Work. Un appelant qui invoque
/// <see cref="RecordAsync"/> sans jamais appeler <c>SaveChangesAsync</c> ensuite ne persiste rien
/// (même règle que <c>IRepository&lt;T&gt;.AddAsync</c>, docs CLAUDE.md §11).
/// </summary>
public class AuditService(IRepository<AuditLog> repository, ICurrentUser currentUser, IAuditContextAccessor contextAccessor)
{
    public Task RecordAsync(
        string action, string entityType, Guid? entityId, object? oldValue, object? newValue,
        string? reason = null, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLog
        {
            Id = Guid.NewGuid(),
            Author = currentUser.Identifier,
            Timestamp = DateTime.UtcNow,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            Reason = reason,
            TechnicalContext = contextAccessor.TechnicalContext
        };

        return repository.AddAsync(entry, cancellationToken);
    }
}
