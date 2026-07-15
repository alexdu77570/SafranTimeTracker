namespace SafranTimeTracker.Domain.Absences;

/// <summary>
/// Types d'absence réels (cahier des charges §22.3, sous-ensemble). Enum C# et non une table
/// référentielle : contrairement à ActivityType, "AbsenceType" ne figure pas dans la liste
/// d'entités minimum du §30. "Disponible" (état par défaut, pas une absence) et "Télétravail"
/// (§22.3 : "n'est pas une absence et ne réduit pas la capacité par défaut") sont explicitement
/// exclus de ce type — ce sont des concepts de la page Disponibilités (§22), pas de l'entité
/// Absence (§23).
/// </summary>
public enum AbsenceType
{
    Conge,
    Rtt,
    Maladie,
    Formation,
    Deplacement,
    Indisponible
}
