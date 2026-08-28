using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[CollectionDefinition( Name, DisableParallelization = true )]
public sealed class EnvironmentSensitiveCollection {
	public const string Name = "infocmp environment";
}
