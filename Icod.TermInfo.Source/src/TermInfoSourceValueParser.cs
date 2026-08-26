using System.Text;

namespace Icod.TermInfo.Source;

/// <summary>
/// Interprets numeric and string values from lexical terminfo source tokens.
/// </summary>
/// <remarks>
/// <para>
/// This layer operates after <see cref="TermInfoSourceLexer"/>. It does not
/// classify capability names, construct unresolved entries, or resolve
/// inheritance.
/// </para>
/// <para>
/// Numeric values follow the System V/ncurses source conventions for decimal,
/// octal, and hexadecimal spelling while materializing into the signed 32-bit
/// numeric model used by <c>Icod.TermInfo</c>. String decoding preserves the
/// historical byte semantics used by compiled terminfo strings.
/// </para>
/// </remarks>
public static class TermInfoSourceValueParser
{
    /// <summary>
    /// Interprets the value of a numeric capability token.
    /// </summary>
    /// <param name="token">A numeric capability token produced by the lexer.</param>
    /// <returns>The decoded value and deterministic value diagnostics.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="token"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="token"/> is not a numeric capability token.
    /// </exception>
    public static TermInfoSourceNumericValueResult ParseNumeric(
        TermInfoSourceToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind != TermInfoSourceTokenKind.NumericCapability)
        {
            throw new ArgumentException(
                "The token must be a numeric capability token.",
                nameof(token));
        }

        int operatorOffset =
            FindUnescapedOperator(
                token.Text,
                '#');
        if (operatorOffset < 0)
        {
            throw new InvalidOperationException(
                "The numeric capability token does not contain its lexical '#' operator.");
        }

        int valueStart =
            operatorOffset + 1;
        if (valueStart == token.Text.Length)
        {
            return NumericError(
                token,
                TermInfoSourceDiagnosticCodes.MissingNumericValue,
                "A numeric capability must contain a value after '#'.",
                valueStart,
                0);
        }

        ReadOnlySpan<char> spelling =
            token.Text.AsSpan(valueStart);

        int numberBase;
        int digitStart;
        if (spelling.Length >= 2
            && spelling[0] == '0'
            && (spelling[1] == 'x'
                || spelling[1] == 'X'))
        {
            numberBase = 16;
            digitStart = 2;

            if (spelling.Length == 2)
            {
                return NumericError(
                    token,
                    TermInfoSourceDiagnosticCodes.InvalidNumericValue,
                    "A hexadecimal numeric capability must contain at least one hexadecimal digit after '0x'.",
                    valueStart,
                    spelling.Length);
            }
        }
        else if (spelling.Length > 1
            && spelling[0] == '0')
        {
            numberBase = 8;
            digitStart = 1;
        }
        else
        {
            numberBase = 10;
            digitStart = 0;
        }

        long value = 0;
        for (int index = digitStart; index < spelling.Length; index++)
        {
            int digit =
                GetDigitValue(
                    spelling[index]);
            if (digit < 0
                || digit >= numberBase)
            {
                return NumericError(
                    token,
                    TermInfoSourceDiagnosticCodes.InvalidNumericValue,
                    $"'{spelling[index]}' is not valid in a base-{numberBase} numeric capability value.",
                    valueStart + index,
                    1);
            }

            if (value > (int.MaxValue - digit) / numberBase)
            {
                return NumericError(
                    token,
                    TermInfoSourceDiagnosticCodes.NumericValueOutOfRange,
                    $"The numeric capability value exceeds the supported signed 32-bit maximum of {int.MaxValue}.",
                    valueStart,
                    spelling.Length);
            }

            value =
                (value * numberBase)
                + digit;
        }

