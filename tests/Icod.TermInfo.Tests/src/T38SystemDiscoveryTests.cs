using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T38SystemDiscoveryTests
{
	private const int CompiledHeaderSize = 12;

	[Fact]
	public void AssemblyIdentifiesT38DevelopmentVersion()
	{
		Assembly assembly =
			typeof(SystemTerminalDescriptionProvider).Assembly;
		Version? assemblyVersion =
			assembly.GetName().Version;
		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;

		Assert.NotNull(assemblyVersion);
		Assert.Equal(
			new Version(0, 9, 0, 0),
			assemblyVersion);
		Assert.NotNull(informationalVersion);
		Assert.True(
			informationalVersion!.StartsWith(
				"0.9.0-beta.2",
				StringComparison.Ordinal),
			$"Unexpected informational version '{informationalVersion}'.");
	}

	[Fact]
	public void PublicSurfaceMatchesT32SystemProviderFreeze()
	{
		Assert.True(
			typeof(ITerminalDescriptionProvider).IsAssignableFrom(
				typeof(SystemTerminalDescriptionProvider)));

		ConstructorInfo constructor =
			Assert.Single(
				typeof(SystemTerminalDescriptionProvider)
					.GetConstructors(
						BindingFlags.Public
						| BindingFlags.Instance));
		ParameterInfo parameter =
			Assert.Single(
				constructor.GetParameters());

		Assert.Equal(
			typeof(SystemTerminalDescriptionProviderOptions),
			parameter.ParameterType);
		Assert.True(parameter.HasDefaultValue);
		Assert.Null(parameter.DefaultValue);

		MethodInfo tryLoad =
			Assert.Single(
				typeof(SystemTerminalDescriptionProvider)
					.GetMethods(
						BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.DeclaredOnly),
				method => !method.IsSpecialName);
		Assert.Equal(
			nameof(SystemTerminalDescriptionProvider.TryLoad),
			tryLoad.Name);

		ParameterInfo[] tryLoadParameters =
			tryLoad.GetParameters();
		Assert.Equal(2, tryLoadParameters.Length);
		Assert.Equal(
			typeof(string),
			tryLoadParameters[0].ParameterType);
		Assert.Equal(
			typeof(TerminalDescription).MakeByRefType(),
			tryLoadParameters[1].ParameterType);

		NotNullWhenAttribute? notNullWhen =
			tryLoadParameters[1]
				.GetCustomAttribute<NotNullWhenAttribute>();
		Assert.NotNull(notNullWhen);
		Assert.True(notNullWhen!.ReturnValue);
	}

	[Fact]
	public void PlatformDefaultRootsAreFrozen()
	{
		Assert.Equal(
			new[]
			{
				"/etc/terminfo",
				"/lib/terminfo",
				"/usr/share/terminfo",
			},
			SystemTerminalDescriptionProvider.GetDefaultRoots(
				TerminalHostPlatform.Linux));

		Assert.Equal(
			new[]
			{
				"/usr/share/terminfo",
			},
			SystemTerminalDescriptionProvider.GetDefaultRoots(
				TerminalHostPlatform.MacOS));

		Assert.Empty(
			SystemTerminalDescriptionProvider.GetDefaultRoots(
				TerminalHostPlatform.Windows));
		Assert.Empty(
			SystemTerminalDescriptionProvider.GetDefaultRoots(
				TerminalHostPlatform.Other));
	}

	[Fact]
	public void EncodedTermInfoPrecedesDirectorySearch()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");
		string searchRoot =
			temporary.CreateSubdirectory("search");
		string defaultRoot =
			temporary.CreateSubdirectory("default");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));
		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));
		WriteLiteralCandidate(
			defaultRoot,
			name,
			CreateEntryWithColumns(84));

		byte[] encodedEntry =
			CreateEntryWithColumns(80);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo:
					"hex:"
					+ Convert.ToHexString(encodedEntry),
				termInfoDirs: "search",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				new[]
				{
					defaultRoot,
				});

		AssertColumns(
			80,
			Load(provider, name));
	}

	[Fact]
	public void EncodedIdentityMismatchFallsThroughToDirectories()
	{
		using TemporaryDirectory temporary = new();
		string name = "n29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");
		byte[] encodedEntry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateRenamedMinimalEntry());

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo:
					"hex:"
					+ Convert.ToHexString(encodedEntry),
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		TerminalDescription terminal =
			Load(
				provider,
				name);

		Assert.Equal(name, terminal.Name);
	}

	[Fact]
	public void ExplicitTermInfoDirectoryPrecedesUserAndSearchList()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string explicitRoot =
			temporary.CreateSubdirectory("explicit");
		string home =
			temporary.CreateSubdirectory("home");
		string searchRoot =
			temporary.CreateSubdirectory("search");

		WriteLiteralCandidate(
			explicitRoot,
			name,
			CreateEntryWithColumns(81));
		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));
		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: "explicit",
				termInfoDirs: "search",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			81,
			Load(provider, name));
	}

	[Fact]
	public void MissingExplicitTermInfoDirectoryFallsThroughToUserDatabase()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: "missing-explicit-root",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			82,
			Load(provider, name));
	}

	[Fact]
	public void UserDatabasePrecedesTermInfoDirs()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");
		string searchRoot =
			temporary.CreateSubdirectory("search");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));
		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: "search",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			82,
			Load(provider, name));
	}

	[Fact]
	public void TermInfoDirsPreservesDeclaredOrder()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string first =
			temporary.CreateSubdirectory("first");
		string second =
			temporary.CreateSubdirectory("second");

		WriteLiteralCandidate(
			first,
			name,
			CreateEntryWithColumns(83));
		WriteLiteralCandidate(
			second,
			name,
			CreateEntryWithColumns(84));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: "first:second");

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			83,
			Load(provider, name));
	}

	[Fact]
	public void EmptyTermInfoDirsComponentExpandsDefaultsInPlace()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string defaultRoot =
			temporary.CreateSubdirectory("default");
		string after =
			temporary.CreateSubdirectory("after");

		WriteLiteralCandidate(
			defaultRoot,
			name,
			CreateEntryWithColumns(84));
		WriteLiteralCandidate(
			after,
			name,
			CreateEntryWithColumns(85));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: "missing::after");

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				new[]
				{
					defaultRoot,
				});

		AssertColumns(
			84,
			Load(provider, name));
	}

	[Fact]
	public void TermInfoDirsPrecedesFinalPlatformDefaults()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string searchRoot =
			temporary.CreateSubdirectory("search");
		string defaultRoot =
			temporary.CreateSubdirectory("default");

		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));
		WriteLiteralCandidate(
			defaultRoot,
			name,
			CreateEntryWithColumns(84));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: "search");

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				new[]
				{
					defaultRoot,
				});

		AssertColumns(
			83,
			Load(provider, name));
	}

	[Fact]
	public void MalformedEncodedTermInfoIsNotHiddenByFallback()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: "hex:0g",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		Assert.Throws<FormatException>(
			() => provider.TryLoad(
				name,
				out _));
	}

	[Fact]
	public void ExplicitTermInfoFileIsNotHiddenAsDirectoryMiss()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string explicitFile =
			Path.Combine(
				temporary.Root,
				"terminfo.db");
		string home =
			temporary.CreateSubdirectory("home");

		File.WriteAllText(
			explicitFile,
			"not a conventional directory tree");
		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));

		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: explicitFile,
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		NotSupportedException exception =
			Assert.Throws<NotSupportedException>(
				() => provider.TryLoad(
					name,
					out _));

		Assert.Contains(
			"not a directory tree",
			exception.Message);
	}

	[Fact]
	public void DisablingEnvironmentIgnoresAllEnvironmentSources()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string explicitRoot =
			temporary.CreateSubdirectory("explicit");
		string searchRoot =
			temporary.CreateSubdirectory("search");
		string home =
			temporary.CreateSubdirectory("home");

		WriteLiteralCandidate(
			explicitRoot,
			name,
			CreateEntryWithColumns(81));
		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));
		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: true,
				useSystemDatabases: false);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfo: explicitRoot,
				termInfoDirs: "search",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			82,
			Load(provider, name));
	}

	[Fact]
	public void DisablingUserDatabaseLeavesEnvironmentSourcesAvailable()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string searchRoot =
			temporary.CreateSubdirectory("search");
		string home =
			temporary.CreateSubdirectory("home");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));
		WriteLiteralCandidate(
			searchRoot,
			name,
			CreateEntryWithColumns(83));

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: "search",
				homeDirectory: home);

		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				Array.Empty<string>());

		AssertColumns(
			83,
			Load(provider, name));
	}

	[Fact]
	public void DisablingSystemDatabasesSuppressesFinalAndEmptyComponentDefaults()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string defaultRoot =
			temporary.CreateSubdirectory("default");

		WriteLiteralCandidate(
			defaultRoot,
			name,
			CreateEntryWithColumns(84));

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				termInfoDirs: ":");

		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				new[]
				{
					defaultRoot,
				});

		Assert.False(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal));
		Assert.Null(terminal);
	}

	[Fact]
	public void WindowsDoesNotInventUserOrDefaultRoots()
	{
		using TemporaryDirectory temporary = new();
		string name = "t29-legacy-minimal";
		string home =
			temporary.CreateSubdirectory("home");

		WriteLiteralCandidate(
			Path.Combine(home, ".terminfo"),
			name,
			CreateEntryWithColumns(82));

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: true,
				useSystemDatabases: true);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root,
				homeDirectory: home,
				platform: TerminalHostPlatform.Windows);

		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				SystemTerminalDescriptionProvider.GetDefaultRoots(
					TerminalHostPlatform.Windows));

		Assert.False(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal));
		Assert.Null(terminal);
	}

	[Fact]
	public void CleanMissReturnsFalseAndNull()
	{
		using TemporaryDirectory temporary = new();
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root);
		SystemTerminalDescriptionProvider provider =
			CreateProvider(
				snapshot,
				Array.Empty<string>());

		Assert.False(
			provider.TryLoad(
				"t29-legacy-minimal",
				out TerminalDescription? terminal));
		Assert.Null(terminal);
	}

	[Fact]
	public void TerminalNameIsValidatedEvenWhenEverySourceIsDisabled()
	{
		using TemporaryDirectory temporary = new();
		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: false);
		SystemTerminalDiscoverySnapshot snapshot =
			CreateSnapshot(
				temporary.Root);
		SystemTerminalDescriptionProvider provider =
			new(
				options,
				snapshot,
				Array.Empty<string>());

		Assert.Throws<ArgumentNullException>(
			() => provider.TryLoad(
				null!,
				out _));
		Assert.Throws<ArgumentException>(
			() => provider.TryLoad(
				"../xterm",
				out _));
	}

	private static SystemTerminalDescriptionProvider CreateProvider(
		SystemTerminalDiscoverySnapshot snapshot,
		IReadOnlyList<string> defaultRoots)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentNullException.ThrowIfNull(defaultRoots);

		return new SystemTerminalDescriptionProvider(
			new SystemTerminalDescriptionProviderOptions(),
			snapshot,
			defaultRoots);
	}

	private static SystemTerminalDiscoverySnapshot CreateSnapshot(
		string currentDirectory,
		string? termInfo = null,
		string? termInfoDirs = null,
		string? homeDirectory = null,
		TerminalHostPlatform platform = TerminalHostPlatform.Linux)
	{
		ArgumentNullException.ThrowIfNull(currentDirectory);

		return new SystemTerminalDiscoverySnapshot(
			termInfo,
			termInfoDirs,
			homeDirectory,
			Path.GetFullPath(currentDirectory),
			platform);
	}

	private static TerminalDescription Load(
		SystemTerminalDescriptionProvider provider,
		string name)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(name);

		Assert.True(
			provider.TryLoad(
				name,
				out TerminalDescription? terminal));
		return Assert.IsType<TerminalDescription>(
			terminal);
	}

	private static void AssertColumns(
		int expected,
		TerminalDescription terminal)
	{
		ArgumentNullException.ThrowIfNull(terminal);

		Assert.Equal<int?>(
			expected,
			terminal.GetNumber(
				NumericCapability.Columns));
	}

	private static byte[] CreateEntryWithColumns(
		int columns)
	{
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin");
		SetLegacyColumns(
			entry,
			columns);
		return entry;
	}

	private static byte[] CreateRenamedMinimalEntry()
	{
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin");
		int namesSize =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.AsSpan(
					2,
					sizeof(ushort)));
		Span<byte> names =
			entry.AsSpan(
				CompiledHeaderSize,
				namesSize);

		int firstSeparator =
			names.IndexOf((byte)'|');
		if (firstSeparator <= 0
			|| firstSeparator + 1 >= names.Length)
		{
			throw new InvalidDataException(
				"The minimal fixture does not contain the expected alias layout.");
		}

		names[0] = (byte)'n';
		names[firstSeparator + 1] =
			(byte)'n';
		return entry;
	}

	private static void SetLegacyColumns(
		byte[] entry,
		int columns)
	{
		ArgumentNullException.ThrowIfNull(entry);

		int names =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.AsSpan(
					2,
					sizeof(ushort)));
		int booleans =
			BinaryPrimitives.ReadUInt16LittleEndian(
				entry.AsSpan(
					4,
					sizeof(ushort)));
		int numericOffset =
			CompiledHeaderSize
			+ names
			+ booleans;

		if ((numericOffset & 1) != 0)
		{
			numericOffset++;
		}

		BinaryPrimitives.WriteInt16LittleEndian(
			entry.AsSpan(
				numericOffset,
				sizeof(short)),
			checked((short)columns));
	}

	private static string WriteLiteralCandidate(
		string root,
		string name,
		byte[] entry)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(entry);

		string directory =
			Path.Combine(
				root,
				name[0].ToString());
		Directory.CreateDirectory(
			directory);

		string path =
			Path.Combine(
				directory,
				name);
		File.WriteAllBytes(
			path,
			entry);
		return path;
	}

	private static byte[] ReadFixture(
		string relativePath)
	{
		ArgumentNullException.ThrowIfNull(relativePath);

		return File.ReadAllBytes(
			Path.Combine(
				AppContext.BaseDirectory,
				"fixtures",
				"compiled-terminfo",
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar)));
	}

	private sealed class TemporaryDirectory : IDisposable
	{
		internal TemporaryDirectory()
		{
			Root =
				Path.Combine(
					Path.GetTempPath(),
					"icod-terminfo-t38-"
					+ Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(
				Root);
		}

		internal string Root
		{
			get;
		}

		internal string CreateSubdirectory(
			string name)
		{
			ArgumentNullException.ThrowIfNull(name);

			string path =
				Path.Combine(
					Root,
					name);
			Directory.CreateDirectory(
				path);
			return path;
		}

		public void Dispose()
		{
			if (Directory.Exists(
					Root))
			{
				Directory.Delete(
					Root,
					recursive: true);
			}
		}
	}
}
