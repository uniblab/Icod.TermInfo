using System.Globalization;

namespace Icod.TermInfo;

/// <summary>
/// Represents one immutable Boolean, numeric, or string value for an extended
/// terminfo capability.
/// </summary>
public readonly struct TermInfoCapabilityValue : IEquatable<TermInfoCapabilityValue>
{
    private readonly TermInfoCapabilityValueKind _kind;
    private readonly bool _booleanValue;
    private readonly int _numberValue;
    private readonly string? _stringValue;

    /// <summary>
    /// Initializes a Boolean extended-capability value.
    /// </summary>
    public TermInfoCapabilityValue(bool value)
    {
        _kind = TermInfoCapabilityValueKind.Boolean;
        _booleanValue = value;
        _numberValue = default;
        _stringValue = null;
    }

    /// <summary>
    /// Initializes a numeric extended-capability value.
    /// </summary>
    public TermInfoCapabilityValue(int value)
    {
        _kind = TermInfoCapabilityValueKind.Number;
        _booleanValue = default;
        _numberValue = value;
        _stringValue = null;
    }

    /// <summary>
    /// Initializes a string extended-capability value.
    /// </summary>
    public TermInfoCapabilityValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _kind = TermInfoCapabilityValueKind.String;
        _booleanValue = default;
        _numberValue = default;
        _stringValue = value;
    }

    /// <summary>
    /// Gets the kind of value carried by this capability.
    /// </summary>
    public TermInfoCapabilityValueKind Kind => _kind;

    /// <summary>
    /// Gets whether this capability value is Boolean.
    /// </summary>
    public bool IsBoolean => _kind == TermInfoCapabilityValueKind.Boolean;

    /// <summary>
    /// Gets whether this capability value is numeric.
    /// </summary>
    public bool IsNumber => _kind == TermInfoCapabilityValueKind.Number;

    /// <summary>
    /// Gets whether this capability value is a string.
    /// </summary>
    public bool IsString => _kind == TermInfoCapabilityValueKind.String;

    /// <summary>
    /// Gets the Boolean value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This capability value is not Boolean.
    /// </exception>
    public bool BooleanValue
    {
        get
        {
            if (!IsBoolean)
            {
                throw CreateWrongKindException(TermInfoCapabilityValueKind.Boolean);
            }

            return _booleanValue;
        }
    }

    /// <summary>
    /// Gets the numeric value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This capability value is not numeric.
    /// </exception>
    public int NumberValue
    {
        get
        {
            if (!IsNumber)
            {
                throw CreateWrongKindException(TermInfoCapabilityValueKind.Number);
            }

            return _numberValue;
        }
    }

    /// <summary>
    /// Gets the string value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This capability value is not a string.
    /// </exception>
    public string StringValue
    {
        get
        {
            if (!IsString)
            {
                throw CreateWrongKindException(TermInfoCapabilityValueKind.String);
            }

            return _stringValue!;
        }
    }

    /// <inheritdoc/>
    public bool Equals(TermInfoCapabilityValue other)
    {
        if (_kind != other._kind)
        {
            return false;
        }

        switch (_kind)
        {
            case TermInfoCapabilityValueKind.Boolean:
                return _booleanValue == other._booleanValue;
            case TermInfoCapabilityValueKind.Number:
                return _numberValue == other._numberValue;
            case TermInfoCapabilityValueKind.String:
                return string.Equals(
                    _stringValue,
                    other._stringValue,
                    StringComparison.Ordinal);
            default:
                throw new InvalidOperationException(
                    $"Unsupported terminfo capability value kind '{_kind}'.");
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is TermInfoCapabilityValue other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        switch (_kind)
        {
            case TermInfoCapabilityValueKind.Boolean:
                return HashCode.Combine(_kind, _booleanValue);
            case TermInfoCapabilityValueKind.Number:
                return HashCode.Combine(_kind, _numberValue);
            case TermInfoCapabilityValueKind.String:
                return HashCode.Combine(_kind, _stringValue);
            default:
                throw new InvalidOperationException(
                    $"Unsupported terminfo capability value kind '{_kind}'.");
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        switch (_kind)
        {
            case TermInfoCapabilityValueKind.Boolean:
                return _booleanValue.ToString();
            case TermInfoCapabilityValueKind.Number:
                return _numberValue.ToString(CultureInfo.InvariantCulture);
            case TermInfoCapabilityValueKind.String:
                return _stringValue ?? string.Empty;
            default:
                throw new InvalidOperationException(
                    $"Unsupported terminfo capability value kind '{_kind}'.");
        }
    }

    private InvalidOperationException CreateWrongKindException(
        TermInfoCapabilityValueKind expectedKind)
    {
        return new InvalidOperationException(
            $"The terminfo capability value has kind '{_kind}', not '{expectedKind}'.");
    }
}
