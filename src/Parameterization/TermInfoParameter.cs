namespace Icod.TermInfo;

/// <summary>
/// Represents one integer or string parameter supplied to a terminfo parameter program.
/// </summary>
public readonly struct TermInfoParameter : IEquatable<TermInfoParameter>
{
    private readonly TermInfoParameterKind _kind;
    private readonly long _integerValue;
    private readonly string? _stringValue;

    /// <summary>
    /// Initializes an integer parameter.
    /// </summary>
    public TermInfoParameter(long value)
    {
        _kind = TermInfoParameterKind.Integer;
        _integerValue = value;
        _stringValue = null;
    }

    /// <summary>
    /// Initializes a string parameter.
    /// </summary>
    public TermInfoParameter(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _kind = TermInfoParameterKind.String;
        _integerValue = default;
        _stringValue = value;
    }

    /// <summary>
    /// Gets whether this parameter contains an integer.
    /// </summary>
    public bool IsInteger => _kind == TermInfoParameterKind.Integer;

    /// <summary>
    /// Gets whether this parameter contains a string.
    /// </summary>
    public bool IsString => _kind == TermInfoParameterKind.String;

    /// <summary>
    /// Gets the integer value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This parameter contains a string.
    /// </exception>
    public long IntegerValue
    {
        get
        {
            if (!IsInteger)
            {
                throw new InvalidOperationException(
                    "The terminfo parameter contains a string, not an integer.");
            }

            return _integerValue;
        }
    }

    /// <summary>
    /// Gets the string value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This parameter contains an integer.
    /// </exception>
    public string StringValue
    {
        get
        {
            if (!IsString)
            {
                throw new InvalidOperationException(
                    "The terminfo parameter contains an integer, not a string.");
            }

            return _stringValue!;
        }
    }

    /// <summary>
    /// Converts an <see cref="int"/> to a terminfo parameter.
    /// </summary>
    public static implicit operator TermInfoParameter(int value)
    {
        return new TermInfoParameter(value);
    }

    /// <summary>
    /// Converts a <see cref="long"/> to a terminfo parameter.
    /// </summary>
    public static implicit operator TermInfoParameter(long value)
    {
        return new TermInfoParameter(value);
    }

    /// <summary>
    /// Converts a string to a terminfo parameter.
    /// </summary>
    public static implicit operator TermInfoParameter(string value)
    {
        return new TermInfoParameter(value);
    }

    /// <inheritdoc/>
    public bool Equals(TermInfoParameter other)
    {
        if (_kind != other._kind)
        {
            return false;
        }

        return (_kind == TermInfoParameterKind.Integer)
            ? _integerValue == other._integerValue
            : string.Equals(_stringValue, other._stringValue, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is TermInfoParameter other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return (_kind == TermInfoParameterKind.Integer)
            ? HashCode.Combine(_kind, _integerValue)
            : HashCode.Combine(_kind, _stringValue);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return (_kind == TermInfoParameterKind.Integer)
            ? _integerValue.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : _stringValue ?? string.Empty;
    }

    internal TermInfoParameterKind Kind => _kind;
}

internal enum TermInfoParameterKind
{
    Integer = 0,
    String = 1,
}
