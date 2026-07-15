namespace SafranTimeTracker.Application.Common.Exceptions;

/// <summary>
/// Conflit métier (chevauchement de périodes, période déjà close, etc.) — traduit en 409 par le
/// middleware central (CLAUDE.md §10, §12). Ce n'est jamais une erreur de format ou de saisie
/// (400) : les données sont valides individuellement, c'est leur combinaison avec l'état existant
/// qui est en conflit.
/// </summary>
public class BusinessConflictException(string message) : Exception(message);
