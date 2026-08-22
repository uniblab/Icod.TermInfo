namespace Icod.TermInfo;

internal abstract record TermInfoInstruction(int Position);

internal sealed record TermInfoLiteralInstruction(
    int Position,
    string Text)
    : TermInfoInstruction(Position);

internal sealed record TermInfoPushParameterInstruction(
    int Position,
    int ParameterIndex)
    : TermInfoInstruction(Position);

internal sealed record TermInfoSetVariableInstruction(
    int Position,
    char VariableName)
    : TermInfoInstruction(Position);

internal sealed record TermInfoGetVariableInstruction(
    int Position,
    char VariableName)
    : TermInfoInstruction(Position);

internal sealed record TermInfoPushIntegerInstruction(
    int Position,
    long Value)
    : TermInfoInstruction(Position);

internal sealed record TermInfoPushCharacterInstruction(
    int Position,
    char Value)
    : TermInfoInstruction(Position);

internal sealed record TermInfoStringLengthInstruction(int Position)
    : TermInfoInstruction(Position);

internal sealed record TermInfoBinaryInstruction(
    int Position,
    TermInfoBinaryOperator Operator)
    : TermInfoInstruction(Position);

internal sealed record TermInfoUnaryInstruction(
    int Position,
    TermInfoUnaryOperator Operator)
    : TermInfoInstruction(Position);

internal sealed record TermInfoIncrementParametersInstruction(int Position)
    : TermInfoInstruction(Position);

internal sealed record TermInfoCharacterOutputInstruction(int Position)
    : TermInfoInstruction(Position);

internal sealed record TermInfoFormatInstruction(
    int Position,
    TermInfoFormatSpecification Specification)
    : TermInfoInstruction(Position);

internal sealed record TermInfoConditionalInstruction(
    int Position,
    IReadOnlyList<TermInfoConditionalBranch> Branches,
    IReadOnlyList<TermInfoInstruction> ElseInstructions)
    : TermInfoInstruction(Position);

internal sealed record TermInfoConditionalBranch(
    IReadOnlyList<TermInfoInstruction> Condition,
    IReadOnlyList<TermInfoInstruction> Body);

internal enum TermInfoBinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    Equal,
    GreaterThan,
    LessThan,
    LogicalAnd,
    LogicalOr,
}

internal enum TermInfoUnaryOperator
{
    LogicalNot,
    BitwiseNot,
}

internal readonly record struct TermInfoFormatSpecification(
    char Conversion,
    bool LeftJustify,
    bool AlwaysSign,
    bool SpaceSign,
    bool AlternateForm,
    bool ZeroPad,
    int? Width,
    int? Precision);
