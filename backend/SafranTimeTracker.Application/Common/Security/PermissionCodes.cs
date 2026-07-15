namespace SafranTimeTracker.Application.Common.Security;

/// <summary>Codes de permission stables (doivent correspondre à Permission.Code, cahier des charges §6.2).</summary>
public static class PermissionCodes
{
    public const string FinancialDataView = "FINANCIAL_DATA_VIEW";

    /// <summary>Autorise une saisie de temps sur une commande clôturée (cahier des charges
    /// §13.4 : "doit être bloquée, sauf droit de correction").</summary>
    public const string TimeEntryCorrection = "TIME_ENTRY_CORRECTION";
}
