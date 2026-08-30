using System.Collections.ObjectModel;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Represents one unresolved termcap terminal description.
/// </summary>
/// <remarks>
/// TC01 deliberately preserves the complete header-name list without assigning
/// canonical-name, alias, or prose-description semantics. Those interpretations
/// belong to the later termcap semantic-model tranche.
/// </remarks>
public sealed class TermcapSourceEntry
{
	internal TermcapSourceEntry(
		IEnumerable<string> names,
		IEnumerable<TermcapSourceField> fields,
		TermcapSourceSpan span
	) {
		ArgumentNullException.ThrowIfNull( names );
		ArgumentNullException.ThrowIfNull( fields );
		ArgumentNullException.ThrowIfNull( span );

		Names =
			new ReadOnlyCollection<string>(
				names.ToArray()
			);
		Fields =
			new ReadOnlyCollection<TermcapSourceField>(
				fields.ToArray()
			);
		Span = span;
	}

	/// <summary>
	/// Gets the ordered header components separated by <c>|</c> in the source.
	/// </summary>
	public IReadOnlyList<string> Names { get; }

	/// <summary>
	/// Gets the capability fields in source order.
	/// </summary>
	public IReadOnlyList<TermcapSourceField> Fields { get; }

	/// <summary>
	/// Gets the source span occupied by this terminal description.
	/// </summary>
	public TermcapSourceSpan Span { get; }
}
