using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S05ContractTests
{
    private const string DevelopmentVersion = "1.4.0-Alpha-8";
    private const string StableAssemblyVersion = "1.0.0.0";

    [Fact]    public void SourceAndRuntimePackagesAdvanceTogetherWithoutChangingAssemblyIdentity()
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
    public void SourcePublicSurfaceIncludesReviewedClassificationContract()
    {
        Assembly assembly =
            typeof(TermInfoSourceParser).Assembly;
        string[] exportedTypes =
            assembly
                .GetExportedTypes()
                .Select(type => type.FullName!)
                .OrderBy(
                    name => name,
                    StringComparer.Ordinal)
                .ToArray();

        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceCapabilityClassification",
            exportedTypes);
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceField",
            exportedTypes);
        Assert.Equal(
            new Version(1, 0, 0, 0),
            assembly.GetName().Version);
    }

    [Fact]
    public void SourcePublicApiBaselineIncludesClassificationAndDiagnostics()
    {
        string root = FindRepositoryRoot();
        string baseline =
            File.ReadAllText(
                Path.Combine(
                    root,
                    "docs",
                    "1.1.0-SOURCE-PUBLIC-API-BASELINE.txt"));

        Assert.Contains(
            "TYPE enum Icod.TermInfo.Source.TermInfoSourceCapabilityClassification [sealed]",
            baseline);
        Assert.Contains(
            "CapabilityClassification",
            baseline);
        Assert.Contains(
            "StandardBooleanCapability",
            baseline);
        Assert.Contains(
            "StandardNumericCapability",
            baseline);
        Assert.Contains(
            "StandardStringCapability",
            baseline);
        Assert.Contains("TIS0020", baseline);
        Assert.Contains("TIS0021", baseline);
    }

    [Fact]
    public void S05ImplementationRecordAndRoadmapLinkArePresent()
    {
        string root = FindRepositoryRoot();
        string recordPath =
            Path.Combine(
                root,
                "docs",
                "1.1.0-S05-CAPABILITY-CLASSIFICATION.md");
        string roadmap =
            File.ReadAllText(
                Path.Combine(
                    root,
                    "Icod.TermInfo-Post-1.0-Development-Roadmap.md"));

        Assert.True(File.Exists(recordPath));
        string record =
            File.ReadAllText(recordPath);
        Assert.Contains("1.1.0-Alpha-5", record);
        Assert.Contains("StandardCapabilityCatalog", record);
        Assert.Contains("termcap", record, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("S06", record);
        Assert.Contains(
            "1.1.0-S05-CAPABILITY-CLASSIFICATION.md",
            roadmap);
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
