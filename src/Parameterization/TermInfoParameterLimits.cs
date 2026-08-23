namespace Icod.TermInfo;

internal static class TermInfoParameterLimits
{
    internal const int MaximumSourceLength = 65_536;
    internal const int MaximumInstructionCount = 8_192;
    internal const int MaximumConditionalNesting = 64;
    internal const int MaximumStackDepth = 1_024;
    internal const int MaximumFormatWidth = 10_000;
    internal const int MaximumFormatPrecision = 10_000;
    internal const int MaximumOutputLength = 1_048_576;
}
