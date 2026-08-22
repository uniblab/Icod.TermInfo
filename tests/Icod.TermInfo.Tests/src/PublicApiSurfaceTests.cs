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
        "Icod.TermInfo.TerminalColorModel",
        "Icod.TermInfo.TerminalColorSupport",
        "Icod.TermInfo.TerminalColorTier",
        "Icod.TermInfo.TerminalColors",
        "Icod.TermInfo.TerminalDatabase",
        "Icod.TermInfo.TerminalDescription",
        "Icod.TermInfo.TerminalDescriptionBuilder",
        "Icod.TermInfo.TerminalEnvironment",
        "Icod.TermInfo.TerminalProfiles",
        "Icod.TermInfo.TerminalRgbColor",
        "Icod.TermInfo.TerminalRgbLayout",
        "Icod.TermInfo.TerminalSize",
        "Icod.TermInfo.TerminalStandardStream",
        "Icod.TermInfo.TermInfoCapabilityValue",
        "Icod.TermInfo.TermInfoCapabilityValueKind",
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
    public void ExportedTypeSetMatchesT13Baseline()
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

    [Fact]
    public void ExtendedCapabilitySurfaceContainsIntendedOperations()
    {
        string[] expectedDescription =
        [
            "get_ExtendedCapabilities/0",
            "TryGetExtendedBoolean/2",
            "TryGetExtendedCapability/2",
            "TryGetExtendedNumber/2",
            "TryGetExtendedString/2",
        ];
        string[] expectedBuilder =
        [
            "RemoveExtended/1",
            "SetExtended/2",
            "SetExtendedBoolean/2",
            "SetExtendedNumber/2",
            "SetExtendedString/2",
        ];

        string[] actualDescription =
            typeof(TerminalDescription)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Contains(
                    "Extended",
                    StringComparison.Ordinal))
                .Select(method =>
                    $"{method.Name}/{method.GetParameters().Length}")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        string[] actualBuilder =
            typeof(TerminalDescriptionBuilder)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Contains(
                    "Extended",
                    StringComparison.Ordinal))
                .Select(method =>
                    $"{method.Name}/{method.GetParameters().Length}")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedDescription
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            actualDescription);
        Assert.Equal(
            expectedBuilder
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            actualBuilder);
    }
}
