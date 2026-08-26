using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S07ResolverTests {
	[Fact]
	public void ResolvesNamedParentRecursivelyThroughAliases() {
		TermInfoSourceDocument document =
			ParseDocument(
				"base|base-alias|S07 base,\n"
				+ "\tam,\n"
				+ "\tcols#80,\n"
				+ "grandchild|S07 grandchild,\n"
				+ "\tlines#24,\n"
				+ "\tuse=base-alias,\n"
				+ "child|S07 child,\n"
				+ "\tclear=child,\n"
				+ "\tuse=grandchild,\n" );

		TermInfoSourceResolveResult result =
			TermInfoSourceResolver.Resolve(
				document,
				"child" );

		TermInfoSourceResolvedEntry entry =
			AssertResolved( result );
		Assert.Equal(
			"child",
			entry.SourceEntry.CanonicalName );
		Assert.True(
			entry.GetBoolean( BooleanCapability.AutoRightMargin ) );
		Assert.Equal(
			80,
			entry.GetNumber( NumericCapability.Columns ) );
		Assert.Equal(
			24,
			entry.GetNumber( NumericCapability.Lines ) );
		Assert.Equal(
			"child",
			entry.GetString( StringCapability.ClearScreen ) );
	}

	[Fact]
	public void MultipleParentsMergeRightToLeftSoLeftwardParentWins() {
		TermInfoSourceDocument document =
			ParseDocument(
				"left|S07 left parent,\n"
				+ "\tcols#80,\n"
				+ "\tclear=left,\n"
				+ "\tVendor=left,\n"
				+ "right|S07 right parent,\n"
				+ "\tcols#90,\n"
				+ "\tclear=right,\n"
				+ "\tVendor=right,\n"
				+ "child|S07 child,\n"
				+ "\tuse=left,\n"
				+ "\tuse=right,\n" );

		TermInfoSourceResolvedEntry entry =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					document,
					"child" ) );

		Assert.Equal(
			80,
			entry.GetNumber( NumericCapability.Columns ) );
		Assert.Equal(
			"left",
			entry.GetString( StringCapability.ClearScreen ) );
		Assert.True(
			entry.TryGetExtended(
				"Vendor",
				out TermInfoCapabilityValue vendor ) );
		Assert.Equal(
			"left",
			vendor.StringValue );
	}

	[Fact]
	public void LocalValuesAndCancellationsOutrankAllParents() {
		TermInfoSourceDocument document =
			ParseDocument(
				"base|S07 base,\n"
				+ "\tam,\n"
				+ "\tcols#80,\n"
				+ "\tclear=base,\n"
				+ "\tVendor=base,\n"
				+ "parent|S07 parent,\n"
				+ "\tlines#24,\n"
				+ "\tuse=base,\n"
				+ "child|S07 child,\n"
				+ "\tam@,\n"
				+ "\tcols#132,\n"
				+ "\tclear@,\n"
				+ "\tVendor@,\n"
				+ "\tuse=parent,\n" );

		TermInfoSourceResolvedEntry entry =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					document,
					"child" ) );

		Assert.False(
			entry.GetBoolean( BooleanCapability.AutoRightMargin ) );
		Assert.Equal(
			132,
			entry.GetNumber( NumericCapability.Columns ) );
		Assert.Equal(
			24,
			entry.GetNumber( NumericCapability.Lines ) );
		Assert.Null(
			entry.GetString( StringCapability.ClearScreen ) );
		Assert.False(
			entry.TryGetExtended(
				"Vendor",
				out _ ) );
	}

	[Fact]
	public void ParentCancellationParticipatesInParentPriority() {
		TermInfoSourceDocument document =
			ParseDocument(
				"left|S07 left parent,\n"
				+ "\tcols@,\n"
				+ "\tVendor@,\n"
				+ "right|S07 right parent,\n"
				+ "\tcols#90,\n"
				+ "\tVendor=right,\n"
				+ "child|S07 child,\n"
				+ "\tuse=left,\n"
				+ "\tuse=right,\n" );

		TermInfoSourceResolvedEntry entry =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					document,
					"child" ) );

		Assert.Null(
			entry.GetNumber( NumericCapability.Columns ) );
		Assert.False(
			entry.TryGetExtended(
				"Vendor",
				out _ ) );
	}

	[Fact]
	public void MissingRootAndMissingParentProduceStableDiagnostics() {
		TermInfoSourceDocument document =
			ParseDocument(
				"child|S07 missing parents,\n"
				+ "\tuse=missing-left,\n"
				+ "\tuse=missing-right,\n" );

		TermInfoSourceResolveResult rootMiss =
			TermInfoSourceResolver.Resolve(
				document,
				"does-not-exist" );
		Assert.True( rootMiss.HasErrors );
		Assert.Null( rootMiss.Entry );
		TermInfoSourceDiagnostic rootDiagnostic =
			Assert.Single( rootMiss.Diagnostics );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MissingSourceEntry,
			rootDiagnostic.Code );
		Assert.Null( rootDiagnostic.Span );

		TermInfoSourceResolveResult parentMiss =
			TermInfoSourceResolver.Resolve(
				document,
				"child" );
		Assert.True( parentMiss.HasErrors );
		Assert.Null( parentMiss.Entry );
		Assert.Equal(
			new[]
			{
				TermInfoSourceDiagnosticCodes.MissingSourceEntry,
				TermInfoSourceDiagnosticCodes.MissingSourceEntry,
			},
			parentMiss.Diagnostics.Select(
				diagnostic => diagnostic.Code ) );
		Assert.True(
			parentMiss.Diagnostics[ 0 ].Span!.Offset
				< parentMiss.Diagnostics[ 1 ].Span!.Offset );
	}

	[Fact]
	public void DetectsDirectAndIndirectCyclesByCanonicalIdentity() {
		TermInfoSourceDocument directDocument =
			ParseDocument(
				"self|self-alias|S07 direct cycle,\n"
				+ "\tuse=self-alias,\n" );
		TermInfoSourceResolveResult direct =
			TermInfoSourceResolver.Resolve(
				directDocument,
				"self" );

		Assert.True( direct.HasErrors );
		Assert.Null( direct.Entry );
		TermInfoSourceDiagnostic directDiagnostic =
			Assert.Single( direct.Diagnostics );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.InheritanceCycle,
			directDiagnostic.Code );
		Assert.Contains(
			"self -> self",
			directDiagnostic.Message );

		TermInfoSourceDocument indirectDocument =
			ParseDocument(
				"a|S07 cycle a,\n"
				+ "\tuse=b,\n"
				+ "b|S07 cycle b,\n"
				+ "\tuse=c,\n"
				+ "c|S07 cycle c,\n"
				+ "\tuse=a,\n" );
		TermInfoSourceResolveResult indirect =
			TermInfoSourceResolver.Resolve(
				indirectDocument,
				"a" );

		Assert.True( indirect.HasErrors );
		Assert.Null( indirect.Entry );
		TermInfoSourceDiagnostic indirectDiagnostic =
			Assert.Single( indirect.Diagnostics );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.InheritanceCycle,
			indirectDiagnostic.Code );
		Assert.Contains(
			"a -> b -> c -> a",
			indirectDiagnostic.Message );
	}

	[Fact]
	public void EnforcesConfiguredInheritanceDepthAtExactEdgeBoundary() {
		TermInfoSourceDocument document =
			ParseDocument(
				"base|S07 depth base,\n"
				+ "\tcols#80,\n"
				+ "middle|S07 depth middle,\n"
				+ "\tuse=base,\n"
				+ "root|S07 depth root,\n"
				+ "\tuse=middle,\n" );

		TermInfoSourceResolveResult allowed =
			TermInfoSourceResolver.Resolve(
				document,
				"root",
				new TermInfoSourceResolverOptions( 2 ) );
		AssertResolved( allowed );

		TermInfoSourceResolveResult rejected =
			TermInfoSourceResolver.Resolve(
				document,
				"root",
				new TermInfoSourceResolverOptions( 1 ) );
		Assert.True( rejected.HasErrors );
		Assert.Null( rejected.Entry );
		TermInfoSourceDiagnostic diagnostic =
			Assert.Single( rejected.Diagnostics );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
			diagnostic.Code );
		Assert.Contains(
			"base",
			diagnostic.Message );
	}

	[Fact]
	public void ZeroDepthAllowsOnlyEntriesWithoutUseReferences() {
		TermInfoSourceDocument document =
			ParseDocument(
				"base|S07 zero depth base,\n"
				+ "\tcols#80,\n"
				+ "child|S07 zero depth child,\n"
				+ "\tuse=base,\n" );
		TermInfoSourceResolverOptions options =
			new( 0 );

		AssertResolved(
			TermInfoSourceResolver.Resolve(
				document,
				"base",
				options ) );

		TermInfoSourceResolveResult child =
			TermInfoSourceResolver.Resolve(
				document,
				"child",
				options );
		Assert.True( child.HasErrors );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
			Assert.Single( child.Diagnostics ).Code );
	}

	[Fact]
	public void CacheDoesNotBypassDepthLimitWhenSubtreeIsReachedDeeper() {
		TermInfoSourceDocument document =
			ParseDocument(
				"leaf|S07 cache-depth leaf,\n"
				+ "\tcols#80,\n"
				+ "shared|S07 cache-depth shared,\n"
				+ "\tuse=leaf,\n"
				+ "branch|S07 cache-depth branch,\n"
				+ "\tuse=shared,\n"
				+ "root|S07 cache-depth root,\n"
				+ "\tuse=branch,\n"
				+ "\tuse=shared,\n" );

		TermInfoSourceResolveResult result =
			TermInfoSourceResolver.Resolve(
				document,
				"root",
				new TermInfoSourceResolverOptions( 2 ) );

		Assert.True( result.HasErrors );
		Assert.Null( result.Entry );
		TermInfoSourceDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermInfoSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
			diagnostic.Code );
		Assert.Contains(
			"leaf",
			diagnostic.Message );
	}

	[Fact]
	public void CallerSuppliedProviderCanResolveAcrossParsedDocuments() {
		TermInfoSourceEntry parent =
			Assert.Single(
				ParseDocument(
					"parent|external-parent|S07 external parent,\n"
					+ "\tcols#80,\n" )
				.Entries );
		TermInfoSourceEntry child =
			Assert.Single(
				ParseDocument(
					"child|S07 external child,\n"
					+ "\tuse=external-parent,\n" )
				.Entries );
		DictionaryEntryProvider provider =
			new(
				child,
				parent );

		TermInfoSourceResolvedEntry entry =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					provider,
					"child" ) );

		Assert.Equal(
			80,
			entry.GetNumber( NumericCapability.Columns ) );
	}

	[Fact]
	public void ProviderFailuresAndContractViolationsAreNotConvertedToMisses() {
		Assert.Throws<IOException>(
			() =>
				TermInfoSourceResolver.Resolve(
					new ThrowingProvider(),
					"terminal" ) );

		Assert.Throws<InvalidOperationException>(
			() =>
				TermInfoSourceResolver.Resolve(
					new SuccessWithNullProvider(),
					"terminal" ) );

		TermInfoSourceEntry entry =
			Assert.Single(
				ParseDocument(
					"terminal|S07 provider contract,\n"
					+ "\tcols#80,\n" )
				.Entries );
		Assert.Throws<InvalidOperationException>(
			() =>
				TermInfoSourceResolver.Resolve(
					new MissWithEntryProvider( entry ),
					"terminal" ) );
	}

	[Fact]
	public void DocumentLookupUsesFirstSourceOrderMatchDeterministically() {
		TermInfoSourceDocument document =
			ParseDocument(
				"first|shared|S07 first alias,\n"
				+ "\tcols#80,\n"
				+ "second|shared|S07 second alias,\n"
				+ "\tcols#132,\n"
				+ "child|S07 duplicate alias consumer,\n"
				+ "\tuse=shared,\n" );

		for ( int iteration = 0; iteration < 5; iteration++ ) {
			TermInfoSourceResolvedEntry entry =
				AssertResolved(
					TermInfoSourceResolver.Resolve(
						document,
						"child" ) );
			Assert.Equal(
				80,
				entry.GetNumber( NumericCapability.Columns ) );
		}
	}

	[Fact]
	public void ComplexGraphIsIndependentOfProviderDictionaryInsertionOrder() {
		TermInfoSourceDocument document =
			ParseDocument(
				"base|S07 deterministic base,\n"
				+ "\tam,\n"
				+ "\tcols#80,\n"
				+ "left|S07 deterministic left,\n"
				+ "\tlines#24,\n"
				+ "\tuse=base,\n"
				+ "right|S07 deterministic right,\n"
				+ "\tcols#90,\n"
				+ "\tVendor=right,\n"
				+ "child|S07 deterministic child,\n"
				+ "\tVendor=child,\n"
				+ "\tuse=left,\n"
				+ "\tuse=right,\n" );
		TermInfoSourceEntry[] entries =
			document.Entries.ToArray();

		DictionaryEntryProvider forward =
			new( entries );
		DictionaryEntryProvider reverse =
			new( entries.Reverse().ToArray() );

		TermInfoSourceResolvedEntry first =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					forward,
					"child" ) );
		TermInfoSourceResolvedEntry second =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					reverse,
					"child" ) );

		Assert.Equal(
			first.GetBoolean( BooleanCapability.AutoRightMargin ),
			second.GetBoolean( BooleanCapability.AutoRightMargin ) );
		Assert.Equal(
			first.GetNumber( NumericCapability.Columns ),
			second.GetNumber( NumericCapability.Columns ) );
		Assert.Equal(
			first.GetNumber( NumericCapability.Lines ),
			second.GetNumber( NumericCapability.Lines ) );
		Assert.True(
			first.TryGetExtended(
				"Vendor",
				out TermInfoCapabilityValue firstVendor ) );
		Assert.True(
			second.TryGetExtended(
				"Vendor",
				out TermInfoCapabilityValue secondVendor ) );
		Assert.Equal(
			firstVendor,
			secondVendor );
	}

	[Fact]
	public void ResolverOptionsRejectUnsafeDepthLimits() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TermInfoSourceResolverOptions( -1 ) );
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TermInfoSourceResolverOptions(
					TermInfoSourceResolverOptions.MaximumSupportedInheritanceDepth
					+ 1 ) );
	}

	[Fact]
	public void ResolvedCapabilityQueriesValidateArguments() {
		TermInfoSourceResolvedEntry entry =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					ParseDocument(
						"terminal|S07 query validation,\n"
						+ "\tcols#80,\n" ),
					"terminal" ) );

		Assert.Throws<ArgumentOutOfRangeException>(
			() => entry.GetBoolean( (BooleanCapability)( -1 ) ) );
		Assert.Throws<ArgumentOutOfRangeException>(
			() => entry.GetNumber( (NumericCapability)( -1 ) ) );
		Assert.Throws<ArgumentOutOfRangeException>(
			() => entry.GetString( (StringCapability)( -1 ) ) );
		Assert.Throws<ArgumentException>(
			() =>
				entry.TryGetExtended(
					" ",
					out _ ) );
	}

	private static TermInfoSourceDocument ParseDocument(
		string source ) {
		ArgumentNullException.ThrowIfNull( source );

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"s07.ti" );
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics ) );
		return parsed.Document;
	}

	private static TermInfoSourceResolvedEntry AssertResolved(
		TermInfoSourceResolveResult result ) {
		ArgumentNullException.ThrowIfNull( result );

		Assert.False(
			result.HasErrors,
			FormatDiagnostics( result.Diagnostics ) );
		Assert.Empty( result.Diagnostics );
		Assert.NotNull( result.Entry );
		return result.Entry!;
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics ) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return string.Join(
			"; ",
			diagnostics.Select(
				diagnostic =>
					diagnostic.Code
					+ " "
					+ diagnostic.Message ) );
	}

	private sealed class DictionaryEntryProvider : ITermInfoSourceEntryProvider {
		private readonly Dictionary<string, TermInfoSourceEntry> _entries =
			new( StringComparer.Ordinal );

		internal DictionaryEntryProvider(
			params TermInfoSourceEntry[] entries ) {
			ArgumentNullException.ThrowIfNull( entries );

			foreach ( TermInfoSourceEntry entry in entries ) {
				_entries[ entry.CanonicalName ] = entry;
				foreach ( string alias in entry.Aliases ) {
					_entries[ alias ] = entry;
				}
			}
		}

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermInfoSourceEntry? entry ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
			return _entries.TryGetValue(
				name,
				out entry );
		}
	}

	private sealed class ThrowingProvider : ITermInfoSourceEntryProvider {
		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermInfoSourceEntry? entry ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
			entry = null;
			throw new IOException( "Synthetic source-provider failure." );
		}
	}

	private sealed class SuccessWithNullProvider : ITermInfoSourceEntryProvider {
		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermInfoSourceEntry? entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			// Deliberately violate the provider contract to test resolver validation.
			entry = null!;
			return true;
		}
	}
	private sealed class MissWithEntryProvider : ITermInfoSourceEntryProvider {
		private readonly TermInfoSourceEntry _entry;

		internal MissWithEntryProvider(
			TermInfoSourceEntry entry ) {
			ArgumentNullException.ThrowIfNull( entry );
			_entry = entry;
		}

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermInfoSourceEntry? entry ) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );
			entry = _entry;
			return false;
		}
	}
}
