using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Application.Financial.Dtos;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Companies;
using SafranTimeTracker.Domain.Resources;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Financial.Services;

/// <summary>
/// Seul point de calcul du coût réel, du coût contractuel et du différentiel
/// (docs/ARCHITECTURE.md §2, cahier des charges §20) : aucun autre service ne doit dupliquer
/// cette logique. Ne persiste rien — le Lot 2 ne crée pas l'entité TimeEntryFinancialSnapshot,
/// qui référence TimeEntry (Lot 3) ; ce service reste réutilisable tel quel par le
/// TimeEntryService du Lot 3 pour produire ce snapshot.
/// </summary>
public class FinancialCalculationService(
    IReadRepository<ResourceTjmHistory> tjmHistoryRepository,
    IReadRepository<ResourceCompanyAssignment> assignmentRepository,
    IReadRepository<Company> companyRepository,
    IReadRepository<CompanyContractHistory> contractHistoryRepository,
    IReadRepository<SettingsEntity> settingsRepository)
{
    private const string CompanyTypeInterneCode = "INTERNE";

    public async Task<FinancialCalculationResultDto> CalculateAsync(
        FinancialCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var heuresParJour = await settingsRepository.Query().Select(s => s.HeuresParJour).FirstAsync(cancellationToken);

        var result = new FinancialCalculationResultDto
        {
            ResourceId = request.ResourceId,
            Date = request.Date,
            HeuresSaisies = request.HeuresSaisies,
            HeuresParJour = heuresParJour,
            TempsJours = request.HeuresSaisies / heuresParJour // §20.1
        };

        var applicableTjm = await tjmHistoryRepository.Query()
            .Where(h => h.ResourceId == request.ResourceId && h.Status == ReferentialStatus.Actif
                && h.StartDate <= request.Date && (h.EndDate == null || h.EndDate >= request.Date))
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (applicableTjm is null)
        {
            // §11.4 : aucun TJM valide à la date -> aucun montant inventé, valorisation incomplète.
            result.ValuationStatus = FinancialValuationStatus.Incomplete;
            return result;
        }

        result.ValuationStatus = FinancialValuationStatus.Complete;
        result.ResourceTjmHistoryId = applicableTjm.Id;
        result.DailyRatePersonne = applicableTjm.DailyRate;
        result.CoutReel = result.TempsJours * applicableTjm.DailyRate; // §20.2

        var applicableAssignment = await assignmentRepository.Query()
            .Where(a => a.ResourceId == request.ResourceId && a.Status == ReferentialStatus.Actif
                && a.StartDate <= request.Date && (a.EndDate == null || a.EndDate >= request.Date))
            .OrderByDescending(a => a.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (applicableAssignment is null)
        {
            // Société non déterminée à la date : coût contractuel et différentiel non applicables.
            return result;
        }

        result.CompanyId = applicableAssignment.CompanyId;

        var companyType = await companyRepository.Query()
            .Where(c => c.Id == applicableAssignment.CompanyId)
            .Select(c => c.CompanyType.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (companyType == CompanyTypeInterneCode)
        {
            // §12.5 / §20.3 : société interne -> coût contractuel et différentiel non applicables.
            return result;
        }

        var applicableContract = await contractHistoryRepository.Query()
            .Where(h => h.CompanyId == applicableAssignment.CompanyId && h.Status == ReferentialStatus.Actif
                && h.StartDate <= request.Date && (h.EndDate == null || h.EndDate >= request.Date))
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (applicableContract is null)
        {
            // Aucun contrat valide à la date pour cette société externe : non applicable.
            return result;
        }

        result.CompanyContractHistoryId = applicableContract.Id;
        result.DailyRateContrat = applicableContract.ContractDailyRate;
        result.CoutContractuel = result.TempsJours * applicableContract.ContractDailyRate; // §20.3
        result.Differentiel = result.CoutContractuel - result.CoutReel; // §20.4

        return result;
    }
}
