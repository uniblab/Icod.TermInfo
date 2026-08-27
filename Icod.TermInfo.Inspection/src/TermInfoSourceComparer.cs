using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Compares unresolved terminfo source entries and documents without flattening
/// inheritance into effective terminal descriptions.
/// </summary>
/// <remarks>
/// <para>
/// Source comparison observes ordered local fields, duplicate declarations,
/// cancellation, disabled fields, <c>use=</c> references, and source identity
/// metadata. Fields are compared positionally so declaration order remains
/// observable.
/// </para>
/// <para>
/// Comments, incidental whitespace, source spans by themselves, and equivalent
/// lexical spellings of successfully decoded numeric/string values are not
/// semantic differences. Retained spans are attached to actual structured
/// differences for diagnostics.
/// </para>
/// </remarks>
public static class TermInfoSourceComparer {
	/// <summary>
	/// Compares two unresolved source entries.
	/// </summary>
	/// <param name="left">The left unresolved entry.</param>
	/// <param name="right">The right unresolved entry.</param>
	/// <returns>A deterministic structured source-aware comparison result.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="left"/> or <paramref name="right"/> is
	/// <see langword="null"/>.
	/// </exception>
	public static TermInfoComparisonResult Compare(
		TermInfoSourceEntry left,
		TermInfoSourceEntry right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		List<TermInfoDifference> differences = [];
		CompareEntry(
			differences,
			left,
			right,
			null,
			null
		);

		return new TermInfoComparisonResult(
			differences
		);
	}

	/// <summary>
	/// Compares two unresolved source documents in entry order.
	/// </summary>
	/// <param name="left">The left unresolved document.</param>
	/// <param name="right">The right unresolved document.</param>
	/// <returns>A deterministic structured source-aware comparison result.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="left"/> or <paramref name="right"/> is
	/// <see langword="null"/>.
	/// </exception>
	public static TermInfoComparisonResult Compare(
		TermInfoSourceDocument left,
		TermInfoSourceDocument right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		List<TermInfoDifference> differences = [];
		int entryCount =
			Math.Max(
				left.Entries.Count,
				right.Entries.Count
			);

		for ( int index = 0; index < entryCount; index++ ) {
			bool leftPresent =
				index < left.Entries.Count;
			bool rightPresent =
				index < right.Entries.Count;

			if ( leftPresent && !rightPresent ) {
				differences.Add(
					CreateSourceDifference(
						TermInfoDifferenceKind.SourceEntryOnlyInLeft,
						left.Entries[ index ],
						null,
						index,
						null
					)
				);
				continue;
			}

			if ( !leftPresent && rightPresent ) {
				differences.Add(
					CreateSourceDifference(
						TermInfoDifferenceKind.SourceEntryOnlyInRight,
						null,
						right.Entries[ index ],
						null,
						index
					)
				);
				continue;
			}

			CompareEntry(
				differences,
				left.Entries[ index ],
				right.Entries[ index ],
				index,
				index
			);
		}

		return new TermInfoComparisonResult(
			differences
		);
	}

	private static void CompareEntry(
		ICollection<TermInfoDifference> differences,
		TermInfoSourceEntry left,
		TermInfoSourceEntry right,
		int? leftEntryIndex,
		int? rightEntryIndex
	) {
		CompareIdentity(
			differences,
			left,
			right,
			leftEntryIndex,
			rightEntryIndex
		);
		CompareFields(
			differences,
			left,
			right,
			leftEntryIndex,
			rightEntryIndex
		);
	}

	private static void CompareIdentity(
		ICollection<TermInfoDifference> differences,
		TermInfoSourceEntry left,
		TermInfoSourceEntry right,
		int? leftEntryIndex,
		int? rightEntryIndex
	) {
		if ( !string.Equals(
			left.CanonicalName,
			right.CanonicalName,
			StringComparison.Ordinal
		) ) {
			differences.Add(
				CreateSourceIdentityTextDifference(
					TermInfoDifferenceKind.IdentityName,
					left,
					right,
					leftEntryIndex,
					rightEntryIndex,
					left.CanonicalName,
					right.CanonicalName
				)
			);
		}

		if ( !left.Aliases.SequenceEqual(
			right.Aliases,
			StringComparer.Ordinal
		) ) {
			differences.Add(
				new TermInfoDifference(
					TermInfoDifferenceKind.IdentityAliases,
					null,
					null,
					null,
					null,
					left.Aliases,
					right.Aliases,
					null,
					null,
					left,
					right,
					leftEntryIndex,
					rightEntryIndex
				)
			);
		}

		if ( !string.Equals(
			left.Description,
			right.Description,
			StringComparison.Ordinal
		) ) {
			differences.Add(
				CreateSourceIdentityTextDifference(
					TermInfoDifferenceKind.IdentityDescription,
					left,
					right,
					leftEntryIndex,
					rightEntryIndex,
					left.Description,
					right.Description
				)
			);
		}
	}

