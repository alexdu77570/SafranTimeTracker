using FluentAssertions;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Imports;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Calcul des champs modifiés pour ImportDiff (§27.6) : fonction pure testée sans base de
/// données (CLAUDE.md §14).</summary>
public class FieldDifferTests
{
    [Fact]
    public void Diff_WithChangedField_ReturnsOneChange()
    {
        var oldValue = new CompanyDto { Nom = "Ancien nom", ContactPrincipal = "A", EmailContact = "a@example.com" };
        var newValue = new CompanyUpdateRequest { Nom = "Nouveau nom", ContactPrincipal = "A", EmailContact = "a@example.com" };

        var changes = FieldDiffer.Diff(oldValue, newValue);

        changes.Should().ContainSingle(c => c.FieldName == "Nom" && c.OldValue == "Ancien nom" && c.NewValue == "Nouveau nom");
    }

    [Fact]
    public void Diff_WithNoChanges_ReturnsEmpty()
    {
        var oldValue = new CompanyDto { Nom = "A", ContactPrincipal = "B", EmailContact = "b@example.com" };
        var newValue = new CompanyUpdateRequest { Nom = "A", ContactPrincipal = "B", EmailContact = "b@example.com" };

        FieldDiffer.Diff(oldValue, newValue).Should().BeEmpty();
    }

    [Fact]
    public void Diff_WithNullToValue_ReportsChange()
    {
        var oldValue = new CompanyDto { Telephone = null };
        var newValue = new CompanyUpdateRequest { Telephone = "0102030405" };

        var changes = FieldDiffer.Diff(oldValue, newValue);

        changes.Should().ContainSingle(c => c.FieldName == "Telephone" && c.OldValue == null && c.NewValue == "0102030405");
    }

    [Fact]
    public void Diff_OnlyComparesPropertiesPresentOnRequestType()
    {
        var oldValue = new CompanyDto { Nom = "A", Code = "CODE-A" };
        var newValue = new CompanyUpdateRequest { Nom = "A" };

        // CompanyUpdateRequest ne porte pas Code (clé métier immuable) : aucun changement ne doit
        // être signalé pour ce champ, même s'il diffère sur l'entité existante.
        FieldDiffer.Diff(oldValue, newValue).Should().BeEmpty();
    }
}
