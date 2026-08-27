using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S02ContractTests
{
    private const string DevelopmentVersion = "1.2.0-Alpha-2";
    private const string StableAssemblyVersion = "1.0.0.0";

    [Fact]
    public void SourceAndRuntimePackagesAdvanceTogetherWithoutChangingAssemblyIdentity()
    {
        string root = FindRepositoryRoot();

        foreach (
            string relativePath
            in new[]
            {
                "Icod.TermInfo.csproj",
                "Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
            })
        {
            XDocument project =
                XDocument.Load(
                    Path.Combine(
                        root,
                        relativePath.Replace(
                            '/',
                            Path.DirectorySeparatorChar)),
                    LoadOptions.None);

            Assert.Equal(
                DevelopmentVersion,
                ReadRequiredProperty(
                    project,
                    "Version"));
            Assert.Equal(
                DevelopmentVersion,
                ReadRequiredProperty(
                    project,
                    "PackageVersion"));
            Assert.Equal(
                StableAssemblyVersion,
                ReadRequiredProperty(
                    project,
                    "AssemblyVersion"));
        }
    }

    [Fact]
    public void SourcePublicSurfaceIncludesReviewedLexicalTypes()
    {
        Assembly assembly =
            typeof(TermInfoSourceLexer).Assembly;
        string[] exportedTypes =
            assembly
                .GetExportedTypes()
                .Select(type => type.FullName!)
                .ToArray();

        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceDiagnostic",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceDiagnosticCodes",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceDiagnosticSeverity",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceLexResult",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceLexer",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceLexerOptions",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceSpan",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceToken",
            exportedTypes
        );
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceTokenKind",
            exportedTypes
        );
        Assert.Equal(
            new Version(1, 0, 0, 0),
            assembly.GetName().Version
        );
    }

    [Fact]
    public void SourcePublicApiBaselineAndVerificationAreCheckedIn()
    {
        string root = FindRepositoryRoot();
        string baseline =
            Path.Combine(
                root,
                "docs",
                "1.1.0-SOURCE-PUBLIC-API-BASELINE.txt");

        Assert.True(
            File.Exists(baseline));
        string baselineText =
            File.ReadAllText(baseline);
        Assert.StartsWith(
            "# Icod.TermInfo.Source public API baseline",
            baselineText);
        Assert.Contains(
            "TYPE class Icod.TermInfo.Source.TermInfoSourceLexer [static]",
            baselineText);
        Assert.Contains(
            "TYPE class Icod.TermInfo.Source.TermInfoSourceSpan [sealed]",
            baselineText);

        foreach (
            string relativePath
            in new[]
            {
                Path.Combine(
                    ".github",
                    "scripts",
                    "verify-release-package.cmd"),
                Path.Combine(
                    ".github",
                    "scripts",
                    "verify-release-package.sh"),
            })
        {
            string verifier =
                File.ReadAllText(
                    Path.Combine(
                        root,
                        relativePath));

            Assert.Contains(
                "1.1.0-SOURCE-PUBLIC-API-BASELINE.txt",
                verifier);
            Assert.Contains(
                "Icod.TermInfo.Source.dll",
                verifier);
            Assert.Contains(
                "--check",
                verifier);
        }
    }

    [Fact]
    public void S02ImplementationRecordAndRoadmapLinkArePresent()
    {
        string root = FindRepositoryRoot();
        string recordPath =
            Path.Combine(
                root,
                "docs",
                "1.1.0-S02-LEXICAL-SOURCE-LOCATION.md");
        string roadmap =
            File.ReadAllText(
                Path.Combine(
                    root,
                    "Icod.TermInfo-Post-1.0-Development-Roadmap.md"));

        Assert.True(
            File.Exists(recordPath));
        string record =
            File.ReadAllText(recordPath);
        Assert.Contains(
            "TIS0001",
            record);
        Assert.True(
            record.Contains(
                "escaped comma",
                StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            "S03",
            record);
        Assert.Contains(
            "1.1.0-S02-LEXICAL-SOURCE-LOCATION.md",
            roadmap);
    }

    [Fact]
    public void SourceAssemblyMarkerIsRetiredAfterRealApiArrives()
    {
        string root = FindRepositoryRoot();

        Assert.False(
            File.Exists(
                Path.Combine(
                    root,
                    "Icod.TermInfo.Source",
                    "src",
                    "SourceAssemblyMarker.cs")));
    }

    private static string ReadRequiredProperty(
        XDocument project,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(propertyName);

        return project
            .Descendants()
            .First(
                element =>
                    element.Name.LocalName
                        == propertyName)
            .Value
            .Trim();
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current =
            new(
                AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(
                    Path.Combine(
                        current.FullName,
                        "Icod.TermInfo.sln")))
            {
                return current.FullName;
            }

            current =
                current.Parent;
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }
}
