using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TerminalProviderTests
{
    [Fact]
    public void InMemoryProviderLoadsCanonicalNameAndAliases()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("sample-a")
                .AddAlias("sample-b")
                .Build();

        InMemoryTerminalDescriptionProvider provider =
            new(new[] { terminal });

        Assert.True(provider.TryLoad("sample", out TerminalDescription? canonical));
        Assert.Same(terminal, canonical);

        Assert.True(provider.TryLoad("sample-a", out TerminalDescription? aliasA));
        Assert.Same(terminal, aliasA);

        Assert.True(provider.TryLoad("sample-b", out TerminalDescription? aliasB));
        Assert.Same(terminal, aliasB);
    }

    [Fact]
    public void InMemoryProviderUsesOrdinalCaseSensitiveNames()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("SAMPLE")
                .Build();

        InMemoryTerminalDescriptionProvider provider =
            new(new[] { terminal });

        Assert.True(provider.TryLoad("sample", out _));
        Assert.True(provider.TryLoad("SAMPLE", out _));
        Assert.False(provider.TryLoad("Sample", out _));
    }

    [Fact]
    public void InMemoryProviderRejectsDuplicateNamesAcrossDescriptions()
    {
        TerminalDescription first =
            new TerminalDescriptionBuilder("first")
                .AddAlias("shared")
                .Build();
        TerminalDescription second =
            new TerminalDescriptionBuilder("shared")
                .Build();

        Assert.Throws<ArgumentException>(
            () => new InMemoryTerminalDescriptionProvider(
                new[] { first, second }));
    }

    [Fact]
    public void DatabaseUsesFirstProviderWhichResolvesName()
    {
        TerminalDescription first =
            new TerminalDescriptionBuilder("shared")
                .SetNumber(NumericCapability.Columns, 80)
                .Build();
        TerminalDescription second =
            new TerminalDescriptionBuilder("shared")
                .SetNumber(NumericCapability.Columns, 132)
                .Build();

        TerminalDatabase database =
            new(
                new ITerminalDescriptionProvider[]
                {
                    new InMemoryTerminalDescriptionProvider(new[] { first }),
                    new InMemoryTerminalDescriptionProvider(new[] { second }),
                });

        TerminalDescription loaded = database.Load("shared");

        Assert.Same(first, loaded);
        Assert.Equal<int?>(80, loaded.GetNumber(NumericCapability.Columns));
    }

    [Fact]
    public void DatabaseSnapshotsProviderSequence()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample").Build();
        InMemoryTerminalDescriptionProvider provider =
            new(new[] { terminal });
        List<ITerminalDescriptionProvider> providers = [provider];

        TerminalDatabase database = new(providers);
        providers.Clear();

        Assert.Same(terminal, database.Load("sample"));
    }

    [Fact]
    public void DatabaseMayBeEmpty()
    {
        TerminalDatabase database =
            new(Array.Empty<ITerminalDescriptionProvider>());

        Assert.False(database.TryLoad("sample", out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Fact]
    public void DatabaseRejectsNullProviderEntries()
    {
        ITerminalDescriptionProvider[] providers =
        [
            null!,
        ];

        Assert.Throws<ArgumentException>(
            () => new TerminalDatabase(providers));
    }

    [Fact]
    public void InMemoryProviderSupportsConcurrentReads()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("sample-a")
                .Build();
        InMemoryTerminalDescriptionProvider provider =
            new(new[] { terminal });
        TerminalDatabase database =
            new(new ITerminalDescriptionProvider[] { provider });

        Parallel.For(
            0,
            256,
            _ =>
            {
                Assert.True(
                    database.TryLoad(
                        "sample-a",
                        out TerminalDescription? loaded));
                Assert.Same(terminal, loaded);
            });
    }
}
