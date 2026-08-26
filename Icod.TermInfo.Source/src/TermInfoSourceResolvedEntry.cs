using Icod.TermInfo;

namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one source entry after all supported <c>use=</c> inheritance has
/// been resolved.
/// </summary>
/// <remarks>
/// S08 can project this resolved source state into the stable
/// <see cref="TerminalDescription"/> runtime model. Source cancellation
/// tombstones remain internal resolution state and materialize as absence.
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
    /// Materializes this resolved source entry into the stable runtime terminal
    /// description model.
    /// </summary>
    /// <remarks>
    /// Only effective terminal identity and capability values are projected.
    /// Source-only inheritance declarations, cancellation tombstones, comments,
    /// tokens, and source locations are not represented by the returned value.
    /// </remarks>
    public TerminalDescription ToTerminalDescription() {
        TerminalDescriptionBuilder builder =
            new( SourceEntry.CanonicalName );

        if ( SourceEntry.Description is string description ) {
            builder.SetDescription( description );
        }

        foreach ( string alias in SourceEntry.Aliases ) {
            builder.AddAlias( alias );
        }

        foreach ( BooleanCapability capability in _state.BooleanCapabilities ) {
            builder.SetBoolean( capability );
        }

        foreach (
            KeyValuePair<NumericCapability, int> pair
            in _state.NumericCapabilities
        ) {
            builder.SetNumber(
                pair.Key,
                pair.Value
            );
        }

        foreach (
            KeyValuePair<StringCapability, string> pair
            in _state.StringCapabilities
        ) {
            builder.SetString(
                pair.Key,
                pair.Value
            );
        }

        foreach (
            KeyValuePair<string, TermInfoCapabilityValue> pair
            in _state.ExtendedCapabilities
        ) {
            builder.SetExtended(
                pair.Key,
                pair.Value
            );
        }

        return builder.Build();
    }

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
