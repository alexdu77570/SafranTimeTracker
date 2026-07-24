using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Api.Security;
using SafranTimeTracker.Application.Common.Security;
using SafranTimeTracker.Application.Reporting.Dtos;
using SafranTimeTracker.Application.Reporting.Services;
using SafranTimeTracker.Domain.Reporting;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>
/// Cahier des charges §21 (Charges), §25 (Tableau de bord), §26 (Reporting et exports). Les
/// rapports financiers (§26.2) sont gardés par FINANCIAL_DATA_VIEW au niveau action (pas au niveau
/// contrôleur : /charges, /dashboard, /operational et les références liées ne portent aucune
/// donnée financière). Toute agrégation vit dans ReportingService/ExportService, jamais ici
/// (contrôleur mince, CLAUDE.md §10, demande explicite de l'utilisateur Lot 5).
/// </summary>
[ApiController]
[Route("api/v1/reporting")]
public class ReportingController(
    ReportingService reportingService, ExportService exportService, IValidator<ReportingFilterQuery> filterValidator) : ControllerBase
{
    [HttpGet("charges")]
    public async Task<ActionResult<ChargesReportDto>> GetCharges([FromQuery] ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var result = await reportingService.GetChargesReportAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var result = await reportingService.GetDashboardAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("operational")]
    public async Task<ActionResult<OperationalReportDto>> GetOperational([FromQuery] ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var result = await reportingService.GetOperationalReportAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("financial")]
    [RequirePermission(PermissionCodes.FinancialDataView)]
    public async Task<ActionResult<FinancialReportDto>> GetFinancial([FromQuery] ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var result = await reportingService.GetFinancialReportAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cahier des charges §17.7 : ferme l'écart identifié à la clôture du Lot 4.</summary>
    [HttpGet("projects/{projectId:guid}/linked-references")]
    public async Task<ActionResult<IReadOnlyList<ProjectLinkedReferenceDto>>> GetProjectLinkedReferences(
        Guid projectId, CancellationToken cancellationToken)
    {
        var result = await reportingService.GetProjectLinkedReferencesAsync(projectId, cancellationToken);
        return Ok(result);
    }

    /// <summary>§26.3 : export réel (jamais un bouton simulé), respecte les mêmes filtres que l'écran.</summary>
    [HttpGet("charges/export")]
    public async Task<IActionResult> ExportCharges(
        [FromQuery] ReportingFilterQuery filter, [FromQuery] ExportFormat format, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var table = await reportingService.GetChargesTableAsync(filter, cancellationToken);
        var result = await exportService.ExportAsync(table, format, "Charges", filter, containsFinancialData: false, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>§26.1/§26.3 (Lot 12) : contenu distinct de /charges/export (charge par
    /// équipe/service/département, jalons en retard, capacité et disponibilité), aucune donnée
    /// financière — pas de garde de permission, comme /charges/export.</summary>
    [HttpGet("operational/export")]
    public async Task<IActionResult> ExportOperational(
        [FromQuery] ReportingFilterQuery filter, [FromQuery] ExportFormat format, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        var table = await reportingService.GetOperationalTableAsync(filter, cancellationToken);
        var result = await exportService.ExportAsync(table, format, "Operationnel", filter, containsFinancialData: false, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("financial/export")]
    [RequirePermission(PermissionCodes.FinancialDataView)]
    public async Task<IActionResult> ExportFinancial(
        [FromQuery] ReportingFilterQuery filter, [FromQuery] ExportFormat format, CancellationToken cancellationToken)
    {
        var validationError = await ValidateFilterAsync(filter, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        // RequirePermissionAttribute a déjà bloqué l'appelant sans FINANCIAL_DATA_VIEW (403) avant
        // d'atteindre ce corps de méthode ; ReportingService ne peut donc pas renvoyer null ici.
        var table = (await reportingService.GetFinancialDifferentialsTableAsync(filter, cancellationToken))!;
        var result = await exportService.ExportAsync(table, format, "Financier", filter, containsFinancialData: true, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>Sous-lot 14.1 (rapport d'audit du Lot 14, constat BE-7) : centralise la validation
    /// de <see cref="ReportingFilterQuery"/> pour les 7 actions qui la lient — sans ce contrôle,
    /// <c>periodType=Personnalisee</c> sans dates atteignait <c>ReportingPeriodResolver</c> et
    /// faisait fuir une <see cref="ArgumentException"/> brute (500) au lieu d'un 400.</summary>
    private async Task<ActionResult?> ValidateFilterAsync(ReportingFilterQuery filter, CancellationToken cancellationToken)
    {
        var validationResult = await filterValidator.ValidateAsync(filter, cancellationToken);
        return validationResult.IsValid ? null : ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
    }
}
