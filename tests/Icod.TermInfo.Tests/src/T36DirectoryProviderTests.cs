using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T36DirectoryProviderTests
{
    private const int CompiledHeaderSize = 12;

    [Fact]
    public void AssemblyIdentifiesT36DevelopmentVersion()
    {
        Assembly assembly =
            typeof(DirectoryTerminalDescriptionProvider).Assembly;
        Version? assemblyVersion =
            assembly.GetName().Version;
        string? informationalVersion =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        Assert.NotNull(assemblyVersion);
        Assert.Equal(
            new Version(0, 9, 0, 0),
            assemblyVersion);
        Assert.NotNull(informationalVersion);
        Assert.True(
            informationalVersion!.StartsWith(
                "0.9.0-alpha.5",
                StringComparison.Ordinal),
            $"Unexpected informational version '{informationalVersion}'.");
    }

    [Fact]
    public void PublicSurfaceMatchesT32DirectoryProviderFreeze()
    {
        Assert.True(
            typeof(ITerminalDescriptionProvider).IsAssignableFrom(
                typeof(DirectoryTerminalDescriptionProvider)));

        ConstructorInfo constructor =
            Assert.Single(
                typeof(DirectoryTerminalDescriptionProvider)
                    .GetConstructors(
                        BindingFlags.Public
                        | BindingFlags.Instance));
        ParameterInfo[] constructorParameters =
            constructor.GetParameters();

        Assert.Equal(2, constructorParameters.Length);
        Assert.Equal(
            typeof(string),
            constructorParameters[0].ParameterType);
        Assert.Equal(
            typeof(CompiledTermInfoParserOptions),
            constructorParameters[1].ParameterType);
        Assert.True(constructorParameters[1].HasDefaultValue);
        Assert.Null(constructorParameters[1].DefaultValue);

        PropertyInfo root =
            typeof(DirectoryTerminalDescriptionProvider)
                .GetProperty(
                    nameof(DirectoryTerminalDescriptionProvider.Root))!;
        Assert.Equal(typeof(string), root.PropertyType);
        Assert.True(root.CanRead);
        Assert.False(root.CanWrite);

        MethodInfo tryLoad =
            Assert.Single(
                typeof(DirectoryTerminalDescriptionProvider)
                    .GetMethods(
                        BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.DeclaredOnly)
                    .Where(method => !method.IsSpecialName));
        Assert.Equal(
            nameof(DirectoryTerminalDescriptionProvider.TryLoad),
            tryLoad.Name);

        ParameterInfo[] tryLoadParameters =
            tryLoad.GetParameters();
        Assert.Equal(2, tryLoadParameters.Length);
        Assert.Equal(
            typeof(string),
            tryLoadParameters[0].ParameterType);
        Assert.Equal(
            typeof(TerminalDescription).MakeByRefType(),
            tryLoadParameters[1].ParameterType);

        NotNullWhenAttribute? notNullWhen =
            tryLoadParameters[1]
                .GetCustomAttribute<NotNullWhenAttribute>();
        Assert.NotNull(notNullWhen);
        Assert.True(notNullWhen!.ReturnValue);
    }

    [Fact]
    public void ConstructorCanonicalizesRootAndSnapshotsParserOptions()
    {
        using TemporaryDirectory temporary = new();
        string suppliedRoot =
            Path.Combine(
                temporary.Root,
                "not-created",
                "..");
        CompiledTermInfoParserOptions options =
            new(64);

        DirectoryTerminalDescriptionProvider provider =
            new(
                suppliedRoot,
                options);

        Assert.Equal(
            Path.GetFullPath(suppliedRoot),
            provider.Root);

        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        WriteLiteralCandidate(
            provider.Root,
            "t29-legacy-minimal",
            entry);

        CompiledTermInfoFormatException exception =
            Assert.Throws<CompiledTermInfoFormatException>(
                () => provider.TryLoad(
                    "t29-legacy-minimal",
                    out _));

        Assert.Equal("entry", exception.Section);
        Assert.Equal(-1, exception.Offset);
    }

    [Fact]
    public void ConstructorRejectsNullOrWhitespaceRoot()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DirectoryTerminalDescriptionProvider(
                null!));
        Assert.Throws<ArgumentException>(
            () => new DirectoryTerminalDescriptionProvider(
                "   "));
    }

    [Fact]
    public void LiteralFirstCharacterLayoutLoadsCanonicalName()
    {
        using TemporaryDirectory temporary = new();
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        WriteLiteralCandidate(
            temporary.Root,
            "t29-legacy-minimal",
            entry);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        TerminalDescription terminal =
            Load(
                provider,
                "t29-legacy-minimal");

        Assert.Equal(
            "t29-legacy-minimal",
            terminal.Name);
        Assert.Equal<int?>(
            80,
            terminal.GetNumber(
                NumericCapability.Columns));
    }

    [Fact]
    public void LowercaseHexadecimalFirstCharacterLayoutLoadsEntry()
    {
        using TemporaryDirectory temporary = new();
        byte[] entry =
            CreateRenamedMinimalEntry();
        string name =
            "n29-legacy-minimal";

        WriteCandidate(
            temporary.Root,
            "6e",
            name,
            entry);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        TerminalDescription terminal =
            Load(
                provider,
                name);

        Assert.Equal(name, terminal.Name);
        Assert.Equal(
            new[] { "n29lm" },
            terminal.Aliases);
    }

    [Fact]
    public void LiteralCandidatePrecedesHexadecimalCandidate()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        byte[] literal =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        byte[] hexadecimal =
            (byte[])literal.Clone();
        SetLegacyColumns(
            hexadecimal,
            99);

        WriteLiteralCandidate(
            temporary.Root,
            name,
            literal);
        WriteCandidate(
            temporary.Root,
            "74",
            name,
            hexadecimal);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        TerminalDescription terminal =
            Load(
                provider,
                name);

        Assert.Equal<int?>(
            80,
            terminal.GetNumber(
                NumericCapability.Columns));
    }

    [Fact]
    public void AliasPathRequiresAndAcceptsExactParsedAlias()
    {
        using TemporaryDirectory temporary = new();
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");

        WriteLiteralCandidate(
            temporary.Root,
            "t29lm",
            entry);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        TerminalDescription terminal =
            Load(
                provider,
                "t29lm");

        Assert.Equal(
            "t29-legacy-minimal",
            terminal.Name);
        Assert.Contains(
            "t29lm",
            terminal.Aliases);
    }

    [Fact]
    public void PresentEntryWithMismatchedIdentityIsAnError()
    {
        using TemporaryDirectory temporary = new();
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        WriteLiteralCandidate(
            temporary.Root,
            "wrong-name",
            entry);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        InvalidDataException exception =
            Assert.Throws<InvalidDataException>(
                () => provider.TryLoad(
                    "wrong-name",
                    out _));

        Assert.Contains(
            "does not declare requested name",
            exception.Message);
    }

    [Fact]
    public void MissingEntryIsCleanMissAndIsNotNegativeCached()
    {
        using TemporaryDirectory temporary = new();
        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        string name =
            "t29-legacy-minimal";

        Assert.False(
            provider.TryLoad(
                name,
                out TerminalDescription? missing));
        Assert.Null(missing);

        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        WriteLiteralCandidate(
            temporary.Root,
            name,
            entry);

        TerminalDescription terminal =
            Load(
                provider,
                name);
        Assert.Equal(name, terminal.Name);
    }

    [Fact]
    public void MalformedEntryPropagatesCompiledFormatFailure()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        byte[] malformed =
            ReadFixture(
                "malformed/unsupported-magic.bin");
        WriteLiteralCandidate(
            temporary.Root,
            name,
            malformed);

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Assert.Throws<CompiledTermInfoFormatException>(
            () => provider.TryLoad(
                name,
                out _));
    }

    [Fact]
    public void FailureIsNotCachedAsMiss()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        string path =
            WriteLiteralCandidate(
                temporary.Root,
                name,
                ReadFixture(
                    "malformed/unsupported-magic.bin"));

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Assert.Throws<CompiledTermInfoFormatException>(
            () => provider.TryLoad(
                name,
                out _));

        File.WriteAllBytes(
            path,
            ReadFixture(
                "compiled/t29-legacy-minimal.bin"));

        TerminalDescription terminal =
            Load(
                provider,
                name);
        Assert.Equal(name, terminal.Name);
    }

    [Fact]
    public void SuccessfulLookupIsCachedPerProviderInstance()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        string path =
            WriteLiteralCandidate(
                temporary.Root,
                name,
                ReadFixture(
                    "compiled/t29-legacy-minimal.bin"));

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        TerminalDescription first =
            Load(
                provider,
                name);

        File.WriteAllBytes(
            path,
            ReadFixture(
                "malformed/unsupported-magic.bin"));

        TerminalDescription second =
            Load(
                provider,
                name);

        Assert.Same(first, second);
    }

    [Fact]
    public void SeparateProvidersDoNotShareSuccessfulEntries()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        string path =
            WriteLiteralCandidate(
                temporary.Root,
                name,
                ReadFixture(
                    "compiled/t29-legacy-minimal.bin"));

        DirectoryTerminalDescriptionProvider firstProvider =
            new(temporary.Root);
        TerminalDescription first =
            Load(
                firstProvider,
                name);
        Assert.Equal<int?>(
            80,
            first.GetNumber(
                NumericCapability.Columns));

        byte[] changed =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        SetLegacyColumns(
            changed,
            99);
        File.WriteAllBytes(
            path,
            changed);

        DirectoryTerminalDescriptionProvider secondProvider =
            new(temporary.Root);
        TerminalDescription second =
            Load(
                secondProvider,
                name);

        Assert.Equal<int?>(
            99,
            second.GetNumber(
                NumericCapability.Columns));
        Assert.Same(
            first,
            Load(
                firstProvider,
                name));
    }

    [Fact]
    public async Task ConcurrentLoadsPublishOneCachedDescription()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        WriteLiteralCandidate(
            temporary.Root,
            name,
            ReadFixture(
                "compiled/t29-legacy-minimal.bin"));

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        Task<TerminalDescription>[] tasks =
            Enumerable
                .Range(
                    0,
                    16)
                .Select(
                    _ => Task.Run(
                        () => Load(
                            provider,
                            name)))
                .ToArray();

        TerminalDescription[] terminals =
            await Task.WhenAll(tasks);

        Assert.All(
            terminals,
            terminal => Assert.Same(
                terminals[0],
                terminal));
    }

    [Fact]
    public void PresentCandidateFailureIsNotConvertedToCleanMiss()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        string directory =
            Path.Combine(
                temporary.Root,
                name[0].ToString());
        Directory.CreateDirectory(
            Path.Combine(
                directory,
                name));

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Exception? exception =
            Record.Exception(
                () => provider.TryLoad(
                    name,
                    out _));

        Assert.NotNull(exception);
    }

    [Fact]
    public void ExactLookupDoesNotRecursivelyScanRoot()
    {
        using TemporaryDirectory temporary = new();
        string name =
            "t29-legacy-minimal";
        string unrelated =
            Path.Combine(
                temporary.Root,
                "unrelated",
                "nested");
        Directory.CreateDirectory(unrelated);
        File.WriteAllBytes(
            Path.Combine(
                unrelated,
                name),
            ReadFixture(
                "compiled/t29-legacy-minimal.bin"));

        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Assert.False(
            provider.TryLoad(
                name,
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("../xterm")]
    [InlineData("..\\xterm")]
    [InlineData("xterm/../vt100")]
    [InlineData("xterm\\..\\vt100")]
    [InlineData("C:\\xterm")]
    public void UnsafeTerminalNamesAreRejectedBeforePathConstruction(
        string name)
    {
        using TemporaryDirectory temporary = new();
        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Assert.Throws<ArgumentException>(
            () => provider.TryLoad(
                name,
                out _));
    }

    [Fact]
    public void NullAndNulTerminalNamesAreRejected()
    {
        using TemporaryDirectory temporary = new();
        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);

        Assert.Throws<ArgumentNullException>(
            () => provider.TryLoad(
                null!,
                out _));
        Assert.Throws<ArgumentException>(
            () => provider.TryLoad(
                "bad\0name",
                out _));
    }

    [Fact]
    public void RootedTerminalNameIsRejected()
    {
        using TemporaryDirectory temporary = new();
        DirectoryTerminalDescriptionProvider provider =
            new(temporary.Root);
        string rootedName =
            Path.GetFullPath(
                Path.Combine(
                    temporary.Root,
                    "xterm"));

        Assert.Throws<ArgumentException>(
            () => provider.TryLoad(
                rootedName,
                out _));
    }

    private static TerminalDescription Load(
        DirectoryTerminalDescriptionProvider provider,
        string name)
    {
        Assert.True(
            provider.TryLoad(
                name,
                out TerminalDescription? terminal));
        return Assert.IsType<TerminalDescription>(
            terminal);
    }

    private static byte[] ReadFixture(string relativePath)
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

    private static string WriteLiteralCandidate(
        string root,
        string name,
        byte[] entry)
    {
        return WriteCandidate(
            root,
            name[0].ToString(),
            name,
            entry);
    }

    private static string WriteCandidate(
        string root,
        string directoryName,
        string name,
        byte[] entry)
    {
        string directory =
            Path.Combine(
                root,
                directoryName);
        Directory.CreateDirectory(directory);

        string path =
            Path.Combine(
                directory,
                name);
        File.WriteAllBytes(
            path,
            entry);
        return path;
    }

    private static byte[] CreateRenamedMinimalEntry()
    {
        byte[] entry =
            ReadFixture(
                "compiled/t29-legacy-minimal.bin");
        int namesSize =
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.AsSpan(
                    2,
                    sizeof(ushort)));
        Span<byte> names =
            entry.AsSpan(
                CompiledHeaderSize,
                namesSize);

        int firstSeparator =
            names.IndexOf((byte)'|');
        if (firstSeparator <= 0
            || firstSeparator + 1 >= names.Length)
        {
            throw new InvalidDataException(
                "The minimal fixture does not contain the expected alias layout.");
        }

        names[0] = (byte)'n';
        names[firstSeparator + 1] =
            (byte)'n';
        return entry;
    }

    private static void SetLegacyColumns(
        byte[] entry,
        int columns)
    {
        ArgumentNullException.ThrowIfNull(entry);

        int names =
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.AsSpan(
                    2,
                    sizeof(ushort)));
        int booleans =
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.AsSpan(
                    4,
                    sizeof(ushort)));
        int numericOffset =
            CompiledHeaderSize
            + names
            + booleans;

        if ((numericOffset & 1) != 0)
        {
            numericOffset++;
        }

        BinaryPrimitives.WriteInt16LittleEndian(
            entry.AsSpan(
                numericOffset,
                sizeof(short)),
            checked((short)columns));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Root =
                Path.Combine(
                    Path.GetTempPath(),
                    "icod-terminfo-t36-"
                    + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(
                    Root,
                    recursive: true);
            }
        }
    }
}
