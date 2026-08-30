using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I07ValidationTests {
	private static readonly TerminalDescription[] BuiltInProfiles = [
		TerminalProfiles.Dumb,
		TerminalProfiles.Ansi,
		TerminalProfiles.Vt100,
		TerminalProfiles.Vt102,
		TerminalProfiles.Vt220,
		TerminalProfiles.Xterm,
		TerminalProfiles.Xterm16Color,
		TerminalProfiles.Xterm88Color,
		TerminalProfiles.Xterm256Color,
		TerminalProfiles.XtermDirect,
		TerminalProfiles.XtermDirect16,
		TerminalProfiles.XtermDirect256,
		TerminalProfiles.WinConsole,
		TerminalProfiles.MsTerminal,
		TerminalProfiles.MsTerminalDirect,
	];

	private static readonly string[] T29CompiledFixtures = [
		"t29-extended.bin",
		"t29-extended32.bin",
		"t29-legacy-alignment.bin",
		"t29-legacy-edge.bin",
		"t29-legacy-minimal.bin",
	];

	[Fact]
	public void EffectiveRenderer_CompilerRoundTrip_PreservesBuiltInsAndT29Corpus() {
		foreach ( TerminalDescription description in BuiltInProfiles ) {
			AssertCompilerRoundTrip(
				description,
				"built-in-" + description.Name
			);
		}

		string repositoryRoot =
			FindRepositoryRoot();
		string fixtureRoot =
			Path.Combine(
				repositoryRoot,
				"tests",
				"Icod.TermInfo.Tests",
				"fixtures",
				"compiled-terminfo",
				"compiled"
			);

		foreach ( string fixtureName in T29CompiledFixtures ) {
			TerminalDescription description =
				CompiledTermInfoParser.Parse(
					File.ReadAllBytes(
						Path.Combine(
							fixtureRoot,
							fixtureName
						)
					)
				);
			AssertCompilerRoundTrip(
				description,
				fixtureName
			);
		}
	}

	[Fact]
	public void SourceCorpus_NormalizedRenderer_PreservesStructureAndResolution() {
		string repositoryRoot =
			FindRepositoryRoot();
		string fixtureRoot =
			Path.Combine(
				repositoryRoot,
				"tests",
				"Icod.TermInfo.Source.Tests",
				"fixtures",
				"source-terminfo",
				"valid"
			);
		string[] fixturePaths =
			Directory.GetFiles(
				fixtureRoot,
				"*.ti",
				SearchOption.TopDirectoryOnly
			)
			.OrderBy(
				path => path,
				StringComparer.Ordinal
			)
			.ToArray();

		Assert.NotEmpty( fixturePaths );

		foreach ( string fixturePath in fixturePaths ) {
			TermInfoSourceParseResult parsed =
				TermInfoSourceParser.Parse(
					File.ReadAllText( fixturePath ),
					fixturePath
				);
			Assert.False( parsed.HasErrors );

			string rendered =
				TermInfoSourceRenderer.Render(
					parsed.Document
				);
			Assert.DoesNotContain( '\r', rendered );

			TermInfoSourceParseResult reparsed =
				TermInfoSourceParser.Parse(
					rendered,
					fixturePath + ".normalized"
				);
			Assert.False( reparsed.HasErrors );

			TermInfoComparisonResult sourceComparison =
				TermInfoSourceComparer.Compare(
					parsed.Document,
					reparsed.Document
				);
			Assert.True(
				sourceComparison.AreEqual,
				$"Normalized source differed structurally for '{Path.GetFileName( fixturePath )}'."
			);

			foreach ( TermInfoSourceEntry entry in parsed.Document.Entries ) {
				TermInfoSourceResolveResult original =
					TermInfoSourceResolver.Resolve(
						parsed.Document,
						entry.CanonicalName
					);
				TermInfoSourceResolveResult normalized =
					TermInfoSourceResolver.Resolve(
						reparsed.Document,
						entry.CanonicalName
					);

				Assert.False( original.HasErrors );
				Assert.False( normalized.HasErrors );
				Assert.NotNull( original.Entry );
				Assert.NotNull( normalized.Entry );

				TermInfoComparisonResult effectiveComparison =
					TerminalDescriptionComparer.Compare(
						original.Entry!.ToTerminalDescription(),
						normalized.Entry!.ToTerminalDescription()
					);
				Assert.True(
					effectiveComparison.AreEqual,
					$"Normalized source resolved differently for '{entry.CanonicalName}'."
				);
			}
		}
	}

	[Fact]
	public void EffectiveRenderer_WrapBoundary_IsStableAtEightyColumns() {
		TerminalDescription fitsExactly =
			new TerminalDescriptionBuilder( "i07-wrap-71" )
				.SetDescription( "I07 wrap boundary" )
				.SetString(
					StringCapability.Bell,
					new string( 'x', 71 )
				)
				.Build();
		TerminalDescription wrapsNextCharacter =
			new TerminalDescriptionBuilder( "i07-wrap-72" )
				.SetDescription( "I07 wrap boundary" )
				.SetString(
					StringCapability.Bell,
					new string( 'x', 72 )
				)
				.Build();

		string[] exactLines =
			TerminalDescriptionSourceRenderer.Render(
				fitsExactly
			)
			.Split( '\n' );
		string[] wrappedLines =
			TerminalDescriptionSourceRenderer.Render(
				wrapsNextCharacter
			)
			.Split( '\n' );

		Assert.Equal( 80, exactLines[ 1 ].Length );
		Assert.StartsWith( "    bel=", exactLines[ 1 ] );
		Assert.EndsWith( ",", exactLines[ 1 ] );
		Assert.Equal( 3, exactLines.Length );

		Assert.Equal( 79, wrappedLines[ 1 ].Length );
		Assert.Equal( "        x,", wrappedLines[ 2 ] );
		Assert.Equal( 4, wrappedLines.Length );
		Assert.All(
			wrappedLines,
			line => Assert.True( line.Length <= 80 )
		);
	}

	[Fact]
	public void Comparison_IsDeterministicAcrossCultureAndExtendedInsertionOrder() {
		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture =
			CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			string[] first =
				DescribeDifferences(
					TerminalDescriptionComparer.Compare(
						CreateComparisonDescription(
							left: true,
							reverseExtendedInsertion: false
						),
						CreateComparisonDescription(
							left: false,
							reverseExtendedInsertion: false
						)
					)
				);

			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			string[] second =
				DescribeDifferences(
					TerminalDescriptionComparer.Compare(
						CreateComparisonDescription(
							left: true,
							reverseExtendedInsertion: true
						),
						CreateComparisonDescription(
							left: false,
							reverseExtendedInsertion: true
						)
					)
				);

			Assert.NotEmpty( first );
			Assert.Equal( first, second );
			Assert.Contains(
				first,
				item => item.Contains(
					"DifferentValueKind|kindMismatch|",
					StringComparison.Ordinal
				)
			);
			Assert.Contains(
				first,
				item => item.Contains(
					"|Feature|",
					StringComparison.Ordinal
				)
			);
			Assert.Contains(
				first,
				item => item.Contains(
					"|feature|",
					StringComparison.Ordinal
				)
			);
		}
		finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void SourceComparison_SequenceStress_IsDeterministicAndStateAware() {
		const string leftSource =
			"i07-parent|I07 parent,am,cols#80,\n"
			+ "i07-sequence|I07 sequence,\n"
			+ "    clear=\\E[H,\n"
			+ "    clear@,\n"
			+ "    .cup=\\E[%i%p1%d;%p2%dH,\n"
			+ "    Vendor=one,\n"
			+ "    use=i07-parent,\n"
			+ "    Vendor=two,\n"
			+ "    use=i07-parent,\n";
		const string rightSource =
			"i07-parent|I07 parent,am,cols#80,\n"
			+ "i07-sequence|I07 sequence,\n"
			+ "    clear@,\n"
			+ "    clear=\\E[H,\n"
			+ "    .cup=\\E[%i%p1%d;%p2%dH,\n"
			+ "    Vendor=two,\n"
			+ "    use=i07-parent,\n"
			+ "    Vendor=one,\n"
			+ "    use=i07-parent,\n";

		TermInfoSourceParseResult left =
			TermInfoSourceParser.Parse(
				leftSource,
				"i07-left-sequence.ti"
			);
		TermInfoSourceParseResult right =
			TermInfoSourceParser.Parse(
				rightSource,
				"i07-right-sequence.ti"
			);
		Assert.False( left.HasErrors );
		Assert.False( right.HasErrors );

		TermInfoSourceEntry leftEntry = left.Document.Entries[ 1 ];
		TermInfoSourceEntry rightEntry = right.Document.Entries[ 1 ];

		Assert.Contains(
			leftEntry.Fields,
			field => field.Kind == TermInfoSourceFieldKind.CancelledCapability
		);
		Assert.Contains(
			leftEntry.Fields,
			field => field.Kind == TermInfoSourceFieldKind.DisabledCapability
		);
		Assert.Equal(
			2,
			leftEntry.Fields.Count(
				field => field.Kind == TermInfoSourceFieldKind.UseReference
			)
		);
		Assert.Equal(
			2,
			leftEntry.Fields.Count(
				field => field.CapabilityName == "Vendor"
			)
		);

		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			string[] first =
				DescribeSourceDifferences(
					TermInfoSourceComparer.Compare(
						leftEntry,
						rightEntry
					)
				);

			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			string[] second =
				DescribeSourceDifferences(
					TermInfoSourceComparer.Compare(
						leftEntry,
						rightEntry
					)
				);

			Assert.NotEmpty( first );
			Assert.Equal( first, second );
			Assert.Contains(
				first,
				item => item.Contains(
					"SourceFieldKind",
					StringComparison.Ordinal
				)
					|| item.Contains(
						"SourceFieldValue",
						StringComparison.Ordinal
					)
			);
		}
		finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	[Fact]
	public void PackageFreeze_RetainsApisDependencyGraphAndPublishingBoundary() {
		string root =
			FindRepositoryRoot();
		XDocument inspectionProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Inspection",
					"Icod.TermInfo.Inspection.csproj"
				),
				LoadOptions.None
			);
		XDocument inspectionTests =
			XDocument.Load(
				Path.Combine(
					root,
					"tests",
					"Icod.TermInfo.Inspection.Tests",
					"Icod.TermInfo.Inspection.Tests.csproj"
				),
				LoadOptions.None
			);

		Assert.Equal(
			new[] {
				@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
				@"..\Icod.TermInfo.csproj",
			},
			ReadProjectReferences( inspectionProject )
		);
		Assert.Equal(
			new[] {
				@"..\..\Icod.TermInfo.Compiler\Icod.TermInfo.Compiler.csproj",
				@"..\..\Icod.TermInfo.Inspection\Icod.TermInfo.Inspection.csproj",
			},
			ReadProjectReferences( inspectionTests )
		);

		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt"
				)
			);
		Assert.DoesNotContain(
			"Icod.TermInfo.Compiler",
			baseline
		);

		foreach (
			string relativePath
			in new[] {
				".github/scripts/verify-release-package.cmd",
				".github/scripts/verify-release-package.sh",
			}
		) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				);

			Assert.Contains(
				"1.4.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier
			);
			foreach ( string framework in new[] { "net8.0", "net9.0", "net10.0" } ) {
				Assert.Contains( framework, verifier );
			}
			foreach (
				string smoke
				in new[] {
					"package-smoke",
					"source-package-smoke",
					"compiler-package-smoke",
					"inspection-package-smoke",
				}
			) {
				Assert.Contains( smoke, verifier );
			}
		}

		string pullRequest =
			ReadRepositoryFile(
				root,
				".github/workflows/pr-build-and-test.yaml"
			);
		string pushMain =
			ReadRepositoryFile(
				root,
				".github/workflows/push-main.yaml"
			);
		string release =
			ReadRepositoryFile(
				root,
				".github/workflows/release.yaml"
			);

		Assert.DoesNotContain( "dotnet nuget push", pullRequest );
		Assert.DoesNotContain( "dotnet nuget push", pushMain );
		Assert.Contains( "dotnet nuget push", release );
		Assert.Contains( "if (15 -ne $files.Count)", release );
		Assert.Contains( "if (16 -ne $assets.Count)", release );
		foreach (
			string packageId
			in new[] {
				"Icod.TermInfo",
				"Icod.TermInfo.Source",
				"Icod.TermInfo.Compiler",
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Tools",
			}
		) {
			Assert.Contains( packageId, release );
		}

		Assert.True(
			File.Exists(
				Path.Combine(
					root,
					"docs",
					"1.3.0-I07-DIFFERENTIAL-VALIDATION-AND-FREEZE.md"
				)
			)
		);
	}

	private static void AssertCompilerRoundTrip(
		TerminalDescription expected,
		string sourceName
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );

		string rendered =
			TerminalDescriptionSourceRenderer.Render(
				expected
			);
		TermInfoSourceCompilationResult compiled =
			TermInfoSourceCompiler.Compile(
				rendered,
				"i07-" + sourceName + ".ti"
			);

		Assert.False( compiled.HasErrors );
		CompiledTermInfoSourceEntry entry =
			Assert.Single( compiled.Entries );
		TerminalDescription actual =
			CompiledTermInfoParser.Parse(
				entry.Data
			);
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				expected,
				actual
			);

		Assert.True(
			comparison.AreEqual,
			$"Compiler-backed Inspection round trip changed '{expected.Name}'."
		);
	}

	private static TerminalDescription CreateComparisonDescription(
		bool left,
		bool reverseExtendedInsertion
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder(
				left
					? "i07-left"
					: "i07-right"
			)
				.SetDescription(
					left
						? "I07 left comparison"
						: "I07 right comparison"
				)
				.AddAlias(
					left
						? "i07-left-alias"
						: "i07-right-alias"
				)
				.SetNumber(
					NumericCapability.Columns,
					left ? 80 : 132
				);

		if ( left ) {
			builder.SetBoolean( BooleanCapability.AutoRightMargin );
		}
		else {
			builder.SetString( StringCapability.Bell, "right" );
		}

		Action addFirst =
			() => {
				if ( left ) {
					builder
						.SetExtendedBoolean( "Feature" )
						.SetExtendedNumber( "kindMismatch", 7 )
						.SetExtendedNumber( "numberValue", 11 );
				}
				else {
					builder
						.SetExtendedBoolean( "feature" )
						.SetExtendedString( "kindMismatch", "7" )
						.SetExtendedNumber( "numberValue", 13 );
				}
			};
		Action addSecond =
			() => {
				builder.SetExtendedString(
					"stringValue",
					left ? "left" : "right"
				);
			};

		if ( reverseExtendedInsertion ) {
			addSecond();
			addFirst();
		}
		else {
			addFirst();
			addSecond();
		}

		return builder.Build();
	}

	private static string[] DescribeDifferences(
		TermInfoComparisonResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		return result.Differences
			.Select(
				difference =>
					string.Join(
						"|",
						difference.Kind,
						difference.CapabilityName ?? string.Empty,
						difference.IsExtendedCapability?.ToString() ?? string.Empty,
						difference.LeftText ?? string.Empty,
						difference.RightText ?? string.Empty,
						DescribeAliases( difference.LeftAliases ),
						DescribeAliases( difference.RightAliases ),
						DescribeCapabilityValue( difference.LeftCapabilityValue ),
						DescribeCapabilityValue( difference.RightCapabilityValue )
					)
			)
			.ToArray();
	}

	private static string[] DescribeSourceDifferences(
		TermInfoComparisonResult result
	) {
		ArgumentNullException.ThrowIfNull( result );

		return result.Differences
			.Select(
				difference =>
					string.Join(
						"|",
						difference.Kind,
						difference.CapabilityName ?? string.Empty,
						FormatNullableInteger( difference.LeftSourceEntryIndex ),
						FormatNullableInteger( difference.RightSourceEntryIndex ),
						FormatNullableInteger( difference.LeftSourceFieldIndex ),
						FormatNullableInteger( difference.RightSourceFieldIndex ),
						difference.LeftSourceField?.Kind.ToString() ?? string.Empty,
						difference.RightSourceField?.Kind.ToString() ?? string.Empty,
						difference.LeftSourceField?.CapabilityName ?? string.Empty,
						difference.RightSourceField?.CapabilityName ?? string.Empty,
						difference.LeftSourceField?.ReferenceName ?? string.Empty,
						difference.RightSourceField?.ReferenceName ?? string.Empty,
						FormatNullableInteger( difference.LeftSourceField?.NumericValue ),
						FormatNullableInteger( difference.RightSourceField?.NumericValue ),
						difference.LeftSourceField?.StringValue ?? string.Empty,
						difference.RightSourceField?.StringValue ?? string.Empty
					)
			)
			.ToArray();
	}

	private static string DescribeAliases(
		IReadOnlyList<string>? aliases
	) {
		return aliases is null
			? string.Empty
			: string.Join( ",", aliases );
	}

	private static string DescribeCapabilityValue(
		TermInfoCapabilityValue? value
	) {
		if ( !value.HasValue ) {
			return string.Empty;
		}

		TermInfoCapabilityValue actual =
			value.Value;
		return actual.Kind switch {
			TermInfoCapabilityValueKind.Boolean =>
				actual.BooleanValue ? "bool:1" : "bool:0",
			TermInfoCapabilityValueKind.Number =>
				"num:"
					+ actual.NumberValue.ToString(
						CultureInfo.InvariantCulture
					),
			TermInfoCapabilityValueKind.String =>
				"str:"
					+ Convert.ToHexString(
						Encoding.UTF8.GetBytes(
							actual.StringValue
						)
					),
			_ => throw new InvalidOperationException(
				$"Unsupported capability value kind '{actual.Kind}'."
			),
		};
	}

	private static string FormatNullableInteger(
		int? value
	) {
		return value.HasValue
			? value.Value.ToString( CultureInfo.InvariantCulture )
			: string.Empty;
	}

	private static string[] ReadProjectReferences(
		XDocument project
	) {
		ArgumentNullException.ThrowIfNull( project );

		return project
			.Descendants()
			.Where(
				element => element.Name.LocalName == "ProjectReference"
			)
			.Select(
				element =>
					element.Attribute( "Include" )?.Value
					?? string.Empty
			)
			.OrderBy(
				value => value,
				StringComparer.Ordinal
			)
			.ToArray();
	}

	private static string ReadRepositoryFile(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return File.ReadAllText(
			Path.Combine(
				root,
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar
				)
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

			current = current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
