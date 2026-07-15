namespace SafranTimeTracker.Application.Common.Dtos;

/// <summary>Paramètres de pagination communs à toutes les listes (CLAUDE.md §12).</summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is > 0 and <= MaxPageSize ? value : _pageSize;
    }
}
