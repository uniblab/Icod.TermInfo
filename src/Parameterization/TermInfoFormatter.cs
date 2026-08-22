using System.Globalization;

namespace Icod.TermInfo;

internal static class TermInfoFormatter
{
    internal static string Format(
        TermInfoFormatSpecification specification,
        TermInfoParameter value,
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return specification.Conversion switch
        {
            's' => FormatString(specification, value, position),
            'd' or 'o' or 'x' or 'X' =>
                FormatInteger(specification, value, position),
            _ => throw new ArgumentOutOfRangeException(nameof(specification)),
        };
    }

    private static string FormatString(
        TermInfoFormatSpecification specification,
        TermInfoParameter value,
        int position)
    {
        if (!value.IsString)
        {
            throw new TermInfoEvaluationException(
                "String formatting requires a string value",
                position);
        }

        string text = value.StringValue;
        if (specification.Precision is int precision && text.Length > precision)
        {
            text = text[..precision];
        }

        return ApplySpaceWidth(
            text,
            specification.Width,
            specification.LeftJustify);
    }

    private static string FormatInteger(
        TermInfoFormatSpecification specification,
        TermInfoParameter value,
        int position)
    {
        if (!value.IsInteger)
        {
            throw new TermInfoEvaluationException(
                "Numeric formatting requires an integer value",
                position);
        }

        long integer = value.IntegerValue;
        string sign = string.Empty;
        string prefix = string.Empty;
        string digits;

        switch (specification.Conversion)
        {
            case 'd':
                ulong magnitude;
                if (integer < 0)
                {
                    sign = "-";
                    magnitude = (ulong)(-(integer + 1)) + 1;
                }
                else
                {
                    magnitude = (ulong)integer;
                    if (specification.AlwaysSign)
                    {
                        sign = "+";
                    }
                    else if (specification.SpaceSign)
                    {
                        sign = " ";
                    }
                }

                digits = magnitude.ToString(CultureInfo.InvariantCulture);
                break;
            case 'o':
                digits = FormatUnsigned(unchecked((ulong)integer), 8, upper: false);
                break;
            case 'x':
                digits = unchecked((ulong)integer).ToString("x", CultureInfo.InvariantCulture);
                if (specification.AlternateForm && integer != 0)
                {
                    prefix = "0x";
                }
                break;
            case 'X':
                digits = unchecked((ulong)integer).ToString("X", CultureInfo.InvariantCulture);
                if (specification.AlternateForm && integer != 0)
                {
                    prefix = "0X";
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(specification));
        }

        if (specification.Precision is int precision)
        {
            if (precision == 0 && integer == 0)
            {
                digits = string.Empty;
            }
            else if (digits.Length < precision)
            {
                digits = new string('0', precision - digits.Length) + digits;
            }
        }

        if (specification.Conversion == 'o' && specification.AlternateForm)
        {
            if (digits.Length == 0 || digits[0] != '0')
            {
                prefix = "0";
            }
        }

        string leading = sign + prefix;
        int width = specification.Width ?? 0;
        int paddingLength = width - leading.Length - digits.Length;
        if (paddingLength <= 0)
        {
            return leading + digits;
        }

        if (specification.LeftJustify)
        {
            return leading + digits + new string(' ', paddingLength);
        }

        if (specification.ZeroPad && specification.Precision is null)
        {
            return leading + new string('0', paddingLength) + digits;
        }

        return new string(' ', paddingLength) + leading + digits;
    }

    private static string ApplySpaceWidth(
        string value,
        int? width,
        bool leftJustify)
    {
        ArgumentNullException.ThrowIfNull(value);

        int paddingLength = (width ?? 0) - value.Length;
        if (paddingLength <= 0)
        {
            return value;
        }

        return leftJustify
            ? value + new string(' ', paddingLength)
            : new string(' ', paddingLength) + value;
    }

    private static string FormatUnsigned(
        ulong value,
        int radix,
        bool upper)
    {
        if (radix is < 2 or > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(radix));
        }

        const string LowerDigits = "0123456789abcdef";
        const string UpperDigits = "0123456789ABCDEF";
        string alphabet = upper ? UpperDigits : LowerDigits;

        if (value == 0)
        {
            return "0";
        }

        Span<char> buffer = stackalloc char[64];
        int index = buffer.Length;
        ulong unsignedRadix = (ulong)radix;

        while (value != 0)
        {
            ulong remainder = value % unsignedRadix;
            buffer[--index] = alphabet[(int)remainder];
            value /= unsignedRadix;
        }

        return new string(buffer[index..]);
    }
}
