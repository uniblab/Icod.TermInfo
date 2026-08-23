using System.Text;

namespace Icod.TermInfo;

/// <summary>
/// Emits terminfo strings while removing or honoring padding directives.
/// </summary>
public static partial class TermInfoOutput
{
    /// <summary>
    /// Writes a string using <c>putp</c>-style affected-line semantics.
    /// </summary>
    public static void PutP(
        string value,
        TextWriter writer,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);

        TPuts(
            value,
            1,
            writer,
            paddingMode,
            delayProvider);
    }

    /// <summary>
    /// Writes a string asynchronously using <c>putp</c>-style
    /// affected-line semantics.
    /// </summary>
    public static ValueTask PutPAsync(
        string value,
        TextWriter writer,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);

        return TPutsAsync(
            value,
            1,
            writer,
            paddingMode,
            delayProvider,
            cancellationToken);
    }

    /// <summary>
    /// Writes a terminfo string to a text writer.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        TextWriter writer,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ValidateAffectedLines(affectedLines);
        ValidatePaddingMode(paddingMode);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ITermInfoDelayProvider provider =
            ResolveDelayProvider(paddingMode, delayProvider);

        foreach (TermInfoOutputSegment segment in segments)
        {
            if (segment.IsPadding)
            {
                Delay(
                    segment,
                    affectedLines,
                    paddingMode,
                    provider);
            }
            else
            {
                writer.Write(segment.Text);
            }
        }
    }

    /// <summary>
    /// Writes a terminfo string through a character callback.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        Action<char> output,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(output);
        ValidateAffectedLines(affectedLines);
        ValidatePaddingMode(paddingMode);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ITermInfoDelayProvider provider =
            ResolveDelayProvider(paddingMode, delayProvider);

        foreach (TermInfoOutputSegment segment in segments)
        {
            if (segment.IsPadding)
            {
                Delay(
                    segment,
                    affectedLines,
                    paddingMode,
                    provider);
                continue;
            }

            foreach (char character in segment.Text!)
            {
                output(character);
            }
        }
    }

    /// <summary>
    /// Writes a terminfo string to a byte stream using the caller-supplied
    /// encoding.
    /// </summary>
    public static void TPuts(
        string value,
        int affectedLines,
        Stream stream,
        Encoding encoding,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ValidateWritableStream(stream);
        ValidateAffectedLines(affectedLines);
        ValidatePaddingMode(paddingMode);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ITermInfoDelayProvider provider =
            ResolveDelayProvider(paddingMode, delayProvider);

        foreach (TermInfoOutputSegment segment in segments)
        {
            if (segment.IsPadding)
            {
                Delay(
                    segment,
                    affectedLines,
                    paddingMode,
                    provider);
                continue;
            }

            byte[] bytes = encoding.GetBytes(segment.Text!);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    /// <summary>
    /// Writes a terminfo string asynchronously to a text writer.
    /// </summary>
    public static async ValueTask TPutsAsync(
        string value,
        int affectedLines,
        TextWriter writer,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ValidateAffectedLines(affectedLines);
        ValidatePaddingMode(paddingMode);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ITermInfoDelayProvider provider =
            ResolveDelayProvider(paddingMode, delayProvider);

        foreach (TermInfoOutputSegment segment in segments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (segment.IsPadding)
            {
                await DelayAsync(
                    segment,
                    affectedLines,
                    paddingMode,
                    provider,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteAsync(
                    segment.Text!.AsMemory(),
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Writes a terminfo string asynchronously to a byte stream using the
    /// caller-supplied encoding.
    /// </summary>
    public static async ValueTask TPutsAsync(
        string value,
        int affectedLines,
        Stream stream,
        Encoding encoding,
        PaddingMode paddingMode = PaddingMode.Ignore,
        ITermInfoDelayProvider? delayProvider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(encoding);
        ValidateWritableStream(stream);
        ValidateAffectedLines(affectedLines);
        ValidatePaddingMode(paddingMode);

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(value);
        ITermInfoDelayProvider provider =
            ResolveDelayProvider(paddingMode, delayProvider);

        foreach (TermInfoOutputSegment segment in segments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (segment.IsPadding)
            {
                await DelayAsync(
                    segment,
                    affectedLines,
                    paddingMode,
                    provider,
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            byte[] bytes = encoding.GetBytes(segment.Text!);
            await stream.WriteAsync(
                bytes.AsMemory(),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static void Delay(
        TermInfoOutputSegment segment,
        int affectedLines,
        PaddingMode paddingMode,
        ITermInfoDelayProvider delayProvider)
    {
        if (paddingMode == PaddingMode.Ignore)
        {
            return;
        }

        delayProvider.Delay(
            ResolveDelay(
                segment,
                affectedLines));
    }

    private static ValueTask DelayAsync(
        TermInfoOutputSegment segment,
        int affectedLines,
        PaddingMode paddingMode,
        ITermInfoDelayProvider delayProvider,
        CancellationToken cancellationToken)
    {
        if (paddingMode == PaddingMode.Ignore)
        {
            return ValueTask.CompletedTask;
        }

        return delayProvider.DelayAsync(
            ResolveDelay(
                segment,
                affectedLines),
            cancellationToken);
    }

    private static TermInfoDelay ResolveDelay(
        TermInfoOutputSegment segment,
        int affectedLines)
    {
        decimal milliseconds =
            ResolveDelayMilliseconds(
                segment,
                affectedLines);

        return new TermInfoDelay(
            TimeSpan.FromMilliseconds((double)milliseconds),
            segment.IsMandatory);
    }

    private static decimal ResolveDelayMilliseconds(
        TermInfoOutputSegment segment,
        int affectedLines)
    {
        decimal milliseconds = segment.Milliseconds;
        decimal maximum =
            TermInfoPaddingParser.MaximumDelayMilliseconds;

        if (segment.MultiplyByAffectedLines)
        {
            if (affectedLines == 0)
            {
                return 0m;
            }

            if (milliseconds >= (maximum / affectedLines))
            {
                return maximum;
            }

            milliseconds *= affectedLines;
        }

        return Math.Min(milliseconds, maximum);
    }

    private static ITermInfoDelayProvider ResolveDelayProvider(
        PaddingMode paddingMode,
        ITermInfoDelayProvider? delayProvider)
    {
        return (paddingMode == PaddingMode.Delay)
            ? (delayProvider ?? SystemTermInfoDelayProvider.Instance)
            : SystemTermInfoDelayProvider.Instance
            ;
    }

    private static void ValidateAffectedLines(int affectedLines)
    {
        if (affectedLines < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(affectedLines),
                "The affected-line count cannot be negative.");
        }
    }

    private static void ValidatePaddingMode(PaddingMode paddingMode)
    {
        if (!Enum.IsDefined(typeof(PaddingMode), paddingMode))
        {
            throw new ArgumentOutOfRangeException(nameof(paddingMode));
        }

        if (paddingMode == PaddingMode.PadCharacters)
        {
            throw new ArgumentException(
                "Pad-character output requires a terminal-aware TermInfoOutputOptions overload.",
                nameof(paddingMode));
        }
    }

    private static void ValidateWritableStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
        {
            throw new ArgumentException(
                "The output stream must be writable.",
                nameof(stream));
        }
    }
}
