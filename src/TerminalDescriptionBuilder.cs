namespace Icod.TermInfo;

/// <summary>
/// Builds immutable <see cref="TerminalDescription"/> instances.
/// </summary>
public sealed class TerminalDescriptionBuilder
{
    private readonly string _name;
    private string? _description;
    private readonly List<string> _aliases = [];
    private readonly HashSet<string> _aliasSet = new(StringComparer.Ordinal);
    private readonly HashSet<BooleanCapability> _booleanCapabilities = [];
    private readonly Dictionary<NumericCapability, int> _numericCapabilities = [];
    private readonly Dictionary<StringCapability, string> _stringCapabilities = [];
    private readonly Dictionary<string, TermInfoCapabilityValue> _extendedCapabilities =
        new(StringComparer.Ordinal);
    private readonly HashSet<BooleanCapability> _canceledBooleanCapabilities = [];
    private readonly HashSet<NumericCapability> _canceledNumericCapabilities = [];
    private readonly HashSet<StringCapability> _canceledStringCapabilities = [];
    private readonly HashSet<string> _canceledExtendedCapabilities =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a builder for the specified canonical terminal name.
    /// </summary>
    public TerminalDescriptionBuilder(string name)
    {
        ValidateTerminalName(name, nameof(name));
        _name = name;
    }

    /// <summary>
    /// Gets the canonical terminal name being built.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Sets or clears the terminal's verbose descriptive name.
    /// </summary>
    public TerminalDescriptionBuilder SetDescription(string? description)
    {
        if (description is not null && string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "The terminal description cannot be empty or whitespace.",
                nameof(description));
        }

