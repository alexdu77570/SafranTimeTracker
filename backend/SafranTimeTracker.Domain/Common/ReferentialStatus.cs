namespace SafranTimeTracker.Domain.Common;

/// <summary>
/// Statut générique Actif/Inactif utilisé par les référentiels qui ne nomment pas d'entité
/// "Status" dédiée au cahier des charges §30 (Department, Service, Team, Company,
/// ApplicationReference). Jamais de suppression physique : ce statut est le mécanisme de
/// désactivation (CLAUDE.md §7).
/// </summary>
public enum ReferentialStatus
{
    Actif,
    Inactif
}
