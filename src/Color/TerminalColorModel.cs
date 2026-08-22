namespace Icod.TermInfo;

/// <summary>
/// Identifies the semantic color-selection model advertised by a terminal.
/// </summary>
public enum TerminalColorModel
{
    /// <summary>
    /// The terminal does not advertise a usable color-selection model.
    /// </summary>
    None,

    /// <summary>
    /// The terminal selects colors by palette index.
    /// </summary>
    Indexed,

    /// <summary>
    /// The terminal selects colors by a packed direct RGB value.
    /// </summary>
    DirectRgb,
}
