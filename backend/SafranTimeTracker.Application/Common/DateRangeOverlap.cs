namespace SafranTimeTracker.Application.Common;

/// <summary>
/// Test de chevauchement entre deux périodes, partagé par les historiques financiers
/// (ResourceTjmHistory, CompanyContractHistory, ResourceCompanyAssignment — docs/DATABASE.md §5 :
/// même règle d'intégrité pour les trois). Une période sans date de fin est considérée ouverte
/// (non bornée dans le futur).
/// </summary>
public static class DateRangeOverlap
{
    public static bool Overlaps(DateOnly aStart, DateOnly? aEnd, DateOnly bStart, DateOnly? bEnd)
    {
        var aEndOrMax = aEnd ?? DateOnly.MaxValue;
        var bEndOrMax = bEnd ?? DateOnly.MaxValue;
        return aStart <= bEndOrMax && bStart <= aEndOrMax;
    }
}
