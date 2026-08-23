using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T37DiscoveryInputTests
{
    [Fact]
    public void OptionsSurfaceMatchesT32Freeze()
    {
        ConstructorInfo constructor =
            Assert.Single(
                typeof(SystemTerminalDescriptionProviderOptions)
                    .GetConstructors(
                        BindingFlags.Public
                        | BindingFlags.Instance));
        ParameterInfo[] parameters =
            constructor.GetParameters();

        Assert.Equal(4, parameters.Length);
        Assert.Equal(typeof(bool), parameters[0].ParameterType);
        Assert.Equal(typeof(bool), parameters[1].ParameterType);
        Assert.Equal(typeof(bool), parameters[2].ParameterType);
        Assert.Equal(
            typeof(CompiledTermInfoParserOptions),
            parameters[3].ParameterType);
        Assert.All(
            parameters,
            parameter => Assert.True(parameter.HasDefaultValue));

        string[] propertyNames =
            typeof(SystemTerminalDescriptionProviderOptions)
                .GetProperties(
                    BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(
            new[]
            {
                "ParserOptions",
                "UseEnvironment",
                "UseSystemDatabases",
                "UseUserDatabase",
            },
            propertyNames);
    }

    [Fact]
    public void OptionsDefaultsAndParserLimitAreSnapshotted()
    {
        CompiledTermInfoParserOptions parserOptions =
            new(4096);
        SystemTerminalDescriptionProviderOptions options =
            new(
                parserOptions: parserOptions);

        Assert.True(options.UseEnvironment);
        Assert.True(options.UseUserDatabase);
        Assert.True(options.UseSystemDatabases);
        Assert.Equal(
            4096,
            options.ParserOptions.MaximumEntrySize);
        Assert.NotSame(
            parserOptions,
            options.ParserOptions);
    }

    [Fact]
    public void OptionsCanDisableEachDiscoverySourceIndependently()
    {
        SystemTerminalDescriptionProviderOptions options =
            new(
                useEnvironment: false,
                useUserDatabase: false,
                useSystemDatabases: false);

        Assert.False(options.UseEnvironment);
        Assert.False(options.UseUserDatabase);
        Assert.False(options.UseSystemDatabases);
    }

    [Fact]
    public void EnvironmentSnapshotCapturesInputsOnce()
    {
        Dictionary<string, string?> environment =
            new(StringComparer.Ordinal)
            {
                ["TERMINFO"] = "hex:0102",
                ["TERMINFO_DIRS"] = "one:two",
            };
        string? home =
            Path.Combine(
                Path.GetTempPath(),
                "t37-home-a");
        string currentDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "t37-current-a");

        SystemTerminalDiscoverySnapshot snapshot =
            SystemTerminalDiscoverySnapshot.Capture(
                new SystemTerminalDescriptionProviderOptions(),
                name => environment[name],
                () => home,
                () => currentDirectory,
                () => TerminalHostPlatform.Linux);

        environment["TERMINFO"] = "b64:AAAA";
        environment["TERMINFO_DIRS"] = "changed";
        home =
            Path.Combine(
                Path.GetTempPath(),
                "t37-home-b");
        currentDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "t37-current-b");

        Assert.Equal(
            "hex:0102",
            snapshot.TermInfo);
        Assert.Equal(
            "one:two",
            snapshot.TermInfoDirs);
        Assert.Equal(
            Path.Combine(
                Path.GetTempPath(),
                "t37-home-a"),
            snapshot.HomeDirectory);
        Assert.Equal(
            Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    "t37-current-a")),
            snapshot.CurrentDirectory);
        Assert.Equal(
            TerminalHostPlatform.Linux,
            snapshot.Platform);
    }

    [Fact]
    public void DisabledEnvironmentAndUserSourcesAreNotRead()
    {
        int environmentReadCount = 0;
        int homeReadCount = 0;

        SystemTerminalDiscoverySnapshot snapshot =
            SystemTerminalDiscoverySnapshot.Capture(
                new SystemTerminalDescriptionProviderOptions(
                    useEnvironment: false,
                    useUserDatabase: false,
                    useSystemDatabases: false),
                _ =>
                {
                    environmentReadCount++;
                    return "unexpected";
                },
                () =>
                {
                    homeReadCount++;
                    return "unexpected";
                },
                () => Path.GetTempPath(),
                () => TerminalHostPlatform.Windows);

        Assert.Equal(0, environmentReadCount);
        Assert.Equal(0, homeReadCount);
        Assert.Null(snapshot.TermInfo);
        Assert.Null(snapshot.TermInfoDirs);
        Assert.Null(snapshot.HomeDirectory);
    }

    [Fact]
    public void HexEncodedTermInfoLoadsThroughCommonParser()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        string encoded =
            "hex:"
            + Convert.ToHexString(entry);

        Assert.True(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "t29-legacy-minimal",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));

        Assert.NotNull(terminal);
        Assert.Equal(
            "t29-legacy-minimal",
            terminal!.Name);
        Assert.Equal<int?>(
            80,
            terminal.GetNumber(
                NumericCapability.Columns));
    }

    [Fact]
    public void Base64EncodedTermInfoLoadsThroughCommonParser()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-extended32.bin");
        string encoded =
            "b64:"
            + Convert.ToBase64String(entry);

        Assert.True(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "t29-extended32",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));

        Assert.NotNull(terminal);
        Assert.Equal<int?>(
            16_777_216,
            terminal!.GetNumber(
                NumericCapability.Colors));
    }

    [Fact]
    public void UrlSafeUnpaddedBase64IsAccepted()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-extended.bin");
        string payload =
            Convert.ToBase64String(entry)
                .TrimEnd('=')
                .Replace(
                    '+',
                    '-')
                .Replace(
                    '/',
                    '_');

        Assert.True(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                "b64:" + payload,
                "t29-extended",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));

        Assert.NotNull(terminal);
        Assert.Equal(
            "t29-extended",
            terminal!.Name);
    }

    [Fact]
    public void EncodedAliasIdentityIsAccepted()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        string encoded =
            "hex:"
            + Convert.ToHexString(entry);

        Assert.True(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "t29lm",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));

        Assert.NotNull(terminal);
        Assert.Equal(
            "t29-legacy-minimal",
            terminal!.Name);
    }

    [Fact]
    public void EncodedIdentityMismatchIsNotAccepted()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        string encoded =
            "hex:"
            + Convert.ToHexString(entry);

        Assert.False(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "different-terminal",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Theory]
    [InlineData("HEX:0011")]
    [InlineData("B64:AAAA")]
    [InlineData("/some/database")]
    public void NonEncodedTermInfoValueIsLeftForT38(
        string value)
    {
        Assert.False(
            SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                value,
                "xterm",
                new CompiledTermInfoParserOptions(),
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Theory]
    [InlineData("hex:0")]
    [InlineData("hex:0g")]
    [InlineData("b64:A")]
    [InlineData("b64:A===")]
    [InlineData("b64:AA*A")]
    public void InvalidEncodedSyntaxIsFormatError(
        string value)
    {
        Assert.Throws<FormatException>(
            () => SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                value,
                "xterm",
                new CompiledTermInfoParserOptions(),
                out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EncodedPayloadIsBoundedBeforeParsing(
        bool useHex)
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        string encoded =
            useHex
                ? "hex:" + Convert.ToHexString(entry)
                : "b64:" + Convert.ToBase64String(entry)
        ;

        Assert.Throws<FormatException>(
            () => SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "t29-legacy-minimal",
                new CompiledTermInfoParserOptions(64),
                out _));
    }

    [Fact]
    public void DecodedMalformedEntryRetainsCompiledFormatException()
    {
        byte[] entry =
            ReadFixture(
                "malformed/unsupported-magic.bin");
        string encoded =
            "hex:"
            + Convert.ToHexString(entry);

        Assert.Throws<CompiledTermInfoFormatException>(
            () => SystemTerminalDiscoveryInputs.TryLoadEncodedTermInfo(
                encoded,
                "xterm",
                new CompiledTermInfoParserOptions(),
                out _));
    }

    [Fact]
    public void WindowsTermInfoDirsSplittingPreservesDriveLetterColons()
    {
        string[] components =
            SystemTerminalDiscoveryInputs.SplitTermInfoDirs(
                @"C:\terminfo;D:\shared\terminfo",
                TerminalHostPlatform.Windows);

        Assert.Equal(
            new[]
            {
                @"C:\terminfo",
                @"D:\shared\terminfo",
            },
            components);
    }

    [Fact]
    public void UnixTermInfoDirsSplittingUsesColon()
    {
        string[] components =
            SystemTerminalDiscoveryInputs.SplitTermInfoDirs(
                "/one:/two::/three",
                TerminalHostPlatform.Linux);

        Assert.Equal(
            new[]
            {
                "/one",
                "/two",
                string.Empty,
                "/three",
            },
            components);
    }

    [Fact]
    public void EmptyComponentsExpandDefaultsAndRootsAreDeduplicated()
    {
        string currentDirectory =
            Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    "icod-t37-base"));
        SystemTerminalDiscoverySnapshot snapshot =
            new(
                termInfo: null,
                termInfoDirs: "explicit::explicit:",
                homeDirectory: null,
                currentDirectory: currentDirectory,
                platform: TerminalHostPlatform.Linux);
        string[] defaults =
        [
            "default-a",
            "default-b",
            "default-a",
        ];

        IReadOnlyList<string> roots =
            SystemTerminalDiscoveryInputs.ResolveTermInfoDirs(
                snapshot,
                defaults);

        Assert.Equal(
            new[]
            {
                Path.GetFullPath(
                    "explicit",
                    currentDirectory),
                Path.GetFullPath(
                    "default-a",
                    currentDirectory),
                Path.GetFullPath(
                    "default-b",
                    currentDirectory),
            },
            roots);
    }

    [Fact]
    public void WindowsRootDeduplicationIsCaseInsensitive()
    {
        string currentDirectory =
            Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    "icod-t37-windows-base"));
        SystemTerminalDiscoverySnapshot snapshot =
            new(
                termInfo: null,
                termInfoDirs: "One;one",
                homeDirectory: null,
                currentDirectory: currentDirectory,
                platform: TerminalHostPlatform.Windows);

        IReadOnlyList<string> roots =
            SystemTerminalDiscoveryInputs.ResolveTermInfoDirs(
                snapshot,
                Array.Empty<string>());

        Assert.Single(roots);
    }

    [Fact]
    public void RelativeRootsUseSnapshottedCurrentDirectory()
    {
        string currentDirectory =
            Path.GetFullPath(
                Path.Combine(
                    Path.GetTempPath(),
                    "icod-t37-relative-base"));
        SystemTerminalDiscoverySnapshot snapshot =
            new(
                termInfo: null,
                termInfoDirs: "relative",
                homeDirectory: null,
                currentDirectory: currentDirectory,
                platform: TerminalHostPlatform.Linux);

        IReadOnlyList<string> roots =
            SystemTerminalDiscoveryInputs.ResolveTermInfoDirs(
                snapshot,
                Array.Empty<string>());

        string root =
            Assert.Single(roots);
        Assert.Equal(
            Path.GetFullPath(
                "relative",
                currentDirectory),
            root);
    }

    [Fact]
    public void UnsetTermInfoDirsDoesNotInjectDefaults()
    {
        SystemTerminalDiscoverySnapshot snapshot =
            new(
                termInfo: null,
                termInfoDirs: null,
                homeDirectory: null,
                currentDirectory: Path.GetFullPath(Path.GetTempPath()),
                platform: TerminalHostPlatform.Linux);

        IReadOnlyList<string> roots =
            SystemTerminalDiscoveryInputs.ResolveTermInfoDirs(
                snapshot,
                new[]
                {
                    "/not-yet-a-t38-default",
                });

        Assert.Empty(roots);
    }

    private static byte[] ReadFixture(
        string relativePath)
    {
        return File.ReadAllBytes(
            Path.Combine(
                AppContext.BaseDirectory,
                "fixtures",
                "compiled-terminfo",
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
    }
}
