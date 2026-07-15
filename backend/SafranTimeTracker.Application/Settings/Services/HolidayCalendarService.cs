using Mapster;
using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Settings.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Settings;

namespace SafranTimeTracker.Application.Settings.Services;

public class HolidayCalendarService(IRepository<HolidayCalendar> repository, ICurrentUser currentUser)
{
    public async Task<PagedResult<HolidayCalendarDto>> GetListAsync(
        PaginationQuery pagination, int? year, CancellationToken cancellationToken = default)
    {
        var query = repository.Query();
        if (year is not null)
        {
            query = query.Where(h => h.Date.Year == year);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(h => h.Date)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToType<HolidayCalendarDto>()
            .ToListAsync(cancellationToken);

        return new PagedResult<HolidayCalendarDto> { Items = items, Page = pagination.Page, PageSize = pagination.PageSize, TotalCount = totalCount };
    }

    public Task<HolidayCalendarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.Query().Where(h => h.Id == id).ProjectToType<HolidayCalendarDto>().FirstOrDefaultAsync(cancellationToken);

    public async Task<HolidayCalendarDto> CreateAsync(HolidayCalendarCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = request.Adapt<HolidayCalendar>();
        entity.Id = Guid.NewGuid();
        entity.Statut = ReferentialStatus.Actif;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = currentUser.Identifier;

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.Adapt<HolidayCalendarDto>();
    }
}
