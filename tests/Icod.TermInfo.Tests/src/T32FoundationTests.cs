using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T32FoundationTests
{
    [Fact]
    public void AssemblyIdentifiesT32DevelopmentVersion()
    {
        Assembly assembly = typeof(TerminalDescription).Assembly;
        Version? assemblyVersion = assembly.GetName().Version;
        string? informationalVersion =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        Assert.NotNull(assemblyVersion);
        Assert.Equal(new Version(0, 9, 0, 0), assemblyVersion);
        Assert.NotNull(informationalVersion);
        Assert.True(
            informationalVersion!.StartsWith(
                "0.9.0-alpha.1",
                StringComparison.Ordinal),
            $"Unexpected informational version '{informationalVersion}'.");
    }

    [Fact]
    public void BuiltInDatabaseRemainsInMemoryOnly()
    {
        FieldInfo? providersField =
            typeof(TerminalDatabase).GetField(
                "_providers",
                BindingFlags.NonPublic
                | BindingFlags.Instance);

        Assert.NotNull(providersField);

        object? rawProviders =
            providersField!.GetValue(TerminalDatabase.BuiltIn);
        IReadOnlyList<ITerminalDescriptionProvider> providers =
            Assert.IsAssignableFrom<
                IReadOnlyList<ITerminalDescriptionProvider>>(
                    rawProviders);

        Assert.Single(providers);
        Assert.IsType<InMemoryTerminalDescriptionProvider>(
            providers[0]);

        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                "linux",
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Fact]
    public void AcquisitionImplementationRemainsReservedForLaterTranches()
    {
        string[] reservedTypeNames =
        [
            "CompiledTermInfoParserOptions",
            "CompiledTermInfoParser",
            "CompiledTermInfoFormatException",
            "DirectoryTerminalDescriptionProvider",
            "SystemTerminalDescriptionProviderOptions",
            "SystemTerminalDescriptionProvider",
        ];

        HashSet<string> actualTypeNames =
            typeof(TerminalDescription).Assembly
                .GetTypes()
                .Select(type => type.Name)
                .ToHashSet(StringComparer.Ordinal);

        Assert.All(
            reservedTypeNames,
            reserved => Assert.DoesNotContain(
                reserved,
                actualTypeNames));
    }
}
