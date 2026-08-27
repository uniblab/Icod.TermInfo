namespace Icod.TermInfo.Inspection;

/// <summary>
/// Compares two effective <see cref="TerminalDescription"/> values without
/// converting either description to source text.
/// </summary>
/// <remarks>
/// Comparison observes effective identity metadata and effective capabilities
/// only. Source cancellation, disabled fields, inheritance, comments, and source
/// provenance are not present in <see cref="TerminalDescription"/> and are not
/// reconstructed by this comparer.
/// </remarks>
public static class TerminalDescriptionComparer {
	/// <summary>
	/// Compares two effective terminal descriptions.
	/// </summary>
	/// <param name="left">The left terminal description.</param>
	/// <param name="right">The right terminal description.</param>
	/// <returns>A deterministic structured comparison result.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="left"/> or <paramref name="right"/> is
	/// <see langword="null"/>.
	/// </exception>
	public static TermInfoComparisonResult Compare(
		TerminalDescription left,
		TerminalDescription right
	) {
		ArgumentNullException.ThrowIfNull( left );
		ArgumentNullException.ThrowIfNull( right );

		List<TermInfoDifference> differences = [];

		CompareIdentity(
			differences,
			left,
			right
		);
		CompareStandardBooleans(
			differences,
			left,
			right
		);
		CompareStandardNumbers(
			differences,
			left,
			right
		);
		CompareStandardStrings(
			differences,
			left,
			right
		);
		CompareExtendedCapabilities(
			differences,
			left,
			right
		);

		return new TermInfoComparisonResult(
			differences
		);
	}

	private static void CompareIdentity(
		ICollection<TermInfoDifference> differences,
		TerminalDescription left,
		TerminalDescription right
	) {
		if ( !string.Equals(
			left.Name,
			right.Name,
			StringComparison.Ordinal
		) ) {
			differences.Add(
				CreateIdentityTextDifference(
					TermInfoDifferenceKind.IdentityName,
					left.Name,
					right.Name
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
					null
				)
			);
		}

		if ( !string.Equals(
			left.Description,
			right.Description,
			StringComparison.Ordinal
		) ) {
			differences.Add(
				CreateIdentityTextDifference(
					TermInfoDifferenceKind.IdentityDescription,
					left.Description,
					right.Description
				)
			);
		}
	}

	private static void CompareStandardBooleans(
		ICollection<TermInfoDifference> differences,
		TerminalDescription left,
		TerminalDescription right
	) {
		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			bool leftPresent =
				left.GetBoolean( metadata.Capability );
			bool rightPresent =
				right.GetBoolean( metadata.Capability );

			AddCapabilityDifference(
				differences,
				metadata.ShortName,
				isExtendedCapability: false,
				leftPresent,
				new TermInfoCapabilityValue( true ),
				rightPresent,
				new TermInfoCapabilityValue( true )
			);
		}
	}

	private static void CompareStandardNumbers(
		ICollection<TermInfoDifference> differences,
		TerminalDescription left,
		TerminalDescription right
	) {
		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			int? leftValue =
				left.GetNumber( metadata.Capability );
			int? rightValue =
				right.GetNumber( metadata.Capability );

			AddCapabilityDifference(
				differences,
				metadata.ShortName,
				isExtendedCapability: false,
				leftValue.HasValue,
				leftValue.HasValue
					? new TermInfoCapabilityValue( leftValue.Value )
					: default,
				rightValue.HasValue,
				rightValue.HasValue
					? new TermInfoCapabilityValue( rightValue.Value )
					: default
			);
		}
	}

	private static void CompareStandardStrings(
		ICollection<TermInfoDifference> differences,
		TerminalDescription left,
		TerminalDescription right
	) {
		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			string? leftValue =
				left.GetString( metadata.Capability );
			string? rightValue =
				right.GetString( metadata.Capability );

			AddCapabilityDifference(
				differences,
				metadata.ShortName,
				isExtendedCapability: false,
				leftValue is not null,
				leftValue is not null
					? new TermInfoCapabilityValue( leftValue )
					: default,
				rightValue is not null,
				rightValue is not null
					? new TermInfoCapabilityValue( rightValue )
					: default
			);
		}
	}

	private static void CompareExtendedCapabilities(
		ICollection<TermInfoDifference> differences,
		TerminalDescription left,
		TerminalDescription right
	) {
		string[] names =
			left.ExtendedCapabilities.Keys
				.Concat(
					right.ExtendedCapabilities.Keys
				)
				.Distinct(
					StringComparer.Ordinal
				)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();

		foreach ( string name in names ) {
			bool leftPresent =
				left.ExtendedCapabilities.TryGetValue(
					name,
					out TermInfoCapabilityValue leftValue
				);
			bool rightPresent =
				right.ExtendedCapabilities.TryGetValue(
					name,
					out TermInfoCapabilityValue rightValue
				);

			AddCapabilityDifference(
				differences,
				name,
				isExtendedCapability: true,
				leftPresent,
				leftValue,
				rightPresent,
				rightValue
			);
		}
	}

	private static void AddCapabilityDifference(
		ICollection<TermInfoDifference> differences,
		string capabilityName,
		bool isExtendedCapability,
		bool leftPresent,
		TermInfoCapabilityValue leftValue,
		bool rightPresent,
		TermInfoCapabilityValue rightValue
	) {
		TermInfoDifferenceKind? kind = null;

		if ( leftPresent && !rightPresent ) {
			kind = TermInfoDifferenceKind.OnlyInLeft;
		}
		else if ( !leftPresent && rightPresent ) {
			kind = TermInfoDifferenceKind.OnlyInRight;
		}
		else if ( leftPresent && rightPresent ) {
			if ( leftValue.Kind != rightValue.Kind ) {
				kind = TermInfoDifferenceKind.DifferentValueKind;
			}
			else if ( !leftValue.Equals( rightValue ) ) {
				kind = TermInfoDifferenceKind.DifferentValue;
			}
		}

		if ( kind is null ) {
			return;
		}

		differences.Add(
			new TermInfoDifference(
				kind.Value,
				capabilityName,
				isExtendedCapability,
				null,
				null,
				null,
				null,
				leftPresent
					? leftValue
					: null,
				rightPresent
					? rightValue
					: null
			)
		);
	}

	private static TermInfoDifference CreateIdentityTextDifference(
		TermInfoDifferenceKind kind,
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
			null
		);
	}
}
