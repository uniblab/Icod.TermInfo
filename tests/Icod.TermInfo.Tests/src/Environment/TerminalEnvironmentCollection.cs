using Xunit;

namespace Icod.TermInfo.Tests;

[CollectionDefinition(
    "TerminalEnvironment",
    DisableParallelization = true)]
public sealed class TerminalEnvironmentCollection
{
    public const string Name = "TerminalEnvironment";
}
