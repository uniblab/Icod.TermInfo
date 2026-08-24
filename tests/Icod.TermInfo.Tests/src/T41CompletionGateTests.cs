using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T41CompletionGateTests
{
	[Fact]
	public void SupportedCompiledEntryFlowsThroughParserProvidersAndBuiltInFallback()
	{
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin");
		TerminalDescription parsed =
			CompiledTermInfoParser.Parse(
				entry);

		Assert.Equal(
			"t29-legacy-minimal",
			parsed.Name);
		Assert.Equal<int?>(
			80,
			parsed.GetNumber(
				NumericCapability.Columns));

		using TemporaryDirectory temporary = new();

		WriteLiteralCandidate(
			temporary.Root,
			parsed.Name,
			entry);

		DirectoryTerminalDescriptionProvider explicitProvider =
			new(
				temporary.Root);

		Assert.True(
			explicitProvider.TryLoad(
				parsed.Name,
				out TerminalDescription? explicitTerminal));
		Assert.NotNull(
			explicitTerminal);
		Assert.Equal(
			parsed.Name,
			explicitTerminal!.Name);

		SystemTerminalDescriptionProviderOptions options =
			new(
				useEnvironment: true,
				useUserDatabase: false,
				useSystemDatabases: false);
		SystemTerminalDiscoverySnapshot snapshot =
			new(
				termInfo: temporary.Root,
				termInfoDirs: null,
				homeDirectory: null,
				currentDirectory: temporary.Root,
				platform: TerminalHostPlatform.Linux);
		SystemTerminalDescriptionProvider systemProvider =
			new(
				options,
				snapshot,
				Array.Empty<string>());
		TerminalDatabase database =
			new(
				new ITerminalDescriptionProvider[]
				{
					systemProvider,
					TerminalDatabase.BuiltIn,
				});

		Assert.True(
			database.TryLoad(
				parsed.Name,
				out TerminalDescription? systemTerminal));
		Assert.NotNull(
			systemTerminal);
		Assert.Equal<int?>(
			80,
			systemTerminal!.GetNumber(
				NumericCapability.Columns));

		Assert.Same(
			TerminalProfiles.Xterm,
			database.Load(
				"xterm"));
		Assert.False(
			TerminalDatabase.BuiltIn.TryLoad(
				parsed.Name,
				out TerminalDescription? leaked));
		Assert.Null(
			leaked);
	}

	[Fact]
	public void ReleaseWorkflowBoundaryIsFrozen()
	{
		string root =
			FindRepositoryRoot();
		string pushMain =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"push-main.yaml")));
		string pullRequest =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"pr-build-and-test.yaml")));

		Assert.StartsWith(
			"name: build and publish\n"
			+ "\n"
			+ "on:\n"
			+ "  push:\n"
			+ "    branches:\n"
			+ "      - main\n"
			+ "\n"
			+ "permissions:\n",
			pushMain);
		Assert.DoesNotContain(
			"pull_request:",
			pushMain);
		Assert.Contains(
			"dotnet pack Icod.TermInfo.csproj",
			pushMain);
		Assert.Contains(
			"NuGet/login@v1",
			pushMain);
		Assert.Contains(
			"dotnet nuget push",
			pushMain);

		Assert.StartsWith(
			"name: pr-build-and-test\n"
			+ "\n"
			+ "on:\n"
			+ "  pull_request:\n"
			+ "\n"
			+ "permissions:\n"
			+ "  contents: read\n"
			+ "\n"
			+ "jobs:\n",
			pullRequest);
		Assert.Contains(
			"dotnet pack Icod.TermInfo.csproj",
			pullRequest);
		Assert.Contains(
			"verify-release-package.sh artifacts",
			pullRequest);
		Assert.Contains(
			"actions/upload-artifact@v4",
			pullRequest);
		Assert.Contains(
			"name: icod-terminfo-pr-packages",
			pullRequest);
		Assert.Contains(
			"artifacts/*.nupkg",
			pullRequest);
		Assert.Contains(
			"artifacts/*.snupkg",
			pullRequest);

		string[] forbiddenPublicationFragments =
		[
			"NuGet/login",
			"dotnet nuget push",
			"packages: write",
			"id-token: write",
			"deploy:",
		];

		foreach (string fragment in forbiddenPublicationFragments)
		{
			Assert.False(
				pullRequest.Contains(
					fragment,
					StringComparison.Ordinal),
				$"Pull-request workflow contains forbidden publication fragment '{fragment}'.");
		}
	}

	private static string FindRepositoryRoot()
	{
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory);

		while (current is not null)
		{
			if (File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.csproj"))
				&& File.Exists(
					Path.Combine(
						current.FullName,
						".github",
						"workflows",
						"push-main.yaml")))
			{
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root.");
	}

	private static string NormalizeLineEndings(
		string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		return value
			.Replace(
				"\r\n",
				"\n")
			.Replace(
				'\r',
				'\n');
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

	private sealed class TemporaryDirectory : IDisposable
	{
		internal TemporaryDirectory()
		{
			Root =
				Path.Combine(
					Path.GetTempPath(),
					"icod-terminfo-t41-"
					+ Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(
				Root);
		}

		internal string Root
		{
			get;
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
