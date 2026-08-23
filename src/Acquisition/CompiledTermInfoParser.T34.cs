using System.Buffers.Binary;
using System.Text;

namespace Icod.TermInfo;

public static partial class CompiledTermInfoParser
{
    private const ushort ExtendedNumberMagic = 0x021E;
    private const int ExtendedHeaderSize = 10;

    private static int GetNumericWidth(ushort magic)
    {
        if (magic == LegacyMagic)
        {
            return sizeof(short);
        }

        if (magic == ExtendedNumberMagic)
        {
            return sizeof(int);
        }

        throw new InvalidOperationException(
            $"Unsupported compiled terminfo magic 0x{magic:X4}.");
    }

    private static int ReadNumericValue(
        ReadOnlySpan<byte> entry,
        int offset,
        int numericWidth)
    {
        if (numericWidth == sizeof(short))
        {
            return BinaryPrimitives.ReadInt16LittleEndian(
                entry.Slice(
                    offset,
                    sizeof(short)));
        }

        if (numericWidth == sizeof(int))
        {
            return BinaryPrimitives.ReadInt32LittleEndian(
                entry.Slice(
                    offset,
                    sizeof(int)));
        }

        throw new InvalidOperationException(
            $"Unsupported compiled numeric width {numericWidth}.");
    }

    private static void ReadExtendedSection(
        ReadOnlySpan<byte> entry,
        CompiledLayout conventionalLayout,
        int numericWidth,
        TerminalDescriptionBuilder builder)
    {
        int headerOffset = conventionalLayout.EndOffset;
        if ((headerOffset & 1) != 0)
        {
            int alignedOffset =
                CheckedEnd(
                    headerOffset,
                    1,
                    1,
                    "extended-alignment");
            EnsureAvailable(
                entry,
                alignedOffset,
                "extended-alignment");

            if (entry[headerOffset] != 0)
            {
                throw CreateFormatException(
                    "The extended-section alignment byte must be zero.",
                    headerOffset,
                    "extended-alignment");
            }

            headerOffset = alignedOffset;
        }

        ExtendedHeader extendedHeader =
            ReadExtendedHeader(
                entry,
                headerOffset);
        ValidateExtendedHeader(extendedHeader);

        ExtendedLayout extendedLayout =
            ReadExtendedLayout(
                entry,
                headerOffset,
                extendedHeader,
                numericWidth);

        int nameTableStart =
            FindExtendedNameTableStart(
                entry,
                extendedHeader,
                extendedLayout);

        HashSet<string> names =
            new(StringComparer.Ordinal);

        ReadExtendedBooleans(
            entry,
            extendedHeader,
            extendedLayout,
            nameTableStart,
            names,
            builder);
        ReadExtendedNumbers(
            entry,
            extendedHeader,
            extendedLayout,
            nameTableStart,
            numericWidth,
            names,
            builder);
        ReadExtendedStrings(
            entry,
            extendedHeader,
            extendedLayout,
            nameTableStart,
            names,
            builder);

    }

