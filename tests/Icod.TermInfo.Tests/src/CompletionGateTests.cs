using System.Reflection;
using System.Runtime.InteropServices;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class CompletionGateTests
{
    [Fact]
    public void NativeImportsAreLimitedToPlatformTerminalApis()
    {
        string[] modules =
            typeof(TerminalDescription).Assembly
                .GetTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Static
                    | BindingFlags.Instance))
                .Select(method => method.GetCustomAttribute<DllImportAttribute>())
                .Where(attribute => attribute is not null)
                .Select(attribute => attribute!.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();

        Assert.Equal(
            new[]
            {
                "kernel32.dll",
                "libc",
            },
            modules);
    }

    [Fact]
    public void ExportedSurfaceHasNoMutableProcessGlobalState()
    {
        Type[] exportedTypes =
            typeof(TerminalDescription).Assembly.GetExportedTypes();

        FieldInfo[] mutableStaticFields =
            exportedTypes
                .SelectMany(type => type.GetFields(
                    BindingFlags.Public | BindingFlags.Static))
                .Where(field => !field.IsInitOnly && !field.IsLiteral)
                .ToArray();

        PropertyInfo[] mutableStaticProperties =
            exportedTypes
                .SelectMany(type => type.GetProperties(
                    BindingFlags.Public | BindingFlags.Static))
                .Where(property =>
                    property.SetMethod is { IsPublic: true, IsStatic: true })
                .ToArray();

        Assert.Empty(mutableStaticFields);
        Assert.Empty(mutableStaticProperties);
    }

    [Fact]
    public void BuiltInProfilesRetainReleaseContractBoundaries()
    {
        Assert.Equal<int?>(
            8,
            TerminalProfiles.Ansi.GetNumber(NumericCapability.Colors));
        Assert.Equal<int?>(
            64,
            TerminalProfiles.Ansi.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(
            TerminalProfiles.Vt100.GetNumber(NumericCapability.Colors));
        Assert.Null(
            TerminalProfiles.Vt100.GetNumber(NumericCapability.ColorPairs));
        Assert.Equal("dumb", TerminalProfiles.Dumb.Name);

        Assert.Same(
            TerminalProfiles.Vt100,
            TerminalDatabase.BuiltIn.Load("vt100-am"));

        string[] unsupportedNames =
        [
            "xterm",
            "xterm-256color",
            "screen",
            "tmux",
            "linux",
            "cygwin",
            "rxvt",
        ];

        foreach (string name in unsupportedNames)
        {
            Assert.False(
                TerminalDatabase.BuiltIn.TryLoad(
                    name,
                    out TerminalDescription? terminal));
            Assert.Null(terminal);
        }
    }

    [Fact]
    public void IgnoreModeRemovesPaddingFromEveryBuiltInCapabilityString()
    {
        TerminalDescription[] terminals =
        [
            TerminalProfiles.Ansi,
            TerminalProfiles.Vt100,
            TerminalProfiles.Dumb,
        ];

        foreach (TerminalDescription terminal in terminals)
        {
            foreach (StringCapability capability in Enum.GetValues<StringCapability>())
            {
                string? value = terminal.GetString(capability);
                if (value is null)
                {
                    continue;
                }

                using StringWriter writer = new();
                TermInfoOutput.TPuts(
                    value,
                    affectedLines: 3,
                    writer,
                    PaddingMode.Ignore);

                Assert.DoesNotContain(
                    "$<",
                    writer.ToString(),
                    StringComparison.Ordinal);
            }
        }
    }
}