        return new TermInfoSourceNumericValueResult(
            checked((int)value),
            Array.Empty<TermInfoSourceDiagnostic>());
    }

    /// <summary>
    /// Interprets the value of a string capability token.
    /// </summary>
    /// <param name="token">A string capability token produced by the lexer.</param>
    /// <returns>The decoded value and deterministic value diagnostics.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="token"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="token"/> is not a string capability token.
    /// </exception>
    public static TermInfoSourceStringValueResult ParseString(
        TermInfoSourceToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind != TermInfoSourceTokenKind.StringCapability)
        {
            throw new ArgumentException(
                "The token must be a string capability token.",
                nameof(token));
        }

        int operatorOffset =
            FindUnescapedOperator(
                token.Text,
                '=');
        if (operatorOffset < 0)
        {
            throw new InvalidOperationException(
                "The string capability token does not contain its lexical '=' operator.");
        }

        List<TermInfoSourceDiagnostic> diagnostics = [];
        StringBuilder value =
            new(
                Math.Max(
                    0,
                    token.Text.Length - operatorOffset - 1));

        bool hasErrors = false;
        bool previousScannerCharacterWasPercent = false;
        int index =
            operatorOffset + 1;

        while (index < token.Text.Length)
        {
            char current =
                token.Text[index];

            if (current == '\0')
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        token,
                        TermInfoSourceDiagnosticCodes.EmbeddedNullCharacter,
                        TermInfoSourceDiagnosticSeverity.Error,
                        "A terminfo source string cannot contain a literal NUL character.",
                        index,
                        1));
                hasErrors = true;
                index++;
                previousScannerCharacterWasPercent = false;
                continue;
            }

            if (IsLineBreak(current))
            {
                int next =
                    ConsumeLineBreak(
                        token.Text,
                        index);
                int indentationStart =
                    next;
                next =
                    ConsumeIndentation(
                        token.Text,
                        next);

                if (next < token.Text.Length
                    && next == indentationStart)
                {
                    diagnostics.Add(
                        CreateDiagnostic(
                            token,
                            TermInfoSourceDiagnosticCodes.UnindentedStringContinuation,
                            TermInfoSourceDiagnosticSeverity.Error,
                            "A multiline terminfo string must continue on an indented source line.",
                            index,
                            indentationStart - index));
                    hasErrors = true;
                }

                index = next;
                continue;
            }

            if (current == '^'
                && !previousScannerCharacterWasPercent)
            {
                if (index + 1 >= token.Text.Length)
                {
                    diagnostics.Add(
                        CreateDiagnostic(
                            token,
                            TermInfoSourceDiagnosticCodes.IncompleteControlEscape,
                            TermInfoSourceDiagnosticSeverity.Error,
                            "A '^' control-character escape must be followed by a character.",
                            index,
                            1));
                    hasErrors = true;
                    break;
                }

                char target =
                    token.Text[index + 1];
                if (IsLineBreak(target))
                {
                    diagnostics.Add(
                        CreateDiagnostic(
                            token,
                            TermInfoSourceDiagnosticCodes.IncompleteControlEscape,
                            TermInfoSourceDiagnosticSeverity.Error,
                            "A '^' control-character escape cannot end at a source line boundary.",
                            index,
                            1));
                    hasErrors = true;
                    index++;
                    previousScannerCharacterWasPercent = false;
                    continue;
                }

                if (target == '?')
                {
                    value.Append('\x7f');
                }
                else
                {
                    if (target < '\x20'
                        || target > '\x7e')
                    {
                        diagnostics.Add(
                            CreateDiagnostic(
                                token,
                                TermInfoSourceDiagnosticCodes.InvalidControlEscape,
                                TermInfoSourceDiagnosticSeverity.Warning,
                                "The character following '^' is outside the printable ASCII range used by terminfo control notation.",
                                index,
                                2));
                    }

                    int translated =
                        target & 0x1f;
                    if (translated == 0)
                    {
                        translated = 0x80;
                    }

                    value.Append(
                        (char)translated);
                }

                index += 2;
                previousScannerCharacterWasPercent = false;
                continue;
            }

            if (current == '\\')
            {
                if (index + 1 >= token.Text.Length)
                {
                    diagnostics.Add(
                        CreateDiagnostic(
                            token,
                            TermInfoSourceDiagnosticCodes.IncompleteBackslashEscape,
                            TermInfoSourceDiagnosticSeverity.Error,
                            "A backslash escape must be followed by a character.",
                            index,
                            1));
                    hasErrors = true;
                    break;
                }

                char escape =
                    token.Text[index + 1];

                if (IsLineBreak(escape))
                {
                    int next =
                        ConsumeLineBreak(
                            token.Text,
                            index + 1);
                    index =
                        ConsumeIndentation(
                            token.Text,
                            next);
                    continue;
                }

                if (escape >= '0'
                    && escape <= '7')
                {
                    int number =
                        escape - '0';
                    int consumedDigits = 1;
                    int digitOffset =
                        index + 2;

                    while (consumedDigits < 3
                        && digitOffset < token.Text.Length)
                    {
                        char digitCharacter =
                            token.Text[digitOffset];
                        if (digitCharacter < '0'
                            || digitCharacter > '9')
                        {
                            break;
                        }

                        if (digitCharacter > '7')
                        {
                            diagnostics.Add(
                                CreateDiagnostic(
                                    token,
                                    TermInfoSourceDiagnosticCodes.NonOctalDigitInStringEscape,
                                    TermInfoSourceDiagnosticSeverity.Warning,
                                    $"'{digitCharacter}' is not an octal digit but is retained using ncurses-compatible source semantics.",
                                    digitOffset,
                                    1));
                        }

                        number =
                            (number * 8)
                            + (digitCharacter - '0');
                        consumedDigits++;
                        digitOffset++;
                    }

                    int translated =
                        number & 0xff;
                    if (translated == 0)
                    {
                        translated = 0x80;
                    }

                    value.Append(
                        (char)translated);
                    index +=
                        1 + consumedDigits;
                    previousScannerCharacterWasPercent = false;
                    continue;
                }

                char translatedEscape;
                bool knownEscape = true;
                switch (escape)
                {
                    case 'E':
                    case 'e':
                        translatedEscape = '\x1b';
                        break;

                    case 'a':
                        translatedEscape = '\a';
                        break;

                    case 'n':
                    case 'l':
                        translatedEscape = '\n';
                        break;

                    case 'r':
                        translatedEscape = '\r';
                        break;

                    case 't':
                        translatedEscape = '\t';
                        break;

                    case 'b':
                        translatedEscape = '\b';
                        break;

                    case 'f':
                        translatedEscape = '\f';
                        break;

                    case 's':
                        translatedEscape = ' ';
                        break;

                    case '^':
                        translatedEscape = '^';
                        break;

                    case '\\':
                        translatedEscape = '\\';
                        break;

                    case ',':
                        translatedEscape = ',';
                        break;

                    case ':':
                        translatedEscape = ':';
                        break;

                    case '|':
                        translatedEscape = '|';
                        break;

                    default:
                        translatedEscape = escape;
                        knownEscape = false;
                        break;
                }

                if (!knownEscape)
                {
                    diagnostics.Add(
                        CreateDiagnostic(
                            token,
                            TermInfoSourceDiagnosticCodes.UnknownStringEscape,
                            TermInfoSourceDiagnosticSeverity.Warning,
                            $"'\\{escape}' is not a defined terminfo string escape; the escaped character is retained.",
                            index,
                            2));
                }

                value.Append(
                    translatedEscape);
                index += 2;
                previousScannerCharacterWasPercent =
                    escape == '%';
                continue;
            }

            value.Append(current);
            index++;
            previousScannerCharacterWasPercent =
                current == '%';
        }

        return new TermInfoSourceStringValueResult(
            hasErrors
                ? null
                : value.ToString(),
            diagnostics);
    }

    private static TermInfoSourceNumericValueResult NumericError(
        TermInfoSourceToken token,
        string code,
        string message,
        int relativeOffset,
        int length)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        return new TermInfoSourceNumericValueResult(
            value: null,
            new[]
            {
                CreateDiagnostic(
                    token,
                    code,
                    TermInfoSourceDiagnosticSeverity.Error,
                    message,
                    relativeOffset,
                    length),
            });
    }

    private static TermInfoSourceDiagnostic CreateDiagnostic(
        TermInfoSourceToken token,
        string code,
        TermInfoSourceDiagnosticSeverity severity,
        string message,
        int relativeOffset,
        int length)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(message);

        if (relativeOffset < 0
            || relativeOffset > token.Text.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(relativeOffset));
        }

        if (length < 0
            || length > token.Text.Length - relativeOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length));
        }

        return new TermInfoSourceDiagnostic(
            code,
            severity,
            message,
            CreateRelativeSpan(
                token,
                relativeOffset,
                length));
    }

    private static TermInfoSourceSpan CreateRelativeSpan(
        TermInfoSourceToken token,
        int relativeOffset,
        int length)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (relativeOffset < 0
            || relativeOffset > token.Text.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(relativeOffset));
        }

        if (length < 0
            || length > token.Text.Length - relativeOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length));
        }

        int line =
            token.Span.Line;
        int column =
            token.Span.Column;
        bool previousWasCarriageReturn = false;

        for (int index = 0; index < relativeOffset; index++)
        {
            char current =
                token.Text[index];
            if (current == '\r')
            {
                line++;
                column = 1;
                previousWasCarriageReturn = true;
            }
            else if (current == '\n')
            {
                if (!previousWasCarriageReturn)
                {
                    line++;
                }

                column = 1;
                previousWasCarriageReturn = false;
            }
            else
            {
                column++;
                previousWasCarriageReturn = false;
            }
        }

        return new TermInfoSourceSpan(
            token.Span.SourceName,
            checked(token.Span.Offset + relativeOffset),
            line,
            column,
            length);
    }

    private static int FindUnescapedOperator(
        string text,
        char operation)
    {
        ArgumentNullException.ThrowIfNull(text);

        for (int index = 0; index < text.Length; index++)
        {
            if (text[index] != operation)
            {
                continue;
            }

            int backslashes = 0;
            for (int scan = index - 1;
                scan >= 0
                && text[scan] == '\\';
                scan--)
            {
                backslashes++;
            }

            if ((backslashes & 1) == 0)
            {
                return index;
            }
        }

        return -1;
    }

    private static int GetDigitValue(
        char value)
    {
        if (value >= '0'
            && value <= '9')
        {
            return value - '0';
        }

        if (value >= 'a'
            && value <= 'f')
        {
            return value - 'a' + 10;
        }

        if (value >= 'A'
            && value <= 'F')
        {
            return value - 'A' + 10;
        }

        return -1;
    }

    private static bool IsLineBreak(
        char value)
    {
        return value == '\r'
            || value == '\n';
    }

    private static int ConsumeLineBreak(
        string text,
        int index)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (index < 0
            || index >= text.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }

        if (text[index] == '\r')
        {
            index++;
            if (index < text.Length
                && text[index] == '\n')
            {
                index++;
            }

            return index;
        }

        if (text[index] == '\n')
        {
            return index + 1;
        }

        throw new ArgumentException(
            "The supplied index does not identify a source line break.",
            nameof(index));
    }

    private static int ConsumeIndentation(
        string text,
        int index)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (index < 0
            || index > text.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }

        while (index < text.Length
            && (text[index] == ' '
                || text[index] == '\t'))
        {
            index++;
        }

        return index;
    }
}
