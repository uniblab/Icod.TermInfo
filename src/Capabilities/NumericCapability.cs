namespace Icod.TermInfo;

/// <summary>
/// Identifies a numeric terminal capability.
/// </summary>
public enum NumericCapability
{
    /// <summary>
    /// The terminal profile's default column count (<c>cols</c>).
    /// </summary>
    Columns,

    /// <summary>
    /// The terminal profile's default line count (<c>lines</c>).
    /// </summary>
    Lines,

    /// <summary>
    /// The number of supported colors (<c>colors</c>).
    /// </summary>
    Colors,

    /// <summary>
    /// The number of supported color pairs (<c>pairs</c>).
    /// </summary>
    ColorPairs,
}
