namespace SafranTimeTracker.Application.Common.Dtos;

/// <summary>Enveloppe de pagination commune à toutes les listes (CLAUDE.md §12).</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
