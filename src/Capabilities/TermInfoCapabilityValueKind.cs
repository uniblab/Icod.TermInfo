namespace Icod.TermInfo;

/// <summary>
/// Identifies the value kind of a terminfo capability.
/// </summary>
public enum TermInfoCapabilityValueKind
{
    /// <summary>
    /// The capability carries a Boolean value.
    /// </summary>
    Boolean,

    /// <summary>
    /// The capability carries a numeric value.
    /// </summary>
    Number,

    /// <summary>
    /// The capability carries a string value.
    /// </summary>
    String,
}
