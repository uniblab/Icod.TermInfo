using System.Buffers.Binary;
using System.Text;

namespace Icod.TermInfo;

/// <summary>
/// Parses conventional compiled terminfo entries from caller-supplied bytes.
/// </summary>
public static partial class CompiledTermInfoParser
{
    private const ushort LegacyMagic = 0x011A;
    private const int HeaderSize = 12;

    private const byte BooleanAbsent = 0x00;
    private const byte BooleanPresent = 0x01;
    private const byte BooleanCanceled = 0xFE;

    private const short ValueAbsent = -1;
    private const short ValueCanceled = -2;

    private static readonly CompiledTermInfoParserOptions DefaultOptions =
        new();

    /// <summary>
    /// Parses one supported conventional compiled terminfo entry into an
    /// immutable terminal description.
    /// </summary>
    /// <param name="entry">The complete compiled entry.</param>
    /// <param name="options">
    /// Optional parser resource limits. Default limits are used when omitted.
    /// </param>
    /// <returns>The parsed immutable terminal description.</returns>
    /// <exception cref="CompiledTermInfoFormatException">
    /// The entry is malformed, exceeds the configured size limit, or uses a
    /// compiled layout not implemented by this tranche.
    /// </exception>
    public static TerminalDescription Parse(
        ReadOnlySpan<byte> entry,
        CompiledTermInfoParserOptions? options = null)
    {
        CompiledTermInfoParserOptions effectiveOptions =
            options ?? DefaultOptions;

        if (entry.Length > effectiveOptions.MaximumEntrySize)
        {
            throw CreateFormatException(
                $"The compiled entry is {entry.Length} bytes, exceeding the configured maximum of {effectiveOptions.MaximumEntrySize} bytes.",
                -1,
                "entry");
        }

        CompiledHeader header = ReadHeader(entry);

        if (header.Magic != LegacyMagic
            && header.Magic != ExtendedNumberMagic)
        {
            throw CreateFormatException(
                $"Unsupported compiled terminfo magic 0x{header.Magic:X4}. Supported magic values are 0432 and 01036.",
                0,
                "header");
        }

        int numericWidth = GetNumericWidth(header.Magic);
        CompiledLayout layout =
            ReadLayout(
                entry,
                header,
                numericWidth);
        ValidateStandardTableCounts(header);

        TerminalDescriptionBuilder builder =
            CreateBuilder(
                entry.Slice(
                    layout.NamesOffset,
                    header.NamesSize),
                layout.NamesOffset);

        ReadBooleans(
            entry,
            header,
            layout,
            builder);
        ReadNumbers(
            entry,
            header,
            layout,
            numericWidth,
            builder);
        ReadStrings(
            entry,
            header,
            layout,
            builder);

        if (layout.EndOffset < entry.Length)
        {
            ReadExtendedSection(
                entry,
                layout,
                numericWidth,
                builder);
        }

        return builder.Build();
    }

    private static CompiledHeader ReadHeader(ReadOnlySpan<byte> entry)
    {
        if (entry.Length < HeaderSize)
        {
            throw CreateFormatException(
                "The compiled entry does not contain the complete six-short header.",
                entry.Length,
                "header");
        }

        return new CompiledHeader(
            BinaryPrimitives.ReadUInt16LittleEndian(entry[0..2]),
            BinaryPrimitives.ReadUInt16LittleEndian(entry[2..4]),
            BinaryPrimitives.ReadUInt16LittleEndian(entry[4..6]),
            BinaryPrimitives.ReadUInt16LittleEndian(entry[6..8]),
            BinaryPrimitives.ReadUInt16LittleEndian(entry[8..10]),
            BinaryPrimitives.ReadUInt16LittleEndian(entry[10..12]));
    }

    private static CompiledLayout ReadLayout(
        ReadOnlySpan<byte> entry,
        CompiledHeader header,
        int numericWidth)
    {
        int namesOffset = HeaderSize;
        int namesEnd =
            CheckedEnd(
                namesOffset,
                header.NamesSize,
                1,
                "names");
        EnsureAvailable(
            entry,
            namesEnd,
            "names");

        int booleanOffset = namesEnd;
        int booleanEnd =
            CheckedEnd(
                booleanOffset,
                header.BooleanCount,
                1,
                "booleans");
        EnsureAvailable(
            entry,
            booleanEnd,
            "booleans");

        int numericOffset = booleanEnd;
        if ((numericOffset & 1) != 0)
        {
            int alignedOffset =
                CheckedEnd(
                    numericOffset,
                    1,
                    1,
                    "alignment");
            EnsureAvailable(
                entry,
                alignedOffset,
                "alignment");

            if (entry[numericOffset] != 0)
            {
                throw CreateFormatException(
                    "The numeric alignment byte must be zero.",
                    numericOffset,
                    "alignment");
            }

            numericOffset = alignedOffset;
        }

        int numericEnd =
            CheckedEnd(
                numericOffset,
                header.NumericCount,
                numericWidth,
                "numerics");
        EnsureAvailable(
            entry,
            numericEnd,
            "numerics");

        int stringOffsetTableOffset = numericEnd;
        int stringOffsetTableEnd =
            CheckedEnd(
                stringOffsetTableOffset,
                header.StringCount,
                sizeof(short),
                "string-offsets");
        EnsureAvailable(
            entry,
            stringOffsetTableEnd,
            "string-offsets");

        int stringTableOffset = stringOffsetTableEnd;
        int stringTableEnd =
            CheckedEnd(
                stringTableOffset,
                header.StringTableSize,
                1,
                "string-table");
        EnsureAvailable(
            entry,
            stringTableEnd,
            "string-table");

        return new CompiledLayout(
            namesOffset,
            booleanOffset,
            numericOffset,
            stringOffsetTableOffset,
            stringTableOffset,
            stringTableEnd);
    }

