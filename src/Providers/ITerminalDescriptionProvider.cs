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
    bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal);
}
