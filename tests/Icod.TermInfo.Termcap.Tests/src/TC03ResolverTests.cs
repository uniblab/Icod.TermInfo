using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC03ResolverTests
{
	[Fact]
	public void LocalFieldsOverrideInheritedFieldsAndPreserveProvenance() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:am:co#80:cl=base:\n"
				+ "child|Child terminal:co#132:tc=base:\n"
			);
		TermcapSourceEntry baseEntry = document.Entries[0];
		TermcapSourceEntry childEntry = document.Entries[1];

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.False( result.HasErrors );
		Assert.Empty( result.Diagnostics );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		Assert.Same( childEntry, resolved.SourceEntry );
		Assert.Equal(
			new[] { "co", "am", "cl" },
			resolved.Fields
				.Select( field => field.CapabilityName )
				.ToArray()
		);

		TermcapSourceResolvedField columns =
			GetRequiredField(
				resolved,
				"co"
			);
		Assert.Same( childEntry, columns.SourceEntry );
		Assert.Equal( 0, columns.InheritanceDepth );
		Assert.False( columns.IsInherited );
		Assert.Equal( 132, columns.SourceField.NumericValue );

		TermcapSourceResolvedField automaticMargin =
			GetRequiredField(
				resolved,
				"am"
			);
		Assert.Same( baseEntry, automaticMargin.SourceEntry );
		Assert.Equal( 1, automaticMargin.InheritanceDepth );
		Assert.True( automaticMargin.IsInherited );
		Assert.Equal(
			baseEntry.Fields[0].Span.Offset,
			automaticMargin.SourceField.Span.Offset
		);
	}

	[Fact]
	public void CancellationSuppressesInheritedCapabilityAcrossMultipleLevels() {
		TermcapSourceDocument document =
			ParseDocument(
				"grand|Grand terminal:co#80:cl=grand:\n"
				+ "parent|Parent terminal:co@:tc=grand:\n"
				+ "child|Child terminal:am:tc=parent:\n"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.False( result.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		Assert.False(
			resolved.TryGetField(
				"co",
				out _
			)
		);
		TermcapSourceResolvedField clear =
			GetRequiredField(
				resolved,
				"cl"
			);
		Assert.Equal( 2, clear.InheritanceDepth );
		Assert.Equal( "grand", clear.SourceField.StringValue );
	}

	[Fact]
	public void DisabledLocalFieldDoesNotSuppressInheritedCapability() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:co#80:\n"
				+ "child|Child terminal:.co#132:tc=base:\n"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.False( result.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		TermcapSourceResolvedField columns =
			GetRequiredField(
				resolved,
				"co"
			);
		Assert.Equal( 80, columns.SourceField.NumericValue );
		Assert.Equal( 1, columns.InheritanceDepth );
	}

	[Fact]
	public void FirstLocalOccurrenceWinsDeterministically() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:co#80:\n"
				+ "child|Child terminal:co#132:co@:tc=base:\n"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.False( result.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		TermcapSourceResolvedField columns =
			GetRequiredField(
				resolved,
				"co"
			);
		Assert.Equal( 132, columns.SourceField.NumericValue );
		Assert.Equal( 0, columns.InheritanceDepth );
	}

	[Fact]
	public void UnmappedFieldsParticipateInInheritanceByExactCode() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:!!=base:\n"
				+ "child|Child terminal:tc=base:\n"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.False( result.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		TermcapSourceResolvedField vendor =
			GetRequiredField(
				resolved,
				"!!"
			);
		Assert.Equal( "base", vendor.SourceField.StringValue );
		Assert.Equal( 1, vendor.InheritanceDepth );
	}

	[Fact]
	public void DocumentLookupUsesHeaderComponentsAndSourceOrder() {
		TermcapSourceDocument document =
			ParseDocument(
				"first|shared|First terminal:co#80:\n"
				+ "second|shared|Second terminal:co#132:\n"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"shared"
			);

		Assert.False( result.HasErrors );
		TermcapSourceResolvedEntry resolved =
			Assert.IsType<TermcapSourceResolvedEntry>( result.Entry );
		Assert.Same( document.Entries[0], resolved.SourceEntry );
		TermcapSourceResolvedField columns =
			GetRequiredField(
				resolved,
				"co"
			);
		Assert.Equal( 80, columns.SourceField.NumericValue );
	}

	[Fact]
	public void MissingInheritedEntryProducesReferenceDiagnostic() {
		TermcapSourceDocument document =
			ParseDocument(
				"child|Child terminal:am:tc=missing:\n",
				"missing.tc"
			);
		TermcapSourceField reference = document.Entries[0].Fields[1];

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"child"
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Entry );
		TermcapSourceDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapSourceDiagnosticCodes.MissingSourceEntry,
			diagnostic.Code
		);
		Assert.Same( reference.Span, diagnostic.Span );
	}

	[Theory]
	[InlineData(
		"one|One terminal:tc=one:\n",
		"one"
	)]
	[InlineData(
		"one|One terminal:tc=two:\ntwo|Two terminal:tc=one:\n",
		"one"
	)]
	public void DirectAndIndirectCyclesProduceDeterministicDiagnostic(
		string source,
		string rootName
	) {
		TermcapSourceDocument document =
			ParseDocument(
				source,
				"cycle.tc"
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				rootName
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Entry );
		TermcapSourceDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapSourceDiagnosticCodes.InheritanceCycle,
			diagnostic.Code
		);
		Assert.Contains( " -> ", diagnostic.Message );
		Assert.NotNull( diagnostic.Span );
	}

	[Fact]
	public void MaximumInheritanceDepthIsBounded() {
		TermcapSourceDocument document =
			ParseDocument(
				"one|One terminal:tc=two:\n"
				+ "two|Two terminal:tc=three:\n"
				+ "three|Three terminal:co#80:\n"
			);
		TermcapSourceResolverOptions options =
			new(
				maximumInheritanceDepth: 1
			);

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				document,
				"one",
				options
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Entry );
		TermcapSourceDiagnostic diagnostic =
			Assert.Single( result.Diagnostics );
		Assert.Equal(
			TermcapSourceDiagnosticCodes.MaximumInheritanceDepthExceeded,
			diagnostic.Code
		);
		Assert.Contains( "1", diagnostic.Message );
	}

	[Fact]
	public void CallerSuppliedProviderParticipatesWithoutGlobalDiscovery() {
		TermcapSourceDocument document =
			ParseDocument(
				"base|Base terminal:co#80:\n"
				+ "child|Child terminal:tc=base:\n"
			);
		DictionaryEntryProvider provider =
			new( document.Entries );

		TermcapSourceResolveResult result =
			TermcapSourceResolver.Resolve(
				provider,
				"child"
			);

		Assert.False( result.HasErrors );
		Assert.Equal(
			new[] { "child", "base" },
			provider.RequestedNames.ToArray()
		);
	}

	[Fact]
	public void InvalidProviderSuccessContractThrows() {
		TermcapSourceDocument document =
			ParseDocument(
				"child|Child terminal:co#80:\n"
			);
		ITermcapSourceEntryProvider provider =
			new InvalidProvider(
				document.Entries[0]
			);

		Assert.Throws<InvalidOperationException>(
			() =>
				TermcapSourceResolver.Resolve(
					provider,
					"child"
				)
		);
	}

	private static TermcapSourceResolvedField GetRequiredField(
		TermcapSourceResolvedEntry entry,
		string capabilityName
	) {
		ArgumentNullException.ThrowIfNull( entry );
		ArgumentException.ThrowIfNullOrWhiteSpace( capabilityName );

		Assert.True(
			entry.TryGetField(
				capabilityName,
				out TermcapSourceResolvedField? field
			)
		);
		return Assert.IsType<TermcapSourceResolvedField>( field );
	}

	private static TermcapSourceDocument ParseDocument(
		string source,
		string? sourceName = null
	) {
		ArgumentNullException.ThrowIfNull( source );

		TermcapSourceParseResult result =
			TermcapSourceParser.Parse(
				source,
				sourceName
			);
		Assert.False( result.HasErrors );
		return result.Document;
	}

	private sealed class DictionaryEntryProvider : ITermcapSourceEntryProvider
	{
		private readonly IReadOnlyDictionary<string, TermcapSourceEntry> _entries;

		internal DictionaryEntryProvider(
			IEnumerable<TermcapSourceEntry> entries
		) {
			ArgumentNullException.ThrowIfNull( entries );

			Dictionary<string, TermcapSourceEntry> dictionary =
				new( StringComparer.Ordinal );
			foreach ( TermcapSourceEntry entry in entries ) {
				foreach ( string name in entry.Names ) {
					dictionary.TryAdd( name, entry );
				}
			}
			_entries = dictionary;
		}

		internal List<string> RequestedNames { get; } = [];

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermcapSourceEntry? entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			RequestedNames.Add( name );
			return _entries.TryGetValue(
				name,
				out entry
			);
		}
	}

	private sealed class InvalidProvider : ITermcapSourceEntryProvider
	{
		private readonly TermcapSourceEntry _entry;

		internal InvalidProvider(
			TermcapSourceEntry entry
		) {
			ArgumentNullException.ThrowIfNull( entry );
			_entry = entry;
		}

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TermcapSourceEntry? entry
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			entry = _entry;
			return false;
		}
	}
}
