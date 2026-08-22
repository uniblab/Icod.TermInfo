using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Describes the capabilities of one terminal profile.
/// </summary>
public sealed class TerminalDescription
{
    private readonly HashSet<BooleanCapability> _booleanCapabilities;
    private readonly IReadOnlyDictionary<NumericCapability, int> _numericCapabilities;
    private readonly IReadOnlyDictionary<StringCapability, string> _stringCapabilities;
    private readonly IReadOnlyDictionary<string, TermInfoCapabilityValue> _extendedCapabilities;

    internal TerminalDescription(
        string name,
        IEnumerable<string> aliases,
        IEnumerable<BooleanCapability> booleanCapabilities,
        IDictionary<NumericCapability, int> numericCapabilities,
        IDictionary<StringCapability, string> stringCapabilities,
        IDictionary<string, TermInfoCapabilityValue> extendedCapabilities)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(aliases);
        ArgumentNullException.ThrowIfNull(booleanCapabilities);
        ArgumentNullException.ThrowIfNull(numericCapabilities);
        ArgumentNullException.ThrowIfNull(stringCapabilities);
        ArgumentNullException.ThrowIfNull(extendedCapabilities);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The terminal name cannot be empty or whitespace.",
                nameof(name));
        }

        Name = name;

        string[] aliasArray = aliases.ToArray();
        for (int i = 0; i < aliasArray.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(aliasArray[i]))
            {
                throw new ArgumentException(
                    "Terminal aliases cannot be null, empty, or whitespace.",
                    nameof(aliases));
            }
        }

        foreach (string extendedName in extendedCapabilities.Keys)
        {
            ValidateExtendedCapabilityName(
                extendedName,
                nameof(extendedCapabilities));
        }

        Aliases = Array.AsReadOnly(aliasArray);
        _booleanCapabilities = new HashSet<BooleanCapability>(booleanCapabilities);
        _numericCapabilities =
            new ReadOnlyDictionary<NumericCapability, int>(
                new Dictionary<NumericCapability, int>(numericCapabilities));
        _stringCapabilities =
            new ReadOnlyDictionary<StringCapability, string>(
                new Dictionary<StringCapability, string>(stringCapabilities));
        _extendedCapabilities =
            new ReadOnlyDictionary<string, TermInfoCapabilityValue>(
                new Dictionary<string, TermInfoCapabilityValue>(
                    extendedCapabilities,
                    StringComparer.Ordinal));
    }

    /// <summary>
    /// Gets the canonical terminal name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the aliases accepted for the terminal profile.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Gets the immutable, case-sensitive extended capabilities advertised by
    /// this terminal description.
    /// </summary>
    public IReadOnlyDictionary<string, TermInfoCapabilityValue> ExtendedCapabilities =>
        _extendedCapabilities;

    /// <summary>
    /// Gets whether the profile advertises the specified boolean capability.
    /// </summary>
    public bool GetBoolean(BooleanCapability capability)
    {
        Validate(capability);

        return _booleanCapabilities.Contains(capability);
    }

    /// <summary>
    /// Gets a numeric capability, or <see langword="null"/> when the known
    /// capability is not present in this profile.
    /// </summary>
    public int? GetNumber(NumericCapability capability)
    {
        Validate(capability);

        if (_numericCapabilities.TryGetValue(capability, out int value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets a string capability, or <see langword="null"/> when the known
    /// capability is not present in this profile.
    /// </summary>
    public string? GetString(StringCapability capability)
    {
        Validate(capability);

        if (_stringCapabilities.TryGetValue(capability, out string? value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets a required string capability.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The terminal profile does not advertise the capability.
    /// </exception>
    public string GetRequiredString(StringCapability capability)
    {
        Validate(capability);

        return GetString(capability)
            ?? throw new InvalidOperationException(
                $"Terminal '{Name}' does not provide capability '{capability}'.");
    }

    /// <summary>
    /// Expands a parameterized string capability using isolated variable storage.
    /// </summary>
    public string Expand(
        StringCapability capability,
        params TermInfoParameter[] parameters)
    {
        Validate(capability);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterExpander.Expand(
            GetRequiredString(capability),
            parameters);
    }

    /// <summary>
    /// Expands a parameterized string capability using the supplied context for
    /// persistent uppercase variables.
    /// </summary>
    public string Expand(
        StringCapability capability,
        TermInfoExpansionContext context,
        params TermInfoParameter[] parameters)
    {
        Validate(capability);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(parameters);

        return TermInfoParameterExpander.Expand(
            GetRequiredString(capability),
            context,
            parameters);
    }

    /// <summary>
    /// Looks up a boolean capability by its traditional terminfo short name.
    /// </summary>
    /// <remarks>
    /// A return value of <see langword="false"/> means the capability name is
    /// known but the capability is absent from this profile. Unknown names are
    /// rejected with <see cref="ArgumentException"/>.
    /// </remarks>
    public bool TryGetBoolean(
        string name,
        out bool value)
    {
        ValidateCapabilityName(name);

        if (!CapabilityCatalog.TryGetBoolean(name, out BooleanCapability capability))
        {
            throw CreateUnknownCapabilityException("boolean", name);
        }

        value = GetBoolean(capability);
        return value;
    }

    /// <summary>
    /// Looks up a numeric capability by its traditional terminfo short name.
    /// </summary>
    public bool TryGetNumber(
        string name,
        out int value)
    {
        ValidateCapabilityName(name);

        if (!CapabilityCatalog.TryGetNumeric(name, out NumericCapability capability))
        {
            throw CreateUnknownCapabilityException("numeric", name);
        }

        int? result = GetNumber(capability);
        if (result is null)
        {
            value = default;
            return false;
        }

        value = result.Value;
        return true;
    }

    /// <summary>
    /// Looks up a string capability by its traditional terminfo short name.
    /// </summary>
    public bool TryGetString(
        string name,
        [NotNullWhen(true)] out string? value)
    {
        ValidateCapabilityName(name);

        if (!CapabilityCatalog.TryGetString(name, out StringCapability capability))
        {
            throw CreateUnknownCapabilityException("string", name);
        }

        value = GetString(capability);
        return value is not null;
    }

    /// <summary>
    /// Looks up an extended capability by its case-sensitive name.
    /// </summary>
    public bool TryGetExtendedCapability(
        string name,
        out TermInfoCapabilityValue value)
    {
        ValidateCapabilityName(name);
        return _extendedCapabilities.TryGetValue(name, out value);
    }

    /// <summary>
    /// Looks up an extended Boolean capability by its case-sensitive name.
    /// </summary>
    /// <remarks>
    /// A missing name returns <see langword="false"/>. If the name exists with a
    /// different value kind, this method throws <see cref="InvalidOperationException"/>.
    /// </remarks>
    public bool TryGetExtendedBoolean(
        string name,
        out bool value)
    {
        ValidateCapabilityName(name);

        if (!_extendedCapabilities.TryGetValue(
                name,
                out TermInfoCapabilityValue capability))
        {
            value = default;
            return false;
        }

        value = capability.BooleanValue;
        return true;
    }

    /// <summary>
    /// Looks up an extended numeric capability by its case-sensitive name.
    /// </summary>
    /// <remarks>
    /// A missing name returns <see langword="false"/>. If the name exists with a
    /// different value kind, this method throws <see cref="InvalidOperationException"/>.
    /// </remarks>
    public bool TryGetExtendedNumber(
        string name,
        out int value)
    {
        ValidateCapabilityName(name);

        if (!_extendedCapabilities.TryGetValue(
                name,
                out TermInfoCapabilityValue capability))
        {
            value = default;
            return false;
        }

        value = capability.NumberValue;
        return true;
    }

    /// <summary>
    /// Looks up an extended string capability by its case-sensitive name.
    /// </summary>
    /// <remarks>
    /// A missing name returns <see langword="false"/>. If the name exists with a
    /// different value kind, this method throws <see cref="InvalidOperationException"/>.
    /// </remarks>
    public bool TryGetExtendedString(
        string name,
        [NotNullWhen(true)] out string? value)
    {
        ValidateCapabilityName(name);

        if (!_extendedCapabilities.TryGetValue(
                name,
                out TermInfoCapabilityValue capability))
        {
            value = null;
            return false;
        }

        value = capability.StringValue;
        return true;
    }

    private static ArgumentException CreateUnknownCapabilityException(
        string kind,
        string name)
    {
        return new ArgumentException(
            $"Unknown {kind} terminfo capability '{name}'.",
            nameof(name));
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

    private static void ValidateCapabilityName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The capability name cannot be empty or whitespace.",
                nameof(name));
        }
    }

    private static void ValidateExtendedCapabilityName(
        string name,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(name, parameterName);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Extended capability names cannot be empty or whitespace.",
                parameterName);
        }

        if (CapabilityCatalog.IsStandardName(name))
        {
            throw new ArgumentException(
                $"'{name}' is a standard terminfo capability name and cannot be shadowed by an extended capability.",
                parameterName);
        }
    }
}
