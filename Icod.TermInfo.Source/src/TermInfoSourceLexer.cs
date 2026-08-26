using System.Text;

namespace Icod.TermInfo.Source;

/// <summary>
/// Tokenizes System V/ncurses-compatible terminfo source without interpreting
/// capability values.
/// </summary>
public static class TermInfoSourceLexer
{
    /// <summary>
    /// Tokenizes caller-supplied terminfo source text.
    /// </summary>
    /// <param name="source">Complete terminfo source text.</param>
    /// <param name="sourceName">
    /// Optional caller-supplied source identity used only in locations and
    /// diagnostics.
    /// </param>
    /// <param name="options">Optional immutable lexer limits.</param>
    /// <returns>The tokens and deterministic diagnostics.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static TermInfoSourceLexResult Tokenize(
        string source,
        string? sourceName = null,
        TermInfoSourceLexerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        TermInfoSourceLexerOptions effectiveOptions =
            options
            ?? new TermInfoSourceLexerOptions();

        if (source.Length > effectiveOptions.MaximumSourceLength)
        {
            return CreateMaximumLengthResult(
                sourceName,
                effectiveOptions.MaximumSourceLength);
        }

        return new TermInfoSourceScanner(
                source,
                sourceName)
            .Scan();
    }

    /// <summary>
    /// Reads and tokenizes terminfo source from a text reader.
    /// </summary>
    /// <param name="reader">Reader supplying the complete source document.</param>
    /// <param name="sourceName">
    /// Optional caller-supplied source identity used only in locations and
    /// diagnostics.
    /// </param>
    /// <param name="options">Optional immutable lexer limits.</param>
    /// <returns>The tokens and deterministic diagnostics.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="reader"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// I/O failures from <paramref name="reader"/> are not converted into source
    /// diagnostics.
    /// </remarks>
    public static TermInfoSourceLexResult Tokenize(
        TextReader reader,
        string? sourceName = null,
        TermInfoSourceLexerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);

        TermInfoSourceLexerOptions effectiveOptions =
            options
            ?? new TermInfoSourceLexerOptions();
        StringBuilder builder = new(
            Math.Min(
                effectiveOptions.MaximumSourceLength,
                8_192));
        char[] buffer =
            new char[4_096];

        while (true)
        {
            int read =
                reader.Read(
                    buffer,
                    0,
                    buffer.Length);
            if (read == 0)
            {
                break;
            }

            if (builder.Length
                > effectiveOptions.MaximumSourceLength - read)
            {
                return CreateMaximumLengthResult(
                    sourceName,
                    effectiveOptions.MaximumSourceLength);
            }

            builder.Append(
                buffer,
                0,
                read);
        }

        return new TermInfoSourceScanner(
                builder.ToString(),
                sourceName)
            .Scan();
    }

    private static TermInfoSourceLexResult CreateMaximumLengthResult(
        string? sourceName,
        int maximumSourceLength)
    {
        if (maximumSourceLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumSourceLength));
        }

        TermInfoSourceDiagnostic diagnostic =
            new(
                TermInfoSourceDiagnosticCodes.MaximumSourceLengthExceeded,
                TermInfoSourceDiagnosticSeverity.Error,
                $"Terminfo source exceeds the configured maximum length of {maximumSourceLength} UTF-16 code units.",
                span: null);

        return new TermInfoSourceLexResult(
            Array.Empty<TermInfoSourceToken>(),
            new[]
            {
                diagnostic,
            });
    }
}
