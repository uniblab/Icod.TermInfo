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
