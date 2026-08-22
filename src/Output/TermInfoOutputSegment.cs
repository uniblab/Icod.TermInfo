namespace Icod.TermInfo;

internal readonly record struct TermInfoOutputSegment
{
    private TermInfoOutputSegment(
        string? text,
        decimal milliseconds,
        bool multiplyByAffectedLines,
        bool isMandatory)
    {
        Text = text;
        Milliseconds = milliseconds;
        MultiplyByAffectedLines = multiplyByAffectedLines;
        IsMandatory = isMandatory;
    }

    internal string? Text { get; }

    internal decimal Milliseconds { get; }

    internal bool MultiplyByAffectedLines { get; }

    internal bool IsMandatory { get; }

    internal bool IsPadding => Text is null;

    internal static TermInfoOutputSegment CreateText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return new TermInfoOutputSegment(
            text,
            0m,
            false,
            false);
    }

    internal static TermInfoOutputSegment CreatePadding(
        decimal milliseconds,
        bool multiplyByAffectedLines,
        bool isMandatory)
    {
        if (milliseconds < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds));
        }

        return new TermInfoOutputSegment(
            null,
            milliseconds,
            multiplyByAffectedLines,
            isMandatory);
    }
}
