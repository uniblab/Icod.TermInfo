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
		string main =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"main.yaml")));
		string pullRequest =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"pull-request.yaml")));
		string release =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"release.yaml")));

		Assert.StartsWith(
			"name: main\n"
			+ "\n"
			+ "on:\n"
			+ "  push:\n"
			+ "    branches:\n"
			+ "      - main\n",
			main);
		Assert.DoesNotContain(
			"pull_request:",
			main);
		Assert.Contains(
			"CONFIGURATION: Release",
			main);
		Assert.Contains(
			"windows-11-arm",
			main);
		Assert.Contains(
			"ubuntu-24.04-arm",
			main);
		Assert.Contains(
			"macos-15-intel",
			main);
		Assert.Contains(
			"./packaging/PackPackages.ps1",
			main);
		Assert.Contains(
			"./packaging/VerifyPackageArtifact.ps1",
			main);
		Assert.Contains(
			"./packaging/BuildToolArchives.ps1",
			main);

		Assert.StartsWith(
			"name: pull-request\n"
			+ "\n"
			+ "on:\n"
			+ "  pull_request:\n",
			pullRequest);
		Assert.Contains(
			"CONFIGURATION: Staging",
			pullRequest);
		Assert.Contains(
			"./packaging/PackPackages.ps1",
			pullRequest);
		Assert.Contains(
			"./packaging/VerifyPackageArtifact.ps1",
			pullRequest);
		Assert.Contains(
			"./packaging/BuildToolArchives.ps1",
			pullRequest);
		Assert.DoesNotContain(
			"CONFIGURATION: Release",
			pullRequest);

		Assert.StartsWith(
			"name: release\n"
			+ "\n"
			+ "on:\n"
			+ "  push:\n"
			+ "    tags:\n"
			+ "      - 'v*'\n",
			release);
		Assert.Contains(
			"Require tagged commit in main",
			release);
		Assert.Contains(
			"git merge-base --is-ancestor $env:GITHUB_SHA origin/main",
			release);
		Assert.Contains(
			"Validate tag and suite version",
			release);
		Assert.Contains(
			"./packaging/PackPackages.ps1",
			release);
		Assert.Contains(
			"./packaging/BuildToolArchives.ps1",
			release);
		Assert.Contains(
			"NuGet/login@v1",
			release);
		Assert.Contains(
			"dotnet nuget push",
			release);
		Assert.Contains(
			"gh @arguments",
			release);

		string[] forbiddenPublicationFragments =
		[
			"NuGet/login",
			"dotnet nuget push",
			"packages: write",
			"id-token: write",
		];

		foreach (string workflow in new[] { pullRequest, main })
		{
			foreach (string fragment in forbiddenPublicationFragments)
			{
				Assert.False(
					workflow.Contains(
						fragment,
						StringComparison.Ordinal),
					$"Non-publishing workflow contains forbidden publication fragment '{fragment}'.");
			}
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
						"main.yaml")))
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
