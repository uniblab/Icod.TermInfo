namespace Icod.TermInfo;

/// <summary>
/// Describes one standard terminfo capability and its fixed compiled-table
/// position.
/// </summary>
/// <typeparam name="TCapability">
/// The managed standard-capability enum type.
/// </typeparam>
public sealed class StandardCapabilityMetadata<TCapability>
    where TCapability : struct, Enum
{
    internal StandardCapabilityMetadata(
        TCapability capability,
        int binaryIndex,
        string shortName,
        string longName,
        string termcapCode,
        TermInfoCapabilityValueKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(binaryIndex);
        ArgumentNullException.ThrowIfNull(shortName);
        ArgumentNullException.ThrowIfNull(longName);
        ArgumentNullException.ThrowIfNull(termcapCode);

        Capability = capability;
        BinaryIndex = binaryIndex;
        ShortName = shortName;
        LongName = longName;
        TermcapCode = termcapCode;
        Kind = kind;
    }

    /// <summary>
    /// Gets the capability value kind.
    /// </summary>
    public TermInfoCapabilityValueKind Kind { get; }

    /// <summary>
    /// Gets the managed capability identifier.
    /// </summary>
    public TCapability Capability { get; }

    /// <summary>
    /// Gets the capability's zero-based position in the conventional compiled
    /// table for its value kind.
    /// </summary>
    public int BinaryIndex { get; }

    /// <summary>
    /// Gets the traditional terminfo short name.
    /// </summary>
    public string ShortName { get; }

    /// <summary>
    /// Gets the long/variable terminfo name.
    /// </summary>
    public string LongName { get; }

    /// <summary>
    /// Gets the corresponding termcap code used by the selected ncurses
    /// compatibility baseline.
    /// </summary>
    public string TermcapCode { get; }
}