    private static int CheckedEnd(
        int offset,
        int count,
        int elementSize,
        string section)
    {
        try
        {
            return checked(
                offset
                + checked(count * elementSize));
        }
        catch (OverflowException exception)
        {
            throw CreateFormatException(
                "Compiled section length arithmetic overflowed.",
                -1,
                section,
                exception);
        }
    }

    private static void EnsureAvailable(
        ReadOnlySpan<byte> entry,
        int requiredEnd,
        string section)
    {
        if (requiredEnd > entry.Length)
        {
            throw CreateFormatException(
                $"The {section} section extends beyond the compiled entry.",
                entry.Length,
                section);
        }
    }

    private static void ValidateStandardTableCounts(
        CompiledHeader header)
    {
        if (header.BooleanCount
            > StandardCapabilityCatalog.BooleanCapabilities.Count)
        {
            throw CreateFormatException(
                "The Boolean table contains standard positions outside the frozen capability catalog.",
                4,
                "booleans");
        }

        if (header.NumericCount
            > StandardCapabilityCatalog.NumericCapabilities.Count)
        {
            throw CreateFormatException(
                "The numeric table contains standard positions outside the frozen capability catalog.",
                6,
                "numerics");
        }

        if (header.StringCount
            > StandardCapabilityCatalog.StringCapabilities.Count)
        {
            throw CreateFormatException(
                "The string table contains standard positions outside the frozen capability catalog.",
                8,
                "string-offsets");
        }
    }

    private static TerminalDescriptionBuilder CreateBuilder(
        ReadOnlySpan<byte> namesSection,
        int namesOffset)
    {
        if (namesSection.IsEmpty)
        {
            throw CreateFormatException(
                "The names section cannot be empty.",
                namesOffset,
                "names");
        }

        int terminator = namesSection.IndexOf((byte)0);
        if (terminator != namesSection.Length - 1)
        {
            int errorOffset =
                namesOffset
                + Math.Max(
                    0,
                    namesSection.Length - 1);

            throw CreateFormatException(
                "The names section must contain exactly one terminating NUL byte at its declared end.",
                errorOffset,
                "names");
        }

        string identity =
            Encoding.Latin1.GetString(
                namesSection[..terminator]);
        string[] fields =
            identity.Split(
                '|',
                StringSplitOptions.None);

        if (fields.Length < 2)
        {
            throw CreateFormatException(
                "The names section must contain a canonical name and verbose description.",
                namesOffset,
                "names");
        }

        try
        {
            TerminalDescriptionBuilder builder =
                new(fields[0]);
            builder.SetDescription(fields[^1]);

            for (int index = 1;
                index < fields.Length - 1;
                index++)
            {
                builder.AddAlias(fields[index]);
            }

            return builder;
        }
        catch (ArgumentException exception)
        {
            throw CreateFormatException(
                "The names section contains invalid terminal identity data.",
                namesOffset,
                "names",
                exception);
        }
    }

    private static void ReadBooleans(
        ReadOnlySpan<byte> entry,
        CompiledHeader header,
        CompiledLayout layout,
        TerminalDescriptionBuilder builder)
    {
        for (int index = 0;
            index < header.BooleanCount;
            index++)
        {
            int offset = layout.BooleanOffset + index;
            BooleanCapability capability =
                GetCapabilityAtBinaryIndex(
                    StandardCapabilityCatalog.BooleanCapabilities,
                    index);

            switch (entry[offset])
            {
                case BooleanAbsent:
                    break;

                case BooleanPresent:
                    builder.SetBoolean(capability);
                    break;

                case BooleanCanceled:
                    builder.CancelBoolean(capability);
                    break;

                default:
                    throw CreateFormatException(
                        $"Invalid Boolean value 0x{entry[offset]:X2}.",
                        offset,
                        "booleans");
            }
        }
    }

