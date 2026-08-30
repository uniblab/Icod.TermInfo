using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Represents one termcap source entry after <c>tc=</c> inheritance and
/// cancellation have been resolved.
/// </summary>
/// <remarks>
/// The effective fields remain termcap source fields. TC03 does not convert
/// them into Runtime capability identities or a <c>TerminalDescription</c>.
/// </remarks>
public sealed class TermcapSourceResolvedEntry
{
	private readonly IReadOnlyDictionary<string, TermcapSourceResolvedField> _byCapabilityName;

	internal TermcapSourceResolvedEntry(
		TermcapSourceEntry sourceEntry,
		IEnumerable<TermcapSourceResolvedField> fields
	) {
		ArgumentNullException.ThrowIfNull( sourceEntry );
		ArgumentNullException.ThrowIfNull( fields );

		TermcapSourceResolvedField[] fieldArray =
			fields.ToArray();
		Dictionary<string, TermcapSourceResolvedField> byCapabilityName =
			new( StringComparer.Ordinal );
		foreach ( TermcapSourceResolvedField field in fieldArray ) {
			if ( !byCapabilityName.TryAdd( field.CapabilityName, field ) ) {
				throw new ArgumentException(
					$"The resolved field set contains duplicate capability '{field.CapabilityName}'.",
					nameof( fields )
				);
			}
		}

		SourceEntry = sourceEntry;
		Fields =
			new ReadOnlyCollection<TermcapSourceResolvedField>(
				fieldArray
			);
		_byCapabilityName =
			new ReadOnlyDictionary<string, TermcapSourceResolvedField>(
				byCapabilityName
			);
	}

	/// <summary>
	/// Gets the unresolved source entry whose local fields head this resolved
	/// result.
	/// </summary>
	public TermcapSourceEntry SourceEntry { get; }

	/// <summary>
	/// Gets effective non-canceled capability fields in deterministic priority
	/// order: local fields first, followed by inherited fields.
	/// </summary>
	public IReadOnlyList<TermcapSourceResolvedField> Fields { get; }

	/// <summary>
	/// Attempts to get the effective field for an exact two-character termcap
	/// capability name.
	/// </summary>
	public bool TryGetField(
		string capabilityName,
		[NotNullWhen( true )] out TermcapSourceResolvedField? field
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );
		if ( capabilityName.Length != 2 ) {
			throw new ArgumentException(
				"A termcap capability name must contain exactly two characters.",
				nameof( capabilityName )
			);
		}

		return _byCapabilityName.TryGetValue(
			capabilityName,
			out field
		);
	}
}
