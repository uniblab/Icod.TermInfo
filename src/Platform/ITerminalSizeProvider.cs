namespace Icod.TermInfo;

internal interface ITerminalSizeProvider
{
    bool TryGetSize(
        TerminalStandardStream stream,
        out TerminalSize size);
}
