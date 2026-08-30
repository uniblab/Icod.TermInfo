using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Supplies unresolved termcap source entries by terminal name.
/// </summary>
/// <remarks>
/// <para>
/// Providers are caller-owned acquisition components used only for explicit
/// <c>tc=</c> inheritance resolution. They may draw entries from parsed
/// documents, files, generated sources, or other caller-controlled stores.
/// </para>
/// <para>
/// Returning <see langword="false"/> means a clean lookup miss and requires a
/// null result. Returning <see langword="true"/> requires a non-null entry.
/// Provider failures must be reported by throwing rather than being converted
/// into clean misses.
/// </para>
/// </remarks>
public interface ITermcapSourceEntryProvider
{
	/// <summary>
	/// Attempts to load an unresolved termcap source entry by terminal name.
	/// </summary>
	bool TryLoad(
		string name,
		[NotNullWhen( true )] out TermcapSourceEntry? entry
	);
}
