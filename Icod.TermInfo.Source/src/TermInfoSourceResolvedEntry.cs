using Icod.TermInfo;

namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one source entry after all supported <c>use=</c> inheritance has
/// been resolved.
/// </summary>
/// <remarks>
/// S07 intentionally exposes capability queries rather than materializing a
/// <see cref="TerminalDescription"/>. Conversion to the stable runtime model is
/// S08 work. Source cancellation tombstones remain internal resolution state.
/// </remarks>
public sealed class TermInfoSourceResolvedEntry
{
    private readonly TermInfoSourceCapabilityState _state;

    internal TermInfoSourceResolvedEntry(
        TermInfoSourceEntry sourceEntry,
        TermInfoSourceCapabilityState state)
    {
        ArgumentNullException.ThrowIfNull(sourceEntry);
        ArgumentNullException.ThrowIfNull(state);

        SourceEntry = sourceEntry;
        _state = state.Clone();
    }

    /// <summary>
    /// Gets the unresolved source entry whose local fields head this resolved
    /// result.
    /// </summary>
    public TermInfoSourceEntry SourceEntry { get; }

    /// <summary>
    /// Gets whether the resolved entry advertises a standard Boolean capability.
    /// </summary>
    public bool GetBoolean(
        BooleanCapability capability)
    {
        Validate(capability);
        return _state.BooleanCapabilities.Contains(capability);
    }

    /// <summary>
    /// Gets a resolved standard numeric capability, or <see langword="null"/>
    /// when it is absent or canceled.
    /// </summary>
    public int? GetNumber(
        NumericCapability capability)
    {
        Validate(capability);

        if (_state.NumericCapabilities.TryGetValue(
                capability,
                out int value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets a resolved standard string capability, or <see langword="null"/>
    /// when it is absent or canceled.
    /// </summary>
    public string? GetString(
        StringCapability capability)
    {
        Validate(capability);

        if (_state.StringCapabilities.TryGetValue(
                capability,
                out string? value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Attempts to get one resolved case-sensitive extended capability.
    /// </summary>
    public bool TryGetExtended(
        string name,
        out TermInfoCapabilityValue value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _state.ExtendedCapabilities.TryGetValue(
            name,
            out value);
    }

    internal TermInfoSourceCapabilityState CloneCapabilityState()
    {
        return _state.Clone();
    }

    private static void Validate(
        BooleanCapability capability)
    {
        if (!Enum.IsDefined(
                typeof(BooleanCapability),
                capability))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capability),
                capability,
                "The Boolean capability is not defined.");
        }
    }

    private static void Validate(
        NumericCapability capability)
    {
        if (!Enum.IsDefined(
                typeof(NumericCapability),
                capability))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capability),
                capability,
                "The numeric capability is not defined.");
        }
    }

    private static void Validate(
        StringCapability capability)
    {
        if (!Enum.IsDefined(
                typeof(StringCapability),
                capability))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capability),
                capability,
                "The string capability is not defined.");
        }
    }
}
