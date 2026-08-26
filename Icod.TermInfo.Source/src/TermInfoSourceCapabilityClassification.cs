namespace Icod.TermInfo.Source;

/// <summary>
/// Classifies a capability name found in terminfo source.
/// </summary>
public enum TermInfoSourceCapabilityClassification
{
    /// <summary>
    /// The name maps to one capability in the standard terminfo catalog.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// The name is a recognized non-standard capability.
    /// </summary>
    KnownExtended = 1,

    /// <summary>
    /// The name is syntactically valid but is not currently recognized.
    /// </summary>
    UnknownExtended = 2,

    /// <summary>
    /// The name is invalid or reserved in terminfo source.
    /// </summary>
    Invalid = 3,
}