        _description = description;
        return this;
    }

    /// <summary>
    /// Adds an alternate terminal name.
    /// </summary>
    public TerminalDescriptionBuilder AddAlias(string alias)
    {
        ValidateTerminalName(alias, nameof(alias));

        if (string.Equals(alias, _name, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A terminal alias cannot equal the canonical terminal name.",
                nameof(alias));
        }

        if (!_aliasSet.Add(alias))
        {
            throw new ArgumentException(
                $"Terminal alias '{alias}' has already been added.",
                nameof(alias));
        }

        _aliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Sets whether the terminal advertises a boolean capability.
    /// </summary>
    public TerminalDescriptionBuilder SetBoolean(
        BooleanCapability capability,
        bool value = true)
    {
        Validate(capability);

        if (value)
        {
            _canceledBooleanCapabilities.Remove(capability);
            _booleanCapabilities.Add(capability);
        }
        else
        {
            _booleanCapabilities.Remove(capability);
            _canceledBooleanCapabilities.Remove(capability);
        }

        return this;
    }

    /// <summary>
    /// Sets a numeric capability.
    /// </summary>
    public TerminalDescriptionBuilder SetNumber(
        NumericCapability capability,
        int value)
    {
        Validate(capability);
        _canceledNumericCapabilities.Remove(capability);
        _numericCapabilities[capability] = value;
        return this;
    }

    /// <summary>
    /// Removes a numeric capability from the description being built.
    /// </summary>
    public TerminalDescriptionBuilder RemoveNumber(
        NumericCapability capability)
    {
        Validate(capability);
        _numericCapabilities.Remove(capability);
        _canceledNumericCapabilities.Remove(capability);
        return this;
    }

    /// <summary>
    /// Sets a string capability.
    /// </summary>
    public TerminalDescriptionBuilder SetString(
        StringCapability capability,
        string value)
    {
        Validate(capability);
        ArgumentNullException.ThrowIfNull(value);

        _canceledStringCapabilities.Remove(capability);
        _stringCapabilities[capability] = value;
        return this;
    }

    /// <summary>
    /// Removes a string capability from the description being built.
    /// </summary>
    public TerminalDescriptionBuilder RemoveString(
        StringCapability capability)
    {
        Validate(capability);
        _stringCapabilities.Remove(capability);
        _canceledStringCapabilities.Remove(capability);
        return this;
    }

    /// <summary>
    /// Sets an extended Boolean capability by its case-sensitive name. A
    /// <see langword="false"/> value removes the capability.
    /// </summary>
    public TerminalDescriptionBuilder SetExtendedBoolean(
        string name,
        bool value = true)
    {
        ValidateExtendedCapabilityName(name);

        if (value)
        {
            _canceledExtendedCapabilities.Remove(name);
            _extendedCapabilities[name] = new TermInfoCapabilityValue(true);
        }
        else
        {
            _extendedCapabilities.Remove(name);
            _canceledExtendedCapabilities.Remove(name);
        }

        return this;
    }

    /// <summary>
    /// Sets an extended numeric capability by its case-sensitive name.
    /// </summary>
    public TerminalDescriptionBuilder SetExtendedNumber(
        string name,
        int value)
    {
        ValidateExtendedCapabilityName(name);
        _canceledExtendedCapabilities.Remove(name);
        _extendedCapabilities[name] = new TermInfoCapabilityValue(value);
        return this;
    }

    /// <summary>
    /// Sets an extended string capability by its case-sensitive name.
    /// </summary>
    public TerminalDescriptionBuilder SetExtendedString(
        string name,
        string value)
    {
        ValidateExtendedCapabilityName(name);
        ArgumentNullException.ThrowIfNull(value);

        _canceledExtendedCapabilities.Remove(name);
        _extendedCapabilities[name] = new TermInfoCapabilityValue(value);
        return this;
    }

    /// <summary>
    /// Sets an extended capability by its case-sensitive name. A Boolean
    /// <see langword="false"/> value removes the capability.
    /// </summary>
    public TerminalDescriptionBuilder SetExtended(
        string name,
        TermInfoCapabilityValue value)
    {
        ValidateExtendedCapabilityName(name);

        if (value.IsBoolean && !value.BooleanValue)
        {
            _extendedCapabilities.Remove(name);
            _canceledExtendedCapabilities.Remove(name);
        }
        else
        {
            _canceledExtendedCapabilities.Remove(name);
            _extendedCapabilities[name] = value;
        }

        return this;
    }

    /// <summary>
    /// Removes an extended capability from the description being built.
    /// </summary>
    public TerminalDescriptionBuilder RemoveExtended(string name)
    {
        ValidateExtendedCapabilityName(name);
        _extendedCapabilities.Remove(name);
        _canceledExtendedCapabilities.Remove(name);
        return this;
    }

    internal TerminalDescriptionBuilder Inherit(TerminalDescription source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (BooleanCapability capability in source.BooleanCapabilities)
        {
            if (!_booleanCapabilities.Contains(capability)
                && !_canceledBooleanCapabilities.Contains(capability))
            {
                _booleanCapabilities.Add(capability);
            }
        }

        foreach (KeyValuePair<NumericCapability, int> pair in source.NumericCapabilities)
        {
            if (!_numericCapabilities.ContainsKey(pair.Key)
                && !_canceledNumericCapabilities.Contains(pair.Key))
            {
                _numericCapabilities[pair.Key] = pair.Value;
            }
        }

        foreach (KeyValuePair<StringCapability, string> pair in source.StringCapabilities)
        {
            if (!_stringCapabilities.ContainsKey(pair.Key)
                && !_canceledStringCapabilities.Contains(pair.Key))
            {
                _stringCapabilities[pair.Key] = pair.Value;
            }
        }

        foreach (KeyValuePair<string, TermInfoCapabilityValue> pair
            in source.ExtendedCapabilities)
        {
            if (!_extendedCapabilities.ContainsKey(pair.Key)
                && !_canceledExtendedCapabilities.Contains(pair.Key))
            {
                _extendedCapabilities[pair.Key] = pair.Value;
            }
        }

        return this;
    }

    internal TerminalDescriptionBuilder CancelBoolean(
        BooleanCapability capability)
    {
        Validate(capability);
        _booleanCapabilities.Remove(capability);
        _canceledBooleanCapabilities.Add(capability);
        return this;
    }

    internal TerminalDescriptionBuilder CancelNumber(
        NumericCapability capability)
    {
        Validate(capability);
        _numericCapabilities.Remove(capability);
        _canceledNumericCapabilities.Add(capability);
        return this;
    }

    internal TerminalDescriptionBuilder CancelString(
        StringCapability capability)
    {
        Validate(capability);
        _stringCapabilities.Remove(capability);
        _canceledStringCapabilities.Add(capability);
        return this;
    }

    internal TerminalDescriptionBuilder CancelExtended(string name)
    {
        ValidateExtendedCapabilityName(name);
        _extendedCapabilities.Remove(name);
        _canceledExtendedCapabilities.Add(name);
        return this;
    }

    internal bool IsBooleanCanceled(BooleanCapability capability)
    {
        Validate(capability);
        return _canceledBooleanCapabilities.Contains(capability);
    }

    internal bool IsNumberCanceled(NumericCapability capability)
    {
        Validate(capability);
        return _canceledNumericCapabilities.Contains(capability);
    }

    internal bool IsStringCanceled(StringCapability capability)
    {
        Validate(capability);
        return _canceledStringCapabilities.Contains(capability);
    }

    internal bool IsExtendedCanceled(string name)
    {
        ValidateExtendedCapabilityName(name);
        return _canceledExtendedCapabilities.Contains(name);
    }

    /// <summary>
    /// Creates an immutable snapshot of the current builder state.
    /// </summary>
    public TerminalDescription Build()
    {
        return new TerminalDescription(
            _name,
            _description,
            _aliases,
            _booleanCapabilities,
            _numericCapabilities,
            _stringCapabilities,
            _extendedCapabilities);
    }

    private static void Validate(BooleanCapability capability)
    {
        if (!Enum.IsDefined(typeof(BooleanCapability), capability))
        {
            throw new ArgumentOutOfRangeException(nameof(capability));
        }
    }

    private static void Validate(NumericCapability capability)
    {
        if (!Enum.IsDefined(typeof(NumericCapability), capability))
        {
            throw new ArgumentOutOfRangeException(nameof(capability));
        }
    }

    private static void Validate(StringCapability capability)
    {
        if (!Enum.IsDefined(typeof(StringCapability), capability))
        {
            throw new ArgumentOutOfRangeException(nameof(capability));
        }
    }

    private static void ValidateExtendedCapabilityName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The extended capability name cannot be empty or whitespace.",
                nameof(name));
        }

        if (CapabilityCatalog.IsStandardName(name))
        {
            throw new ArgumentException(
                $"'{name}' is a standard terminfo capability name and cannot be shadowed by an extended capability.",
                nameof(name));
        }
    }

    private static void ValidateTerminalName(
        string name,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(name, parameterName);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The terminal name cannot be empty or whitespace.",
                parameterName);
        }
    }
}
