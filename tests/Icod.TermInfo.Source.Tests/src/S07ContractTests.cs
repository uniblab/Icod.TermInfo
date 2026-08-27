using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S07ContractTests
{
    private const string DevelopmentVersion = "1.3.0-Alpha-3";
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
    public void SourcePublicSurfaceIncludesReviewedResolverContract()
    {
        Assembly assembly =
            typeof(TermInfoSourceResolver).Assembly;
        string[] exportedTypes =
            assembly
                .GetExportedTypes()
                .Select(type => type.FullName!)
                .OrderBy(
                    name => name,
                    StringComparer.Ordinal)
                .ToArray();

        Assert.Contains(
            "Icod.TermInfo.Source.ITermInfoSourceEntryProvider",
            exportedTypes);
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceResolveResult",
            exportedTypes);
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceResolvedEntry",
            exportedTypes);
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceResolver",
            exportedTypes);
        Assert.Contains(
            "Icod.TermInfo.Source.TermInfoSourceResolverOptions",
            exportedTypes);
        Assert.Equal(
            new Version(1, 0, 0, 0),
            assembly.GetName().Version);
    }

    [Fact]
    public void SourcePublicApiBaselineIncludesResolverAndDiagnostics()
    {
        string root = FindRepositoryRoot();
        string baseline =
            File.ReadAllText(
                Path.Combine(
                    root,
                    "docs",
                    "1.1.0-SOURCE-PUBLIC-API-BASELINE.txt"));

        Assert.Contains(
            "TYPE interface Icod.TermInfo.Source.ITermInfoSourceEntryProvider [abstract]",
            baseline);
        Assert.Contains(
            "TYPE class Icod.TermInfo.Source.TermInfoSourceResolver [static]",
            baseline);
        Assert.Contains(
            "TYPE class Icod.TermInfo.Source.TermInfoSourceResolvedEntry [sealed]",
            baseline);
        Assert.Contains("TIS0022", baseline);
        Assert.Contains("TIS0023", baseline);
        Assert.Contains("TIS0024", baseline);
    }

    [Fact]
    public void S07ImplementationRecordAndRoadmapLinkArePresent()
    {
        string root = FindRepositoryRoot();
        string recordPath =
            Path.Combine(
                root,
                "docs",
                "1.1.0-S07-USE-INHERITANCE-RESOLVER.md");
        string roadmap =
            File.ReadAllText(
                Path.Combine(
                    root,
                    "Icod.TermInfo-Post-1.0-Development-Roadmap.md"));

        Assert.True(File.Exists(recordPath));
        string record =
            File.ReadAllText(recordPath);
        Assert.Contains("1.1.0-Alpha-7", record);
        Assert.Contains("ITermInfoSourceEntryProvider", record);
        Assert.Contains("TIS0022", record);
        Assert.Contains("TIS0023", record);
        Assert.Contains("TIS0024", record);
        Assert.Contains("S08", record);
        Assert.Contains(
            "1.1.0-S07-USE-INHERITANCE-RESOLVER.md",
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
