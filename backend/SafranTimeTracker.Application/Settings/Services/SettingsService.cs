using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Settings.Dtos;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Settings.Services;

/// <summary>Ligne singleton : pas de liste, pas de création (cahier des charges §28.2).</summary>
public class SettingsService(IRepository<SettingsEntity> repository)
{
    public Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default) =>
        repository.Query().ProjectToType<SettingsDto>().FirstAsync(cancellationToken);

    public async Task<SettingsDto> UpdateAsync(SettingsUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var singletonId = await repository.Query().Select(s => s.Id).FirstAsync(cancellationToken);
        var entity = await repository.GetByIdAsync(singletonId, cancellationToken)
            ?? throw new InvalidOperationException("La ligne de paramètres singleton est introuvable.");

        request.Adapt(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentActor.PlaceholderIdentifier;

        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<SettingsDto>();
    }
}
