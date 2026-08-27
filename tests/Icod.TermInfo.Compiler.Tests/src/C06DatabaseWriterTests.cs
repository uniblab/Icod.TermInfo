using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C06DatabaseWriterTests {
	[Fact]
	public void Write_CompilationResult_ProducesProviderConsumableDatabase() {
		const string source =
			"""
			c06-child|alias-c06|C06 child,
				cols#132,
				use=c06-base,

			c06-base|C06 base,
				am,
				lines#41,
			""";
		string root = CreateTemporaryRoot();

		try {
			TermInfoSourceCompilationResult result =
				TermInfoSourceCompiler.Compile(
					source,
					"C06-database.ti"
				);

			Assert.False( result.HasErrors );
			CompiledTermInfoDatabaseWriter.Write(
				root,
				result
			);

			Assert.True(
				File.Exists(
					Path.Combine(
						root,
						"63",
						"c06-child"
					)
				)
			);
			Assert.True(
				File.Exists(
					Path.Combine(
						root,
						"61",
						"alias-c06"
					)
				)
			);

			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True(
				provider.TryLoad(
					"c06-child",
					out TerminalDescription? child
				)
			);
			Assert.NotNull( child );
			Assert.Equal(
				132,
				child.GetNumber(
					NumericCapability.Columns
				)
			);
			Assert.Equal(
				41,
				child.GetNumber(
					NumericCapability.Lines
				)
			);
			Assert.True(
				child.GetBoolean(
					BooleanCapability.AutoRightMargin
				)
			);

			Assert.True(
				provider.TryLoad(
					"alias-c06",
					out TerminalDescription? alias
				)
			);
			Assert.NotNull( alias );
			Assert.Equal( "c06-child", alias.Name );
		}
		finally {
			DeleteTemporaryRoot( root );
		}
	}

	[Fact]
	public void Write_TerminalDescription_ProducesProviderConsumableDatabase() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "c06-description" )
				.AddAlias( "alias-description" )
				.SetDescription( "C06 description" )
				.SetNumber(
					NumericCapability.Columns,
					101
				)
				.Build();
		string root = CreateTemporaryRoot();

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				description
			);

			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True(
				provider.TryLoad(
					"alias-description",
					out TerminalDescription? loaded
				)
			);
			Assert.NotNull( loaded );
			Assert.Equal( "c06-description", loaded.Name );
			Assert.Equal(
				101,
				loaded.GetNumber(
					NumericCapability.Columns
				)
			);
		}
		finally {
			DeleteTemporaryRoot( root );
		}
	}

	[Fact]
	public void Write_DescriptionBatchWithRepresentationFailure_DoesNotCreateOutputRoot() {
		TerminalDescription valid =
			new TerminalDescriptionBuilder( "c06-valid" )
				.SetDescription( "C06 valid" )
				.Build();
		TerminalDescription invalid =
			new TerminalDescriptionBuilder( "c06-invalid" )
				.Build();
		string root = CreateTemporaryRoot();

		Assert.Throws<InvalidOperationException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					root,
					new[] {
						valid,
						invalid,
					}
				)
		);
		Assert.False( Directory.Exists( root ) );
	}

	[Fact]
	public void Write_CompilationWithErrors_DoesNotCreateOutputRoot() {
		const string source =
			"""
			c06-broken|C06 broken,
				use=c06-missing,
			""";
		string root = CreateTemporaryRoot();

		TermInfoSourceCompilationResult result =
			TermInfoSourceCompiler.Compile(
				source,
				"C06-broken.ti"
			);

		Assert.True( result.HasErrors );
		Assert.Throws<InvalidOperationException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					root,
					result
				)
		);
		Assert.False( Directory.Exists( root ) );
	}

	[Fact]
	public void Write_DefaultOverwritePolicy_PreservesExistingEntry() {
		string root = CreateTemporaryRoot();

		try {
			TermInfoSourceCompilationResult first =
				CompileSingle(
					80
				);
			TermInfoSourceCompilationResult second =
				CompileSingle(
					132
				);

			CompiledTermInfoDatabaseWriter.Write(
				root,
				first
			);
			Assert.Throws<IOException>(
				() =>
					CompiledTermInfoDatabaseWriter.Write(
						root,
						second
					)
			);

			DirectoryTerminalDescriptionProvider provider =
				new( root );
			Assert.True(
				provider.TryLoad(
					"c06-overwrite",
					out TerminalDescription? terminal
				)
			);
			Assert.NotNull( terminal );
			Assert.Equal(
				80,
				terminal.GetNumber(
					NumericCapability.Columns
				)
			);
		}
		finally {
			DeleteTemporaryRoot( root );
		}
	}

	[Fact]
	public void Write_OverwriteExisting_ReplacesCanonicalAndAliasEntries() {
		string root = CreateTemporaryRoot();

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				CompileSingle(
					80
				)
			);
			CompiledTermInfoDatabaseWriter.Write(
				root,
				CompileSingle(
					132
				),
				new CompiledTermInfoDatabaseWriterOptions(
					overwriteExisting: true
				)
			);

			DirectoryTerminalDescriptionProvider provider =
				new( root );

			foreach ( string name in new[] {
				"c06-overwrite",
				"alias-overwrite",
			} ) {
				Assert.True(
					provider.TryLoad(
						name,
						out TerminalDescription? terminal
					)
				);
				Assert.NotNull( terminal );
				Assert.Equal(
					132,
					terminal.GetNumber(
						NumericCapability.Columns
					)
				);
			}

			Assert.Empty(
				Directory.EnumerateFiles(
					root,
					"*.tmp",
					SearchOption.AllDirectories
				)
			);
		}
		finally {
			DeleteTemporaryRoot( root );
		}
	}

	[Theory]
	[InlineData( "../c06-escape" )]
	[InlineData( "..\\c06-escape" )]
	public void Write_UnsafeTerminalNames_AreRejectedBeforeFilesystemMutation(
		string terminalName
	) {
		ArgumentNullException.ThrowIfNull( terminalName );

		string source =
			$"""
			{terminalName}|C06 unsafe,
				cols#80,
			""";
		string root = CreateTemporaryRoot();
		TermInfoSourceCompilationResult result =
			TermInfoSourceCompiler.Compile(
				source,
				"C06-unsafe.ti"
			);

		Assert.False( result.HasErrors );
		Assert.Throws<ArgumentException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					root,
					result
				)
		);
		Assert.False( Directory.Exists( root ) );
	}

	[Fact]
	public void Write_NullAndWhitespaceArguments_Throw() {
		TermInfoSourceCompilationResult result =
			CompileSingle(
				80
			);

		Assert.Throws<ArgumentNullException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					null!,
					result
				)
		);
		Assert.Throws<ArgumentException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					" ",
					result
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					CreateTemporaryRoot(),
					(TermInfoSourceCompilationResult)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					CreateTemporaryRoot(),
					(IEnumerable<CompiledTermInfoSourceEntry>)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					CreateTemporaryRoot(),
					(TerminalDescription)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				CompiledTermInfoDatabaseWriter.Write(
					CreateTemporaryRoot(),
					(IEnumerable<TerminalDescription>)null!
				)
		);
	}

	private static TermInfoSourceCompilationResult CompileSingle(
		int columns
	) {
		string source =
			$"""
			c06-overwrite|alias-overwrite|C06 overwrite,
				cols#{columns},
			""";

		TermInfoSourceCompilationResult result =
			TermInfoSourceCompiler.Compile(
				source,
				"C06-overwrite.ti"
			);
		Assert.False( result.HasErrors );
		return result;
	}

	private static string CreateTemporaryRoot() {
		return Path.Combine(
			Path.GetTempPath(),
			"Icod.TermInfo.Compiler.C06."
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
