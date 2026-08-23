namespace Icod.TermInfo.Sample;

internal static class SampleAcquisition
{
	internal static TerminalDescription ParseCompiledEntry(
		ReadOnlySpan<byte> entry)
	{
		return CompiledTermInfoParser.Parse(
			entry);
	}

	internal static TerminalDescription LoadExplicitRoot(
		string root,
		string name)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(name);

		DirectoryTerminalDescriptionProvider provider =
			new(
				root);
		return new TerminalDatabase(
			new ITerminalDescriptionProvider[]
			{
				provider,
			})
			.Load(
				name);
	}

	internal static SystemTerminalDescriptionProvider CreateRestrictedSystemProvider()
	{
		return new SystemTerminalDescriptionProvider(
			new SystemTerminalDescriptionProviderOptions(
				useEnvironment: false,
				useUserDatabase: false,
				useSystemDatabases: false));
	}

	internal static SystemTerminalDescriptionProvider CreateSystemProvider()
	{
		return new SystemTerminalDescriptionProvider();
	}

	internal static TerminalDatabase CreateSystemWithBuiltInFallback(
		SystemTerminalDescriptionProvider? systemProvider = null)
	{
		SystemTerminalDescriptionProvider system =
			systemProvider
			?? CreateSystemProvider();

		return new TerminalDatabase(
			new ITerminalDescriptionProvider[]
			{
				system,
				TerminalDatabase.BuiltIn,
			});
	}
}
