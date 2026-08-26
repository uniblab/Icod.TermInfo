namespace Icod.TermInfo.Source;

/// <summary>
/// Parses terminfo source into an unresolved document model.
/// </summary>
/// <remarks>
/// <para>
/// S04 composes the S02 lexer with the S03 value parser. It preserves source
/// order and provenance but does not classify capability names, apply
/// cancellation, resolve <c>use=</c> inheritance, or construct
/// <c>TerminalDescription</c> values.
/// </para>
/// </remarks>
public static class TermInfoSourceParser
{
    /// <summary>
    /// Parses terminfo source text into unresolved entries.
    /// </summary>
    /// <param name="source">The complete source text.</param>
    /// <param name="sourceName">An optional source identity for diagnostics.</param>
    /// <param name="options">Optional lexer/resource-limit settings.</param>
    /// <returns>The parsed unresolved document and diagnostics.</returns>
    public static TermInfoSourceParseResult Parse(
        string source,
        string? sourceName = null,
        TermInfoSourceLexerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ParseLexResult(
            TermInfoSourceLexer.Tokenize(
                source,
                sourceName,
                options));
    }

    /// <summary>
    /// Reads and parses terminfo source into unresolved entries.
    /// </summary>
    /// <param name="reader">The source reader.</param>
    /// <param name="sourceName">An optional source identity for diagnostics.</param>
    /// <param name="options">Optional lexer/resource-limit settings.</param>
    /// <returns>The parsed unresolved document and diagnostics.</returns>
    public static TermInfoSourceParseResult Parse(
        TextReader reader,
        string? sourceName = null,
        TermInfoSourceLexerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return ParseLexResult(
            TermInfoSourceLexer.Tokenize(
                reader,
                sourceName,
                options));
    }

    private static TermInfoSourceParseResult ParseLexResult(
        TermInfoSourceLexResult lexResult)
    {
        ArgumentNullException.ThrowIfNull(lexResult);

        List<TermInfoSourceDiagnostic> diagnostics =
            [.. lexResult.Diagnostics];
        List<TermInfoSourceEntry> entries = [];
        IReadOnlyList<TermInfoSourceToken> tokens =
            lexResult.Tokens;

        int index = 0;
        while (index < tokens.Count)
        {
            if (tokens[index].Kind
                != TermInfoSourceTokenKind.TerminalName)
            {
                index++;
                continue;
            }

            TermInfoSourceToken nameToken =
                tokens[index];
            string canonicalName =
                nameToken.Text;
            List<string> aliases = [];
            string? description = null;
            TermInfoSourceToken lastSemanticToken =
                nameToken;
            index++;

            while (index < tokens.Count)
            {
                TermInfoSourceToken token =
                    tokens[index];
                if (token.Kind == TermInfoSourceTokenKind.Alias)
                {
                    aliases.Add(token.Text);
                    lastSemanticToken = token;
                    index++;
                    continue;
                }

                if (token.Kind == TermInfoSourceTokenKind.Description)
                {
                    description = token.Text;
                    lastSemanticToken = token;
                    index++;
                    continue;
                }

                break;
            }

            List<TermInfoSourceField> fields = [];
            while (index < tokens.Count
                && tokens[index].Kind
                    != TermInfoSourceTokenKind.TerminalName)
            {
                TermInfoSourceToken token =
                    tokens[index];
                TermInfoSourceField? field =
                    CreateField(
                        token,
                        diagnostics);
                if (field is not null)
                {
                    fields.Add(field);
                    lastSemanticToken = token;
                }

                index++;
            }

            entries.Add(
                new TermInfoSourceEntry(
                    canonicalName,
                    aliases,
                    description,
                    fields,
                    CreateEntrySpan(
                        nameToken.Span,
                        lastSemanticToken.Span)));
        }

        TermInfoSourceDiagnostic[] orderedDiagnostics =
            diagnostics
                .Select(
                    (diagnostic, ordinal) =>
                        new
                        {
                            Diagnostic = diagnostic,
                            Ordinal = ordinal,
                        })
                .OrderBy(
                    item => item.Diagnostic.Span?.Offset
                        ?? int.MaxValue)
                .ThenBy(
                    item => item.Diagnostic.Span?.Length
                        ?? int.MaxValue)
                .ThenBy(
                    item => item.Ordinal)
                .Select(item => item.Diagnostic)
                .ToArray();

        return new TermInfoSourceParseResult(
            new TermInfoSourceDocument(
                entries,
                tokens),
            orderedDiagnostics);
    }

