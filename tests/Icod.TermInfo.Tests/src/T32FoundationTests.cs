using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T32FoundationTests
{
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
    public void IoBackedProviderImplementationRemainsReservedForLaterTranches()
    {
        string[] reservedTypeNames =
        [
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
