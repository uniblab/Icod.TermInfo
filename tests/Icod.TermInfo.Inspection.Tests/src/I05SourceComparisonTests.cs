using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I05SourceComparisonTests {
	[Fact]
	public void Compare_EntrySelf_HasNoDifferences() {
		TermInfoSourceEntry entry =
			ParseEntry(
				"entry|I05 self comparison,am,cols#80,use=base,",
				"i05-self.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				entry,
				entry
			);

		Assert.True( result.AreEqual );
		Assert.Empty( result.Differences );
	}

	[Fact]
	public void Compare_IdentityMetadata_IsStructuredAndCarriesSourceContext() {
		TermInfoSourceEntry left =
			ParseEntry(
				"left|common|left-alias|Left description,am,",
				"i05-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"right|common|right-alias|Right description,am,",
				"i05-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.Equal( 3, result.Differences.Count );
		Assert.Equal(
			new[] {
				TermInfoDifferenceKind.IdentityName,
				TermInfoDifferenceKind.IdentityAliases,
				TermInfoDifferenceKind.IdentityDescription,
			},
			result.Differences
				.Select( difference => difference.Kind )
				.ToArray()
		);

		foreach ( TermInfoDifference difference in result.Differences ) {
			Assert.True( difference.IsSourceDifference );
			Assert.False( difference.IsCapabilityDifference );
			Assert.Same( left, difference.LeftSourceEntry );
			Assert.Same( right, difference.RightSourceEntry );
			Assert.Null( difference.LeftSourceEntryIndex );
			Assert.Null( difference.RightSourceEntryIndex );
			Assert.Null( difference.LeftSourceField );
			Assert.Null( difference.RightSourceField );
			Assert.Equal(
				"i05-left.ti",
				difference.LeftSourceSpan!.SourceName
			);
			Assert.Equal(
				"i05-right.ti",
				difference.RightSourceSpan!.SourceName
			);
		}
	}

	[Fact]
	public void Compare_FieldKinds_ExposePresentCancelledAndDisabledStates() {
		TermInfoSourceEntry left =
			ParseEntry(
				"entry|I05 field kinds,am,cols#80,clear=left,",
				"i05-kinds-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"entry|I05 field kinds,am@,.cols,clear@,",
				"i05-kinds-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.Equal( 3, result.Differences.Count );
		Assert.All(
			result.Differences,
			difference =>
				Assert.Equal(
					TermInfoDifferenceKind.SourceFieldKind,
					difference.Kind
				)
		);
		Assert.Equal(
			TermInfoSourceFieldKind.BooleanCapability,
			result.Differences[ 0 ].LeftSourceField!.Kind
		);
		Assert.Equal(
			TermInfoSourceFieldKind.CancelledCapability,
			result.Differences[ 0 ].RightSourceField!.Kind
		);
		Assert.Equal(
			TermInfoSourceFieldKind.NumericCapability,
			result.Differences[ 1 ].LeftSourceField!.Kind
		);
		Assert.Equal(
			TermInfoSourceFieldKind.DisabledCapability,
			result.Differences[ 1 ].RightSourceField!.Kind
		);
		Assert.Equal(
			TermInfoSourceFieldKind.StringCapability,
			result.Differences[ 2 ].LeftSourceField!.Kind
		);
		Assert.Equal(
			TermInfoSourceFieldKind.CancelledCapability,
			result.Differences[ 2 ].RightSourceField!.Kind
		);
	}

	[Fact]
	public void Compare_UseReferencesAndLocalValues_AreDistinctDifferences() {
		TermInfoSourceEntry left =
			ParseEntry(
				"entry|I05 use and values,use=base-left,cols#80,clear=left,",
				"i05-values-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"entry|I05 use and values,use=base-right,cols#132,clear=right,",
				"i05-values-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.Equal(
			new[] {
				TermInfoDifferenceKind.SourceUseReference,
				TermInfoDifferenceKind.SourceFieldValue,
				TermInfoDifferenceKind.SourceFieldValue,
			},
			result.Differences
				.Select( difference => difference.Kind )
				.ToArray()
		);
		Assert.Equal(
			"base-left",
			result.Differences[ 0 ].LeftSourceField!.ReferenceName
		);
		Assert.Equal(
			"base-right",
			result.Differences[ 0 ].RightSourceField!.ReferenceName
		);
		Assert.Equal( 0, result.Differences[ 0 ].LeftSourceFieldIndex );
		Assert.Equal( 0, result.Differences[ 0 ].RightSourceFieldIndex );
		Assert.Equal(
			"i05-values-left.ti",
			result.Differences[ 0 ].LeftSourceSpan!.SourceName
		);
		Assert.Equal(
			"i05-values-right.ti",
			result.Differences[ 0 ].RightSourceSpan!.SourceName
		);
	}

	[Fact]
	public void Compare_CapabilityIdentityAndRightOnlyFields_AreReported() {
		TermInfoSourceEntry left =
			ParseEntry(
				"entry|I05 capability identity,cols#80,",
				"i05-capability-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"entry|I05 capability identity,lines#80,am,",
				"i05-capability-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.Equal( 2, result.Differences.Count );
		Assert.Equal(
			TermInfoDifferenceKind.SourceFieldCapability,
			result.Differences[ 0 ].Kind
		);
		Assert.Equal(
			"cols",
			result.Differences[ 0 ].LeftSourceField!.CanonicalCapabilityName
		);
		Assert.Equal(
			"lines",
			result.Differences[ 0 ].RightSourceField!.CanonicalCapabilityName
		);
		Assert.Equal(
			TermInfoDifferenceKind.SourceFieldOnlyInRight,
			result.Differences[ 1 ].Kind
		);
		Assert.Null( result.Differences[ 1 ].LeftSourceField );
		Assert.Equal(
			TermInfoSourceFieldKind.BooleanCapability,
			result.Differences[ 1 ].RightSourceField!.Kind
		);
	}

	[Fact]
	public void Compare_DuplicateFieldsAndOrdering_RemainObservable() {
		TermInfoSourceEntry left =
			ParseEntry(
				"entry|I05 duplicate ordering,cols#80,cols#132,",
				"i05-duplicates-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"entry|I05 duplicate ordering,cols#132,cols#80,",
				"i05-duplicates-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.Equal( 2, result.Differences.Count );
		Assert.All(
			result.Differences,
			difference =>
				Assert.Equal(
					TermInfoDifferenceKind.SourceFieldValue,
					difference.Kind
				)
		);
		Assert.Equal( 0, result.Differences[ 0 ].LeftSourceFieldIndex );
		Assert.Equal( 1, result.Differences[ 1 ].LeftSourceFieldIndex );
		Assert.Equal( 80, result.Differences[ 0 ].LeftSourceField!.NumericValue );
		Assert.Equal( 132, result.Differences[ 0 ].RightSourceField!.NumericValue );
		Assert.Equal( 132, result.Differences[ 1 ].LeftSourceField!.NumericValue );
		Assert.Equal( 80, result.Differences[ 1 ].RightSourceField!.NumericValue );
	}

	[Fact]
	public void Compare_FieldOnlyOnOneSide_PreservesFieldPosition() {
		TermInfoSourceEntry left =
			ParseEntry(
				"entry|I05 field presence,am,cols#80,",
				"i05-presence-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"entry|I05 field presence,am,",
				"i05-presence-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		TermInfoDifference difference =
			Assert.Single( result.Differences );
		Assert.Equal(
			TermInfoDifferenceKind.SourceFieldOnlyInLeft,
			difference.Kind
		);
		Assert.Equal( 1, difference.LeftSourceFieldIndex );
		Assert.Null( difference.RightSourceFieldIndex );
		Assert.NotNull( difference.LeftSourceField );
		Assert.Null( difference.RightSourceField );
		Assert.Equal(
			"i05-presence-left.ti",
			difference.LeftSourceSpan!.SourceName
		);
		Assert.Null( difference.RightSourceSpan );
	}

	[Fact]
	public void Compare_Document_PreservesEntryOrderAndMissingEntries() {
		TermInfoSourceDocument left =
			ParseDocument(
				"one|First entry,am,\n"
					+ "two|Second entry,cols#80,\n",
				"i05-document-left.ti"
			);
		TermInfoSourceDocument right =
			ParseDocument(
				"one|First entry,am,\n",
				"i05-document-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		TermInfoDifference difference =
			Assert.Single( result.Differences );
		Assert.Equal(
			TermInfoDifferenceKind.SourceEntryOnlyInLeft,
			difference.Kind
		);
		Assert.Equal( 1, difference.LeftSourceEntryIndex );
		Assert.Null( difference.RightSourceEntryIndex );
		Assert.Equal(
			"two",
			difference.LeftSourceEntry!.CanonicalName
		);
		Assert.Null( difference.RightSourceEntry );
		Assert.Equal(
			"i05-document-left.ti",
			difference.LeftSourceSpan!.SourceName
		);
		Assert.Null( difference.RightSourceSpan );

		TermInfoDifference reverseDifference =
			Assert.Single(
				TermInfoSourceComparer.Compare(
					right,
					left
				).Differences
			);
		Assert.Equal(
			TermInfoDifferenceKind.SourceEntryOnlyInRight,
			reverseDifference.Kind
		);
		Assert.Null( reverseDifference.LeftSourceEntry );
		Assert.Equal(
			"two",
			reverseDifference.RightSourceEntry!.CanonicalName
		);
	}

	[Fact]
	public void Compare_EquivalentLexicalSpellings_AreSourceSemanticallyEqual() {
		TermInfoSourceEntry left =
			ParseEntry(
				@"entry|I05 lexical equality,cols#0x50,clear=\E[H,",
				"i05-lexical-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				@"entry|I05 lexical equality,cols#80,clear=\033[H,",
				"i05-lexical-right.ti"
			);

		TermInfoComparisonResult result =
			TermInfoSourceComparer.Compare(
				left,
				right
			);

		Assert.True( result.AreEqual );
		Assert.Empty( result.Differences );
	}

	[Fact]
	public void Compare_SourceMayDifferWhileEffectiveDescriptionsAreEqual() {
		TermInfoSourceDocument leftDocument =
			ParseDocument(
				"base|Base profile,cols#40,am,\n"
					+ "child|Child profile,cols#80,use=base,\n",
				"i05-effective-left.ti"
			);
		TermInfoSourceDocument rightDocument =
			ParseDocument(
				"base|Base profile,cols#40,am,\n"
					+ "child|Child profile,use=base,cols#80,\n",
				"i05-effective-right.ti"
			);
		TermInfoSourceEntry leftChild =
			leftDocument.Entries[ 1 ];
		TermInfoSourceEntry rightChild =
			rightDocument.Entries[ 1 ];

		TermInfoComparisonResult sourceResult =
			TermInfoSourceComparer.Compare(
				leftChild,
				rightChild
			);

		Assert.False( sourceResult.AreEqual );
		Assert.Equal( 2, sourceResult.Differences.Count );

		TerminalDescription leftEffective =
			Resolve(
				leftDocument,
				"child"
			);
		TerminalDescription rightEffective =
			Resolve(
				rightDocument,
				"child"
			);
		TermInfoComparisonResult effectiveResult =
			TerminalDescriptionComparer.Compare(
				leftEffective,
				rightEffective
			);

		Assert.True( effectiveResult.AreEqual );
		Assert.Empty( effectiveResult.Differences );
	}

	[Fact]
	public void Compare_Reversal_SwapsSourceSidesAndOneSidedKinds() {
		TermInfoSourceEntry left =
			ParseEntry(
				"left|left-alias|Left description,use=base-left,cols#80,am,",
				"i05-reverse-left.ti"
			);
		TermInfoSourceEntry right =
			ParseEntry(
				"right|right-alias|Right description,use=base-right,cols#132,",
				"i05-reverse-right.ti"
			);

		TermInfoComparisonResult forward =
			TermInfoSourceComparer.Compare(
				left,
				right
			);
		TermInfoComparisonResult reverse =
			TermInfoSourceComparer.Compare(
				right,
				left
			);

		Assert.Equal(
			forward.Differences.Count,
			reverse.Differences.Count
		);

		for ( int index = 0; index < forward.Differences.Count; index++ ) {
			TermInfoDifference first =
				forward.Differences[ index ];
			TermInfoDifference second =
				reverse.Differences[ index ];

			Assert.Equal(
				ReverseKind( first.Kind ),
				second.Kind
			);
			Assert.Same(
				first.LeftSourceEntry,
				second.RightSourceEntry
			);
			Assert.Same(
				first.RightSourceEntry,
				second.LeftSourceEntry
			);
			Assert.Same(
				first.LeftSourceField,
				second.RightSourceField
			);
			Assert.Same(
				first.RightSourceField,
				second.LeftSourceField
			);
			Assert.Equal(
				first.LeftSourceFieldIndex,
				second.RightSourceFieldIndex
			);
			Assert.Equal(
				first.RightSourceFieldIndex,
				second.LeftSourceFieldIndex
			);
			Assert.Equal( first.LeftText, second.RightText );
			Assert.Equal( first.RightText, second.LeftText );
			AssertAliasSequence(
				first.LeftAliases,
				second.RightAliases
			);
			AssertAliasSequence(
				first.RightAliases,
				second.LeftAliases
			);
		}
	}

	[Fact]
	public void Compare_NullArguments_AreRejected() {
		TermInfoSourceEntry entry =
			ParseEntry(
				"entry|I05 null entry,am,",
				"i05-null-entry.ti"
			);
		TermInfoSourceDocument document =
			ParseDocument(
				"entry|I05 null document,am,",
				"i05-null-document.ti"
			);

		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceComparer.Compare(
					(TermInfoSourceEntry)null!,
					entry
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceComparer.Compare(
					entry,
					(TermInfoSourceEntry)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceComparer.Compare(
					(TermInfoSourceDocument)null!,
					document
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceComparer.Compare(
					document,
					(TermInfoSourceDocument)null!
				)
		);
	}

	private static TermInfoSourceEntry ParseEntry(
		string source,
		string sourceName
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );

		TermInfoSourceDocument document =
			ParseDocument(
				source,
				sourceName
			);
		return Assert.Single( document.Entries );
	}

	private static TermInfoSourceDocument ParseDocument(
		string source,
		string sourceName
	) {
		ArgumentNullException.ThrowIfNull( source );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				sourceName
			);
		Assert.False(
			parsed.HasErrors,
			string.Join(
				Environment.NewLine,
				parsed.Diagnostics.Select(
					diagnostic => diagnostic.Message
				)
			)
		);
		return parsed.Document;
	}

	private static TerminalDescription Resolve(
		TermInfoSourceDocument document,
		string name
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				document,
				name
			);
		Assert.False(
			resolved.HasErrors,
			string.Join(
				Environment.NewLine,
				resolved.Diagnostics.Select(
					diagnostic => diagnostic.Message
				)
			)
		);
		Assert.NotNull( resolved.Entry );
		return resolved.Entry!.ToTerminalDescription();
	}

	private static void AssertAliasSequence(
		IReadOnlyList<string>? expected,
		IReadOnlyList<string>? actual
	) {
		if ( expected is null ) {
			Assert.Null( actual );
			return;
		}

		Assert.NotNull( actual );
		Assert.Equal(
			expected.ToArray(),
			actual!.ToArray()
		);
	}

	private static TermInfoDifferenceKind ReverseKind(
		TermInfoDifferenceKind kind
	) {
		return kind switch {
			TermInfoDifferenceKind.SourceEntryOnlyInLeft =>
				TermInfoDifferenceKind.SourceEntryOnlyInRight,
			TermInfoDifferenceKind.SourceEntryOnlyInRight =>
				TermInfoDifferenceKind.SourceEntryOnlyInLeft,
			TermInfoDifferenceKind.SourceFieldOnlyInLeft =>
				TermInfoDifferenceKind.SourceFieldOnlyInRight,
			TermInfoDifferenceKind.SourceFieldOnlyInRight =>
				TermInfoDifferenceKind.SourceFieldOnlyInLeft,
			_ => kind,
		};
	}
}
