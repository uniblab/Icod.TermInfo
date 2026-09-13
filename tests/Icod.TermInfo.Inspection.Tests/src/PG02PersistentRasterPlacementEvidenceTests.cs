using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG02PersistentRasterPlacementEvidenceTests {
	[Fact]
	public void EvidenceKindMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterPlacementEvidenceKind.CapabilityDerived,
				PersistentRasterPlacementEvidenceKind.Declared,
				PersistentRasterPlacementEvidenceKind.Verified,
			},
			Enum.GetValues<PersistentRasterPlacementEvidenceKind>()
		);
		Assert.Equal( 0, (int)PersistentRasterPlacementEvidenceKind.CapabilityDerived );
		Assert.Equal( 1, (int)PersistentRasterPlacementEvidenceKind.Declared );
		Assert.Equal( 2, (int)PersistentRasterPlacementEvidenceKind.Verified );
	}

	[Fact]
	public void ConstructorPreservesImmutableEvidenceFields() {
		PersistentRasterPlacementEvidence evidence = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			isPositive: true,
			PersistentRasterPlacementEvidenceKind.Verified,
			"runtime-probe",
			7
		);

		Assert.Equal( PersistentRasterPlacementSubject.SourceRectangle, evidence.Subject );
		Assert.True( evidence.IsPositive );
		Assert.Equal( PersistentRasterPlacementEvidenceKind.Verified, evidence.Kind );
		Assert.Equal( "runtime-probe", evidence.SourceLabel );
		Assert.Equal( 7, evidence.SourceOrdinal );
	}

	[Fact]
	public void ConstructorRejectsUndefinedEnumsInvalidLabelsAndNegativeOrdinals() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterPlacementEvidence(
				(PersistentRasterPlacementSubject)99,
				true,
				PersistentRasterPlacementEvidenceKind.Declared,
				"source",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				(PersistentRasterPlacementEvidenceKind)99,
				"source",
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Declared,
				" ",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterPlacementEvidence(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Declared,
				"source",
				-1
			)
		);
	}

	[Fact]
	public void EvidenceOptionsEnforceStableBounds() {
		PersistentRasterPlacementEvidenceOptions defaults = new();
		Assert.Equal(
			PersistentRasterPlacementEvidenceOptions.DefaultMaximumEvidenceCount,
			defaults.MaximumEvidenceCount
		);
		Assert.Equal( 256, PersistentRasterPlacementEvidenceOptions.DefaultMaximumEvidenceCount );
		Assert.Equal( 4096, PersistentRasterPlacementEvidenceOptions.MaximumSupportedEvidenceCount );
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterPlacementEvidenceOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterPlacementEvidenceOptions( 4097 )
		);
	}

	[Fact]
	public void SnapshotCopiesBoundsAndCanonicallyOrdersEvidence() {
		PersistentRasterPlacementEvidence[] input = [
			new(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"z",
				2
			),
			new(
				PersistentRasterPlacementSubject.SourceRectangle,
				true,
				PersistentRasterPlacementEvidenceKind.Declared,
				"b",
				1
			),
			new(
				PersistentRasterPlacementSubject.SourceRectangle,
				false,
				PersistentRasterPlacementEvidenceKind.Declared,
				"a",
				1
			),
		];

		IReadOnlyList<PersistentRasterPlacementEvidence> snapshot =
			PersistentRasterPlacementEvidence.Snapshot( input );
		input[ 0 ] = new PersistentRasterPlacementEvidence(
			PersistentRasterPlacementSubject.SourceRectangle,
			true,
			PersistentRasterPlacementEvidenceKind.CapabilityDerived,
			"replacement",
			0
		);

		Assert.Equal( 3, snapshot.Count );
		Assert.False( snapshot[ 0 ].IsPositive );
		Assert.Equal( "a", snapshot[ 0 ].SourceLabel );
		Assert.True( snapshot[ 1 ].IsPositive );
		Assert.Equal( "b", snapshot[ 1 ].SourceLabel );
		Assert.Equal( PersistentRasterPlacementSubject.SignedZOrder, snapshot[ 2 ].Subject );
		Assert.Equal( "z", snapshot[ 2 ].SourceLabel );
		Assert.Throws<ArgumentException>(
			() => PersistentRasterPlacementEvidence.Snapshot(
				input,
				new PersistentRasterPlacementEvidenceOptions( 2 )
			)
		);
	}

	[Fact]
	public void SnapshotRejectsNullCollectionAndNullElements() {
		Assert.Throws<ArgumentNullException>(
			() => PersistentRasterPlacementEvidence.Snapshot( null! )
		);
		Assert.Throws<ArgumentException>(
			() => PersistentRasterPlacementEvidence.Snapshot(
				new PersistentRasterPlacementEvidence[] { null! }
			)
		);
	}
}