	private static void CompareFields(
		ICollection<TermInfoDifference> differences,
		TermInfoSourceEntry left,
		TermInfoSourceEntry right,
		int? leftEntryIndex,
		int? rightEntryIndex
	) {
		int fieldCount =
			Math.Max(
				left.Fields.Count,
				right.Fields.Count
			);

		for ( int index = 0; index < fieldCount; index++ ) {
			bool leftPresent =
				index < left.Fields.Count;
			bool rightPresent =
				index < right.Fields.Count;

			if ( leftPresent && !rightPresent ) {
				differences.Add(
					CreateSourceDifference(
						TermInfoDifferenceKind.SourceFieldOnlyInLeft,
						left,
						right,
						leftEntryIndex,
						rightEntryIndex,
						left.Fields[ index ],
						null,
						index,
						null
					)
				);
				continue;
			}

			if ( !leftPresent && rightPresent ) {
				differences.Add(
					CreateSourceDifference(
						TermInfoDifferenceKind.SourceFieldOnlyInRight,
						left,
						right,
						leftEntryIndex,
						rightEntryIndex,
						null,
						right.Fields[ index ],
						null,
						index
					)
				);
				continue;
			}

			TermInfoSourceField leftField =
				left.Fields[ index ];
			TermInfoSourceField rightField =
				right.Fields[ index ];
			TermInfoDifferenceKind? kind =
				ClassifyFieldDifference(
					leftField,
					rightField
				);

			if ( kind is not null ) {
				differences.Add(
					CreateSourceDifference(
						kind.Value,
						left,
						right,
						leftEntryIndex,
						rightEntryIndex,
						leftField,
						rightField,
						index,
						index
					)
				);
			}
		}
	}

	private static TermInfoDifferenceKind? ClassifyFieldDifference(
		TermInfoSourceField left,
		TermInfoSourceField right
	) {
		if ( left.Kind != right.Kind ) {
			return TermInfoDifferenceKind.SourceFieldKind;
		}

		if ( left.Kind == TermInfoSourceFieldKind.UseReference ) {
			return string.Equals(
				left.ReferenceName,
				right.ReferenceName,
				StringComparison.Ordinal
			)
				? null
				: TermInfoDifferenceKind.SourceUseReference;
		}

		if ( !string.Equals(
			GetSemanticCapabilityName( left ),
			GetSemanticCapabilityName( right ),
			StringComparison.Ordinal
		) ) {
			return TermInfoDifferenceKind.SourceFieldCapability;
		}

		return left.Kind switch {
			TermInfoSourceFieldKind.NumericCapability =>
				NumericValuesEqual( left, right )
					? null
					: TermInfoDifferenceKind.SourceFieldValue,
			TermInfoSourceFieldKind.StringCapability =>
				StringValuesEqual( left, right )
					? null
					: TermInfoDifferenceKind.SourceFieldValue,
			_ => null,
		};
	}

	private static bool NumericValuesEqual(
		TermInfoSourceField left,
		TermInfoSourceField right
	) {
		if ( left.NumericValue.HasValue
			|| right.NumericValue.HasValue ) {
			return left.NumericValue == right.NumericValue;
		}

		return string.Equals(
			left.Text,
			right.Text,
			StringComparison.Ordinal
		);
	}

	private static bool StringValuesEqual(
		TermInfoSourceField left,
		TermInfoSourceField right
	) {
		if ( left.StringValue is not null
			|| right.StringValue is not null ) {
			return string.Equals(
				left.StringValue,
				right.StringValue,
				StringComparison.Ordinal
			);
		}

		return string.Equals(
			left.Text,
			right.Text,
			StringComparison.Ordinal
		);
	}

	private static string? GetSemanticCapabilityName(
		TermInfoSourceField field
	) {
		return field.CanonicalCapabilityName
			?? field.CapabilityName;
	}

	private static TermInfoDifference CreateSourceIdentityTextDifference(
		TermInfoDifferenceKind kind,
		TermInfoSourceEntry left,
		TermInfoSourceEntry right,
		int? leftEntryIndex,
		int? rightEntryIndex,
		string? leftText,
		string? rightText
	) {
		return new TermInfoDifference(
			kind,
			null,
			null,
			leftText,
			rightText,
			null,
			null,
			null,
			null,
			left,
			right,
			leftEntryIndex,
			rightEntryIndex
		);
	}

	private static TermInfoDifference CreateSourceDifference(
		TermInfoDifferenceKind kind,
		TermInfoSourceEntry? leftEntry,
		TermInfoSourceEntry? rightEntry,
		int? leftEntryIndex,
		int? rightEntryIndex,
		TermInfoSourceField? leftField = null,
		TermInfoSourceField? rightField = null,
		int? leftFieldIndex = null,
		int? rightFieldIndex = null
	) {
		return new TermInfoDifference(
			kind,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			leftEntry,
			rightEntry,
			leftEntryIndex,
			rightEntryIndex,
			leftField,
			rightField,
			leftFieldIndex,
			rightFieldIndex
		);
	}
}
