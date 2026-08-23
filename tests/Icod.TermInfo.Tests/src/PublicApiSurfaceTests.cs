using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class PublicApiSurfaceTests
{
    private static readonly string[] ExpectedExportedTypes =
    [
        "Icod.TermInfo.BooleanCapability",
        "Icod.TermInfo.CompiledTermInfoFormatException",
        "Icod.TermInfo.CompiledTermInfoParser",
        "Icod.TermInfo.CompiledTermInfoParserOptions",
        "Icod.TermInfo.DirectoryTerminalDescriptionProvider",
        "Icod.TermInfo.ITerminalDescriptionProvider",
        "Icod.TermInfo.ITermInfoDelayProvider",
        "Icod.TermInfo.InMemoryTerminalDescriptionProvider",
        "Icod.TermInfo.NumericCapability",
        "Icod.TermInfo.PaddingMode",
        "Icod.TermInfo.StandardCapabilityCatalog",
        "Icod.TermInfo.SystemTerminalDescriptionProvider",
        "Icod.TermInfo.SystemTerminalDescriptionProviderOptions",
        "Icod.TermInfo.StandardCapabilityMetadata`1",
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
        "Icod.TermInfo.TermInfoOutputOptions",
        "Icod.TermInfo.TermInfoPaddingFormatException",
        "Icod.TermInfo.TermInfoParameter",
        "Icod.TermInfo.TermInfoParameterExpander",
        "Icod.TermInfo.TermInfoParameterProgram",
        "Icod.TermInfo.WindowsVirtualTerminal",
    ];

    [Fact]
    public void ExportedTypeSetMatchesCurrentContract()
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
    public void TerminalProfilesExposeOnlyContractedBuiltIns()
    {
        string[] expected =
        [
            "Ansi",
            "Dumb",
            "MsTerminal",
            "MsTerminalDirect",
            "Vt100",
            "Vt102",
            "Vt220",
            "WinConsole",
            "Xterm",
            "Xterm16Color",
            "Xterm88Color",
            "Xterm256Color",
            "XtermDirect",
            "XtermDirect16",
            "XtermDirect256",
        ];

        PropertyInfo[] properties =
            typeof(TerminalProfiles)
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly);

        Assert.All(
            properties,
            property =>
            {
                Assert.Equal(typeof(TerminalDescription), property.PropertyType);
                Assert.True(property.CanRead);
                Assert.False(property.CanWrite);
            });

        Assert.Equal(
            expected.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            properties
                .Select(property => property.Name)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void ColorFacadeContainsOnlyIntendedOperations()
    {
        string[] expected =
        [
            "ExpandBackground(TerminalDescription,Int32)",
            "ExpandBackground(TerminalDescription,TerminalRgbColor)",
            "ExpandForeground(TerminalDescription,Int32)",
            "ExpandForeground(TerminalDescription,TerminalRgbColor)",
            "GetColorSupport(TerminalDescription)",
        ];

        string[] actual =
            typeof(TerminalColors)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Select(FormatMethod)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expected.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            actual);
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
            "ExpandExtendedString/2",
            "ExpandExtendedString/3",
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

    [Fact]
    public void StandardCapabilityMetadataAndEnumerationSurfaceIsFrozen()
    {
        string[] expectedCatalogProperties =
        [
            "BooleanCapabilities",
            "NumericCapabilities",
            "StringCapabilities",
        ];
        string[] actualCatalogProperties =
            typeof(StandardCapabilityCatalog)
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedCatalogProperties
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            actualCatalogProperties);

        string[] expectedCatalogMethods =
        [
            "GetMetadata(BooleanCapability)",
            "GetMetadata(NumericCapability)",
            "GetMetadata(StringCapability)",
            "TryGetBoolean(String,StandardCapabilityMetadata`1&)",
            "TryGetNumeric(String,StandardCapabilityMetadata`1&)",
            "TryGetString(String,StandardCapabilityMetadata`1&)",
        ];
        string[] actualCatalogMethods =
            typeof(StandardCapabilityCatalog)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Select(FormatMethod)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedCatalogMethods
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            actualCatalogMethods);

        Type metadataType = typeof(StandardCapabilityMetadata<>);
        Assert.DoesNotContain(
            metadataType.GetConstructors(
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance),
            constructor => constructor.IsPublic);

        string[] expectedMetadataProperties =
        [
            "BinaryIndex",
            "Capability",
            "Kind",
            "LongName",
            "ShortName",
            "TermcapCode",
        ];
        PropertyInfo[] metadataProperties =
            metadataType.GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.DeclaredOnly);

        Assert.All(
            metadataProperties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.False(property.CanWrite);
            });
        Assert.Equal(
            expectedMetadataProperties
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            metadataProperties
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void SemanticCompletionDescriptionAndProgramSurfaceIsFrozen()
    {
        PropertyInfo description =
            typeof(TerminalDescription).GetProperty(
                nameof(TerminalDescription.Description))!;
        Assert.Equal(typeof(string), description.PropertyType);
        Assert.True(description.CanRead);
        Assert.False(description.CanWrite);

        string[] expectedEnumerationProperties =
        [
            "BooleanCapabilities",
            "ExtendedCapabilities",
            "NumericCapabilities",
            "StringCapabilities",
        ];
        string[] actualEnumerationProperties =
            typeof(TerminalDescription)
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly)
                .Where(property =>
                    property.Name.EndsWith(
                        "Capabilities",
                        StringComparison.Ordinal))
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedEnumerationProperties
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            actualEnumerationProperties);

        string[] expectedProgramMembers =
        [
            "Expand/1",
            "Expand/2",
            "Parse/1",
            "get_Source/0",
        ];
        string[] actualProgramMembers =
            typeof(TermInfoParameterProgram)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Select(method =>
                    $"{method.Name}/{method.GetParameters().Length}")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedProgramMembers
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            actualProgramMembers);

        Assert.DoesNotContain(
            typeof(TerminalDescription).Assembly.GetExportedTypes(),
            type =>
                type.Name.Contains("Analysis", StringComparison.Ordinal)
                || type.Name.Contains("Instruction", StringComparison.Ordinal)
                || (type.Name.Contains("Parser", StringComparison.Ordinal)
                    && type != typeof(CompiledTermInfoParser)
                    && type != typeof(CompiledTermInfoParserOptions))
                || type.Name.Contains("Cache", StringComparison.Ordinal));

        Assert.DoesNotContain(
            typeof(TerminalDescription).GetMembers(
                BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.DeclaredOnly),
            member =>
                member.Name.Contains("Cache", StringComparison.Ordinal)
                || member.Name.Contains(
                    "ParameterProgram",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void NullabilityContractsAreFrozen()
    {
        NullabilityInfoContext nullability = new();

        PropertyInfo description =
            typeof(TerminalDescription).GetProperty(
                nameof(TerminalDescription.Description))!;
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(description).ReadState);

        PropertyInfo terminal =
            typeof(TermInfoOutputOptions).GetProperty(
                nameof(TermInfoOutputOptions.Terminal))!;
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(terminal).ReadState);

        PropertyInfo delayProvider =
            typeof(TermInfoOutputOptions).GetProperty(
                nameof(TermInfoOutputOptions.DelayProvider))!;
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(delayProvider).ReadState);

        MethodInfo tryLoad =
            typeof(ITerminalDescriptionProvider).GetMethod(
                nameof(ITerminalDescriptionProvider.TryLoad))!;
        ParameterInfo result = tryLoad.GetParameters()[1];
        NotNullWhenAttribute? notNullWhen =
            result.GetCustomAttribute<NotNullWhenAttribute>();

        Assert.NotNull(notNullWhen);
        Assert.True(notNullWhen.ReturnValue);
    }

    [Fact]
    public void TerminalAwareOutputSurfaceIsFrozen()
    {
        ConstructorInfo[] constructors =
            typeof(TermInfoOutputOptions).GetConstructors(
                BindingFlags.Public
                | BindingFlags.Instance);

        Assert.Single(constructors);
        Assert.Equal(
            new[]
            {
                "TerminalDescription",
                "Nullable`1",
                "PaddingMode",
                "ITermInfoDelayProvider",
            },
            constructors[0]
                .GetParameters()
                .Select(parameter => parameter.ParameterType.Name)
                .ToArray());

        string[] expectedOptionProperties =
        [
            "BaudRate",
            "DelayProvider",
            "PaddingMode",
            "Terminal",
        ];
        PropertyInfo[] optionProperties =
            typeof(TermInfoOutputOptions)
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly);

        Assert.All(
            optionProperties,
            property =>
            {
                Assert.True(property.CanRead);
                Assert.False(property.CanWrite);
            });
        Assert.Equal(
            expectedOptionProperties
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            optionProperties
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray());

        string[] expectedOutputMethods =
        [
            "PutP(String,TextWriter,TermInfoOutputOptions)",
            "PutPAsync(String,TextWriter,TermInfoOutputOptions,CancellationToken)",
            "TPuts(String,Int32,Action`1,TermInfoOutputOptions)",
            "TPuts(String,Int32,Stream,Encoding,TermInfoOutputOptions)",
            "TPuts(String,Int32,TextWriter,TermInfoOutputOptions)",
            "TPutsAsync(String,Int32,Stream,Encoding,TermInfoOutputOptions,CancellationToken)",
            "TPutsAsync(String,Int32,TextWriter,TermInfoOutputOptions,CancellationToken)",
        ];
        string[] actualOutputMethods =
            typeof(TermInfoOutput)
                .GetMethods(
                    BindingFlags.Public
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly)
                .Where(method =>
                    method.GetParameters().Any(
                        parameter =>
                            parameter.ParameterType
                            == typeof(TermInfoOutputOptions)))
                .Select(FormatMethod)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            expectedOutputMethods
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray(),
            actualOutputMethods);
    }

    private static string FormatMethod(MethodInfo method)
    {
        return $"{method.Name}({string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.Name))})";
    }
}
