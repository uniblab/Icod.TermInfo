using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T29BinaryReadinessTests
{
    private const ushort LegacyMagic = 0x011A;
    private const ushort ExtendedNumberMagic = 0x021E;

    [Fact]
    public void FixtureManifestPinsHashesAndGeneratorProvenance()
    {
        using JsonDocument manifest = LoadManifest();
        JsonElement root = manifest.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        JsonElement generator = root.GetProperty("generator");
        Assert.Equal("tic", generator.GetProperty("tool").GetString());
        Assert.Equal(
            "ncurses 6.5.20250216",
            generator.GetProperty("version").GetString());
        Assert.False(generator.GetProperty("normalTestsRequireGenerator").GetBoolean());

        VerifyHashes(root.GetProperty("fixtures"), "binary", "sha256");
        VerifyHashes(
            root.GetProperty("fixtures"),
            "source",
            "sourceSha256",
            normalizeTextLineEndings: true);
        VerifyHashes(
            root.GetProperty("adversarialSeeds"),
            "binary",
            "sha256");
    }

    [Fact]
    public void ValidFixtureHeadersUseFrozenLittleEndianFormats()
    {
        using JsonDocument manifest = LoadManifest();

        foreach (JsonElement fixture in manifest.RootElement
            .GetProperty("fixtures")
            .EnumerateArray())
        {
            byte[] bytes = ReadFixture(fixture.GetProperty("binary").GetString()!);
            CompiledHeader header = ReadHeader(bytes);
            JsonElement expectedHeader = fixture.GetProperty("header");
            string magicOctal = fixture.GetProperty("magicOctal").GetString()!;

            ushort expectedMagic = magicOctal switch
            {
                "0432" => LegacyMagic,
                "01036" => ExtendedNumberMagic,
                _ => throw new InvalidDataException(
                    $"Unknown fixture magic contract '{magicOctal}'."),
            };

            Assert.Equal(expectedMagic, header.Magic);
            Assert.Equal(
                expectedHeader.GetProperty("namesSize").GetInt32(),
                header.NamesSize);
            Assert.Equal(
                expectedHeader.GetProperty("booleanCount").GetInt32(),
                header.BooleanCount);
            Assert.Equal(
                expectedHeader.GetProperty("numericCount").GetInt32(),
                header.NumericCount);
            Assert.Equal(
                expectedHeader.GetProperty("stringCount").GetInt32(),
                header.StringCount);
            Assert.Equal(
                expectedHeader.GetProperty("stringTableSize").GetInt32(),
                header.StringTableSize);

            int unalignedNumericOffset =
                12 + header.NamesSize + header.BooleanCount;
            bool hasAlignmentByte = (unalignedNumericOffset & 1) != 0;
            Assert.Equal(
                expectedHeader.GetProperty("numericAlignmentByte").GetBoolean(),
                hasAlignmentByte);

            if (hasAlignmentByte)
            {
                Assert.Equal<byte>(0, bytes[unalignedNumericOffset]);
            }

            JsonElement expected = fixture.GetProperty("expected");
            string expectedNames = string.Join(
                "|",
                new[]
                {
                    expected.GetProperty("name").GetString()!,
                }
                .Concat(
                    expected.GetProperty("aliases")
                        .EnumerateArray()
                        .Select(item => item.GetString()!))
                .Append(expected.GetProperty("description").GetString()!));
            Assert.Equal(expectedNames, ReadNames(bytes, header));
        }
    }

    [Fact]
    public void LegacyEdgeFixturePinsAbsentCanceledPaddingAndHighByteSemantics()
    {
        byte[] bytes = ReadFixture("compiled/t29-legacy-edge.bin");
        CompiledHeader header = ReadHeader(bytes);

        int bwIndex =
            StandardCapabilityCatalog.GetMetadata(
                BooleanCapability.AutoLeftMargin).BinaryIndex;
        int linesIndex =
            StandardCapabilityCatalog.GetMetadata(
                NumericCapability.Lines).BinaryIndex;
        int belIndex =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.Bell).BinaryIndex;
        int kbsIndex =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.KeyBackspace).BinaryIndex;
        int clearIndex =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.ClearScreen).BinaryIndex;
        int cupIndex =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.CursorAddress).BinaryIndex;

        Assert.Equal<byte>(0xFE, ReadBooleanByte(bytes, header, bwIndex));
        Assert.Equal(-2, ReadNumeric(bytes, header, linesIndex));
        Assert.Equal(-2, ReadStringOffset(bytes, header, belIndex));
        Assert.Equal("\u0080", ReadString(bytes, header, kbsIndex));
        Assert.Equal(
            "\u001b[H\u001b[2J$<5>",
            ReadString(bytes, header, clearIndex));
        Assert.Equal(
            "\u001b[%i%p1%d;%p2%dH$<2*>",
            ReadString(bytes, header, cupIndex));
    }

    [Fact]
    public void LegacyMinimalFixturePinsOrdinaryAbsentSentinels()
    {
        byte[] bytes = ReadFixture("compiled/t29-legacy-minimal.bin");
        CompiledHeader header = ReadHeader(bytes);

        int bwIndex =
            StandardCapabilityCatalog.GetMetadata(
                BooleanCapability.AutoLeftMargin).BinaryIndex;
        int tabWidthIndex =
            StandardCapabilityCatalog.GetMetadata(
                NumericCapability.InitialTabWidth).BinaryIndex;
        int bellIndex =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.Bell).BinaryIndex;

        Assert.Equal<byte>(0, ReadBooleanByte(bytes, header, bwIndex));
        Assert.Equal(-1, ReadNumeric(bytes, header, tabWidthIndex));
        Assert.Equal(-1, ReadStringOffset(bytes, header, bellIndex));
    }

    [Fact]
    public void ExtendedFixturesPinNcursesCountsAndThirtyTwoBitNumbers()
    {
        using JsonDocument manifest = LoadManifest();
        JsonElement fixtures = manifest.RootElement.GetProperty("fixtures");

        foreach (JsonElement fixture in fixtures.EnumerateArray())
        {
            if (!fixture.TryGetProperty(
                    "extendedHeader",
                    out JsonElement expectedExtension))
            {
                continue;
            }

            byte[] bytes = ReadFixture(fixture.GetProperty("binary").GetString()!);
            CompiledHeader header = ReadHeader(bytes);
            ExtendedHeader extension = ReadExtendedHeader(bytes, header);

            Assert.Equal(
                expectedExtension.GetProperty("booleanCount").GetInt32(),
                extension.BooleanCount);
            Assert.Equal(
                expectedExtension.GetProperty("numericCount").GetInt32(),
                extension.NumericCount);
            Assert.Equal(
                expectedExtension.GetProperty("stringCount").GetInt32(),
                extension.StringCount);
            Assert.Equal(
                expectedExtension.GetProperty("stringTableItemCount").GetInt32(),
                extension.StringTableItemCount);
            Assert.Equal(
                expectedExtension.GetProperty("stringTableSize").GetInt32(),
                extension.StringTableSize);
        }

        byte[] extended = ReadFixture("compiled/t29-extended.bin");
        CompiledHeader extendedHeader = ReadHeader(extended);
        Assert.Equal(12345, ReadFirstExtendedNumeric(extended, extendedHeader));

        byte[] extended32 = ReadFixture("compiled/t29-extended32.bin");
        CompiledHeader extended32Header = ReadHeader(extended32);
        Assert.Equal(
            2147483640,
            ReadFirstExtendedNumeric(extended32, extended32Header));
        int colorsIndex =
            StandardCapabilityCatalog.GetMetadata(
                NumericCapability.Colors).BinaryIndex;
        int pairsIndex =
            StandardCapabilityCatalog.GetMetadata(
                NumericCapability.ColorPairs).BinaryIndex;

        Assert.Equal(ExtendedNumberMagic, extended32Header.Magic);
        Assert.Equal(16777216, ReadNumeric(extended32, extended32Header, colorsIndex));
        Assert.Equal(65536, ReadNumeric(extended32, extended32Header, pairsIndex));
    }

    [Fact]
    public void FixtureSemanticManifestsFitTheCompletedPublicModel()
    {
        using JsonDocument manifest = LoadManifest();

        foreach (JsonElement fixture in manifest.RootElement
            .GetProperty("fixtures")
            .EnumerateArray())
        {
            JsonElement expected = fixture.GetProperty("expected");
            TerminalDescription terminal = BuildExpectedTerminal(expected);

            Assert.Equal(expected.GetProperty("name").GetString(), terminal.Name);
            Assert.Equal(
                expected.GetProperty("description").GetString(),
                terminal.Description);

            string[] aliases =
                expected.GetProperty("aliases")
                    .EnumerateArray()
                    .Select(item => item.GetString()!)
                    .ToArray();
            Assert.Equal(aliases, terminal.Aliases);

            AssertExpectedStandardValues(terminal, expected);
            AssertExpectedExtendedValues(terminal, expected);
            AssertExpectedAbsentOrCanceledValues(terminal, expected);
        }

        TerminalDescription edge =
            BuildExpectedTerminal(
                FindFixture("legacy-edge").GetProperty("expected"));
        Assert.Equal(
            "\u001b[2;3H$<2*>",
            edge.Expand(StringCapability.CursorAddress, 1, 2));
        Assert.Equal(
            "\u0080",
            edge.GetString(StringCapability.KeyBackspace));
    }

    [Fact]
    public void ExtendedStandardNameCollisionHasAFrozenRejectionTarget()
    {
        byte[] collision =
            ReadFixture("malformed/extended-standard-name-collision.bin");
        byte[] marker = Encoding.ASCII.GetBytes("cup\0");

        Assert.True(collision.AsSpan().IndexOf(marker) >= 0);

        TerminalDescriptionBuilder builder =
            new("t29-standard-name-collision");
        Assert.Throws<ArgumentException>(
            () => builder.SetExtendedBoolean("cup"));
    }

    [Fact]
    public void ProviderFalseMeansCleanMissAndFailuresAreNotSwallowed()
    {
        TerminalDescription hit =
            new TerminalDescriptionBuilder("t29-provider-hit").Build();
        MissProvider miss = new();
        HitProvider success = new(hit);
        TerminalDatabase database =
            new(new ITerminalDescriptionProvider[] { miss, success });

        Assert.True(database.TryLoad("t29-provider-hit", out TerminalDescription? found));
        Assert.Same(hit, found);
        Assert.Equal(1, miss.CallCount);
        Assert.Equal(1, success.CallCount);

        ThrowingProvider failure = new();
        HitProvider neverReached = new(hit);
        TerminalDatabase failingDatabase =
            new(new ITerminalDescriptionProvider[] { failure, neverReached });

        IOException exception = Assert.Throws<IOException>(
            () => failingDatabase.TryLoad("t29-provider-hit", out _));
        Assert.Equal("synthetic provider failure", exception.Message);
        Assert.Equal(0, neverReached.CallCount);
    }

    [Fact]
    public void ProductionAssemblyContainsNoPrematureSystemProviderImplementation()
    {
        Assembly assembly = typeof(TerminalDatabase).Assembly;
        string[] reservedTypeNames =
        [
            "SystemTerminalDescriptionProviderOptions",
            "SystemTerminalDescriptionProvider",
        ];
        HashSet<string> actualTypeNames =
            assembly.GetTypes()
                .Select(type => type.Name)
                .ToHashSet(StringComparer.Ordinal);

        Assert.All(
            reservedTypeNames,
            reserved => Assert.DoesNotContain(
                reserved,
                actualTypeNames));
        Assert.DoesNotContain(
            assembly.GetManifestResourceNames(),
            name => name.Contains(
                "fixture",
                StringComparison.OrdinalIgnoreCase));

        byte[] assemblyImage = File.ReadAllBytes(assembly.Location);
        foreach (string forbiddenLiteral in new[]
        {
            "TERMINFO",
            "TERMINFO_DIRS",
            "/usr/share/terminfo",
        })
        {
            Assert.True(
                assemblyImage.AsSpan().IndexOf(
                    Encoding.UTF8.GetBytes(forbiddenLiteral)) < 0,
                $"Production assembly contains reserved 0.9 literal '{forbiddenLiteral}'.");
            Assert.True(
                assemblyImage.AsSpan().IndexOf(
                    Encoding.Unicode.GetBytes(forbiddenLiteral)) < 0,
                $"Production assembly contains reserved 0.9 literal '{forbiddenLiteral}'.");
        }
    }

    private static JsonDocument LoadManifest()
    {
        return JsonDocument.Parse(
            File.ReadAllText(FixturePath("manifests/manifest.json")));
    }

    private static JsonElement FindFixture(string id)
    {
        using JsonDocument manifest = LoadManifest();
        foreach (JsonElement fixture in manifest.RootElement
            .GetProperty("fixtures")
            .EnumerateArray())
        {
            if (string.Equals(
                    id,
                    fixture.GetProperty("id").GetString(),
                    StringComparison.Ordinal))
            {
                return fixture.Clone();
            }
        }

        throw new InvalidOperationException($"Fixture '{id}' is not in the manifest.");
    }

    private static string FixturePath(string relativePath)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "fixtures",
            "compiled-terminfo",
            relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static byte[] ReadFixture(string relativePath)
    {
        return File.ReadAllBytes(FixturePath(relativePath));
    }

    private static void VerifyHashes(
        JsonElement entries,
        string pathProperty,
        string hashProperty,
        bool normalizeTextLineEndings = false)
    {
        foreach (JsonElement entry in entries.EnumerateArray())
        {
            string relativePath = entry.GetProperty(pathProperty).GetString()!;
            string expected = entry.GetProperty(hashProperty).GetString()!;
            byte[] bytes = File.ReadAllBytes(FixturePath(relativePath));
            if (normalizeTextLineEndings)
            {
                string text =
                    Encoding.UTF8.GetString(bytes)
                        .Replace("\r\n", "\n", StringComparison.Ordinal)
                        .Replace('\r', '\n');
                bytes = Encoding.UTF8.GetBytes(text);
            }

            string actual =
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            Assert.Equal(expected, actual);
        }
    }

    private static CompiledHeader ReadHeader(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 12)
        {
            throw new InvalidDataException("The fixture does not contain a six-short header.");
        }

        return new CompiledHeader(
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[0..2]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[2..4]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..6]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[10..12]));
    }

    private static ExtendedHeader ReadExtendedHeader(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header)
    {
        int offset = GetStringTableOffset(header) + header.StringTableSize;
        if ((offset & 1) != 0)
        {
            offset++;
        }

        if (bytes.Length < offset + 10)
        {
            throw new InvalidDataException("The fixture has no complete extended header.");
        }

        return new ExtendedHeader(
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..(offset + 2)]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[(offset + 2)..(offset + 4)]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[(offset + 4)..(offset + 6)]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[(offset + 6)..(offset + 8)]),
            BinaryPrimitives.ReadUInt16LittleEndian(bytes[(offset + 8)..(offset + 10)]));
    }


    private static int ReadFirstExtendedNumeric(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header)
    {
        ExtendedHeader extension = ReadExtendedHeader(bytes, header);
        if (extension.NumericCount < 1)
        {
            throw new InvalidDataException(
                "The fixture has no extended numeric capability.");
        }

        int offset = GetStringTableOffset(header) + header.StringTableSize;
        if ((offset & 1) != 0)
        {
            offset++;
        }

        offset += 10 + extension.BooleanCount;
        if ((offset & 1) != 0)
        {
            offset++;
        }

        if (header.Magic == ExtendedNumberMagic)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(
                bytes[offset..(offset + 4)]);
        }

        return BinaryPrimitives.ReadInt16LittleEndian(
            bytes[offset..(offset + 2)]);
    }

    private static string ReadNames(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header)
    {
        ReadOnlySpan<byte> names = bytes.Slice(12, header.NamesSize);
        if (names.Length == 0 || names[^1] != 0)
        {
            throw new InvalidDataException("Fixture names section is not NUL-terminated.");
        }

        return Encoding.ASCII.GetString(names[..^1]);
    }

    private static byte ReadBooleanByte(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header,
        int binaryIndex)
    {
        Assert.InRange(binaryIndex, 0, header.BooleanCount - 1);
        return bytes[12 + header.NamesSize + binaryIndex];
    }

    private static int ReadNumeric(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header,
        int binaryIndex)
    {
        Assert.InRange(binaryIndex, 0, header.NumericCount - 1);

        int width =
            (header.Magic == ExtendedNumberMagic)
                ? 4
                : 2
        ;
        int offset = GetNumericOffset(header) + (binaryIndex * width);

        return (width == 4)
            ? BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..(offset + 4)])
            : BinaryPrimitives.ReadInt16LittleEndian(bytes[offset..(offset + 2)])
        ;
    }

    private static int ReadStringOffset(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header,
        int binaryIndex)
    {
        Assert.InRange(binaryIndex, 0, header.StringCount - 1);

        int offset = GetStringOffsetTableOffset(header) + (binaryIndex * 2);
        return BinaryPrimitives.ReadInt16LittleEndian(bytes[offset..(offset + 2)]);
    }

    private static string? ReadString(
        ReadOnlySpan<byte> bytes,
        CompiledHeader header,
        int binaryIndex)
    {
        int relativeOffset = ReadStringOffset(bytes, header, binaryIndex);
        if (relativeOffset < 0)
        {
            return null;
        }

        int start = GetStringTableOffset(header) + relativeOffset;
        int terminator = bytes[start..].IndexOf((byte)0);
        if (terminator < 0)
        {
            throw new InvalidDataException("Fixture string is not NUL-terminated.");
        }

        return Encoding.Latin1.GetString(bytes.Slice(start, terminator));
    }

    private static int GetNumericOffset(CompiledHeader header)
    {
        int offset = 12 + header.NamesSize + header.BooleanCount;
        return ((offset & 1) == 0)
            ? offset
            : offset + 1
        ;
    }

    private static int GetStringOffsetTableOffset(CompiledHeader header)
    {
        int numericWidth =
            (header.Magic == ExtendedNumberMagic)
                ? 4
                : 2
        ;
        return GetNumericOffset(header) + (header.NumericCount * numericWidth);
    }

    private static int GetStringTableOffset(CompiledHeader header)
    {
        return GetStringOffsetTableOffset(header) + (header.StringCount * 2);
    }

    private static TerminalDescription BuildExpectedTerminal(JsonElement expected)
    {
        TerminalDescriptionBuilder builder =
            new(expected.GetProperty("name").GetString()!);

        builder.SetDescription(expected.GetProperty("description").GetString());
        foreach (JsonElement alias in expected.GetProperty("aliases").EnumerateArray())
        {
            builder.AddAlias(alias.GetString()!);
        }

        if (expected.TryGetProperty("booleans", out JsonElement booleans))
        {
            foreach (JsonProperty property in booleans.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetBoolean(
                        property.Name,
                        out StandardCapabilityMetadata<BooleanCapability>? metadata));
                builder.SetBoolean(metadata!.Capability, property.Value.GetBoolean());
            }
        }

        if (expected.TryGetProperty("numbers", out JsonElement numbers))
        {
            foreach (JsonProperty property in numbers.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetNumeric(
                        property.Name,
                        out StandardCapabilityMetadata<NumericCapability>? metadata));
                builder.SetNumber(metadata!.Capability, property.Value.GetInt32());
            }
        }

        if (expected.TryGetProperty("strings", out JsonElement strings))
        {
            foreach (JsonProperty property in strings.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetString(
                        property.Name,
                        out StandardCapabilityMetadata<StringCapability>? metadata));
                builder.SetString(metadata!.Capability, property.Value.GetString()!);
            }
        }

        if (expected.TryGetProperty(
                "extendedBooleans",
                out JsonElement extendedBooleans))
        {
            foreach (JsonProperty property in extendedBooleans.EnumerateObject())
            {
                builder.SetExtendedBoolean(
                    property.Name,
                    property.Value.GetBoolean());
            }
        }

        if (expected.TryGetProperty(
                "extendedNumbers",
                out JsonElement extendedNumbers))
        {
            foreach (JsonProperty property in extendedNumbers.EnumerateObject())
            {
                builder.SetExtendedNumber(
                    property.Name,
                    property.Value.GetInt32());
            }
        }

        if (expected.TryGetProperty(
                "extendedStrings",
                out JsonElement extendedStrings))
        {
            foreach (JsonProperty property in extendedStrings.EnumerateObject())
            {
                builder.SetExtendedString(
                    property.Name,
                    property.Value.GetString()!);
            }
        }

        return builder.Build();
    }

    private static void AssertExpectedStandardValues(
        TerminalDescription terminal,
        JsonElement expected)
    {
        if (expected.TryGetProperty("booleans", out JsonElement booleans))
        {
            foreach (JsonProperty property in booleans.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetBoolean(
                        property.Name,
                        out StandardCapabilityMetadata<BooleanCapability>? metadata));
                Assert.Equal(
                    property.Value.GetBoolean(),
                    terminal.GetBoolean(metadata!.Capability));
            }
        }

        if (expected.TryGetProperty("numbers", out JsonElement numbers))
        {
            foreach (JsonProperty property in numbers.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetNumeric(
                        property.Name,
                        out StandardCapabilityMetadata<NumericCapability>? metadata));
                Assert.Equal<int?>(
                    property.Value.GetInt32(),
                    terminal.GetNumber(metadata!.Capability));
            }
        }

        if (expected.TryGetProperty("strings", out JsonElement strings))
        {
            foreach (JsonProperty property in strings.EnumerateObject())
            {
                Assert.True(
                    StandardCapabilityCatalog.TryGetString(
                        property.Name,
                        out StandardCapabilityMetadata<StringCapability>? metadata));
                Assert.Equal(
                    property.Value.GetString(),
                    terminal.GetString(metadata!.Capability));
            }
        }
    }

    private static void AssertExpectedExtendedValues(
        TerminalDescription terminal,
        JsonElement expected)
    {
        if (expected.TryGetProperty(
                "extendedBooleans",
                out JsonElement extendedBooleans))
        {
            foreach (JsonProperty property in extendedBooleans.EnumerateObject())
            {
                Assert.True(
                    terminal.TryGetExtendedBoolean(
                        property.Name,
                        out bool value));
                Assert.Equal(property.Value.GetBoolean(), value);
            }
        }

        if (expected.TryGetProperty(
                "extendedNumbers",
                out JsonElement extendedNumbers))
        {
            foreach (JsonProperty property in extendedNumbers.EnumerateObject())
            {
                Assert.True(
                    terminal.TryGetExtendedNumber(
                        property.Name,
                        out int value));
                Assert.Equal(property.Value.GetInt32(), value);
            }
        }

        if (expected.TryGetProperty(
                "extendedStrings",
                out JsonElement extendedStrings))
        {
            foreach (JsonProperty property in extendedStrings.EnumerateObject())
            {
                Assert.True(
                    terminal.TryGetExtendedString(
                        property.Name,
                        out string? value));
                Assert.Equal(property.Value.GetString(), value);
            }
        }
    }

    private static void AssertExpectedAbsentOrCanceledValues(
        TerminalDescription terminal,
        JsonElement expected)
    {
        AssertMissingBooleans(terminal, expected, "absentBooleans");
        AssertMissingBooleans(terminal, expected, "canceledBooleans");
        AssertMissingNumbers(terminal, expected, "absentNumbers");
        AssertMissingNumbers(terminal, expected, "canceledNumbers");
        AssertMissingStrings(terminal, expected, "absentStrings");
        AssertMissingStrings(terminal, expected, "canceledStrings");
    }

    private static void AssertMissingBooleans(
        TerminalDescription terminal,
        JsonElement expected,
        string propertyName)
    {
        if (!expected.TryGetProperty(propertyName, out JsonElement names))
        {
            return;
        }

        foreach (JsonElement name in names.EnumerateArray())
        {
            Assert.True(
                StandardCapabilityCatalog.TryGetBoolean(
                    name.GetString()!,
                    out StandardCapabilityMetadata<BooleanCapability>? metadata));
            Assert.False(terminal.GetBoolean(metadata!.Capability));
        }
    }

    private static void AssertMissingNumbers(
        TerminalDescription terminal,
        JsonElement expected,
        string propertyName)
    {
        if (!expected.TryGetProperty(propertyName, out JsonElement names))
        {
            return;
        }

        foreach (JsonElement name in names.EnumerateArray())
        {
            Assert.True(
                StandardCapabilityCatalog.TryGetNumeric(
                    name.GetString()!,
                    out StandardCapabilityMetadata<NumericCapability>? metadata));
            Assert.Null(terminal.GetNumber(metadata!.Capability));
        }
    }

    private static void AssertMissingStrings(
        TerminalDescription terminal,
        JsonElement expected,
        string propertyName)
    {
        if (!expected.TryGetProperty(propertyName, out JsonElement names))
        {
            return;
        }

        foreach (JsonElement name in names.EnumerateArray())
        {
            Assert.True(
                StandardCapabilityCatalog.TryGetString(
                    name.GetString()!,
                    out StandardCapabilityMetadata<StringCapability>? metadata));
            Assert.Null(terminal.GetString(metadata!.Capability));
        }
    }

    private readonly record struct CompiledHeader(
        ushort Magic,
        int NamesSize,
        int BooleanCount,
        int NumericCount,
        int StringCount,
        int StringTableSize);

    private readonly record struct ExtendedHeader(
        int BooleanCount,
        int NumericCount,
        int StringCount,
        int StringTableItemCount,
        int StringTableSize);

    private sealed class MissProvider : ITerminalDescriptionProvider
    {
        public int CallCount { get; private set; }

        public bool TryLoad(
            string name,
            [NotNullWhen(true)] out TerminalDescription? terminal)
        {
            ArgumentNullException.ThrowIfNull(name);

            CallCount++;
            terminal = null;
            return false;
        }
    }

    private sealed class HitProvider : ITerminalDescriptionProvider
    {
        private readonly TerminalDescription _terminal;

        public HitProvider(TerminalDescription terminal)
        {
            ArgumentNullException.ThrowIfNull(terminal);
            _terminal = terminal;
        }

        public int CallCount { get; private set; }

        public bool TryLoad(
            string name,
            [NotNullWhen(true)] out TerminalDescription? terminal)
        {
            ArgumentNullException.ThrowIfNull(name);

            CallCount++;
            if (string.Equals(name, _terminal.Name, StringComparison.Ordinal))
            {
                terminal = _terminal;
                return true;
            }

            terminal = null;
            return false;
        }
    }

    private sealed class ThrowingProvider : ITerminalDescriptionProvider
    {
        public bool TryLoad(
            string name,
            [NotNullWhen(true)] out TerminalDescription? terminal)
        {
            ArgumentNullException.ThrowIfNull(name);

            terminal = null;
            throw new IOException("synthetic provider failure");
        }
    }
}
