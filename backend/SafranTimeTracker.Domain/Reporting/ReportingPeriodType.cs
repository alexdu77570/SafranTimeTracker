namespace SafranTimeTracker.Domain.Reporting;

/// <summary>Granularité de période utilisée par les filtres de reporting (§21.1 "période") et les
/// exports (§26.3). <c>Personnalisee</c> exige des bornes explicites (from/to) ; les autres valeurs
/// sont résolues depuis une date de référence par <c>ReportingPeriodResolver</c> (fonction pure,
/// testée sans base de données, CLAUDE.md §14).</summary>
public enum ReportingPeriodType
{
    Jour,
    Semaine,
    Mois,
    Annee,
    Personnalisee
}
