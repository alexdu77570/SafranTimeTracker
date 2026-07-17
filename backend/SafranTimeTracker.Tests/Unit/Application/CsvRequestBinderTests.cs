using FluentAssertions;
using SafranTimeTracker.Application.Companies.Dtos;
using SafranTimeTracker.Application.Imports;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Liaison ligne CSV → DTO de requête (Lot 6, §27) : fonction pure testée sans base de
/// données (CLAUDE.md §14). Utilise CompanyCreateRequest comme DTO représentatif (string, Guid,
/// enum via CompanyTypeId qui reste un Guid ici — voir ResourceCreateRequest pour la liste
/// Guid).</summary>
public class CsvRequestBinderTests
{
    [Fact]
    public void Bind_WithMatchingHeaders_PopulatesAllProperties()
    {
        var companyTypeId = Guid.NewGuid();
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nom"] = "Externe Conseil",
            ["Code"] = "EXT-CONSEIL",
            ["CompanyTypeId"] = companyTypeId.ToString(),
            ["ContactPrincipal"] = "Jean Dupont",
            ["EmailContact"] = "jean.dupont@example.com"
        };

        var request = CsvRequestBinder.Bind<CompanyCreateRequest>(row);

        request.Nom.Should().Be("Externe Conseil");
        request.Code.Should().Be("EXT-CONSEIL");
        request.CompanyTypeId.Should().Be(companyTypeId);
        request.ContactPrincipal.Should().Be("Jean Dupont");
        request.EmailContact.Should().Be("jean.dupont@example.com");
    }

    [Fact]
    public void Bind_WithMissingOrEmptyColumn_LeavesPropertyAtDefault()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nom"] = "Externe Conseil",
            ["Telephone"] = ""
        };

        var request = CsvRequestBinder.Bind<CompanyCreateRequest>(row);

        request.Nom.Should().Be("Externe Conseil");
        request.Telephone.Should().BeNull();
        request.Code.Should().Be(string.Empty);
    }

    [Fact]
    public void ReadOptionalId_WithIdColumn_ReturnsGuid()
    {
        var id = Guid.NewGuid();
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Id"] = id.ToString() };

        CsvRequestBinder.ReadOptionalId(row).Should().Be(id);
    }

    [Fact]
    public void ReadOptionalId_WithoutIdColumn_ReturnsNull()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Nom"] = "A" };

        CsvRequestBinder.ReadOptionalId(row).Should().BeNull();
    }

    [Fact]
    public void ReadRequiredGuid_WithMissingColumn_ThrowsFormatException()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var act = () => CsvRequestBinder.ReadRequiredGuid(row, "ResourceId");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Bind_WithGuidListColumn_SplitsOnSemicolon()
    {
        var role1 = Guid.NewGuid();
        var role2 = Guid.NewGuid();
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nom"] = "Test",
            ["Prenom"] = "Test",
            ["DepartmentId"] = Guid.NewGuid().ToString(),
            ["ServiceId"] = Guid.NewGuid().ToString(),
            ["OperationalRoleIds"] = $"{role1};{role2}"
        };

        var request = CsvRequestBinder.Bind<SafranTimeTracker.Application.Resources.Dtos.ResourceCreateRequest>(row);

        request.OperationalRoleIds.Should().Equal(role1, role2);
    }
}
