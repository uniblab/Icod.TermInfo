using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Source;

/// <summary>
/// Supplies unresolved terminfo source entries by canonical name or alias.
/// </summary>
/// <remarks>
/// <para>
/// Providers are caller-owned acquisition components for source inheritance.
/// They may draw entries from one or more parsed documents, files, generated
/// sources, or other stores.
/// </para>
/// <para>
/// Returning <see langword="false"/> means a clean lookup miss and requires a
/// null result. Returning <see langword="true"/> requires a non-null entry.
/// Provider failures must be reported by throwing rather than being converted
/// into clean misses.
/// </para>
/// </remarks>
public interface ITermInfoSourceEntryProvider
{
    /// <summary>
    /// Attempts to load an unresolved source entry by canonical name or alias.
    /// </summary>
    bool TryLoad(
        string name,
        [NotNullWhen(true)] out TermInfoSourceEntry? entry);
}
