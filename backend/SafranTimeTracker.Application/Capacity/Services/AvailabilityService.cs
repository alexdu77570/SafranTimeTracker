using Microsoft.EntityFrameworkCore;
using SafranTimeTracker.Application.Capacity.Dtos;
using SafranTimeTracker.Application.Common.Persistence;
using SafranTimeTracker.Domain.Absences;
using SafranTimeTracker.Domain.Activities;
using SafranTimeTracker.Domain.Common;
using SafranTimeTracker.Domain.Resources;
using SafranTimeTracker.Domain.Settings;
using SafranTimeTracker.Domain.Time;
using SettingsEntity = SafranTimeTracker.Domain.Settings.Settings;

namespace SafranTimeTracker.Application.Capacity.Services;

/// <summary>
/// Calculs de capacité (cahier des charges §29.1-29.4). "Jour ouvré" = du lundi au vendredi
/// (cohérent avec Settings.JoursOuvresParSemaine = 5 par défaut) ; les jours fériés (§22.2, §29.2)
/// sont comptés séparément de la capacité théorique, comme l'exprime la formule du §29.2. Les
/// "indisponibilités" du §29.2 sont couvertes par AbsenceType.Indisponible (pas de mécanisme
/// distinct) : voir docs/IMPLEMENTATION_STATUS.md pour cette simplification.
/// </summary>
public class AvailabilityService(
    IReadRepository<Resource> resourceRepository,
    IReadRepository<ResourceCapacityPeriod> capacityPeriodRepository,
    IReadRepository<HolidayCalendar> holidayRepository,
    IReadRepository<Absence> absenceRepository,
    IReadRepository<TimeEntry> timeEntryRepository,
    IReadRepository<ActivityType> activityTypeRepository,
    IReadRepository<SettingsEntity> settingsRepository)
{
    public async Task<AvailabilityResultDto?> GetAvailabilityAsync(
        Guid resourceId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var resource = await resourceRepository.GetByIdAsync(resourceId, cancellationToken);
        if (resource is null)
        {
            return null;
        }

        var paysParDefaut = await settingsRepository.Query().Select(s => s.PaysParDefaut).FirstAsync(cancellationToken);

        var capacityPeriods = await capacityPeriodRepository.Query()
            .Where(p => p.ResourceId == resourceId && p.Status == ReferentialStatus.Actif
                && p.StartDate <= endDate && (p.EndDate == null || p.EndDate >= startDate))
            .ToListAsync(cancellationToken);

        var holidays = await holidayRepository.Query()
            .Where(h => h.Pays == paysParDefaut && h.Statut == ReferentialStatus.Actif && h.Date >= startDate && h.Date <= endDate)
            .Select(h => h.Date)
            .ToListAsync(cancellationToken);
        var holidaySet = holidays.ToHashSet();

        var validatedAbsences = await absenceRepository.Query()
            .Where(a => a.ResourceId == resourceId && a.Statut == AbsenceStatus.Valide
                && a.DateDebut <= endDate && a.DateFin >= startDate)
            .ToListAsync(cancellationToken);

        var result = new AvailabilityResultDto { ResourceId = resourceId, StartDate = startDate, EndDate = endDate };
        var absencesValideesHeures = 0m;
        var joursFeriesHeures = 0m;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            result.JoursOuvres++;
            var dailyCapacity = GetApplicableDailyCapacity(resource, capacityPeriods, date);
            result.CapaciteTheorique += dailyCapacity; // §29.1

            if (holidaySet.Contains(date))
            {
                result.JoursFeries++;
                joursFeriesHeures += dailyCapacity;
                continue; // un jour férié n'est pas aussi compté en absence
            }

            var absence = validatedAbsences.FirstOrDefault(a => a.DateDebut <= date && a.DateFin >= date);
            if (absence is not null)
            {
                var joursAbsence = absence.DemiJournee ? 0.5m : 1m;
                result.JoursAbsenceValidee += joursAbsence;
                absencesValideesHeures += dailyCapacity * joursAbsence;
            }
        }

        result.CapaciteReelle = result.CapaciteTheorique - absencesValideesHeures - joursFeriesHeures; // §29.2
        result.TauxDisponibilite = result.CapaciteTheorique > 0
            ? Math.Round(result.CapaciteReelle / result.CapaciteTheorique * 100, 2)
            : 0m; // §29.3

        var (chargeRun, chargeHorsRun) = await GetWorkloadAsync(resourceId, startDate, endDate, cancellationToken);
        result.ChargeRunHeures = chargeRun;
        result.ChargeHorsRunHeures = chargeHorsRun;

        return result;
    }

    /// <summary>§29.1 : capacité journalière applicable à une date (variation de capacité si une
    /// période la couvre, sinon la capacité par défaut de la ressource). Public et pur : testable
    /// unitairement sans base de données (CLAUDE.md §14).</summary>
    public static decimal GetApplicableDailyCapacity(Resource resource, List<ResourceCapacityPeriod> capacityPeriods, DateOnly date)
    {
        var applicablePeriod = capacityPeriods
            .Where(p => p.StartDate <= date && (p.EndDate == null || p.EndDate >= date))
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefault();

        return applicablePeriod?.DailyCapacity ?? resource.DailyCapacity;
    }

    /// <summary>§29.4 : classification RUN/hors RUN pilotée par ActivityType.IsRun, jamais une
    /// liste de types codée en dur ici.</summary>
    private async Task<(decimal ChargeRun, decimal ChargeHorsRun)> GetWorkloadAsync(
        Guid resourceId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var entries = await timeEntryRepository.Query()
            .Where(t => t.ResourceId == resourceId && t.Statut == ReferentialStatus.Actif
                && t.Date >= startDate && t.Date <= endDate)
            .Select(t => new { t.ActivityTypeId, t.DureeHeures })
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return (0m, 0m);
        }

        var activityTypeIds = entries.Select(e => e.ActivityTypeId).Distinct().ToList();
        var runByActivityType = await activityTypeRepository.Query()
            .Where(a => activityTypeIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.IsRun, cancellationToken);

        var chargeRun = entries.Where(e => runByActivityType.GetValueOrDefault(e.ActivityTypeId)).Sum(e => e.DureeHeures);
        var chargeHorsRun = entries.Where(e => !runByActivityType.GetValueOrDefault(e.ActivityTypeId)).Sum(e => e.DureeHeures);

        return (chargeRun, chargeHorsRun);
    }
}
