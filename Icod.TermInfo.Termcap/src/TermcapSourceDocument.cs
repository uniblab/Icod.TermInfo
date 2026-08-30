using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Represents a parsed collection of unresolved termcap terminal descriptions.
/// </summary>
public sealed class TermcapSourceDocument
{
	internal TermcapSourceDocument(
		IEnumerable<TermcapSourceEntry> entries
	) {
		ArgumentNullException.ThrowIfNull( entries );

		Entries =
			new ReadOnlyCollection<TermcapSourceEntry>(
				entries.ToArray()
			);
	}

	/// <summary>
	/// Gets parsed terminal descriptions in source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceEntry> Entries { get; }
}
