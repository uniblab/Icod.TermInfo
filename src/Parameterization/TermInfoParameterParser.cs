using System.Globalization;
using System.Text;

namespace Icod.TermInfo;

internal sealed class TermInfoParameterParser
{
    private readonly string _source;
    private int _position;

    internal TermInfoParameterParser(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    internal IReadOnlyList<TermInfoInstruction> Parse()
    {
        ParseSequenceResult result = ParseSequence(TermInfoDelimiter.None);
        if (result.Delimiter != TermInfoDelimiter.EndOfInput)
        {
            throw CreateFormatException(
                "Unexpected conditional delimiter",
                result.DelimiterPosition);
        }

        return result.Instructions;
    }

    private ParseSequenceResult ParseSequence(TermInfoDelimiter allowedDelimiters)
    {
        List<TermInfoInstruction> instructions = [];
        StringBuilder literal = new();
        int literalStart = _position;

        while (_position < _source.Length)
        {
            if (_source[_position] != '%')
            {
                if (literal.Length == 0)
                {
                    literalStart = _position;
                }

                literal.Append(_source[_position]);
                _position++;
                continue;
            }

            int percentPosition = _position;
            if (_position + 1 >= _source.Length)
            {
                throw CreateFormatException(
                    "A parameter directive cannot end with '%'",
                    percentPosition);
            }

            char code = _source[_position + 1];
            if (code == '%')
            {
                if (literal.Length == 0)
                {
                    literalStart = percentPosition;
                }

                literal.Append('%');
                _position += 2;
                continue;
            }

            TermInfoDelimiter delimiter = GetDelimiter(code);
            if (delimiter != TermInfoDelimiter.None)
            {
                if ((allowedDelimiters & delimiter) == 0)
                {
                    throw CreateFormatException(
                        $"Unexpected conditional directive '%{code}'",
                        percentPosition);
                }

                FlushLiteral(instructions, literal, literalStart);
                _position += 2;
                return new ParseSequenceResult(
                    instructions,
                    delimiter,
                    percentPosition);
            }

            FlushLiteral(instructions, literal, literalStart);

            if (code == '?')
            {
                instructions.Add(ParseConditional(percentPosition));
                literalStart = _position;
                continue;
            }

            instructions.Add(ParseDirective(percentPosition, code));
            literalStart = _position;
        }

        FlushLiteral(instructions, literal, literalStart);
        return new ParseSequenceResult(
            instructions,
            TermInfoDelimiter.EndOfInput,
            _source.Length);
    }

    private TermInfoInstruction ParseConditional(int percentPosition)
    {
        _position += 2;

        ParseSequenceResult condition = ParseSequence(TermInfoDelimiter.Then);
        if (condition.Delimiter != TermInfoDelimiter.Then)
        {
            throw CreateFormatException(
                "Conditional expression is missing '%t'",
                percentPosition);
        }

        ParseSequenceResult body = ParseSequence(
            TermInfoDelimiter.Else | TermInfoDelimiter.End);
        if (body.Delimiter == TermInfoDelimiter.EndOfInput)
        {
            throw CreateFormatException(
                "Conditional expression is missing '%;'",
                percentPosition);
        }

        List<TermInfoConditionalBranch> branches =
        [
            new TermInfoConditionalBranch(
                condition.Instructions,
                body.Instructions),
        ];

        if (body.Delimiter == TermInfoDelimiter.End)
        {
            return new TermInfoConditionalInstruction(
                percentPosition,
                branches,
                Array.Empty<TermInfoInstruction>());
        }

        while (true)
        {
            ParseSequenceResult elseCandidate = ParseSequence(
                TermInfoDelimiter.Then | TermInfoDelimiter.End);

            if (elseCandidate.Delimiter == TermInfoDelimiter.EndOfInput)
            {
                throw CreateFormatException(
                    "Conditional expression is missing '%;'",
                    percentPosition);
            }

            if (elseCandidate.Delimiter == TermInfoDelimiter.End)
            {
                return new TermInfoConditionalInstruction(
                    percentPosition,
                    branches,
                    elseCandidate.Instructions);
            }

            ParseSequenceResult elseIfBody = ParseSequence(
                TermInfoDelimiter.Else | TermInfoDelimiter.End);
            if (elseIfBody.Delimiter == TermInfoDelimiter.EndOfInput)
            {
                throw CreateFormatException(
                    "Conditional expression is missing '%;'",
                    percentPosition);
            }

            branches.Add(
                new TermInfoConditionalBranch(
                    elseCandidate.Instructions,
                    elseIfBody.Instructions));

            if (elseIfBody.Delimiter == TermInfoDelimiter.End)
            {
                return new TermInfoConditionalInstruction(
                    percentPosition,
                    branches,
                    Array.Empty<TermInfoInstruction>());
            }
        }
    }

    private TermInfoInstruction ParseDirective(
        int percentPosition,
        char code)
    {
        switch (code)
        {
            case 'p':
                return ParseParameter(percentPosition);
            case 'P':
                return ParseVariable(percentPosition, set: true);
            case 'g':
                return ParseVariable(percentPosition, set: false);
            case '\'':
                return ParseCharacterConstant(percentPosition);
            case '{':
                return ParseIntegerConstant(percentPosition);
            case 'l':
                _position += 2;
                return new TermInfoStringLengthInstruction(percentPosition);
            case 'i':
                _position += 2;
                return new TermInfoIncrementParametersInstruction(percentPosition);
            case 'c':
                _position += 2;
                return new TermInfoCharacterOutputInstruction(percentPosition);
            case 'd':
            case 'o':
            case 'x':
            case 'X':
            case 's':
                return ParseRequiredFormat(percentPosition);
            case '+':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Add);
            case '-':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Subtract);
            case '*':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Multiply);
            case '/':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Divide);
            case 'm':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Modulo);
            case '&':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.BitwiseAnd);
            case '|':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.BitwiseOr);
            case '^':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.BitwiseXor);
            case '=':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.Equal);
            case '>':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.GreaterThan);
            case '<':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.LessThan);
            case 'A':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.LogicalAnd);
            case 'O':
                _position += 2;
                return new TermInfoBinaryInstruction(
                    percentPosition,
                    TermInfoBinaryOperator.LogicalOr);
            case '!':
                _position += 2;
                return new TermInfoUnaryInstruction(
                    percentPosition,
                    TermInfoUnaryOperator.LogicalNot);
            case '~':
                _position += 2;
                return new TermInfoUnaryInstruction(
                    percentPosition,
                    TermInfoUnaryOperator.BitwiseNot);
            case ':':
            case '#':
            case ' ':
            case '.':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return ParseRequiredFormat(percentPosition);
            default:
                throw CreateFormatException(
                    $"Unknown parameter directive '%{code}'",
                    percentPosition);
        }
    }

    private TermInfoInstruction ParseParameter(int percentPosition)
    {
        if (_position + 2 >= _source.Length)
        {
            throw CreateFormatException(
                "Parameter reference is missing an index",
                percentPosition);
        }

        char parameter = _source[_position + 2];
        if (parameter is < '1' or > '9')
        {
            throw CreateFormatException(
                "Parameter references must use p1 through p9",
                percentPosition);
        }

        _position += 3;
        return new TermInfoPushParameterInstruction(
            percentPosition,
            parameter - '1');
    }

    private TermInfoInstruction ParseVariable(
        int percentPosition,
        bool set)
    {
        if (_position + 2 >= _source.Length)
        {
            throw CreateFormatException(
                "Variable directive is missing a variable name",
                percentPosition);
        }

        char name = _source[_position + 2];
        if (!IsVariableName(name))
        {
            throw CreateFormatException(
                "Terminfo variables must be named A-Z or a-z",
                percentPosition);
        }

        _position += 3;
        return set
            ? new TermInfoSetVariableInstruction(percentPosition, name)
            : new TermInfoGetVariableInstruction(percentPosition, name);
    }

    private TermInfoInstruction ParseCharacterConstant(int percentPosition)
    {
        if (_position + 3 >= _source.Length || _source[_position + 3] != '\'')
        {
            throw CreateFormatException(
                "Character constants must contain exactly one character",
                percentPosition);
        }

        char value = _source[_position + 2];
        _position += 4;
        return new TermInfoPushCharacterInstruction(percentPosition, value);
    }

    private TermInfoInstruction ParseIntegerConstant(int percentPosition)
    {
        int start = _position + 2;
        int close = _source.IndexOf('}', start);
        if (close < 0)
        {
            throw CreateFormatException(
                "Integer constant is missing '}'",
                percentPosition);
        }

        string text = _source[start..close];
        if (text.Length == 0 ||
            !long.TryParse(
                text,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out long value))
        {
            throw CreateFormatException(
                "Integer constant is invalid",
                percentPosition);
        }

        _position = close + 1;
        return new TermInfoPushIntegerInstruction(percentPosition, value);
    }

    private TermInfoInstruction ParseRequiredFormat(int percentPosition)
    {
        if (!TryParseFormat(percentPosition, out TermInfoFormatInstruction instruction))
        {
            throw CreateFormatException(
                "Invalid printf-style terminfo format",
                percentPosition);
        }

        return instruction;
    }

    private bool TryParseFormat(
        int percentPosition,
        out TermInfoFormatInstruction instruction)
    {
        int index = percentPosition + 1;
        if (index >= _source.Length)
        {
            instruction = null!;
            return false;
        }

        if (_source[index] == ':')
        {
            index++;
        }

        bool leftJustify = false;
        bool alwaysSign = false;
        bool spaceSign = false;
        bool alternateForm = false;

        bool readingFlags = true;
        while (readingFlags && index < _source.Length)
        {
            switch (_source[index])
            {
                case '-':
                    leftJustify = true;
                    index++;
                    break;
                case '+':
                    alwaysSign = true;
                    index++;
                    break;
                case ' ':
                    spaceSign = true;
                    index++;
                    break;
                case '#':
                    alternateForm = true;
                    index++;
                    break;
                default:
                    readingFlags = false;
                    break;
            }
        }

        int widthStart = index;
        while (index < _source.Length && char.IsAsciiDigit(_source[index]))
        {
            index++;
        }

        int? width = null;
        bool zeroPad = false;
        if (index > widthStart)
        {
            string widthText = _source[widthStart..index];
            if (!int.TryParse(
                widthText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int parsedWidth))
            {
                throw CreateFormatException(
                    "Format width is too large",
                    percentPosition);
            }

            if (parsedWidth > 10_000)
            {
                throw CreateFormatException(
                    "Format width cannot exceed 10000",
                    percentPosition);
            }

            width = parsedWidth;
            zeroPad = widthText.Length > 1 && widthText[0] == '0';
        }

        int? precision = null;
        if (index < _source.Length && _source[index] == '.')
        {
            index++;
            int precisionStart = index;
            while (index < _source.Length && char.IsAsciiDigit(_source[index]))
            {
                index++;
            }

            if (index == precisionStart)
            {
                throw CreateFormatException(
                    "Format precision requires digits",
                    percentPosition);
            }

            if (!int.TryParse(
                _source[precisionStart..index],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int parsedPrecision))
            {
                throw CreateFormatException(
                    "Format precision is too large",
                    percentPosition);
            }

            if (parsedPrecision > 10_000)
            {
                throw CreateFormatException(
                    "Format precision cannot exceed 10000",
                    percentPosition);
            }

            precision = parsedPrecision;
        }

        if (index >= _source.Length ||
            _source[index] is not ('d' or 'o' or 'x' or 'X' or 's'))
        {
            instruction = null!;
            return false;
        }

        char conversion = _source[index];
        if (conversion == 's' &&
            (alwaysSign || spaceSign || alternateForm))
        {
            throw CreateFormatException(
                "String formatting does not accept sign or alternate-form flags",
                percentPosition);
        }

        _position = index + 1;
        instruction = new TermInfoFormatInstruction(
            percentPosition,
            new TermInfoFormatSpecification(
                conversion,
                leftJustify,
                alwaysSign,
                spaceSign,
                alternateForm,
                zeroPad,
                width,
                precision));
        return true;
    }

    private static bool IsVariableName(char value)
    {
        return value is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    private static TermInfoDelimiter GetDelimiter(char code)
    {
        return code switch
        {
            't' => TermInfoDelimiter.Then,
            'e' => TermInfoDelimiter.Else,
            ';' => TermInfoDelimiter.End,
            _ => TermInfoDelimiter.None,
        };
    }

    private static void FlushLiteral(
        ICollection<TermInfoInstruction> instructions,
        StringBuilder literal,
        int literalStart)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        ArgumentNullException.ThrowIfNull(literal);

        if (literal.Length == 0)
        {
            return;
        }

        instructions.Add(
            new TermInfoLiteralInstruction(
                literalStart,
                literal.ToString()));
        literal.Clear();
    }

    private static TermInfoFormatException CreateFormatException(
        string message,
        int position)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new TermInfoFormatException(message, position);
    }

    private readonly record struct ParseSequenceResult(
        IReadOnlyList<TermInfoInstruction> Instructions,
        TermInfoDelimiter Delimiter,
        int DelimiterPosition);

    [Flags]
    private enum TermInfoDelimiter
    {
        None = 0,
        Then = 1,
        Else = 2,
        End = 4,
        EndOfInput = 8,
    }
}
