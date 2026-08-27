using System.Buffers.Binary;
using System.Globalization;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C07RoundTripDifferentialTests {
	[Theory]
	[InlineData( "t29-legacy-minimal", 0x011A )]
	[InlineData( "t29-legacy-alignment", 0x011A )]
	[InlineData( "t29-legacy-edge", 0x011A )]
	[InlineData( "t29-extended", 0x011A )]
	[InlineData( "t29-extended32", 0x021E )]
	public void T29SourceCompiler_MatchesPinnedTicSemantics(
		string fixtureName,
		int expectedMagic
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( fixtureName );

		string repositoryRoot = FindRepositoryRoot();
		string sourcePath =
			GetCorpusPath(
				repositoryRoot,
				"source",
				fixtureName + ".ti"
			);
		string compiledPath =
			GetCorpusPath(
				repositoryRoot,
				"compiled",
				fixtureName + ".bin"
			);
		string source =
			File.ReadAllText( sourcePath );

		TermInfoSourceCompilationResult firstResult =
			TermInfoSourceCompiler.Compile(
				source,
				sourcePath
			);
		TermInfoSourceCompilationResult secondResult =
			TermInfoSourceCompiler.Compile(
				source,
				sourcePath
			);

		Assert.False( firstResult.HasErrors );
		Assert.False( secondResult.HasErrors );

		byte[] first =
			Assert.Single( firstResult.Entries ).Data;
		byte[] second =
			Assert.Single( secondResult.Entries ).Data;

		Assert.Equal( first, second );
		Assert.Equal(
			(ushort)expectedMagic,
			BinaryPrimitives.ReadUInt16LittleEndian(
				first.AsSpan(
					0,
					sizeof( ushort )
				)
			)
		);

		TerminalDescription actual =
			CompiledTermInfoParser.Parse( first );
		TerminalDescription expected =
			CompiledTermInfoParser.Parse(
				File.ReadAllBytes( compiledPath )
			);

		AssertSemanticallyEquivalent(
			expected,
			actual
		);

		TerminalDescription rewritten =
			CompiledTermInfoParser.Parse(
				CompiledTermInfoWriter.Write(
					expected
				)
			);
		AssertSemanticallyEquivalent(
			expected,
			rewritten
		);
	}

	[Fact]
	public void Writer_IsDeterministicAcrossExtendedInsertionOrderAndCulture() {
		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture =
			CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "tr-TR" );
			byte[] first =
				CompiledTermInfoWriter.Write(
					CreateDeterminismDescription(
						reverseExtendedInsertion: false
					)
				);

			CultureInfo.CurrentCulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture =
				CultureInfo.GetCultureInfo( "fr-FR" );
			byte[] second =
				CompiledTermInfoWriter.Write(
					CreateDeterminismDescription(
						reverseExtendedInsertion: true
					)
				);

			Assert.Equal( first, second );

			TerminalDescription parsed =
				CompiledTermInfoParser.Parse( first );
			Assert.Equal(
				(int?)100_000,
				parsed.GetNumber(
					NumericCapability.Columns
				)
			);
			Assert.Equal(
				"caf\u00e9",
				parsed.ExtendedCapabilities[
					"AlphaString"
				].StringValue
			);
		}
		finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void T29SourceCompiler_DatabaseOutput_IsConsumedWithoutSemanticLoss() {
		string repositoryRoot = FindRepositoryRoot();
		string sourcePath =
			GetCorpusPath(
				repositoryRoot,
				"source",
				"t29-extended32.ti"
			);
		string source =
			File.ReadAllText( sourcePath );
		string outputRoot = CreateTemporaryRoot();

		try {
			TermInfoSourceCompilationResult result =
				TermInfoSourceCompiler.Compile(
					source,
					sourcePath
				);
			Assert.False( result.HasErrors );

			CompiledTermInfoSourceEntry entry =
				Assert.Single( result.Entries );
			TerminalDescription expected =
				CompiledTermInfoParser.Parse(
					entry.Data
				);

			CompiledTermInfoDatabaseWriter.Write(
				outputRoot,
				result
			);

			DirectoryTerminalDescriptionProvider provider =
				new( outputRoot );
			Assert.True(
				provider.TryLoad(
					entry.CanonicalName,
					out TerminalDescription? canonical
				)
			);
			Assert.NotNull( canonical );
			AssertSemanticallyEquivalent(
				expected,
				canonical
			);

			string aliasName =
				Assert.Single( entry.Aliases );
			Assert.True(
				provider.TryLoad(
					aliasName,
					out TerminalDescription? alias
				)
			);
			Assert.NotNull( alias );
			AssertSemanticallyEquivalent(
				expected,
				alias
			);
		}
		finally {
			DeleteTemporaryRoot( outputRoot );
		}
	}

	private static TerminalDescription CreateDeterminismDescription(
		bool reverseExtendedInsertion
	) {
		TerminalDescriptionBuilder builder =
			new TerminalDescriptionBuilder( "c07-deterministic" )
				.AddAlias( "c07-det" )
				.SetDescription( "C07 deterministic terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber(
					NumericCapability.Columns,
					100_000
				)
				.SetString(
					StringCapability.ClearScreen,
					"\u001b[H\u001b[2J"
				);

		if ( reverseExtendedInsertion ) {
			builder
				.SetExtendedString(
					"ZuluString",
					"omega"
				)
				.SetExtendedString(
					"AlphaString",
					"caf\u00e9"
				)
				.SetExtendedNumber(
					"ZuluNumber",
					200_000
				)
				.SetExtendedNumber(
					"AlphaNumber",
					12345
				)
				.SetExtendedBoolean( "ZuluBoolean" )
				.SetExtendedBoolean( "AlphaBoolean" );
		}
		else {
			builder
				.SetExtendedBoolean( "AlphaBoolean" )
				.SetExtendedBoolean( "ZuluBoolean" )
				.SetExtendedNumber(
					"AlphaNumber",
					12345
				)
				.SetExtendedNumber(
					"ZuluNumber",
					200_000
				)
				.SetExtendedString(
					"AlphaString",
					"caf\u00e9"
				)
				.SetExtendedString(
					"ZuluString",
					"omega"
				);
		}

		return builder.Build();
	}

	private static void AssertSemanticallyEquivalent(
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
		Assert.Equal<BooleanCapability>(
			expected.BooleanCapabilities,
			actual.BooleanCapabilities
		);
		Assert.Equal<KeyValuePair<NumericCapability, int>>(
			expected.NumericCapabilities,
			actual.NumericCapabilities
		);
		Assert.Equal<KeyValuePair<StringCapability, string>>(
			expected.StringCapabilities,
			actual.StringCapabilities
		);

		string[] expectedExtendedNames =
			expected.ExtendedCapabilities.Keys
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();
		string[] actualExtendedNames =
			actual.ExtendedCapabilities.Keys
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();

		Assert.Equal(
			expectedExtendedNames,
			actualExtendedNames
		);
		foreach ( string name in expectedExtendedNames ) {
			Assert.Equal(
				expected.ExtendedCapabilities[name],
				actual.ExtendedCapabilities[name]
			);
		}
	}

	private static string GetCorpusPath(
		string repositoryRoot,
		string area,
		string fileName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( repositoryRoot );
		ArgumentException.ThrowIfNullOrWhiteSpace( area );
		ArgumentException.ThrowIfNullOrWhiteSpace( fileName );

		return Path.Combine(
			repositoryRoot,
			"tests",
			"Icod.TermInfo.Tests",
			"fixtures",
			"compiled-terminfo",
			area,
			fileName
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

	private static string CreateTemporaryRoot() {
		return Path.Combine(
			Path.GetTempPath(),
			"Icod.TermInfo.Compiler.C07."
				+ Guid.NewGuid().ToString( "N" )
		);
	}

	private static void DeleteTemporaryRoot(
		string root
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );

		if ( Directory.Exists( root ) ) {
			Directory.Delete(
				root,
				recursive: true
			);
		}
	}
}
