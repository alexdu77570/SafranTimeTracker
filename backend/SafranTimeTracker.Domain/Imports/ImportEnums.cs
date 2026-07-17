namespace SafranTimeTracker.Domain.Imports;

/// <summary>Les 16 types importables (cahier des charges §27.1). "Organisation" couvre les trois
/// niveaux Department/Service/Team en une seule liste d'import (une colonne "Niveau" par ligne),
/// conformément à leur regroupement en un seul item du §27.1.</summary>
public enum ImportEntityType
{
    Resources,
    Users,
    Companies,
    ResourceCompanyAssignments,
    ResourceTjmHistories,
    CompanyContractHistories,
    Projects,
    Budgets,
    Orders,
    ProjectParticipants,
    Plannings,
    TimeEntries,
    Absences,
    Milestones,
    Applications,
    Organisation
}

/// <summary>§27.2. Le mode Complet ne supprime jamais physiquement les entités absentes du
/// fichier : il les désactive/archive (CLAUDE.md §7), et n'est proposé que pour les types portant
/// un statut d'archivage (voir <c>IImportAdapter.SupportsCompleteMode</c>).</summary>
public enum ImportMode
{
    Ajout,
    MiseAJour,
    Complet
}

/// <summary>Cycle de vie d'un <see cref="ImportBatch"/> : Previsualise/Simule ne persistent rien
/// (§27.4 "ne pas modifier les données avant confirmation") ; seul Confirme exécute réellement
/// l'import et écrit ImportDiff.</summary>
public enum ImportBatchStatus
{
    Previsualise,
    Simule,
    Confirme,
    Echoue
}

public enum ImportDiffType
{
    Ajout,
    Modification,
    Suppression,
    Inchange,
    Erreur
}
