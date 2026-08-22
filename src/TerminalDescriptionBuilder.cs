namespace Icod.TermInfo;

/// <summary>
/// Builds immutable <see cref="TerminalDescription"/> instances.
/// </summary>
public sealed class TerminalDescriptionBuilder
{
    private readonly string _name;
    private readonly List<string> _aliases = [];
    private readonly HashSet<string> _aliasSet = new(StringComparer.Ordinal);
    private readonly HashSet<BooleanCapability> _booleanCapabilities = [];
    private readonly Dictionary<NumericCapability, int> _numericCapabilities = [];
    private readonly Dictionary<StringCapability, string> _stringCapabilities = [];

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
            _booleanCapabilities.Add(capability);
        }
        else
        {
            _booleanCapabilities.Remove(capability);
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
        return this;
    }

    /// <summary>
    /// Creates an immutable snapshot of the current builder state.
    /// </summary>
    public TerminalDescription Build()
    {
        return new TerminalDescription(
            _name,
            _aliases,
            _booleanCapabilities,
            _numericCapabilities,
            _stringCapabilities);
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
