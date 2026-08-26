namespace Icod.TermInfo.Source;

internal sealed class TermInfoSourceScanner
{
    private readonly string _source;
    private readonly string? _sourceName;
    private readonly List<TermInfoSourceToken> _tokens = [];
    private readonly List<TermInfoSourceDiagnostic> _diagnostics = [];
    private readonly int[] _lineStarts;
    private bool _hasEntry;

    internal TermInfoSourceScanner(
        string source,
        string? sourceName)
    {
        ArgumentNullException.ThrowIfNull(source);

        _source = source;
        _sourceName = sourceName;
        _lineStarts = BuildLineStarts(source);
    }

    internal TermInfoSourceLexResult Scan()
    {
        int offset = 0;
        int fieldStart = -1;
        int fieldStartColumn = 0;

        while (offset < _source.Length)
        {
            if (fieldStart < 0)
            {
                char trivia = _source[offset];

                if (IsInterFieldWhitespace(trivia))
                {
                    offset++;
                    continue;
                }

                (int line, int column) =
                    GetLineAndColumn(offset);

                if (trivia == '#'
                    && column == 1)
                {
                    offset =
                        ScanComment(
                            offset,
                            line,
                            column);
                    continue;
                }

                if (trivia == ',')
                {
                    AddDiagnostic(
                        TermInfoSourceDiagnosticCodes.EmptyField,
                        "A field separator appears without a field before it.",
                        CreateSpan(
                            offset,
                            1));
                    offset++;
                    continue;
                }

                fieldStart = offset;
                fieldStartColumn = column;
            }

            char current = _source[offset];
            if (current == ','
                && !IsEscaped(offset, fieldStart))
            {
                ProcessField(
                    fieldStart,
                    offset,
                    fieldStartColumn);
                fieldStart = -1;
                fieldStartColumn = 0;
            }

            offset++;
        }

        if (fieldStart >= 0)
        {
            ProcessField(
                fieldStart,
                _source.Length,
                fieldStartColumn);

            AddDiagnostic(
                TermInfoSourceDiagnosticCodes.MissingFieldTerminator,
                "The source field is not terminated by a comma.",
                CreateSpan(
                    fieldStart,
                    _source.Length - fieldStart));
        }

        return new TermInfoSourceLexResult(
            _tokens,
            _diagnostics);
    }

