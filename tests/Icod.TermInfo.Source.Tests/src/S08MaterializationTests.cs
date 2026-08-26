using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S08MaterializationTests {
	[Fact]
	public void MaterializesResolvedIdentityAndInheritanceIntoRuntimeModel() {
		TermInfoSourceDocument document =
			ParseDocument(
				"parent|parent-alias|S08 parent,\n"
				+ "\tam,\n"
				+ "\tlines#24,\n"
				+ "\tclear=parent-clear,\n"
				+ "\tVendor=parent,\n"
				+ "child|child-alias|S08 child,\n"
				+ "\tcols#132,\n"
				+ "\tuse=parent,\n"
			);

		TermInfoSourceResolvedEntry resolved =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					document,
					"child"
				)
			);
		TerminalDescription description =
			resolved.ToTerminalDescription();

		Assert.Equal( "child", description.Name );
		Assert.Equal( "S08 child", description.Description );
		Assert.Equal(
			new[] { "child-alias" },
			description.Aliases
		);
		Assert.True(
			description.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Equal(
			132,
			description.GetNumber( NumericCapability.Columns )
		);
		Assert.Equal(
			24,
			description.GetNumber( NumericCapability.Lines )
		);
		Assert.Equal(
			"parent-clear",
			description.GetString( StringCapability.ClearScreen )
		);
		Assert.True(
			description.TryGetExtendedString(
				"Vendor",
				out string? vendor
			)
		);
		Assert.Equal( "parent", vendor );
	}

	[Fact]
	public void CancellationTombstonesMaterializeAsAbsence() {
		TermInfoSourceDocument document =
			ParseDocument(
				"parent|S08 cancellation parent,\n"
				+ "\tam,\n"
				+ "\tcols#80,\n"
				+ "\tclear=parent,\n"
				+ "\tXBool,\n"
				+ "\tXNum#7,\n"
				+ "\tXStr=parent,\n"
				+ "child|S08 cancellation child,\n"
				+ "\tam@,\n"
				+ "\tcols@,\n"
				+ "\tclear@,\n"
				+ "\tXBool@,\n"
				+ "\tXNum@,\n"
				+ "\tXStr@,\n"
				+ "\tuse=parent,\n"
			);

		TerminalDescription description =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					document,
					"child"
				)
			)
			.ToTerminalDescription();

		Assert.False(
			description.GetBoolean( BooleanCapability.AutoRightMargin )
		);
		Assert.Null(
			description.GetNumber( NumericCapability.Columns )
		);
		Assert.Null(
			description.GetString( StringCapability.ClearScreen )
		);
		Assert.False(
			description.TryGetExtendedCapability(
				"XBool",
				out _
			)
		);
		Assert.False(
			description.TryGetExtendedCapability(
				"XNum",
				out _
			)
		);
		Assert.False(
			description.TryGetExtendedCapability(
				"XStr",
				out _
			)
		);
		Assert.Empty( description.ExtendedCapabilities );
	}

	[Theory]
	[InlineData( "t29-legacy-minimal" )]
	[InlineData( "t29-legacy-alignment" )]
	[InlineData( "t29-legacy-edge" )]
	[InlineData( "t29-extended" )]
	[InlineData( "t29-extended32" )]
	public void SourceAndCompiledFixturesProduceEquivalentRuntimeDescriptions(
		string fixtureName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( fixtureName );

		string root = FindRepositoryRoot();
		string fixtureRoot =
			Path.Combine(
				root,
				"tests",
				"Icod.TermInfo.Tests",
				"fixtures",
				"compiled-terminfo"
			);
		string sourcePath =
			Path.Combine(
				fixtureRoot,
				"source",
				fixtureName + ".ti"
			);
		string compiledPath =
			Path.Combine(
				fixtureRoot,
				"compiled",
				fixtureName + ".bin"
			);

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				File.ReadAllText( sourcePath ),
				sourcePath
			);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		TermInfoSourceEntry sourceEntry =
			Assert.Single( parsed.Document.Entries );

		TermInfoSourceResolvedEntry resolved =
			AssertResolved(
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					sourceEntry.CanonicalName
				)
			);
		TerminalDescription sourceDescription =
			resolved.ToTerminalDescription();
		TerminalDescription compiledDescription =
			CompiledTermInfoParser.Parse(
				File.ReadAllBytes( compiledPath )
			);

		AssertEquivalent(
			compiledDescription,
			sourceDescription
		);
	}

	private static TermInfoSourceDocument ParseDocument(
		string source
	) {
		ArgumentNullException.ThrowIfNull( source );

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source,
				"s08.ti"
			);
		Assert.False(
			parsed.HasErrors,
			FormatDiagnostics( parsed.Diagnostics )
		);
		return parsed.Document;
	}

	private static TermInfoSourceResolvedEntry AssertResolved(
		TermInfoSourceResolveResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		Assert.False(
			result.HasErrors,
			FormatDiagnostics( result.Diagnostics )
		);
		Assert.Empty( result.Diagnostics );
		return Assert.IsType<TermInfoSourceResolvedEntry>( result.Entry );
	}

	private static void AssertEquivalent(
		TerminalDescription expected,
		TerminalDescription actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		Assert.Equal( expected.Name, actual.Name );
		Assert.Equal( expected.Description, actual.Description );
		Assert.Equal(
			expected.Aliases.ToArray(),
			actual.Aliases.ToArray()
		);
		Assert.Equal(
			expected.BooleanCapabilities.ToArray(),
			actual.BooleanCapabilities.ToArray()
		);
		Assert.Equal(
			expected.NumericCapabilities.ToArray(),
			actual.NumericCapabilities.ToArray()
		);
		Assert.Equal(
			expected.StringCapabilities.ToArray(),
			actual.StringCapabilities.ToArray()
		);

		KeyValuePair<string, TermInfoCapabilityValue>[] expectedExtended =
			expected.ExtendedCapabilities
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.ToArray();
		KeyValuePair<string, TermInfoCapabilityValue>[] actualExtended =
			actual.ExtendedCapabilities
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.ToArray();
		Assert.Equal(
			expectedExtended,
			actualExtended
		);
	}

	private static string FormatDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return string.Join(
			"; ",
			diagnostics.Select(
				diagnostic =>
					diagnostic.Code
						+ " "
						+ diagnostic.Message
			)
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
				Path.Combine(
					current.FullName,
					"Icod.TermInfo.sln"
				)
			) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
