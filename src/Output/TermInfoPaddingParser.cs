using System.Globalization;

namespace Icod.TermInfo;

internal static class TermInfoPaddingParser
{
    internal const decimal MaximumDelayMilliseconds = 30000m;

    internal static IReadOnlyList<TermInfoOutputSegment> Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        List<TermInfoOutputSegment> segments = [];
        int scan = 0;
        int textStart = 0;

        while (scan < value.Length)
        {
            if ((value[scan] != '$')
                || ((scan + 1) >= value.Length)
                || (value[scan + 1] != '<'))
            {
                scan++;
                continue;
            }

            if (scan > textStart)
            {
                segments.Add(
                    TermInfoOutputSegment.CreateText(
                        value[textStart..scan]));
            }

            int directiveStart = scan;
            int contentStart = scan + 2;
            int close = value.IndexOf('>', contentStart);
            if (close < 0)
            {
                throw CreateFormatException(
                    directiveStart,
                    "The padding directive is missing its closing '>'.");
            }

            segments.Add(
                ParseDirective(
                    value,
                    directiveStart,
                    contentStart,
                    close));

            scan = close + 1;
            textStart = scan;
        }

        if (textStart < value.Length)
        {
            segments.Add(
                TermInfoOutputSegment.CreateText(
                    value[textStart..]));
        }

        return segments.AsReadOnly();
    }

    private static TermInfoOutputSegment ParseDirective(
        string value,
        int directiveStart,
        int contentStart,
        int close)
    {
        ArgumentNullException.ThrowIfNull(value);

        int index = contentStart;
        int numberStart = index;

        while ((index < close)
            && char.IsAsciiDigit(value[index]))
        {
            index++;
        }

        if (index == numberStart)
        {
            throw CreateFormatException(
                directiveStart,
                "A padding directive must begin with a nonnegative number.");
        }

        if ((index < close) && (value[index] == '.'))
        {
            index++;

            if ((index >= close)
                || !char.IsAsciiDigit(value[index]))
            {
                throw CreateFormatException(
                    directiveStart,
                    "A padding delay may contain one decimal digit after '.'.");
            }

            index++;

            if ((index < close)
                && char.IsAsciiDigit(value[index]))
            {
                throw CreateFormatException(
                    directiveStart,
                    "A padding delay may contain at most one decimal digit.");
            }
        }

        string numberText = value[numberStart..index];
        if (!decimal.TryParse(
            numberText,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out decimal milliseconds))
        {
            throw CreateFormatException(
                directiveStart,
                "The padding delay is not a valid number.");
        }

        bool multiplyByAffectedLines = false;
        bool isMandatory = false;

        while (index < close)
        {
            switch (value[index])
            {
                case '*':
                    if (multiplyByAffectedLines)
                    {
                        throw CreateFormatException(
                            directiveStart,
                            "A padding directive may contain '*' only once.");
                    }

                    multiplyByAffectedLines = true;
                    break;

                case '/':
                    if (isMandatory)
                    {
                        throw CreateFormatException(
                            directiveStart,
                            "A padding directive may contain '/' only once.");
                    }

                    isMandatory = true;
                    break;

                default:
                    throw CreateFormatException(
                        directiveStart,
                        $"Unexpected character '{value[index]}' in padding directive.");
            }

            index++;
        }

        return TermInfoOutputSegment.CreatePadding(
            milliseconds,
            multiplyByAffectedLines,
            isMandatory);
    }

    private static TermInfoPaddingFormatException CreateFormatException(
        int position,
        string message)
    {
        return new TermInfoPaddingFormatException(
            $"Malformed terminfo padding at position {position}: {message}",
            position);
    }
}
