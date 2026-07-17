using System.Text;
using FluentAssertions;
using SafranTimeTracker.Application.Imports;

namespace SafranTimeTracker.Tests.Unit.Application;

/// <summary>Cahier des charges §27.3 étape 4 (encodage/séparateur) : fonction pure testée sans
/// base de données (CLAUDE.md §14).</summary>
public class CsvFileParserTests
{
    [Fact]
    public void Parse_WithCommaSeparator_ReturnsHeadersAndRows()
    {
        var csv = "Nom,Code\nApplication A,APP-A\nApplication B,APP-B\n";

        var result = CsvFileParser.Parse(Encoding.UTF8.GetBytes(csv));

        result.Headers.Should().Equal("Nom", "Code");
        result.Rows.Should().HaveCount(2);
        result.Rows[0]["Nom"].Should().Be("Application A");
        result.Rows[1]["Code"].Should().Be("APP-B");
    }

    [Fact]
    public void Parse_WithSemicolonSeparator_DetectsSeparatorFromHeaderLine()
    {
        var csv = "Nom;Code\nApplication A;APP-A\n";

        var result = CsvFileParser.Parse(Encoding.UTF8.GetBytes(csv));

        result.Headers.Should().Equal("Nom", "Code");
        result.Rows.Should().ContainSingle();
        result.Rows[0]["Code"].Should().Be("APP-A");
    }

    [Fact]
    public void Parse_WithQuotedFieldContainingSeparatorAndEscapedQuote_ParsesCorrectly()
    {
        var csv = "Nom,Commentaire\n\"Dupont, Jean\",\"Dit \"\"Le Chef\"\"\"\n";

        var result = CsvFileParser.Parse(Encoding.UTF8.GetBytes(csv));

        result.Rows.Should().ContainSingle();
        result.Rows[0]["Nom"].Should().Be("Dupont, Jean");
        result.Rows[0]["Commentaire"].Should().Be("Dit \"Le Chef\"");
    }

    [Fact]
    public void Parse_WithUtf8Bom_StripsBomFromFirstHeader()
    {
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var content = bom.Concat(Encoding.UTF8.GetBytes("Nom,Code\nA,B\n")).ToArray();

        var result = CsvFileParser.Parse(content);

        result.Headers[0].Should().Be("Nom");
    }

    [Fact]
    public void Parse_WithEmptyContent_ReturnsNoHeadersAndNoRows()
    {
        var result = CsvFileParser.Parse([]);

        result.Headers.Should().BeEmpty();
        result.Rows.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SkipsBlankLines()
    {
        var csv = "Nom,Code\nA,B\n\n\nC,D\n";

        var result = CsvFileParser.Parse(Encoding.UTF8.GetBytes(csv));

        result.Rows.Should().HaveCount(2);
    }

    [Fact]
    public void ComputeChecksum_SameContentTwice_ReturnsSameChecksum()
    {
        var content = Encoding.UTF8.GetBytes("Nom,Code\nA,B\n");

        CsvFileParser.ComputeChecksum(content).Should().Be(CsvFileParser.ComputeChecksum(content));
    }

    [Fact]
    public void ComputeChecksum_DifferentContent_ReturnsDifferentChecksum()
    {
        var checksumA = CsvFileParser.ComputeChecksum(Encoding.UTF8.GetBytes("A"));
        var checksumB = CsvFileParser.ComputeChecksum(Encoding.UTF8.GetBytes("B"));

        checksumA.Should().NotBe(checksumB);
    }
}