    private static void ReadNumbers(
        ReadOnlySpan<byte> entry,
        CompiledHeader header,
        CompiledLayout layout,
        int numericWidth,
        TerminalDescriptionBuilder builder)
    {
        for (int index = 0;
            index < header.NumericCount;
            index++)
        {
            int offset =
                layout.NumericOffset
                + (index * numericWidth);
            int value =
                ReadNumericValue(
                    entry,
                    offset,
                    numericWidth);
            NumericCapability capability =
                GetCapabilityAtBinaryIndex(
                    StandardCapabilityCatalog.NumericCapabilities,
                    index);

            if (value == ValueAbsent)
            {
                continue;
            }

            if (value == ValueCanceled)
            {
                builder.CancelNumber(capability);
                continue;
            }

            if (value < ValueCanceled)
            {
                throw CreateFormatException(
                    $"Invalid negative numeric value {value}.",
                    offset,
                    "numerics");
            }

            builder.SetNumber(
                capability,
                value);
        }
    }

    private static void ReadStrings(
        ReadOnlySpan<byte> entry,
        CompiledHeader header,
        CompiledLayout layout,
        TerminalDescriptionBuilder builder)
    {
        ReadOnlySpan<byte> stringTable =
            entry.Slice(
                layout.StringTableOffset,
                header.StringTableSize);

        for (int index = 0;
            index < header.StringCount;
            index++)
        {
            int offsetEntry =
                layout.StringOffsetTableOffset
                + (index * sizeof(short));
            short relativeOffset =
                BinaryPrimitives.ReadInt16LittleEndian(
                    entry.Slice(
                        offsetEntry,
                        sizeof(short)));
            StringCapability capability =
                GetCapabilityAtBinaryIndex(
                    StandardCapabilityCatalog.StringCapabilities,
                    index);

            if (relativeOffset == ValueAbsent)
            {
                continue;
            }

            if (relativeOffset == ValueCanceled)
            {
                builder.CancelString(capability);
                continue;
            }

            if (relativeOffset < ValueCanceled)
            {
                throw CreateFormatException(
                    $"Invalid negative string offset {relativeOffset}.",
                    offsetEntry,
                    "string-offsets");
            }

            if (relativeOffset >= stringTable.Length)
            {
                throw CreateFormatException(
                    $"String offset {relativeOffset} lies outside the declared string table.",
                    offsetEntry,
                    "string-offsets");
            }

            ReadOnlySpan<byte> valueBytes =
                stringTable[relativeOffset..];
            int terminator = valueBytes.IndexOf((byte)0);
            if (terminator < 0)
            {
                throw CreateFormatException(
                    "A string capability is not NUL-terminated inside the declared string table.",
                    layout.StringTableOffset + relativeOffset,
                    "string-table");
            }

            string value =
                Encoding.Latin1.GetString(
                    valueBytes[..terminator]);
            builder.SetString(
                capability,
                value);
        }
    }

    private static TCapability GetCapabilityAtBinaryIndex<TCapability>(
        IReadOnlyList<StandardCapabilityMetadata<TCapability>> catalog,
        int binaryIndex)
        where TCapability : struct, Enum
    {
        StandardCapabilityMetadata<TCapability> metadata =
            catalog[binaryIndex];

        if (metadata.BinaryIndex != binaryIndex)
        {
            throw new InvalidOperationException(
                $"The standard capability catalog is not ordered by compiled binary index at position {binaryIndex}.");
        }

        return metadata.Capability;
    }

    private static CompiledTermInfoFormatException CreateFormatException(
        string message,
        int offset,
        string section,
        Exception? innerException = null)
    {
        return new CompiledTermInfoFormatException(
            message,
            offset,
            section,
            innerException);
    }

    private readonly struct CompiledHeader
    {
        public CompiledHeader(
            ushort magic,
            ushort namesSize,
            ushort booleanCount,
            ushort numericCount,
            ushort stringCount,
            ushort stringTableSize)
        {
            Magic = magic;
            NamesSize = namesSize;
            BooleanCount = booleanCount;
            NumericCount = numericCount;
            StringCount = stringCount;
            StringTableSize = stringTableSize;
        }

        public ushort Magic { get; }

        public int NamesSize { get; }

        public int BooleanCount { get; }

        public int NumericCount { get; }

        public int StringCount { get; }

        public int StringTableSize { get; }
    }

    private readonly struct CompiledLayout
    {
        public CompiledLayout(
            int namesOffset,
            int booleanOffset,
            int numericOffset,
            int stringOffsetTableOffset,
            int stringTableOffset,
            int endOffset)
        {
            NamesOffset = namesOffset;
            BooleanOffset = booleanOffset;
            NumericOffset = numericOffset;
            StringOffsetTableOffset = stringOffsetTableOffset;
            StringTableOffset = stringTableOffset;
            EndOffset = endOffset;
        }

        public int NamesOffset { get; }

        public int BooleanOffset { get; }

        public int NumericOffset { get; }

        public int StringOffsetTableOffset { get; }

        public int StringTableOffset { get; }

        public int EndOffset { get; }
    }
}
