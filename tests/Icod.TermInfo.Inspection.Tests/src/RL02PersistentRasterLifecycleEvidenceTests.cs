using System.Globalization;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL02PersistentRasterLifecycleEvidenceTests {
	[Fact]
	public void EvidenceSubjectMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				"RasterDisplay",
				"PersistentUpload",
				"AcknowledgedUpload",
				"PlacementCreation",
				"MultiplePlacements",
				"PlacementUpdate",
				"PlacementDeletion",
				"ResourceDeletion",
			},
			Enum.GetNames<PersistentRasterLifecycleEvidenceSubject>()
		);
		Assert.Equal(
			new[] { 0, 1, 2, 3, 4, 5, 6, 7 },
			Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
				.Select( value => (int)value )
				.ToArray()
		);
	}

	[Fact]
	public void EvidenceKindMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				"CapabilityDerived",
				"Declared",
				"Verified",
			},
			Enum.GetNames<PersistentRasterLifecycleEvidenceKind>()
		);
		Assert.Equal(
			new[] { 0, 1, 2 },
			Enum.GetValues<PersistentRasterLifecycleEvidenceKind>()
				.Select( value => (int)value )
				.ToArray()
		);
	}

	[Fact]
	public void EvidencePreservesRawFactWithoutForcedSupportJudgment() {
		PersistentRasterLifecycleEvidence evidence = new(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.Verified,
			" caller:probe ",
			sourceOrdinal: 7
		);

		Assert.Equal(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			evidence.Subject
		);
		Assert.False( evidence.IsPositive );
		Assert.Equal(
			PersistentRasterLifecycleEvidenceKind.Verified,
			evidence.Kind
		);
		Assert.Equal( " caller:probe ", evidence.SourceLabel );
		Assert.Equal( 7, evidence.SourceOrdinal );
		Assert.DoesNotContain(
			typeof( PersistentRasterLifecycleEvidence ).GetProperties(),
			property => property.PropertyType
				== typeof( PersistentRasterLifecycleSupportStatus )
		);
	}

	[Fact]
	public void EvidenceRejectsInvalidInputs() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleEvidence(
				(PersistentRasterLifecycleEvidenceSubject)99,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"caller",
				sourceOrdinal: 0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				(PersistentRasterLifecycleEvidenceKind)99,
				"caller",
				sourceOrdinal: 0
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				null!,
				sourceOrdinal: 0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"   ",
				sourceOrdinal: 0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"caller",
				sourceOrdinal: -1
			)
		);
	}

	[Fact]
	public void SnapshotCopiesRetainsDuplicatesAndUsesCanonicalOrdinalOrdering() {
		PersistentRasterLifecycleEvidence alphaNegative = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"alpha",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence zetaNegative = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"zeta",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence alphaPositive = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: true,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"alpha",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence declared = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.Declared,
			"alpha-declared",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence upload = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"upload",
			sourceOrdinal: 0
		);
		PersistentRasterLifecycleEvidence later = CreateEvidence(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			isPositive: false,
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
			"later",
			sourceOrdinal: 1
		);
		List<PersistentRasterLifecycleEvidence> source = [
			later,
			upload,
			declared,
			alphaPositive,
			zetaNegative,
			alphaNegative,
			alphaNegative,
		];

		IReadOnlyList<PersistentRasterLifecycleEvidence> snapshot =
			PersistentRasterLifecycleEvidence.Snapshot( source );
		source.Clear();

		Assert.Equal(
			new[] {
				alphaNegative,
				alphaNegative,
				zetaNegative,
				alphaPositive,
				declared,
				upload,
				later,
			},
			snapshot
		);
		Assert.Equal( 7, snapshot.Count );
		Assert.False( snapshot is PersistentRasterLifecycleEvidence[] );
		if ( snapshot is IList<PersistentRasterLifecycleEvidence> list ) {
			Assert.True( list.IsReadOnly );
		}
	}

	[Fact]
	public void SnapshotRejectsNullElementsAndConfiguredOverflow() {
		Assert.Throws<ArgumentNullException>(
			() => PersistentRasterLifecycleEvidence.Snapshot( null! )
		);
		Assert.Throws<ArgumentException>(
			() => PersistentRasterLifecycleEvidence.Snapshot(
				new PersistentRasterLifecycleEvidence[] {
					CreateEvidence(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						isPositive: true,
						PersistentRasterLifecycleEvidenceKind.Declared,
						"valid",
						sourceOrdinal: 0
					),
					null!,
				}
			)
		);
		Assert.Throws<ArgumentException>(
			() => PersistentRasterLifecycleEvidence.Snapshot(
				Enumerable.Range( 0, 3 )
					.Select(
						index => CreateEvidence(
							PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
							isPositive: true,
							PersistentRasterLifecycleEvidenceKind.Declared,
							$"item-{index}",
							sourceOrdinal: index
						)
					),
				new PersistentRasterLifecycleEvidenceOptions(
					maximumEvidenceCount: 2
				)
			)
		);
	}

	[Fact]
	public void EvidenceOptionsFreezeBounds() {
		Assert.Equal(
			256,
			PersistentRasterLifecycleEvidenceOptions.DefaultMaximumEvidenceCount
		);
		Assert.Equal(
			4096,
			PersistentRasterLifecycleEvidenceOptions.MaximumSupportedEvidenceCount
		);
		Assert.Equal(
			256,
			new PersistentRasterLifecycleEvidenceOptions().MaximumEvidenceCount
		);
		Assert.Equal(
			1,
			new PersistentRasterLifecycleEvidenceOptions( 1 ).MaximumEvidenceCount
		);
		Assert.Equal(
			4096,
			new PersistentRasterLifecycleEvidenceOptions( 4096 ).MaximumEvidenceCount
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleEvidenceOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterLifecycleEvidenceOptions( 4097 )
		);
	}

	[Fact]
	public void SnapshotOrderingIsCultureIndependent() {
		PersistentRasterLifecycleEvidence[] evidence = [
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"İ",
				sourceOrdinal: 0
			),
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"I",
				sourceOrdinal: 0
			),
			CreateEvidence(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				isPositive: true,
				PersistentRasterLifecycleEvidenceKind.Declared,
				"i",
				sourceOrdinal: 0
			),
		];
		string[] baseline = PersistentRasterLifecycleEvidence.Snapshot( evidence )
			.Select( item => item.SourceLabel )
			.ToArray();
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			Assert.Equal(
				baseline,
				PersistentRasterLifecycleEvidence.Snapshot( evidence )
					.Select( item => item.SourceLabel )
					.ToArray()
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static PersistentRasterLifecycleEvidence CreateEvidence(
		PersistentRasterLifecycleEvidenceSubject subject,
		bool isPositive,
		PersistentRasterLifecycleEvidenceKind kind,
		string sourceLabel,
		int sourceOrdinal
	) => new(
		subject,
		isPositive,
		kind,
		sourceLabel,
		sourceOrdinal
	);
}
