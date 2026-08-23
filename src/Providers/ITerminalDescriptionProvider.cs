using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Supplies terminal descriptions by canonical name or alias.
/// </summary>
public interface ITerminalDescriptionProvider
{
    /// <summary>
    /// Attempts to load a terminal description by canonical name or alias.
    /// </summary>
    /// <remarks>
    /// Returning <see langword="false"/> means a clean provider miss. A provider
    /// must not convert permission, I/O, malformed-data, unsupported-format, or
    /// internal parsing failures into a clean miss.
    /// </remarks>
    bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal);
}
