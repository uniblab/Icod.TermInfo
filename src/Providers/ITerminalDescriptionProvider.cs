using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Supplies terminal descriptions by canonical name or alias.
/// </summary>
/// <remarks>
/// Providers are caller-owned acquisition components. Implementations may use
/// memory, explicit directory trees, system discovery, or composed databases,
/// but must preserve the clean-miss versus failure boundary of
/// <see cref="TryLoad"/>.
/// </remarks>
public interface ITerminalDescriptionProvider
{
    /// <summary>
    /// Attempts to load a terminal description by canonical name or alias.
    /// </summary>
    /// <remarks>
    /// Returning <see langword="false"/> means a clean provider miss and
    /// requires a null result. Returning <see langword="true"/> requires a
    /// non-null immutable description. A provider must not convert permission,
    /// I/O, malformed-data, unsupported-format, or internal parsing failures
    /// into a clean miss.
    /// </remarks>
    bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal);
}