    private static ExtendedHeader ReadExtendedHeader(
        ReadOnlySpan<byte> entry,
        int headerOffset)
    {
        int headerEnd =
            CheckedEnd(
                headerOffset,
                ExtendedHeaderSize,
                1,
                "extended-header");
        EnsureAvailable(
            entry,
            headerEnd,
            "extended-header");

        return new ExtendedHeader(
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.Slice(
                    headerOffset,
                    sizeof(ushort))),
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.Slice(
                    headerOffset + 2,
                    sizeof(ushort))),
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.Slice(
                    headerOffset + 4,
                    sizeof(ushort))),
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.Slice(
                    headerOffset + 6,
                    sizeof(ushort))),
            BinaryPrimitives.ReadUInt16LittleEndian(
                entry.Slice(
                    headerOffset + 8,
                    sizeof(ushort))));
    }

    private static void ValidateExtendedHeader(
        ExtendedHeader header)
    {
        int nameCount;
        int expectedItemCount;

        try
        {
            nameCount =
                checked(
                    header.BooleanCount
                    + header.NumericCount
                    + header.StringCount);
            expectedItemCount =
                checked(
                    nameCount
                    + header.StringCount);
        }
        catch (OverflowException exception)
        {
            throw CreateFormatException(
                "Extended capability counts overflowed.",
                -1,
                "extended-header",
                exception);
        }

        if (header.StringTableItemCount != expectedItemCount)
        {
            throw CreateFormatException(
                "The extended string-table item count is inconsistent with the extended capability counts.",
                -1,
                "extended-header");
        }
    }

    private static ExtendedLayout ReadExtendedLayout(
        ReadOnlySpan<byte> entry,
        int headerOffset,
        ExtendedHeader header,
        int numericWidth)
    {
        int booleanOffset =
            CheckedEnd(
                headerOffset,
                ExtendedHeaderSize,
                1,
                "extended-header");
        int booleanEnd =
            CheckedEnd(
                booleanOffset,
                header.BooleanCount,
                1,
                "extended-booleans");
        EnsureAvailable(
            entry,
            booleanEnd,
            "extended-booleans");

        int numericOffset = booleanEnd;
        if ((numericOffset & 1) != 0)
        {
            int alignedOffset =
                CheckedEnd(
                    numericOffset,
                    1,
                    1,
                    "extended-alignment");
            EnsureAvailable(
                entry,
                alignedOffset,
                "extended-alignment");

            if (entry[numericOffset] != 0)
            {
                throw CreateFormatException(
                    "The extended numeric alignment byte must be zero.",
                    numericOffset,
                    "extended-alignment");
            }

            numericOffset = alignedOffset;
        }

        int numericEnd =
            CheckedEnd(
                numericOffset,
                header.NumericCount,
                numericWidth,
                "extended-numerics");
        EnsureAvailable(
            entry,
            numericEnd,
            "extended-numerics");

        int stringOffsetTableOffset = numericEnd;
        int stringOffsetTableEnd =
            CheckedEnd(
                stringOffsetTableOffset,
                header.StringCount,
                sizeof(short),
                "extended-string-offsets");
        EnsureAvailable(
            entry,
            stringOffsetTableEnd,
            "extended-string-offsets");

        int nameCount =
            checked(
                header.BooleanCount
                + header.NumericCount
                + header.StringCount);
        int nameOffsetTableOffset = stringOffsetTableEnd;
        int nameOffsetTableEnd =
            CheckedEnd(
                nameOffsetTableOffset,
                nameCount,
                sizeof(short),
                "extended-name-offsets");
        EnsureAvailable(
            entry,
            nameOffsetTableEnd,
            "extended-name-offsets");

        int stringTableOffset = nameOffsetTableEnd;
        int stringTableEnd =
            CheckedEnd(
                stringTableOffset,
                header.StringTableSize,
                1,
                "extended-string-table");
        EnsureAvailable(
            entry,
            stringTableEnd,
            "extended-string-table");

        if (stringTableEnd != entry.Length)
        {
            throw CreateFormatException(
                "The ncurses extended section does not consume the complete compiled entry.",
                stringTableEnd,
                "extended");
        }

        return new ExtendedLayout(
            booleanOffset,
            numericOffset,
            stringOffsetTableOffset,
            nameOffsetTableOffset,
            stringTableOffset);
    }

    private static int FindExtendedNameTableStart(
        ReadOnlySpan<byte> entry,
        ExtendedHeader header,
        ExtendedLayout layout)
    {
        ReadOnlySpan<byte> stringTable =
            entry.Slice(
                layout.StringTableOffset,
                header.StringTableSize);
        int valueRegionEnd = 0;

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

            if (relativeOffset == ValueAbsent
                || relativeOffset == ValueCanceled)
            {
                continue;
            }

            if (relativeOffset < ValueCanceled)
            {
                throw CreateFormatException(
                    $"Invalid negative extended string offset {relativeOffset}.",
                    offsetEntry,
                    "extended-string-offsets");
            }

            int stringEnd =
                GetTerminatedStringEnd(
                    stringTable,
                    relativeOffset,
                    offsetEntry,
                    "extended-string-table");
            valueRegionEnd =
                Math.Max(
                    valueRegionEnd,
                    stringEnd);
        }

        return valueRegionEnd;
    }

    private static void ReadExtendedBooleans(
        ReadOnlySpan<byte> entry,
        ExtendedHeader header,
        ExtendedLayout layout,
        int nameTableStart,
        HashSet<string> names,
        TerminalDescriptionBuilder builder)
    {
        ReadOnlySpan<byte> stringTable =
            entry.Slice(
                layout.StringTableOffset,
                header.StringTableSize);

        for (int index = 0;
            index < header.BooleanCount;
            index++)
        {
            string name =
                ReadExtendedName(
                    entry,
                    stringTable,
                    layout,
                    nameTableStart,
                    index,
                    names);
            int valueOffset =
                layout.BooleanOffset
                + index;

            switch (entry[valueOffset])
            {
                case BooleanAbsent:
                    break;

                case BooleanPresent:
                    builder.SetExtendedBoolean(name);
                    break;

                case BooleanCanceled:
                    builder.CancelExtended(name);
                    break;

                default:
                    throw CreateFormatException(
                        $"Invalid extended Boolean value 0x{entry[valueOffset]:X2}.",
                        valueOffset,
                        "extended-booleans");
            }
        }
    }

    private static void ReadExtendedNumbers(
        ReadOnlySpan<byte> entry,
        ExtendedHeader header,
        ExtendedLayout layout,
        int nameTableStart,
        int numericWidth,
        HashSet<string> names,
        TerminalDescriptionBuilder builder)
    {
        ReadOnlySpan<byte> stringTable =
            entry.Slice(
                layout.StringTableOffset,
                header.StringTableSize);

        for (int index = 0;
            index < header.NumericCount;
            index++)
        {
            int nameIndex =
                header.BooleanCount
                + index;
            string name =
                ReadExtendedName(
                    entry,
                    stringTable,
                    layout,
                    nameTableStart,
                    nameIndex,
                    names);
            int valueOffset =
                layout.NumericOffset
                + (index * numericWidth);
            int value =
                ReadNumericValue(
                    entry,
                    valueOffset,
                    numericWidth);

            if (value == ValueAbsent)
            {
                continue;
            }

            if (value == ValueCanceled)
            {
                builder.CancelExtended(name);
                continue;
            }

            if (value < ValueCanceled)
            {
                throw CreateFormatException(
                    $"Invalid negative extended numeric value {value}.",
                    valueOffset,
                    "extended-numerics");
            }

            builder.SetExtendedNumber(
                name,
                value);
        }
    }

    private static void ReadExtendedStrings(
        ReadOnlySpan<byte> entry,
        ExtendedHeader header,
        ExtendedLayout layout,
        int nameTableStart,
        HashSet<string> names,
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
            int nameIndex =
                header.BooleanCount
                + header.NumericCount
                + index;
            string name =
                ReadExtendedName(
                    entry,
                    stringTable,
                    layout,
                    nameTableStart,
                    nameIndex,
                    names);
            int offsetEntry =
                layout.StringOffsetTableOffset
                + (index * sizeof(short));
            short relativeOffset =
                BinaryPrimitives.ReadInt16LittleEndian(
                    entry.Slice(
                        offsetEntry,
                        sizeof(short)));

            if (relativeOffset == ValueAbsent)
            {
                continue;
            }

            if (relativeOffset == ValueCanceled)
            {
                builder.CancelExtended(name);
                continue;
            }

            if (relativeOffset < ValueCanceled)
            {
                throw CreateFormatException(
                    $"Invalid negative extended string offset {relativeOffset}.",
                    offsetEntry,
                    "extended-string-offsets");
            }

            string value =
                ReadLatin1String(
                    stringTable,
                    relativeOffset,
                    offsetEntry,
                    "extended-string-table");
            builder.SetExtendedString(
                name,
                value);
        }
    }

    private static string ReadExtendedName(
        ReadOnlySpan<byte> entry,
        ReadOnlySpan<byte> stringTable,
        ExtendedLayout layout,
        int nameTableStart,
        int nameIndex,
        HashSet<string> names)
    {
        int offsetEntry =
            layout.NameOffsetTableOffset
            + (nameIndex * sizeof(short));
        short relativeOffset =
            BinaryPrimitives.ReadInt16LittleEndian(
                entry.Slice(
                    offsetEntry,
                    sizeof(short)));

        if (relativeOffset < 0)
        {
            throw CreateFormatException(
                $"Invalid negative extended name offset {relativeOffset}.",
                offsetEntry,
                "extended-name-offsets");
        }

        int tableRelativeOffset;
        try
        {
            tableRelativeOffset =
                checked(
                    nameTableStart
                    + relativeOffset);
        }
        catch (OverflowException exception)
        {
            throw CreateFormatException(
                "Extended name offset arithmetic overflowed.",
                offsetEntry,
                "extended-name-offsets",
                exception);
        }

        string name =
            ReadLatin1String(
                stringTable,
                tableRelativeOffset,
                offsetEntry,
                "extended-names");

        if (string.IsNullOrWhiteSpace(name))
        {
            throw CreateFormatException(
                "An extended capability name cannot be empty or whitespace.",
                offsetEntry,
                "extended-names");
        }

        if (StandardCapabilityCatalog.IsStandardShortName(name))
        {
            throw CreateFormatException(
                $"Extended capability name '{name}' collides with a standard capability short name.",
                offsetEntry,
                "extended-names");
        }

        if (!names.Add(name))
        {
            throw CreateFormatException(
                $"Duplicate extended capability name '{name}'.",
                offsetEntry,
                "extended-names");
        }

        return name;
    }

    private static string ReadLatin1String(
        ReadOnlySpan<byte> stringTable,
        int relativeOffset,
        int sourceOffset,
        string section)
    {
        int stringEnd =
            GetTerminatedStringEnd(
                stringTable,
                relativeOffset,
                sourceOffset,
                section);
        int length =
            stringEnd
            - relativeOffset
            - 1;

        return Encoding.Latin1.GetString(
            stringTable.Slice(
                relativeOffset,
                length));
    }

    private static int GetTerminatedStringEnd(
        ReadOnlySpan<byte> stringTable,
        int relativeOffset,
        int sourceOffset,
        string section)
    {
        if (relativeOffset < 0
            || relativeOffset >= stringTable.Length)
        {
            throw CreateFormatException(
                $"String offset {relativeOffset} lies outside the declared extended string table.",
                sourceOffset,
                section);
        }

        ReadOnlySpan<byte> value =
            stringTable[relativeOffset..];
        int terminator =
            value.IndexOf((byte)0);

        if (terminator < 0)
        {
            throw CreateFormatException(
                "An extended string is not NUL-terminated inside the declared extended string table.",
                sourceOffset,
                section);
        }

        return relativeOffset
            + terminator
            + 1;
    }

    private readonly struct ExtendedHeader
    {
        public ExtendedHeader(
            ushort booleanCount,
            ushort numericCount,
            ushort stringCount,
            ushort stringTableItemCount,
            ushort stringTableSize)
        {
            BooleanCount = booleanCount;
            NumericCount = numericCount;
            StringCount = stringCount;
            StringTableItemCount = stringTableItemCount;
            StringTableSize = stringTableSize;
        }

        public int BooleanCount { get; }

        public int NumericCount { get; }

        public int StringCount { get; }

        public int StringTableItemCount { get; }

        public int StringTableSize { get; }
    }

    private readonly struct ExtendedLayout
    {
        public ExtendedLayout(
            int booleanOffset,
            int numericOffset,
            int stringOffsetTableOffset,
            int nameOffsetTableOffset,
            int stringTableOffset)
        {
            BooleanOffset = booleanOffset;
            NumericOffset = numericOffset;
            StringOffsetTableOffset = stringOffsetTableOffset;
            NameOffsetTableOffset = nameOffsetTableOffset;
            StringTableOffset = stringTableOffset;
        }

        public int BooleanOffset { get; }

        public int NumericOffset { get; }

        public int StringOffsetTableOffset { get; }

        public int NameOffsetTableOffset { get; }

        public int StringTableOffset { get; }
    }
}