    private static TermInfoSourceField? CreateField(
        TermInfoSourceToken token,
        ICollection<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(diagnostics);

        switch (token.Kind)
        {
            case TermInfoSourceTokenKind.BooleanCapability:
                return CreateCapabilityField(
                    TermInfoSourceFieldKind.BooleanCapability,
                    token,
                    token.Text.Trim(),
                    null,
                    null);

            case TermInfoSourceTokenKind.NumericCapability:
            {
                TermInfoSourceNumericValueResult numeric =
                    TermInfoSourceValueParser.ParseNumeric(token);
                AddDiagnostics(
                    diagnostics,
                    numeric.Diagnostics);
                return CreateCapabilityField(
                    TermInfoSourceFieldKind.NumericCapability,
                    token,
                    NormalizeCapabilityName(
                        TextBeforeOperator(
                            token.Text,
                            '#')),
                    numeric.Value,
                    null);
            }

            case TermInfoSourceTokenKind.StringCapability:
            {
                TermInfoSourceStringValueResult text =
                    TermInfoSourceValueParser.ParseString(token);
                AddDiagnostics(
                    diagnostics,
                    text.Diagnostics);
                return CreateCapabilityField(
                    TermInfoSourceFieldKind.StringCapability,
                    token,
                    NormalizeCapabilityName(
                        TextBeforeOperator(
                            token.Text,
                            '=')),
                    null,
                    text.Value);
            }

            case TermInfoSourceTokenKind.CancelledCapability:
                return CreateCapabilityField(
                    TermInfoSourceFieldKind.CancelledCapability,
                    token,
                    NormalizeCapabilityName(
                        TextBeforeOperator(
                            token.Text,
                            '@')),
                    null,
                    null);

            case TermInfoSourceTokenKind.UseReference:
                return new TermInfoSourceField(
                    TermInfoSourceFieldKind.UseReference,
                    null,
                    TextAfterOperator(
                        token.Text,
                        '=').Trim(),
                    null,
                    null,
                    token.Text,
                    token.Span);

            case TermInfoSourceTokenKind.DisabledCapability:
            {
                string disabled =
                    token.Text;
                if (disabled.Length != 0
                    && disabled[0] == '.')
                {
                    disabled =
                        disabled[1..];
                }
                return CreateCapabilityField(
                    TermInfoSourceFieldKind.DisabledCapability,
                    token,
                    NormalizeCapabilityName(
                        TextBeforeAnyCapabilityOperator(disabled)),
                    null,
                    null);
            }

            default:
                return null;
        }
    }

    private static TermInfoSourceField CreateCapabilityField(
        TermInfoSourceFieldKind kind,
        TermInfoSourceToken token,
        string capabilityName,
        int? numericValue,
        string? stringValue)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(capabilityName);

        return new TermInfoSourceField(
            kind,
            capabilityName,
            null,
            numericValue,
            stringValue,
            token.Text,
            token.Span);
    }

    private static string NormalizeCapabilityName(
        string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Trim();
    }

    private static string TextBeforeOperator(
        string text,
        char operatorCharacter)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index =
            text.IndexOf(operatorCharacter);
        return (index < 0)
            ? text
            : text[..index]
        ;
    }

    private static string TextAfterOperator(
        string text,
        char operatorCharacter)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index =
            text.IndexOf(operatorCharacter);
        return (index < 0)
            ? string.Empty
            : text[(index + 1)..]
        ;
    }

    private static string TextBeforeAnyCapabilityOperator(
        string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int end =
            text.Length;
        foreach (char operatorCharacter in new[] { '#', '=', '@' })
        {
            int index =
                text.IndexOf(operatorCharacter);
            if (index >= 0)
            {
                end =
                    Math.Min(
                        end,
                        index);
            }
        }

        return text[..end];
    }

    private static void AddDiagnostics(
        ICollection<TermInfoSourceDiagnostic> destination,
        IEnumerable<TermInfoSourceDiagnostic> source)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(source);

        foreach (TermInfoSourceDiagnostic diagnostic in source)
        {
            destination.Add(diagnostic);
        }
    }

    private static TermInfoSourceSpan CreateEntrySpan(
        TermInfoSourceSpan first,
        TermInfoSourceSpan last)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(last);

        return new TermInfoSourceSpan(
            first.SourceName,
            first.Offset,
            first.Line,
            first.Column,
            checked(last.EndOffset - first.Offset));
    }
}