    private int ScanComment(
        int start,
        int line,
        int column)
    {
        if (start < 0
            || start >= _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start));
        }

        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(line));
        }

        if (column < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(column));
        }

        int end = start;
        while (end < _source.Length
            && _source[end] != '\r'
            && _source[end] != '\n')
        {
            end++;
        }

        AddToken(
            TermInfoSourceTokenKind.Comment,
            start,
            end);

        return end;
    }

    private void ProcessField(
        int start,
        int end,
        int startColumn)
    {
        ValidateRange(
            start,
            end);

        if (startColumn < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startColumn));
        }

        if (startColumn == 1)
        {
            ProcessHeader(
                start,
                end);
            _hasEntry = true;
            return;
        }

        if (!_hasEntry)
        {
            AddDiagnostic(
                TermInfoSourceDiagnosticCodes.OrphanedCapabilityField,
                "An indented capability field appears before any entry header.",
                CreateSpan(
                    start,
                    end - start));
        }

        ProcessCapability(
            start,
            end);
    }

    private void ProcessHeader(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        List<(int Start, int End)> segments = [];
        int segmentStart = start;

        for (int offset = start; offset < end; offset++)
        {
            if (_source[offset] == '|'
                && !IsEscaped(offset, segmentStart))
            {
                segments.Add(
                    (segmentStart, offset));
                segmentStart = offset + 1;
            }
        }

        segments.Add(
            (segmentStart, end));

        (int canonicalStart, int canonicalEnd) =
            segments[0];
        AddToken(
            TermInfoSourceTokenKind.TerminalName,
            canonicalStart,
            canonicalEnd);

        if (canonicalStart == canonicalEnd)
        {
            AddDiagnostic(
                TermInfoSourceDiagnosticCodes.EmptyTerminalName,
                "The entry header does not contain a canonical terminal name.",
                CreateSpan(
                    canonicalStart,
                    0));
        }

        if (segments.Count == 1)
        {
            return;
        }

        for (int index = 1; index < segments.Count - 1; index++)
        {
            (int aliasStart, int aliasEnd) =
                segments[index];
            AddToken(
                TermInfoSourceTokenKind.Alias,
                aliasStart,
                aliasEnd);

            if (aliasStart == aliasEnd)
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.EmptyHeaderComponent,
                    "The entry header contains an empty alias.",
                    CreateSpan(
                        aliasStart,
                        0));
            }
        }

        (int descriptionStart, int descriptionEnd) =
            segments[^1];

        if (descriptionStart < descriptionEnd
            && !ContainsWhitespace(
                descriptionStart,
                descriptionEnd))
        {
            // ncurses accepts a final names-field component without embedded
            // whitespace as both an alias and the verbose name. Preserve both
            // lexical roles so later source-model work does not lose the alias.
            AddToken(
                TermInfoSourceTokenKind.Alias,
                descriptionStart,
                descriptionEnd);
        }

        AddToken(
            TermInfoSourceTokenKind.Description,
            descriptionStart,
            descriptionEnd);

        if (descriptionStart == descriptionEnd)
        {
            AddDiagnostic(
                TermInfoSourceDiagnosticCodes.EmptyHeaderComponent,
                "The entry header contains an empty descriptive-name component.",
                CreateSpan(
                    descriptionStart,
                    0));
        }
    }

    private void ProcessCapability(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        int effectiveEnd =
            TrimTrailingInterFieldWhitespace(
                start,
                end);

        if (start < effectiveEnd
            && _source[start] == '.')
        {
            if (IsOnlyWhitespace(
                    start + 1,
                    effectiveEnd))
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.MissingCapabilityName,
                    "A disabled capability marker is not followed by a capability name.",
                    CreateSpan(
                        start,
                        Math.Max(
                            1,
                            effectiveEnd - start)));
                AddToken(
                    TermInfoSourceTokenKind.Invalid,
                    start,
                    end);
                return;
            }

            AddToken(
                TermInfoSourceTokenKind.DisabledCapability,
                start,
                end);
            return;
        }

        int operatorOffset =
            FindCapabilityOperator(
                start,
                effectiveEnd);
        if (operatorOffset < 0)
        {
            AddToken(
                TermInfoSourceTokenKind.BooleanCapability,
                start,
                end);
            return;
        }

        int nameEnd =
            TrimTrailingInterFieldWhitespace(
                start,
                operatorOffset);
        if (nameEnd == start)
        {
            AddDiagnostic(
                TermInfoSourceDiagnosticCodes.MissingCapabilityName,
                "A capability operator appears without a capability name.",
                CreateSpan(
                    operatorOffset,
                    1));
            AddToken(
                TermInfoSourceTokenKind.Invalid,
                start,
                end);
            return;
        }

        char operation =
            _source[operatorOffset];

        if (operation == '@')
        {
            if (!IsOnlyWhitespace(
                    operatorOffset + 1,
                    effectiveEnd))
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.UnexpectedTextAfterCancellation,
                    "A cancelled capability cannot contain text after '@'.",
                    CreateSpan(
                        operatorOffset + 1,
                        effectiveEnd - operatorOffset - 1));
            }

            AddToken(
                TermInfoSourceTokenKind.CancelledCapability,
                start,
                end);
            return;
        }

        if (operation == '#')
        {
            AddToken(
                TermInfoSourceTokenKind.NumericCapability,
                start,
                end);
            return;
        }

        if (operation != '=')
        {
            throw new InvalidOperationException(
                "The capability operator scanner returned an unsupported operator.");
        }

        string capabilityName =
            _source.Substring(
                start,
                nameEnd - start);
        if (string.Equals(
                capabilityName,
                "use",
                StringComparison.Ordinal))
        {
            if (IsOnlyWhitespace(
                    operatorOffset + 1,
                    effectiveEnd))
            {
                AddDiagnostic(
                    TermInfoSourceDiagnosticCodes.MissingUseReference,
                    "A use= field must identify a parent terminal entry.",
                    CreateSpan(
                        operatorOffset,
                        Math.Max(
                            1,
                            effectiveEnd - operatorOffset)));
            }

            AddToken(
                TermInfoSourceTokenKind.UseReference,
                start,
                end);
            return;
        }

        AddToken(
            TermInfoSourceTokenKind.StringCapability,
            start,
            end);
    }

    private bool ContainsWhitespace(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        for (int offset = start; offset < end; offset++)
        {
            if (char.IsWhiteSpace(
                    _source[offset]))
            {
                return true;
            }
        }

        return false;
    }

    private int FindCapabilityOperator(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        for (int offset = start; offset < end; offset++)
        {
            char current =
                _source[offset];
            if ((current == '#'
                    || current == '='
                    || current == '@')
                && !IsEscaped(
                    offset,
                    start))
            {
                return offset;
            }
        }

        return -1;
    }

    private bool IsEscaped(
        int offset,
        int lowerBound)
    {
        if (offset < 0
            || offset > _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset));
        }

        if (lowerBound < 0
            || lowerBound > offset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lowerBound));
        }

        int backslashes = 0;
        for (int index = offset - 1;
            index >= lowerBound
            && _source[index] == '\\';
            index--)
        {
            backslashes++;
        }

        return (backslashes & 1) != 0;
    }

    private int TrimTrailingInterFieldWhitespace(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        while (end > start
            && IsInterFieldWhitespace(
                _source[end - 1]))
        {
            end--;
        }

        return end;
    }

    private bool IsOnlyWhitespace(
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        for (int offset = start; offset < end; offset++)
        {
            if (!IsInterFieldWhitespace(
                    _source[offset]))
            {
                return false;
            }
        }

        return true;
    }

    private void AddToken(
        TermInfoSourceTokenKind kind,
        int start,
        int end)
    {
        ValidateRange(
            start,
            end);

        _tokens.Add(
            new TermInfoSourceToken(
                kind,
                _source.Substring(
                    start,
                    end - start),
                CreateSpan(
                    start,
                    end - start)));
    }

    private void AddDiagnostic(
        string code,
        string message,
        TermInfoSourceSpan? span)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        _diagnostics.Add(
            new TermInfoSourceDiagnostic(
                code,
                TermInfoSourceDiagnosticSeverity.Error,
                message,
                span));
    }

    private TermInfoSourceSpan CreateSpan(
        int offset,
        int length)
    {
        if (offset < 0
            || offset > _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset));
        }

        if (length < 0
            || length > _source.Length - offset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length));
        }

        (int line, int column) =
            GetLineAndColumn(offset);

        return new TermInfoSourceSpan(
            _sourceName,
            offset,
            line,
            column,
            length);
    }

    private (int Line, int Column) GetLineAndColumn(
        int offset)
    {
        if (offset < 0
            || offset > _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset));
        }

        int index =
            Array.BinarySearch(
                _lineStarts,
                offset);
        if (index < 0)
        {
            index = ~index - 1;
        }

        return (
            index + 1,
            offset - _lineStarts[index] + 1);
    }

    private static int[] BuildLineStarts(
        string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        List<int> starts =
        [
            0,
        ];

        for (int offset = 0; offset < source.Length; offset++)
        {
            if (source[offset] == '\r')
            {
                if (offset + 1 < source.Length
                    && source[offset + 1] == '\n')
                {
                    offset++;
                }

                starts.Add(offset + 1);
            }
            else if (source[offset] == '\n')
            {
                starts.Add(offset + 1);
            }
        }

        return starts.ToArray();
    }

    private void ValidateRange(
        int start,
        int end)
    {
        if (start < 0
            || start > _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start));
        }

        if (end < start
            || end > _source.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end));
        }
    }

    private static bool IsInterFieldWhitespace(
        char value)
    {
        return value == ' '
            || value == '\t'
            || value == '\r'
            || value == '\n'
            || value == '\f'
            || value == '\v';
    }
}
