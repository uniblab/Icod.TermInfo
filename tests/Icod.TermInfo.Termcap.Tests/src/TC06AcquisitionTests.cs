using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC06AcquisitionTests
{
	[Fact]
	public void ExplicitInlineSourceAcquiresWithoutFilesystem() {
		TermcapAcquisitionOptions options =
			new(
				inlineTermcap: "demo|Demo terminal:am:co#80:"
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.True( result.IsLossless );
		Assert.True( result.Found );
		Assert.NotNull( result.Source );
		Assert.Equal(
			TermcapAcquisitionSourceKind.InlineTermcap,
			result.Source!.Kind
		);
		Assert.Equal( "<inline-termcap>", result.Source.Identifier );
		TerminalDescription description =
			Assert.IsType<TerminalDescription>( result.Description );
		Assert.True(
			description.GetBoolean(
				BooleanCapability.AutoRightMargin
			)
		);
		Assert.Equal(
			80,
			description.GetNumber(
				NumericCapability.Columns
			)
		);
	}

	[Fact]
	public void ExplicitDatabasePathUsesCallerSuppliedFileProvider() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/custom/termcap"] = "demo|Demo terminal:am:co#132:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termcapDatabasePath: "/custom/termcap",
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			new[] { "/custom/termcap" },
			files.OpenedPaths
		);
		Assert.Equal(
			TermcapAcquisitionSourceKind.TermcapDatabasePath,
			result.Source!.Kind
		);
		Assert.Equal( "/custom/termcap", result.Source.Identifier );
		Assert.Equal(
			132,
			result.Description!.GetNumber(
				NumericCapability.Columns
			)
		);
	}

	[Fact]
	public void OrderedTermPathUsesFirstMatchingDatabase() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/first"] = "demo|First terminal:co#80:\n",
					["/second"] = "demo|Second terminal:co#132:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termPath: new[] { "/first", "/second" },
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			80,
			result.Description!.GetNumber(
				NumericCapability.Columns
			)
		);
		Assert.Equal( "/first", result.Source!.Identifier );
		Assert.Equal(
			new[] { "/first" },
			files.OpenedPaths
		);
	}

	[Fact]
	public void MissingDatabaseFallsThroughToNextConfiguredPath() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/present"] = "demo|Demo terminal:co#80:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termPath: new[] { "/missing", "/present" },
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			new[] { "/missing", "/present" },
			files.OpenedPaths
		);
		Assert.Equal( "/present", result.Source!.Identifier );
	}

	[Fact]
	public void InheritanceCanCrossDatabaseBoundaries() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/child"] = "child|Child terminal:am:tc=base:\n",
					["/base"] = "base|Base terminal:co#132:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termPath: new[] { "/child", "/base" },
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"child",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal( "/child", result.Source!.Identifier );
		Assert.True(
			result.Description!.GetBoolean(
				BooleanCapability.AutoRightMargin
			)
		);
		Assert.Equal(
			132,
			result.Description.GetNumber(
				NumericCapability.Columns
			)
		);
	}

	[Fact]
	public void EnvironmentSnapshotUsesInlineTermcapBeforeTermPath() {
		MemoryEnvironmentProvider environment =
			new(
				new Dictionary<string, string?>( StringComparer.Ordinal ) {
					["TERMCAP"] = "demo|Demo terminal:co#80:",
					["TERMPATH"] = "/unused:/also-unused",
					["HOME"] = "/home/demo",
				}
			);
		MemoryFileProvider files = new();
		TermcapAcquisitionOptions options =
			TermcapAcquisitionOptions.FromEnvironment(
				environment,
				files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal( "TERMCAP", result.Source!.Identifier );
		Assert.Empty( files.OpenedPaths );
	}

	[Fact]
	public void EnvironmentSlashTermcapIsDatabasePathAndCanFallThrough() {
		MemoryEnvironmentProvider environment =
			new(
				new Dictionary<string, string?>( StringComparer.Ordinal ) {
					["TERMCAP"] = "/missing",
					["TERMPATH"] = "/fallback",
				}
			);
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/fallback"] = "demo|Demo terminal:co#80:\n",
				}
			);
		TermcapAcquisitionOptions options =
			TermcapAcquisitionOptions.FromEnvironment(
				environment,
				files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			new[] { "/missing", "/fallback" },
			files.OpenedPaths
		);
		Assert.Equal(
			TermcapAcquisitionSourceKind.TermPathDatabase,
			result.Source!.Kind
		);
	}

	[Fact]
	public void ConventionalDefaultsAreNeverUsedUnlessExplicitlySelected() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/etc/termcap"] = "demo|Demo terminal:co#80:\n",
				}
			);
		TermcapAcquisitionOptions disabled = new();

		TermcapAcquisitionResult disabledResult =
			TermcapAcquirer.Acquire(
				"demo",
				disabled
			);

		Assert.False( disabledResult.IsSuccess );
		Assert.False( disabledResult.Found );
		Assert.Empty( files.OpenedPaths );

		TermcapAcquisitionOptions enabled =
			new(
				defaultPathPolicy: TermcapDefaultPathPolicy.Ncurses,
				fileProvider: files
			);
		TermcapAcquisitionResult enabledResult =
			TermcapAcquirer.Acquire(
				"demo",
				enabled
			);

		Assert.True( enabledResult.IsSuccess );
		Assert.Equal(
			TermcapAcquisitionSourceKind.ConventionalDefaultDatabase,
			enabledResult.Source!.Kind
		);
		Assert.Equal( "/etc/termcap", enabledResult.Source.Identifier );
	}

	[Fact]
	public void NcursesDefaultPolicyAddsHomeTermcapAfterSystemPaths() {
		string homeDirectory =
			Path.Combine(
				Path.DirectorySeparatorChar.ToString(),
				"home",
				"demo"
			);
		string homeTermcap =
			Path.Combine(
				homeDirectory,
				".termcap"
			);
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					[homeTermcap] = "demo|Demo terminal:co#80:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				defaultPathPolicy: TermcapDefaultPathPolicy.Ncurses,
				homeDirectory: homeDirectory,
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.IsSuccess );
		Assert.Equal(
			new[] {
				"/etc/termcap",
				"/usr/share/misc/termcap",
				homeTermcap,
			},
			files.OpenedPaths
		);
	}

	[Fact]
	public void FileInputRemainsBoundedByTc01ParserOptions() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/large"] = new string( 'x', 128 ),
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termcapDatabasePath: "/large",
				fileProvider: files,
				parserOptions: new TermcapSourceParserOptions( 32 )
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Description );
		Assert.Contains(
			result.SourceDiagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.MaximumSourceLengthExceeded
		);
	}

	[Fact]
	public void MalformedConfiguredSourceIsNotSilentlyIgnored() {
		MemoryFileProvider files =
			new(
				new Dictionary<string, string>( StringComparer.Ordinal ) {
					["/broken"] = "broken",
					["/later"] = "demo|Demo terminal:co#80:\n",
				}
			);
		TermcapAcquisitionOptions options =
			new(
				termPath: new[] { "/broken", "/later" },
				fileProvider: files
			);

		TermcapAcquisitionResult result =
			TermcapAcquirer.Acquire(
				"demo",
				options
			);

		Assert.True( result.HasErrors );
		Assert.Null( result.Description );
		Assert.NotNull( result.Source );
		Assert.Equal( "/later", result.Source!.Identifier );
		Assert.Contains(
			result.SourceDiagnostics,
			diagnostic =>
				diagnostic.Code == TermcapSourceDiagnosticCodes.MissingHeaderTerminator
		);
	}

	[Fact]
	public void FileProviderFailuresPropagate() {
		TermcapAcquisitionOptions options =
			new(
				termcapDatabasePath: "/denied",
				fileProvider: new ThrowingFileProvider()
			);

		Assert.Throws<UnauthorizedAccessException>(
			() => TermcapAcquirer.Acquire(
				"demo",
				options
			)
		);
	}

	private sealed class MemoryEnvironmentProvider : ITermcapEnvironmentProvider
	{
		private readonly IReadOnlyDictionary<string, string?> _values;

		internal MemoryEnvironmentProvider(
			IReadOnlyDictionary<string, string?> values
		) {
			ArgumentNullException.ThrowIfNull( values );
			_values = values;
		}

		public string? GetEnvironmentVariable(
			string name
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( name );

			return _values.TryGetValue(
				name,
				out string? value
			)
				? value
				: null
			;
		}
	}

	private sealed class MemoryFileProvider : ITermcapFileProvider
	{
		private readonly IReadOnlyDictionary<string, string> _sources;
		private readonly List<string> _openedPaths = [];

		internal MemoryFileProvider()
			: this(
				new Dictionary<string, string>( StringComparer.Ordinal )
			) {
		}

		internal MemoryFileProvider(
			IReadOnlyDictionary<string, string> sources
		) {
			ArgumentNullException.ThrowIfNull( sources );
			_sources = sources;
		}

		internal IReadOnlyList<string> OpenedPaths => _openedPaths;

		public bool TryOpenText(
			string path,
			[NotNullWhen( true )] out TextReader? reader
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( path );

			_openedPaths.Add( path );
			if ( !_sources.TryGetValue( path, out string? source ) ) {
				reader = null;
				return false;
			}

			reader = new StringReader( source );
			return true;
		}
	}

	private sealed class ThrowingFileProvider : ITermcapFileProvider
	{
		public bool TryOpenText(
			string path,
			[NotNullWhen( true )] out TextReader? reader
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( path );

			reader = null;
			throw new UnauthorizedAccessException( "denied" );
		}
	}
}
