/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HW01 Hash-v9 writer public contract.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw01WriterContractTests {
	private const string NamespacePrefix =
		"Icod.TermInfo.BerkeleyDb."
	;

	[Fact]
	public void WriterExportsExactlyTheApprovedThreeTypeAddition() {
		Assembly assembly = LoadBerkeleyDbAssembly();
		string[] writerTypes = assembly.GetExportedTypes()
			.Select( static type => type.FullName )
			.Where( static name =>
				name is not null
				&& name.StartsWith(
					"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabase",
					StringComparison.Ordinal
				)
			)
			.Select( static name => name! )
			.OrderBy( static name => name, StringComparer.Ordinal )
			.ToArray();

		Assert.Equal(
			new[] {
				NamespacePrefix + "BerkeleyDbTerminalDatabaseEntry",
				NamespacePrefix + "BerkeleyDbTerminalDatabaseWriter",
				NamespacePrefix + "BerkeleyDbTerminalDatabaseWriterOptions",
			},
			writerTypes
		);
	}

	[Fact]
	public void WriterMembersMatchTheApprovedContract() {
		Assembly assembly = LoadBerkeleyDbAssembly();
		Type entryType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseEntry"
		);
		Type optionsType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseWriterOptions"
		);
		Type writerType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseWriter"
		);

		Assert.True( entryType.IsSealed );
		Assert.False( entryType.IsAbstract );
		ConstructorInfo entryConstructor = Assert.Single(
			entryType.GetConstructors()
		);
		ParameterInfo[] entryParameters =
			entryConstructor.GetParameters();
		Assert.Equal( 3, entryParameters.Length );
		AssertParameter(
			entryParameters[0],
			"canonicalName",
			typeof( string ),
			hasDefaultValue: false,
			expectedDefaultValue: null
		);
		AssertParameter(
			entryParameters[1],
			"aliases",
			typeof( IEnumerable<string> ),
			hasDefaultValue: false,
			expectedDefaultValue: null
		);
		AssertParameter(
			entryParameters[2],
			"data",
			typeof( byte[] ),
			hasDefaultValue: false,
			expectedDefaultValue: null
		);
		AssertProperties(
			entryType,
			new Dictionary<string, Type>( StringComparer.Ordinal ) {
				["Aliases"] = typeof( IReadOnlyList<string> ),
				["CanonicalName"] = typeof( string ),
				["Data"] = typeof( byte[] ),
			}
		);

		Assert.True( optionsType.IsSealed );
		Assert.False( optionsType.IsAbstract );
		ConstructorInfo optionsConstructor = Assert.Single(
			optionsType.GetConstructors()
		);
		ParameterInfo[] optionParameters =
			optionsConstructor.GetParameters();
		Assert.Equal( 4, optionParameters.Length );
		AssertParameter(
			optionParameters[0],
			"parserOptions",
			typeof( CompiledTermInfoParserOptions ),
			hasDefaultValue: true,
			expectedDefaultValue: null
		);
		AssertParameter(
			optionParameters[1],
			"maximumDatabaseSize",
			typeof( int ),
			hasDefaultValue: true,
			expectedDefaultValue: 67_108_864
		);
		AssertParameter(
			optionParameters[2],
			"maximumRecordCount",
			typeof( int ),
			hasDefaultValue: true,
			expectedDefaultValue: 65_536
		);
		AssertParameter(
			optionParameters[3],
			"overwriteExisting",
			typeof( bool ),
			hasDefaultValue: true,
			expectedDefaultValue: false
		);
		AssertProperties(
			optionsType,
			new Dictionary<string, Type>( StringComparer.Ordinal ) {
				["MaximumDatabaseSize"] = typeof( int ),
				["MaximumRecordCount"] = typeof( int ),
				["OverwriteExisting"] = typeof( bool ),
				["ParserOptions"] = typeof( CompiledTermInfoParserOptions ),
			}
		);
		FieldInfo maximumRecordCount = Assert.Single(
			optionsType.GetFields(
				BindingFlags.Public
				| BindingFlags.Static
				| BindingFlags.DeclaredOnly
			)
		);
		Assert.Equal( "DefaultMaximumRecordCount", maximumRecordCount.Name );
		Assert.True( maximumRecordCount.IsLiteral );
		Assert.Equal( 65_536, maximumRecordCount.GetRawConstantValue() );

		Assert.True( writerType.IsAbstract );
		Assert.True( writerType.IsSealed );
		Assert.Empty( writerType.GetConstructors() );
		MethodInfo write = Assert.Single(
			writerType.GetMethods(
				BindingFlags.Public
				| BindingFlags.Static
				| BindingFlags.DeclaredOnly
			)
		);
		Assert.Equal( "Write", write.Name );
		Assert.Equal( typeof( void ), write.ReturnType );
		ParameterInfo[] writeParameters = write.GetParameters();
		Assert.Equal( 4, writeParameters.Length );
		AssertParameter(
			writeParameters[0],
			"databasePath",
			typeof( string ),
			hasDefaultValue: false,
			expectedDefaultValue: null
		);
		AssertParameter(
			writeParameters[1],
			"entries",
			typeof( IEnumerable<> ).MakeGenericType( entryType ),
			hasDefaultValue: false,
			expectedDefaultValue: null
		);
		AssertParameter(
			writeParameters[2],
			"options",
			optionsType,
			hasDefaultValue: true,
			expectedDefaultValue: null
		);
		AssertParameter(
			writeParameters[3],
			"cancellationToken",
			typeof( CancellationToken ),
			hasDefaultValue: true,
			expectedDefaultValue: null
		);
	}

	[Fact]
	public void WriterNullabilityAndScopeRemainNarrow() {
		Assembly assembly = LoadBerkeleyDbAssembly();
		Type entryType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseEntry"
		);
		Type optionsType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseWriterOptions"
		);
		Type writerType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseWriter"
		);

		NullabilityInfoContext nullability = new();
		ParameterInfo[] entryParameters = Assert.Single(
			entryType.GetConstructors()
		).GetParameters();
		Assert.All(
			entryParameters,
			parameter => Assert.Equal(
				NullabilityState.NotNull,
				nullability.Create( parameter ).ReadState
			)
		);

		ParameterInfo[] optionParameters = Assert.Single(
			optionsType.GetConstructors()
		).GetParameters();
		Assert.Equal(
			NullabilityState.Nullable,
			nullability.Create( optionParameters[0] ).ReadState
		);

		MethodInfo write = Assert.Single(
			writerType.GetMethods(
				BindingFlags.Public
				| BindingFlags.Static
				| BindingFlags.DeclaredOnly
			)
		);
		ParameterInfo[] writeParameters = write.GetParameters();
		Assert.Equal(
			NullabilityState.NotNull,
			nullability.Create( writeParameters[0] ).ReadState
		);
		Assert.Equal(
			NullabilityState.NotNull,
			nullability.Create( writeParameters[1] ).ReadState
		);
		Assert.Equal(
			NullabilityState.Nullable,
			nullability.Create( writeParameters[2] ).ReadState
		);

		Assert.DoesNotContain(
			assembly.GetExportedTypes(),
			type =>
				type.Name.Contains( "WriterResult", StringComparison.Ordinal )
				|| type.Name.Contains( "WriterException", StringComparison.Ordinal )
		);
		Assert.DoesNotContain(
			write.GetParameters(),
			parameter => parameter.ParameterType == typeof( byte[] )
		);
		Assert.DoesNotContain(
			writerType.GetMethods(
				BindingFlags.Public
				| BindingFlags.Static
				| BindingFlags.DeclaredOnly
			),
			method => method.Name.Contains( "Put", StringComparison.Ordinal )
		);
	}

	[Fact]
	public void EntryAndOptionsSnapshotMutableInputs() {
		Assembly assembly = LoadBerkeleyDbAssembly();
		Type entryType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseEntry"
		);
		Type optionsType = RequireType(
			assembly,
			"BerkeleyDbTerminalDatabaseWriterOptions"
		);

		string[] aliases = [ "sample-alias" ];
		byte[] data = [ 1, 2, 3 ];
		object entry = Assert.Single( entryType.GetConstructors() ).Invoke(
			new object?[] { "sample", aliases, data }
		);
		aliases[0] = "mutated";
		data[0] = 255;

		Assert.Equal(
			new[] { "sample-alias" },
			Assert.IsAssignableFrom<IEnumerable<string>>(
				entryType.GetProperty( "Aliases" )!.GetValue( entry )
			)
		);
		byte[] first = Assert.IsType<byte[]>(
			entryType.GetProperty( "Data" )!.GetValue( entry )
		);
		Assert.Equal( new byte[] { 1, 2, 3 }, first );
		first[0] = 254;
		byte[] second = Assert.IsType<byte[]>(
			entryType.GetProperty( "Data" )!.GetValue( entry )
		);
		Assert.Equal( new byte[] { 1, 2, 3 }, second );

		CompiledTermInfoParserOptions parserOptions = new( 4_096 );
		object options = Assert.Single( optionsType.GetConstructors() ).Invoke(
			new object?[] { parserOptions, 8_192, 32, true }
		);
		CompiledTermInfoParserOptions parserSnapshot =
			Assert.IsType<CompiledTermInfoParserOptions>(
				optionsType.GetProperty( "ParserOptions" )!.GetValue( options )
			);
		Assert.NotSame( parserOptions, parserSnapshot );
		Assert.Equal( 4_096, parserSnapshot.MaximumEntrySize );
		Assert.Equal(
			8_192,
			Assert.IsType<int>(
				optionsType.GetProperty( "MaximumDatabaseSize" )!.GetValue( options )
			)
		);
		Assert.Equal(
			32,
			Assert.IsType<int>(
				optionsType.GetProperty( "MaximumRecordCount" )!.GetValue( options )
			)
		);
		Assert.True(
			Assert.IsType<bool>(
				optionsType.GetProperty( "OverwriteExisting" )!.GetValue( options )
			)
		);
	}

	[Theory]
	[InlineData( 0, 1 )]
	[InlineData( -1, 1 )]
	[InlineData( 1, 0 )]
	[InlineData( 1, -1 )]
	public void OptionsRejectNonpositiveResourceLimits(
		int maximumDatabaseSize,
		int maximumRecordCount
	) {
		Type optionsType = RequireType(
			LoadBerkeleyDbAssembly(),
			"BerkeleyDbTerminalDatabaseWriterOptions"
		);
		ConstructorInfo constructor = Assert.Single(
			optionsType.GetConstructors()
		);

		TargetInvocationException exception =
			Assert.Throws<TargetInvocationException>(
				() => constructor.Invoke(
					new object?[] {
						null,
						maximumDatabaseSize,
						maximumRecordCount,
						false,
					}
				)
			);

		Assert.IsType<ArgumentOutOfRangeException>( exception.InnerException );
	}

	[Fact]
	public void WriterRejectsNullAndEmptyEntrySequencesBeforeCreatingDestination() {
		ArgumentException nullElement =
			AssertWriteThrowsWithoutDestination<ArgumentException>(
				new BerkeleyDbTerminalDatabaseEntry[] { null! }
			);
		Assert.Equal( "entries", nullElement.ParamName );

		ArgumentException empty =
			AssertWriteThrowsWithoutDestination<ArgumentException>(
				Array.Empty<BerkeleyDbTerminalDatabaseEntry>()
			);
		Assert.Equal( "entries", empty.ParamName );
	}

	[Fact]
	public void WriterRejectsDuplicateCanonicalOwnership() {
		InvalidOperationException exception =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] {
					CreateEntry( "duplicate" ),
					CreateEntry( "duplicate" ),
				}
			);

		Assert.Contains(
			"terminal name",
			exception.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void WriterRejectsRepeatedAliasOwnershipWithinAndAcrossEntries() {
		InvalidOperationException within =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] { CreateEntry( "within", "shared", "shared" ) }
			);
		Assert.Contains(
			"terminal name",
			within.Message,
			StringComparison.OrdinalIgnoreCase
		);

		InvalidOperationException across =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] {
					CreateEntry( "first", "shared" ),
					CreateEntry( "second", "shared" ),
				}
			);
		Assert.Contains(
			"terminal name",
			across.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void WriterRejectsCanonicalAliasOwnershipCollision() {
		InvalidOperationException exception =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] {
					CreateEntry( "canonical" ),
					CreateEntry( "other", "canonical" ),
				}
			);

		Assert.Contains(
			"terminal name",
			exception.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Theory]
	[MemberData( nameof( UnsafeTerminalIdentities ) )]
	public void WriterRejectsUnsafeCanonicalAndAliasIdentitiesOnEveryHost(
		string unsafeIdentity
	) {
		byte[] validData = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"valid",
			"HW01 fixture"
		);
		ArgumentException canonical =
			AssertWriteThrowsWithoutDestination<ArgumentException>(
				new[] {
					new BerkeleyDbTerminalDatabaseEntry(
						unsafeIdentity,
						[],
						validData
					),
				}
			);
		Assert.Equal( "entries", canonical.ParamName );

		ArgumentException alias =
			AssertWriteThrowsWithoutDestination<ArgumentException>(
				new[] {
					new BerkeleyDbTerminalDatabaseEntry(
						"valid",
						[ unsafeIdentity ],
						validData
					),
				}
			);
		Assert.Equal( "entries", alias.ParamName );
	}

	[Fact]
	public void WriterRequiresParsedCanonicalIdentityAgreement() {
		BerkeleyDbTerminalDatabaseEntry entry = new(
			"declared",
			[],
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				"compiled",
				"HW01 fixture"
			)
		);

		InvalidOperationException exception =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] { entry }
			);
		Assert.Contains(
			"canonical",
			exception.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void WriterRequiresParsedAliasOrderAgreement() {
		BerkeleyDbTerminalDatabaseEntry entry = new(
			"ordered",
			[ "first", "second" ],
			Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
				"ordered",
				"HW01 fixture",
				"second",
				"first"
			)
		);

		InvalidOperationException exception =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] { entry }
			);
		Assert.Contains(
			"alias",
			exception.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void WriterPropagatesCompiledEntryFormatFailures() {
		BerkeleyDbTerminalDatabaseEntry entry = new(
			"malformed",
			[],
			[ 1, 2, 3 ]
		);

		AssertWriteThrowsWithoutDestination<CompiledTermInfoFormatException>(
			new[] { entry }
		);
	}

	[Fact]
	public void WriterEnforcesThePhysicalHashRecordLimit() {
		BerkeleyDbTerminalDatabaseWriterOptions options = new(
			maximumRecordCount: 2
		);

		InvalidOperationException exception =
			AssertWriteThrowsWithoutDestination<InvalidOperationException>(
				new[] { CreateEntry( "limited", "alias" ) },
				options
			);
		Assert.Contains(
			"record limit",
			exception.Message,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void WriterHonorsPreCancellationBeforeCreatingDestination() {
		using CancellationTokenSource source = new();
		source.Cancel();

		AssertWriteThrowsWithoutDestination<OperationCanceledException>(
			new[] { CreateEntry( "cancelled" ) },
			cancellationToken: source.Token
		);
	}

	[Fact]
	public void WriterEnumeratesInputOnceWhenPublishingTheCompleteImage() {
		int enumerationCount = 0;
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> Entries() {
			enumerationCount++;
			yield return CreateEntry( "sample", "sample-alias" );
		}

		string directory = Path.Combine(
			Path.GetTempPath(),
			"icod-terminfo-hw04-enumeration-" + Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( directory );
		string destination = Path.Combine( directory, "terminfo.db" );
		try {
			BerkeleyDbTerminalDatabaseWriter.Write( destination, Entries() );
			Assert.Equal( 1, enumerationCount );
			Assert.True( File.Exists( destination ) );
		} finally {
			Directory.Delete( directory, recursive: true );
		}
	}

	[Fact]
	public void ApiBaselineAndPackageVerifierTrackCurrentWriterContract() {
		string root = FindRepositoryRoot();
		string baselinePath = Path.Combine(
			root,
			"docs",
			"1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt"
		);
		Assert.True(
			File.Exists( baselinePath ),
			$"Missing current BerkeleyDb API baseline: {baselinePath}"
		);

		string verifier = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-berkeleydb-package.ps1"
			)
		);
		Assert.Contains(
			"docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt",
			verifier,
			StringComparison.Ordinal
		);
	}

	public static TheoryData<string> UnsafeTerminalIdentities => new() {
		"",
		" ",
		".",
		"..",
		"sample/name",
		@"sample\name",
		"sample\0name",
		"sample<name",
		"sample>name",
		"sample:name",
		"sample\"name",
		"sample|name",
		"sample?name",
		"sample*name",
		"sample.",
		"sample ",
		"CON",
		"con.txt",
		"PRN",
		"AUX",
		"NUL",
		"CLOCK$",
		"COM1",
		"COM9.log",
		"LPT1",
		"LPT9.log",
		"\u0001",
		"\uD800",
	};

	private static Assembly LoadBerkeleyDbAssembly() {
		string assemblyPath = Path.Combine(
			AppContext.BaseDirectory,
			"Icod.TermInfo.BerkeleyDb.dll"
		);
		Assert.True( File.Exists( assemblyPath ) );
		return Assembly.LoadFrom( assemblyPath );
	}

	private static Type RequireType(
		Assembly assembly,
		string name
	) {
		Type? type = assembly.GetType(
			NamespacePrefix + name,
			throwOnError: false,
			ignoreCase: false
		);
		Assert.NotNull( type );
		return type;
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory = new( AppContext.BaseDirectory );
		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return directory.FullName;
			}
			directory = directory.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root from the test output directory."
		);
	}

	private static BerkeleyDbTerminalDatabaseEntry CreateEntry(
		string canonical,
		params string[] aliases
	) => new(
		canonical,
		aliases,
		Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			canonical,
			"HW01 fixture",
			aliases
		)
	);

	private static TException AssertWriteThrowsWithoutDestination<TException>(
		IEnumerable<BerkeleyDbTerminalDatabaseEntry> entries,
		BerkeleyDbTerminalDatabaseWriterOptions? options = null,
		CancellationToken cancellationToken = default
	) where TException : Exception {
		string directory = Path.Combine(
			Path.GetTempPath(),
			"icod-terminfo-hw01-" + Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( directory );
		string destination = Path.Combine( directory, "terminfo.db" );
		try {
			TException exception = Assert.Throws<TException>(
				() => BerkeleyDbTerminalDatabaseWriter.Write(
					destination,
					entries,
					options,
					cancellationToken
				)
			);
			Assert.False( File.Exists( destination ) );
			return exception;
		} finally {
			Directory.Delete( directory, recursive: true );
		}
	}

	private static void AssertParameter(
		ParameterInfo parameter,
		string expectedName,
		Type expectedType,
		bool hasDefaultValue,
		object? expectedDefaultValue
	) {
		Assert.Equal( expectedName, parameter.Name );
		Assert.Equal( expectedType, parameter.ParameterType );
		Assert.Equal( hasDefaultValue, parameter.HasDefaultValue );
		if ( hasDefaultValue ) {
			Assert.Equal( expectedDefaultValue, parameter.DefaultValue );
		}
	}

	private static void AssertProperties(
		Type type,
		IReadOnlyDictionary<string, Type> expected
	) {
		PropertyInfo[] properties = type.GetProperties(
			BindingFlags.Public
			| BindingFlags.Instance
			| BindingFlags.DeclaredOnly
		);
		Assert.Equal(
			expected.Keys.OrderBy(
				static name => name,
				StringComparer.Ordinal
			),
			properties.Select( static property => property.Name ).OrderBy(
				static name => name,
				StringComparer.Ordinal
			)
		);
		foreach ( PropertyInfo property in properties ) {
			Assert.Equal( expected[property.Name], property.PropertyType );
			Assert.NotNull( property.GetMethod );
			Assert.Null( property.SetMethod );
		}
	}
}
