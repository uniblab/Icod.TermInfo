using System.Collections;
using System.Globalization;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE02PersistentRasterRuntimeObservationTests {
	[Fact]
	public void LifecycleObservationPreservesValidatedRuntimeFact() {
		PersistentRasterRuntimeLifecycleObservation observation = new(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			" caller:probe ",
			int.MaxValue
		);

		Assert.Equal(
			PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
			observation.Subject
		);
		Assert.Equal(
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			observation.Outcome
		);
		Assert.Equal( " caller:probe ", observation.SourceLabel );
		Assert.Equal( int.MaxValue, observation.SourceOrdinal );
	}

	[Fact]
	public void PlacementObservationPreservesValidatedRuntimeFact() {
		PersistentRasterRuntimePlacementObservation observation = new(
			PersistentRasterPlacementSubject.SignedZOrder,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			" caller:probe ",
			int.MaxValue
		);

		Assert.Equal(
			PersistentRasterPlacementSubject.SignedZOrder,
			observation.Subject
		);
		Assert.Equal(
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			observation.Outcome
		);
		Assert.Equal( " caller:probe ", observation.SourceLabel );
		Assert.Equal( int.MaxValue, observation.SourceOrdinal );
	}

	[Fact]
	public void LifecycleObservationValidatesSubjectOutcomeLabelAndOrdinal() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				(PersistentRasterLifecycleEvidenceSubject)99,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"probe",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				(PersistentRasterRuntimeObservationOutcome)99,
				"probe",
				0
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				null!,
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"   ",
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				new string( 'x', 257 ),
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimeLifecycleObservation(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"probe",
				-1
			)
		);
	}

	[Fact]
	public void PlacementObservationValidatesSubjectOutcomeLabelAndOrdinal() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimePlacementObservation(
				(PersistentRasterPlacementSubject)99,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"probe",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimePlacementObservation(
				PersistentRasterPlacementSubject.SourceRectangle,
				(PersistentRasterRuntimeObservationOutcome)99,
				"probe",
				0
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => new PersistentRasterRuntimePlacementObservation(
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterRuntimeObservationOutcome.Supported,
				null!,
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimePlacementObservation(
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"   ",
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimePlacementObservation(
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterRuntimeObservationOutcome.Supported,
				new string( 'x', 257 ),
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimePlacementObservation(
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterRuntimeObservationOutcome.Supported,
				"probe",
				-1
			)
		);
	}

	[Fact]
	public void ObservationSourceLabelBoundaryIsAccepted() {
		foreach ( string sourceLabel in new[] { "x", new string( 'x', 256 ) } ) {
			PersistentRasterRuntimeLifecycleObservation lifecycle = new(
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterRuntimeObservationOutcome.Supported,
				sourceLabel,
				0
			);
			PersistentRasterRuntimePlacementObservation placement = new(
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterRuntimeObservationOutcome.Supported,
				sourceLabel,
				0
			);

			Assert.Equal( sourceLabel, lifecycle.SourceLabel );
			Assert.Equal( sourceLabel, placement.SourceLabel );
		}
	}

	[Fact]
	public void ObservationSetAllowsEmptyImmutableSnapshot() {
		PersistentRasterRuntimeObservationSet observations = new(
			Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
			Array.Empty<PersistentRasterRuntimePlacementObservation>()
		);

		Assert.Empty( observations.LifecycleObservations );
		Assert.Empty( observations.PlacementObservations );
		Assert.Equal( 0, observations.Count );
		Assert.False(
			observations.LifecycleObservations
				is PersistentRasterRuntimeLifecycleObservation[]
		);
		Assert.False(
			observations.PlacementObservations
				is PersistentRasterRuntimePlacementObservation[]
		);
		if (
			observations.LifecycleObservations
				is IList<PersistentRasterRuntimeLifecycleObservation> lifecycleList
		) {
			Assert.True( lifecycleList.IsReadOnly );
		}
		if (
			observations.PlacementObservations
				is IList<PersistentRasterRuntimePlacementObservation> placementList
		) {
			Assert.True( placementList.IsReadOnly );
		}
	}

	[Fact]
	public void ObservationSetCopiesAndRetainsDuplicatesContradictionsAndInconclusiveResults() {
		PersistentRasterRuntimeLifecycleObservation supportedLifecycle = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"probe",
			0
		);
		PersistentRasterRuntimeLifecycleObservation unsupportedLifecycle = new(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			"probe",
			0
		);
		PersistentRasterRuntimePlacementObservation inconclusivePlacement = new(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"probe",
			0
		);
		List<PersistentRasterRuntimeLifecycleObservation> lifecycle = [
			unsupportedLifecycle,
			supportedLifecycle,
			supportedLifecycle,
		];
		List<PersistentRasterRuntimePlacementObservation> placement = [
			inconclusivePlacement,
		];

		PersistentRasterRuntimeObservationSet observations = new(
			lifecycle,
			placement
		);
		lifecycle.Clear();
		placement.Clear();

		Assert.Equal(
			new[] {
				supportedLifecycle,
				supportedLifecycle,
				unsupportedLifecycle,
			},
			observations.LifecycleObservations
		);
		Assert.Equal(
			new[] { inconclusivePlacement },
			observations.PlacementObservations
		);
		Assert.Equal( 4, observations.Count );
	}

	[Fact]
	public void ObservationSetRejectsNullCollectionsAndNullElements() {
		Assert.Throws<ArgumentNullException>(
			() => new PersistentRasterRuntimeObservationSet(
				null!,
				Array.Empty<PersistentRasterRuntimePlacementObservation>()
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => new PersistentRasterRuntimeObservationSet(
				Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
				null!
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeObservationSet(
				new PersistentRasterRuntimeLifecycleObservation[] {
					CreateLifecycle(
						PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
						PersistentRasterRuntimeObservationOutcome.Supported,
						"probe",
						0
					),
					null!,
				},
				Array.Empty<PersistentRasterRuntimePlacementObservation>()
			)
		);
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeObservationSet(
				Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
				new PersistentRasterRuntimePlacementObservation[] {
					CreatePlacement(
						PersistentRasterPlacementSubject.SourceRectangle,
						PersistentRasterRuntimeObservationOutcome.Supported,
						"probe",
						0
					),
					null!,
				}
			)
		);
	}

	[Fact]
	public void ObservationSetEnforcesCombinedConfiguredMaximum() {
		PersistentRasterRuntimeLifecycleObservation lifecycle = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"probe",
			0
		);
		PersistentRasterRuntimePlacementObservation placement = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"probe",
			0
		);
		PersistentRasterRuntimeObservationOptions options = new( 4096 );

		PersistentRasterRuntimeObservationSet maximum = new(
			Enumerable.Repeat( lifecycle, 4095 ),
			new[] { placement },
			options
		);

		Assert.Equal( 4096, maximum.Count );
		Assert.Throws<ArgumentException>(
			() => new PersistentRasterRuntimeObservationSet(
				Enumerable.Repeat( lifecycle, 4096 ),
				new[] { placement },
				options
			)
		);
	}

	[Fact]
	public void ObservationSetEnumeratesEachCallerSequenceExactlyOnce() {
		SingleUseEnumerable<PersistentRasterRuntimeLifecycleObservation> lifecycle = new(
			new[] {
				CreateLifecycle(
					PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
					PersistentRasterRuntimeObservationOutcome.Supported,
					"probe",
					0
				),
			}
		);
		SingleUseEnumerable<PersistentRasterRuntimePlacementObservation> placement = new(
			new[] {
				CreatePlacement(
					PersistentRasterPlacementSubject.SourceRectangle,
					PersistentRasterRuntimeObservationOutcome.Inconclusive,
					"probe",
					0
				),
			}
		);

		PersistentRasterRuntimeObservationSet observations = new(
			lifecycle,
			placement
		);

		Assert.Equal( 2, observations.Count );
		Assert.Equal( 1, lifecycle.EnumerationCount );
		Assert.Equal( 1, placement.EnumerationCount );
	}

	[Fact]
	public void LifecycleCanonicalOrderingIsInputOrderAndCultureIndependent() {
		PersistentRasterRuntimeLifecycleObservation first = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"I",
			0
		);
		PersistentRasterRuntimeLifecycleObservation outcomeLater = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			"I",
			0
		);
		PersistentRasterRuntimeLifecycleObservation ordinalLater = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"I",
			1
		);
		PersistentRasterRuntimeLifecycleObservation lowerAsciiLater = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"i",
			0
		);
		PersistentRasterRuntimeLifecycleObservation dottedILater = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"İ",
			0
		);
		PersistentRasterRuntimeLifecycleObservation subjectLater = CreateLifecycle(
			PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"A",
			0
		);
		PersistentRasterRuntimeLifecycleObservation[] expected = [
			first,
			outcomeLater,
			ordinalLater,
			lowerAsciiLater,
			dottedILater,
			subjectLater,
		];
		PersistentRasterRuntimeLifecycleObservation[] input = [
			subjectLater,
			dottedILater,
			lowerAsciiLater,
			ordinalLater,
			outcomeLater,
			first,
		];

		Assert.Equal(
			expected,
			SnapshotLifecycleUnderCulture( input, "tr-TR" )
		);
		Assert.Equal(
			expected,
			SnapshotLifecycleUnderCulture( input.Reverse(), "fr-FR" )
		);
	}

	[Fact]
	public void PlacementCanonicalOrderingIsInputOrderAndCultureIndependent() {
		PersistentRasterRuntimePlacementObservation first = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"I",
			0
		);
		PersistentRasterRuntimePlacementObservation outcomeLater = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Inconclusive,
			"I",
			0
		);
		PersistentRasterRuntimePlacementObservation ordinalLater = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"I",
			1
		);
		PersistentRasterRuntimePlacementObservation lowerAsciiLater = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"i",
			0
		);
		PersistentRasterRuntimePlacementObservation dottedILater = CreatePlacement(
			PersistentRasterPlacementSubject.SourceRectangle,
			PersistentRasterRuntimeObservationOutcome.Supported,
			"İ",
			0
		);
		PersistentRasterRuntimePlacementObservation subjectLater = CreatePlacement(
			PersistentRasterPlacementSubject.SignedZOrder,
			PersistentRasterRuntimeObservationOutcome.Unsupported,
			"A",
			0
		);
		PersistentRasterRuntimePlacementObservation[] expected = [
			first,
			outcomeLater,
			ordinalLater,
			lowerAsciiLater,
			dottedILater,
			subjectLater,
		];
		PersistentRasterRuntimePlacementObservation[] input = [
			subjectLater,
			dottedILater,
			lowerAsciiLater,
			ordinalLater,
			outcomeLater,
			first,
		];

		Assert.Equal(
			expected,
			SnapshotPlacementUnderCulture( input, "tr-TR" )
		);
		Assert.Equal(
			expected,
			SnapshotPlacementUnderCulture( input.Reverse(), "fr-FR" )
		);
	}

	private static PersistentRasterRuntimeLifecycleObservation CreateLifecycle(
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterRuntimeObservationOutcome outcome,
		string sourceLabel,
		int sourceOrdinal
	) => new(
		subject,
		outcome,
		sourceLabel,
		sourceOrdinal
	);

	private static PersistentRasterRuntimePlacementObservation CreatePlacement(
		PersistentRasterPlacementSubject subject,
		PersistentRasterRuntimeObservationOutcome outcome,
		string sourceLabel,
		int sourceOrdinal
	) => new(
		subject,
		outcome,
		sourceLabel,
		sourceOrdinal
	);

	private static IReadOnlyList<PersistentRasterRuntimeLifecycleObservation>
		SnapshotLifecycleUnderCulture(
			IEnumerable<PersistentRasterRuntimeLifecycleObservation> observations,
			string cultureName
		) {
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo culture = CultureInfo.GetCultureInfo( cultureName );
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
			return new PersistentRasterRuntimeObservationSet(
				observations,
				Array.Empty<PersistentRasterRuntimePlacementObservation>()
			).LifecycleObservations;
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static IReadOnlyList<PersistentRasterRuntimePlacementObservation>
		SnapshotPlacementUnderCulture(
			IEnumerable<PersistentRasterRuntimePlacementObservation> observations,
			string cultureName
		) {
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo culture = CultureInfo.GetCultureInfo( cultureName );
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
			return new PersistentRasterRuntimeObservationSet(
				Array.Empty<PersistentRasterRuntimeLifecycleObservation>(),
				observations
			).PlacementObservations;
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private sealed class SingleUseEnumerable<T> : IEnumerable<T> {
		private readonly IEnumerable<T> _source;

		public SingleUseEnumerable( IEnumerable<T> source ) {
			ArgumentNullException.ThrowIfNull( source );
			_source = source;
		}

		public int EnumerationCount {
			get;
			private set;
		}

		public IEnumerator<T> GetEnumerator() {
			EnumerationCount++;
			if ( EnumerationCount > 1 ) {
				throw new InvalidOperationException(
					"The test sequence was enumerated more than once."
				);
			}
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
