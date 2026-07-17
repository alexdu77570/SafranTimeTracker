using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Audit;
using SafranTimeTracker.Application.Audit.Services;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Settings.Dtos;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Settings.Services;

/// <summary>Ligne singleton : pas de liste, pas de création (cahier des charges §28.2).
/// Modification auditée (§28.3).</summary>
public class SettingsService(IRepository<SettingsEntity> repository, AuditService auditService)
{
    public Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default) =>
        repository.Query().ProjectToType<SettingsDto>().FirstAsync(cancellationToken);

    public async Task<SettingsDto> UpdateAsync(SettingsUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var singletonId = await repository.Query().Select(s => s.Id).FirstAsync(cancellationToken);
        var entity = await repository.GetByIdAsync(singletonId, cancellationToken)
            ?? throw new InvalidOperationException("La ligne de paramètres singleton est introuvable.");

        var oldValue = entity.Adapt<SettingsDto>();
        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentActor.PlaceholderIdentifier;

        await auditService.RecordAsync(
            AuditActions.Update, nameof(SettingsEntity), entity.Id, oldValue, entity.Adapt<SettingsDto>(), cancellationToken: cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<SettingsDto>();
    }
}
