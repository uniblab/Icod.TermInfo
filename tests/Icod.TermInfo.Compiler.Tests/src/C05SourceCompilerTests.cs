using System.Buffers.Binary;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C05SourceCompilerTests {
	[Fact]
	public void Compile_MultipleEntriesWithForwardInheritance_ProducesLoadableEntries() {
		const string source =
			"""
			c05-child|child alias|C05 child,
				cols#132,
				clear@,
				use=c05-base,

			c05-base|base alias|C05 base,
				am,
				lines#40,
				cols#80,
				clear=\E[H\E[2J,
			""";

		TermInfoSourceCompilationResult result =
			TermInfoSourceCompiler.Compile(
				source,
				"C05-inheritance.ti"
			);

		Assert.False( result.HasErrors );
		Assert.Empty( result.Diagnostics );
		Assert.Equal(
			[
				"c05-child",
				"c05-base",
			],
			result.Entries
				.Select( entry => entry.CanonicalName )
				.ToArray()
		);

		TerminalDescription child =
			CompiledTermInfoParser.Parse(
				result.Entries[0].Data
			);
		TerminalDescription parent =
			CompiledTermInfoParser.Parse(
				result.Entries[1].Data
			);

		Assert.Equal( "c05-child", child.Name );
		Assert.Contains( "child alias", child.Aliases );
		Assert.True(
			child.GetBoolean(
				BooleanCapability.AutoRightMargin
			)
		);
		Assert.Equal(
			132,
			child.GetNumber(
				NumericCapability.Columns
			)
		);
		Assert.Equal(
			40,
			child.GetNumber(
				NumericCapability.Lines
			)
		);
		Assert.Null(
			child.GetString(
				StringCapability.ClearScreen
			)
		);

		Assert.Equal( "c05-base", parent.Name );
		Assert.Equal(
			80,
			parent.GetNumber(
				NumericCapability.Columns
			)
		);
		Assert.Equal(
			"\u001b[H\u001b[2J",
			parent.GetString(
				StringCapability.ClearScreen
			)
		);
	}

	[Fact]
	public void Compile_MissingParent_PreservesSourceDiagnosticLocation() {
		const string source =
			"""
			c05-broken|C05 broken,
				cols#80,
				use=c05-missing,
			""";

		TermInfoSourceCompilationResult result =
			TermInfoSourceCompiler.Compile(
				source,
				"C05-missing-parent.ti"
			);

		Assert.True( result.HasErrors );
		Assert.Empty( result.Entries );

		TermInfoSourceDiagnostic diagnostic = Assert.Single(
			result.Diagnostics,
			item => item.Code == TermInfoSourceDiagnosticCodes.MissingSourceEntry
		);
		Assert.NotNull( diagnostic.Span );
		Assert.Equal(
			"C05-missing-parent.ti",
			diagnostic.Span!.SourceName
		);
		Assert.True( diagnostic.Span.Line > 0 );
		Assert.True( diagnostic.Span.Column > 0 );
	}

	[Fact]
	public void Compile_WriterOptions_AreAppliedAfterMaterialization() {
		const string source =
			"""
			c05-wide|C05 wide,
				cols#100000,
			""";

		TermInfoSourceCompilationResult automatic =
			TermInfoSourceCompiler.Compile( source );
		byte[] automaticData =
			Assert.Single( automatic.Entries ).Data;

		Assert.Equal(
			0x021E,
			BinaryPrimitives.ReadUInt16LittleEndian(
				automaticData.AsSpan(
					0,
					sizeof( ushort )
				)
			)
		);

		Assert.Throws<InvalidOperationException>(
			() =>
				TermInfoSourceCompiler.Compile(
					source,
					writerOptions:
						new CompiledTermInfoWriterOptions(
							CompiledTermInfoFormat.Legacy
						)
				)
		);
	}

	[Fact]
	public void Compile_DataProperty_ReturnsIndependentCopies() {
		const string source =
			"""
			c05-copy|C05 copy,
				cols#80,
			""";

		CompiledTermInfoSourceEntry entry =
			Assert.Single(
				TermInfoSourceCompiler
					.Compile( source )
					.Entries
			);
		byte[] first = entry.Data;
		byte[] second = entry.Data;

		Assert.NotSame( first, second );
		Assert.True( first.SequenceEqual( second ) );

		first[0] ^= 0xff;
		Assert.NotEqual( first[0], entry.Data[0] );
	}

	[Fact]
	public void Compile_NullInputs_Throw() {
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceCompiler.Compile(
					(string)null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoSourceCompiler.Compile(
					(TextReader)null!
				)
		);
	}
}
