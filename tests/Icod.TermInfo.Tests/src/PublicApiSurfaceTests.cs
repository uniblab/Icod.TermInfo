using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class PublicApiSurfaceTests
{
    private static readonly string[] ExpectedExportedTypes =
    [
        "Icod.TermInfo.BooleanCapability",
        "Icod.TermInfo.ITerminalDescriptionProvider",
        "Icod.TermInfo.ITermInfoDelayProvider",
        "Icod.TermInfo.InMemoryTerminalDescriptionProvider",
        "Icod.TermInfo.NumericCapability",
        "Icod.TermInfo.PaddingMode",
        "Icod.TermInfo.StringCapability",
        "Icod.TermInfo.TerminalDatabase",
        "Icod.TermInfo.TerminalDescription",
        "Icod.TermInfo.TerminalDescriptionBuilder",
        "Icod.TermInfo.TerminalEnvironment",
        "Icod.TermInfo.TerminalProfiles",
        "Icod.TermInfo.TerminalSize",
        "Icod.TermInfo.TerminalStandardStream",
        "Icod.TermInfo.TermInfoCompatibility",
        "Icod.TermInfo.TermInfoDelay",
        "Icod.TermInfo.TermInfoEvaluationException",
        "Icod.TermInfo.TermInfoExpansionContext",
        "Icod.TermInfo.TermInfoFormatException",
        "Icod.TermInfo.TermInfoOutput",
        "Icod.TermInfo.TermInfoPaddingFormatException",
        "Icod.TermInfo.TermInfoParameter",
        "Icod.TermInfo.TermInfoParameterExpander",
        "Icod.TermInfo.TermInfoParameterProgram",
        "Icod.TermInfo.WindowsVirtualTerminal",
    ];

    [Fact]
    public void ExportedTypeSetMatchesT8Baseline()
    {
        string[] actual =
            typeof(TerminalDescription).Assembly
                .GetExportedTypes()
                .Select(type => type.FullName!)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        string[] expected =
            ExpectedExportedTypes
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExportedTypesUsePackageNamespace()
    {
        Type[] types =
            typeof(TerminalDescription).Assembly.GetExportedTypes();

        Assert.All(
            types,
            type => Assert.Equal("Icod.TermInfo", type.Namespace));
    }

    [Fact]
    public void CompatibilityFacadeContainsOnlyIntendedOperations()
    {
        string[] expected =
        [
            "PutP/4",
            "TParm/2",
            "TParm/3",
            "TPuts/5",
            "TiGetFlag/2",
            "TiGetNum/2",
            "TiGetStr/2",
            "TiParm/2",
            "TiParm/3",
        ];

        string[] actual =
            typeof(TermInfoCompatibility)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Select(method =>
                    $"{method.Name}/{method.GetParameters().Length}")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expected.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            actual);
    }
}
