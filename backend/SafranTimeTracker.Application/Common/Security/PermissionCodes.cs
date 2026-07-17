namespace SafranTimeTracker.Application.Common.Security;

/// <summary>Codes de permission stables (doivent correspondre à Permission.Code, cahier des charges §6.2).</summary>
public static class PermissionCodes
{
    public const string FinancialDataView = "FINANCIAL_DATA_VIEW";

    /// <summary>Autorise une saisie de temps sur une commande clôturée (cahier des charges
    /// §13.4 : "doit être bloquée, sauf droit de correction").</summary>
    public const string TimeEntryCorrection = "TIME_ENTRY_CORRECTION";

    /// <summary>Autorise le recalcul explicite d'une saisie déjà valorisée (cahier des charges
    /// §19.6 : "permission dédiée"), distincte de <see cref="TimeEntryCorrection"/>.</summary>
    public const string TimeEntryRecalculation = "TIME_ENTRY_RECALCULATION";

    /// <summary>Autorise la modification/désactivation d'un utilisateur, le changement de rôle et
    /// l'octroi/retrait de permission (cahier des charges §28.3, CLAUDE.md §17 : "seul un
    /// Administrateur peut attribuer/retirer le rôle Administrateur ou les permissions
    /// financières").</summary>
    public const string UserAdministration = "USER_ADMINISTRATION";

    /// <summary>Autorise l'exécution d'un import (cahier des charges §27). Un import portant sur
    /// un type financier (TJM, contrats, rattachements société, budgets, commandes) exige en plus
    /// <see cref="FinancialDataView"/>, même principe que les autres endpoints financiers.</summary>
    public const string ImportExecute = "IMPORT_EXECUTE";

    /// <summary>Autorise la consultation du journal d'audit (cahier des charges §28.1).</summary>
    public const string AuditView = "AUDIT_VIEW";

    /// <summary>Autorise l'enregistrement d'une réception de commande (<c>OrderReceipt</c>, Lot 6)
    /// au-delà du reste réceptionnable — même principe que <see cref="TimeEntryCorrection"/> :
    /// bloqué par défaut (409), débloqué uniquement par une permission dédiée explicite.</summary>
    public const string OrderReceiptOverride = "ORDER_RECEIPT_OVERRIDE";
}
