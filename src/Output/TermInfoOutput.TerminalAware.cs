using System.Text;

namespace Icod.TermInfo;

/// <summary>
/// Terminal-aware terminfo output overloads.
/// </summary>
public static partial class TermInfoOutput
{
    internal const int MaximumPaddingCharacterCount = 4_194_304;
    private const decimal BitsPerPaddingCharacter = 9m;
    private const int PaddingChunkSize = 256;

    /// <summary>
    /// Writes a string using terminal-aware <c>putp</c>-style one-line
    /// semantics.
    /// </summary>
    public static void PutP(
        string value,
        TextWriter writer,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        TPuts(
            value,
            1,
            writer,
            options);
    }

    /// <summary>
    /// Writes a string asynchronously using terminal-aware <c>putp</c>-style
    /// one-line semantics.
    /// </summary>
    public static ValueTask PutPAsync(
        string value,
        TextWriter writer,
        TermInfoOutputOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);

        return TPutsAsync(
            value,
            1,
            writer,
            options,
            cancellationToken);
    }

    /// <summary>
    /// Writes a terminfo string to a text writer using terminal-aware padding
    /// semantics.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        TextWriter writer,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);
        ValidateAffectedLines(affectedLines);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ResolvedPadding[] padding =
            ResolveTerminalAwarePadding(
                segments,
                affectedLines,
                options);
        ITermInfoDelayProvider delayProvider =
            ResolveTerminalAwareDelayProvider(options);

        for (int i = 0; i < segments.Count; i++)
        {
            TermInfoOutputSegment segment = segments[i];

            if (!segment.IsPadding)
            {
                writer.Write(segment.Text);
                continue;
            }

            WriteResolvedPadding(
                padding[i],
                writer,
                delayProvider);
        }
    }

    /// <summary>
    /// Writes a terminfo string through a character callback using
    /// terminal-aware padding semantics.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        Action<char> output,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(options);
        ValidateAffectedLines(affectedLines);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ResolvedPadding[] padding =
            ResolveTerminalAwarePadding(
                segments,
                affectedLines,
                options);
        ITermInfoDelayProvider delayProvider =
            ResolveTerminalAwareDelayProvider(options);

        for (int i = 0; i < segments.Count; i++)
        {
            TermInfoOutputSegment segment = segments[i];

            if (!segment.IsPadding)
            {
                foreach (char character in segment.Text!)
                {
                    output(character);
                }

                continue;
            }

            WriteResolvedPadding(
                padding[i],
                output,
                delayProvider);
        }
    }

    /// <summary>
    /// Writes a terminfo string to a byte stream using the caller-supplied
    /// encoding and terminal-aware padding semantics.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Encoding.Latin1"/> when exact terminfo capability bytes
    /// are required. Application text encoding remains caller-owned.
    /// </remarks>
    public static void TPuts(
        string value,
        int affectedLines,
        Stream stream,
        Encoding encoding,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(options);
        ValidateWritableStream(stream);
        ValidateAffectedLines(affectedLines);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ResolvedPadding[] padding =
            ResolveTerminalAwarePadding(
                segments,
                affectedLines,
                options);
        ITermInfoDelayProvider delayProvider =
            ResolveTerminalAwareDelayProvider(options);

        for (int i = 0; i < segments.Count; i++)
        {
            TermInfoOutputSegment segment = segments[i];

            if (!segment.IsPadding)
            {
                byte[] bytes = encoding.GetBytes(segment.Text!);
                stream.Write(bytes, 0, bytes.Length);
                continue;
            }

            WriteResolvedPadding(
                padding[i],
                stream,
                encoding,
                delayProvider);
        }
    }

    /// <summary>
    /// Writes a terminfo string asynchronously to a text writer using
    /// terminal-aware padding semantics.
    /// </summary>
    public static async ValueTask TPutsAsync(
        string value,
        int affectedLines,
        TextWriter writer,
        TermInfoOutputOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(options);
        ValidateAffectedLines(affectedLines);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ResolvedPadding[] padding =
            ResolveTerminalAwarePadding(
                segments,
                affectedLines,
                options);
        ITermInfoDelayProvider delayProvider =
            ResolveTerminalAwareDelayProvider(options);

        for (int i = 0; i < segments.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TermInfoOutputSegment segment = segments[i];

            if (!segment.IsPadding)
            {
                await writer.WriteAsync(
                    segment.Text!.AsMemory(),
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            await WriteResolvedPaddingAsync(
                padding[i],
                writer,
                delayProvider,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes a terminfo string asynchronously to a byte stream using the
    /// caller-supplied encoding and terminal-aware padding semantics.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Encoding.Latin1"/> when exact terminfo capability bytes
    /// are required. Application text encoding remains caller-owned.
    /// </remarks>
    public static async ValueTask TPutsAsync(
        string value,
        int affectedLines,
        Stream stream,
        Encoding encoding,
        TermInfoOutputOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(options);
        ValidateWritableStream(stream);
        ValidateAffectedLines(affectedLines);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ResolvedPadding[] padding =
            ResolveTerminalAwarePadding(
                segments,
                affectedLines,
                options);
        ITermInfoDelayProvider delayProvider =
            ResolveTerminalAwareDelayProvider(options);

        for (int i = 0; i < segments.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TermInfoOutputSegment segment = segments[i];

            if (!segment.IsPadding)
            {
                byte[] bytes = encoding.GetBytes(segment.Text!);
                await stream.WriteAsync(
                    bytes.AsMemory(),
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            await WriteResolvedPaddingAsync(
                padding[i],
                stream,
                encoding,
                delayProvider,
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static ResolvedPadding[] ResolveTerminalAwarePadding(
        IReadOnlyList<TermInfoOutputSegment> segments,
        int affectedLines,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentNullException.ThrowIfNull(options);

        ResolvedPadding[] resolved =
            new ResolvedPadding[segments.Count];

        for (int i = 0; i < segments.Count; i++)
        {
            TermInfoOutputSegment segment = segments[i];
            if (!segment.IsPadding)
            {
                continue;
            }

            resolved[i] =
                ResolveTerminalAwarePadding(
                    segment,
                    affectedLines,
                    options);
        }

        return resolved;
    }

    private static ResolvedPadding ResolveTerminalAwarePadding(
        TermInfoOutputSegment segment,
        int affectedLines,
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.PaddingMode == PaddingMode.Ignore)
        {
            return ResolvedPadding.None;
        }

        if (!segment.IsMandatory
            && ShouldSuppressAdvisoryPadding(
                options.Terminal,
                options.BaudRate))
        {
            return ResolvedPadding.None;
        }

        decimal milliseconds =
            ResolveDelayMilliseconds(
                segment,
                affectedLines);
        if (milliseconds == 0m)
        {
            return ResolvedPadding.None;
        }

        TermInfoDelay delay =
            new(
                TimeSpan.FromMilliseconds((double)milliseconds),
                segment.IsMandatory);

        if (options.PaddingMode == PaddingMode.Delay)
        {
            return ResolvedPadding.CreateDelay(delay);
        }

        if (options.Terminal.GetBoolean(
                BooleanCapability.NoPadCharacter))
        {
            return ResolvedPadding.CreateDelay(delay);
        }

        if (options.BaudRate is not int baudRate)
        {
            throw new InvalidOperationException(
                "Pad-character output requires a caller-supplied baud rate.");
        }

        int count =
            ResolvePaddingCharacterCount(
                milliseconds,
                baudRate);
        if (count == 0)
        {
            return ResolvedPadding.None;
        }

        return ResolvedPadding.CreateCharacters(
            ResolvePadCharacter(options.Terminal),
            count);
    }

    private static bool ShouldSuppressAdvisoryPadding(
        TerminalDescription terminal,
        int? baudRate)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        if (terminal.GetBoolean(BooleanCapability.XonXoff))
        {
            return true;
        }

        int? paddingBaudRate =
            terminal.GetNumber(
                NumericCapability.PaddingBaudRate);

        return paddingBaudRate is int threshold
            && baudRate is int actualBaudRate
            && actualBaudRate < threshold;
    }

    private static char ResolvePadCharacter(
        TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        string? pad =
            terminal.GetString(StringCapability.PadChar);
        char character =
            (string.IsNullOrEmpty(pad))
                ? '\0'
                : pad[0]
            ;

        if (character > byte.MaxValue)
        {
            throw new InvalidOperationException(
                "The terminal pad character is outside the one-byte terminfo range.");
        }

        return character;
    }

    private static int ResolvePaddingCharacterCount(
        decimal milliseconds,
        int baudRate)
    {
        if (milliseconds < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds));
        }

        if (baudRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baudRate));
        }

        decimal count =
            decimal.Floor(
                (milliseconds * baudRate)
                / (BitsPerPaddingCharacter * 1000m));

        if (count > MaximumPaddingCharacterCount)
        {
            throw new InvalidOperationException(
                $"Resolved padding requires {count} characters, exceeding the limit of {MaximumPaddingCharacterCount}.");
        }

        return decimal.ToInt32(count);
    }

    private static ITermInfoDelayProvider ResolveTerminalAwareDelayProvider(
        TermInfoOutputOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.DelayProvider
            ?? SystemTermInfoDelayProvider.Instance;
    }

    private static void WriteResolvedPadding(
        ResolvedPadding padding,
        TextWriter writer,
        ITermInfoDelayProvider delayProvider)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(delayProvider);

        switch (padding.Kind)
        {
            case ResolvedPaddingKind.None:
                return;
            case ResolvedPaddingKind.Delay:
                delayProvider.Delay(padding.Delay);
                return;
            case ResolvedPaddingKind.Characters:
                WriteRepeatedCharacter(
                    writer,
                    padding.Character,
                    padding.CharacterCount);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported padding resolution '{padding.Kind}'.");
        }
    }

    private static void WriteResolvedPadding(
        ResolvedPadding padding,
        Action<char> output,
        ITermInfoDelayProvider delayProvider)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(delayProvider);

        switch (padding.Kind)
        {
            case ResolvedPaddingKind.None:
                return;
            case ResolvedPaddingKind.Delay:
                delayProvider.Delay(padding.Delay);
                return;
            case ResolvedPaddingKind.Characters:
                for (int i = 0; i < padding.CharacterCount; i++)
                {
                    output(padding.Character);
                }

                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported padding resolution '{padding.Kind}'.");
        }
    }

    private static void WriteResolvedPadding(
        ResolvedPadding padding,
        Stream stream,
        Encoding encoding,
        ITermInfoDelayProvider delayProvider)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(delayProvider);

        switch (padding.Kind)
        {
            case ResolvedPaddingKind.None:
                return;
            case ResolvedPaddingKind.Delay:
                delayProvider.Delay(padding.Delay);
                return;
            case ResolvedPaddingKind.Characters:
                byte[] bytes =
                    encoding.GetBytes(
                        new string(
                            padding.Character,
                            padding.CharacterCount));
                stream.Write(bytes, 0, bytes.Length);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported padding resolution '{padding.Kind}'.");
        }
    }

    private static async ValueTask WriteResolvedPaddingAsync(
        ResolvedPadding padding,
        TextWriter writer,
        ITermInfoDelayProvider delayProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(delayProvider);

        switch (padding.Kind)
        {
            case ResolvedPaddingKind.None:
                return;
            case ResolvedPaddingKind.Delay:
                await delayProvider.DelayAsync(
                    padding.Delay,
                    cancellationToken).ConfigureAwait(false);
                return;
            case ResolvedPaddingKind.Characters:
                await WriteRepeatedCharacterAsync(
                    writer,
                    padding.Character,
                    padding.CharacterCount,
                    cancellationToken).ConfigureAwait(false);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported padding resolution '{padding.Kind}'.");
        }
    }

    private static async ValueTask WriteResolvedPaddingAsync(
        ResolvedPadding padding,
        Stream stream,
        Encoding encoding,
        ITermInfoDelayProvider delayProvider,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(delayProvider);

        switch (padding.Kind)
        {
            case ResolvedPaddingKind.None:
                return;
            case ResolvedPaddingKind.Delay:
                await delayProvider.DelayAsync(
                    padding.Delay,
                    cancellationToken).ConfigureAwait(false);
                return;
            case ResolvedPaddingKind.Characters:
                byte[] bytes =
                    encoding.GetBytes(
                        new string(
                            padding.Character,
                            padding.CharacterCount));
                await stream.WriteAsync(
                    bytes.AsMemory(),
                    cancellationToken).ConfigureAwait(false);
                return;
            default:
                throw new InvalidOperationException(
                    $"Unsupported padding resolution '{padding.Kind}'.");
        }
    }

    private static void WriteRepeatedCharacter(
        TextWriter writer,
        char character,
        int count)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return;
        }

        string chunk =
            new(
                character,
                Math.Min(PaddingChunkSize, count));
        int remaining = count;

        while (remaining >= chunk.Length)
        {
            writer.Write(chunk);
            remaining -= chunk.Length;
        }

        if (remaining > 0)
        {
            writer.Write(chunk.AsSpan(0, remaining));
        }
    }

    private static async ValueTask WriteRepeatedCharacterAsync(
        TextWriter writer,
        char character,
        int count,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return;
        }

        string chunk =
            new(
                character,
                Math.Min(PaddingChunkSize, count));
        int remaining = count;

        while (remaining >= chunk.Length)
        {
            await writer.WriteAsync(
                chunk.AsMemory(),
                cancellationToken).ConfigureAwait(false);
            remaining -= chunk.Length;
        }

        if (remaining > 0)
        {
            await writer.WriteAsync(
                chunk.AsMemory(0, remaining),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private readonly record struct ResolvedPadding(
        ResolvedPaddingKind Kind,
        TermInfoDelay Delay,
        char Character,
        int CharacterCount)
    {
        internal static ResolvedPadding None => default;

        internal static ResolvedPadding CreateDelay(
            TermInfoDelay delay)
        {
            return new ResolvedPadding(
                ResolvedPaddingKind.Delay,
                delay,
                default,
                0);
        }

        internal static ResolvedPadding CreateCharacters(
            char character,
            int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return new ResolvedPadding(
                ResolvedPaddingKind.Characters,
                default,
                character,
                count);
        }
    }

    private enum ResolvedPaddingKind
    {
        None = 0,
        Delay = 1,
        Characters = 2,
    }
}
