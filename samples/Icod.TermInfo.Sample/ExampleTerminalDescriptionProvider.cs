using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Sample;

internal sealed class ExampleTerminalDescriptionProvider : ITerminalDescriptionProvider
{
    private readonly InMemoryTerminalDescriptionProvider _inner;

    internal ExampleTerminalDescriptionProvider()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("example-terminal")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 80)
                .SetNumber(NumericCapability.Lines, 24)
                .SetString(
                    StringCapability.CursorAddress,
                    "\x1b[%i%p1%d;%p2%dH")
                .SetString(
                    StringCapability.ClearScreen,
                    "\x1b[H\x1b[J")
                .Build();

        _inner =
            new InMemoryTerminalDescriptionProvider(
                new[] { terminal });
    }

    public bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _inner.TryLoad(name, out terminal);
    }
}
