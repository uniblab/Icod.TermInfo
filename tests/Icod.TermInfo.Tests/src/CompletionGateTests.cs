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
        Assert.Equal("xterm", TerminalProfiles.Xterm.Name);
        Assert.Equal("vt102", TerminalProfiles.Vt102.Name);
        Assert.Equal("vt220", TerminalProfiles.Vt220.Name);

        Assert.Same(
            TerminalProfiles.Vt100,
            TerminalDatabase.BuiltIn.Load("vt100-am"));
        Assert.Same(
            TerminalProfiles.Vt102,
            TerminalDatabase.BuiltIn.Load("vt102"));
        Assert.Same(
            TerminalProfiles.Vt220,
            TerminalDatabase.BuiltIn.Load("vt220"));
        Assert.Same(
            TerminalProfiles.Vt220,
            TerminalDatabase.BuiltIn.Load("vt200"));
        Assert.Same(
            TerminalProfiles.Xterm,
            TerminalDatabase.BuiltIn.Load("xterm"));

        string[] unsupportedNames =
        [
            "vt102-w",
            "vt220-w",
            "vt220-8",
            "vt220d",
            "vt320",
            "xterm-16color",
            "xterm-88color",
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
            TerminalProfiles.Xterm,
            TerminalProfiles.Vt220,
            TerminalProfiles.Vt102,
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
