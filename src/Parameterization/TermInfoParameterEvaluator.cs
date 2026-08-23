using System.Text;

namespace Icod.TermInfo;

internal sealed class TermInfoParameterEvaluator
{
    private readonly TermInfoParameter[] _parameters;
    private readonly TermInfoParameter[] _dynamicVariables =
        new TermInfoParameter[26];
    private readonly TermInfoExpansionContext _context;
    private readonly TermInfoParameterProgramAnalysis _analysis;
    private readonly List<TermInfoParameter> _stack = [];
    private readonly StringBuilder _output = new();
    private int _implicitParameterIndex;

    private TermInfoParameterEvaluator(
        TermInfoParameter[] parameters,
        TermInfoExpansionContext context,
        TermInfoParameterProgramAnalysis analysis)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(analysis);

        _parameters = parameters;
        _context = context;
        _analysis = analysis;
    }

    internal static string Evaluate(
        IReadOnlyList<TermInfoInstruction> instructions,
        TermInfoParameterProgramAnalysis analysis,
        TermInfoParameter[] parameters,
        TermInfoExpansionContext context)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        ArgumentNullException.ThrowIfNull(analysis);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(context);

        if (parameters.Length > 9)
        {
            throw new ArgumentException(
                "Terminfo parameter programs accept at most nine parameters.",
                nameof(parameters));
        }

        TermInfoParameterEvaluator evaluator =
            new(
                parameters.ToArray(),
                context,
                analysis);

        lock (context.SyncRoot)
        {
            evaluator.Execute(instructions);
        }

        return evaluator._output.ToString();
    }

    private void Execute(IReadOnlyList<TermInfoInstruction> instructions)
    {
        ArgumentNullException.ThrowIfNull(instructions);

        foreach (TermInfoInstruction instruction in instructions)
        {
            Execute(instruction);
        }
    }

    private void Execute(TermInfoInstruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        switch (instruction)
        {
            case TermInfoLiteralInstruction literal:
                AppendOutput(
                    literal.Text,
                    literal.Position);
                break;
            case TermInfoPushParameterInstruction pushParameter:
                PushParameter(pushParameter);
                break;
            case TermInfoSetVariableInstruction setVariable:
                SetVariable(setVariable);
                break;
            case TermInfoGetVariableInstruction getVariable:
                GetVariable(getVariable);
                break;
            case TermInfoPushIntegerInstruction pushInteger:
                Push(
                    new TermInfoParameter(pushInteger.Value),
                    pushInteger.Position);
                break;
            case TermInfoPushCharacterInstruction pushCharacter:
                Push(
                    new TermInfoParameter((long)pushCharacter.Value),
                    pushCharacter.Position);
                break;
            case TermInfoStringLengthInstruction length:
                PushStringLength(length);
                break;
            case TermInfoBinaryInstruction binary:
                EvaluateBinary(binary);
                break;
            case TermInfoUnaryInstruction unary:
                EvaluateUnary(unary);
                break;
            case TermInfoIncrementParametersInstruction increment:
                IncrementParameters(increment);
                break;
            case TermInfoCharacterOutputInstruction characterOutput:
                OutputCharacter(characterOutput);
                break;
            case TermInfoFormatInstruction format:
                OutputFormatted(format);
                break;
            case TermInfoConditionalInstruction conditional:
                EvaluateConditional(conditional);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown terminfo instruction type '{instruction.GetType().FullName}'.");
        }
    }

    private void PushParameter(TermInfoPushParameterInstruction instruction)
    {
        int index = instruction.ParameterIndex;
        if (index < 0 || index >= 9)
        {
            throw new ArgumentOutOfRangeException(nameof(instruction));
        }

        if (index >= _parameters.Length)
        {
            throw new TermInfoEvaluationException(
                $"Parameter p{index + 1} was not supplied",
                instruction.Position);
        }

        Push(
            _parameters[index],
            instruction.Position);
    }

    private void SetVariable(TermInfoSetVariableInstruction instruction)
    {
        TermInfoParameter value = Pop(instruction.Position);
        char name = instruction.VariableName;

        if (name is >= 'a' and <= 'z')
        {
            _dynamicVariables[name - 'a'] = value;
        }
        else if (name is >= 'A' and <= 'Z')
        {
            _context.SetStaticVariable(name, value);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private void GetVariable(TermInfoGetVariableInstruction instruction)
    {
        char name = instruction.VariableName;

        if (name is >= 'a' and <= 'z')
        {
            Push(
                _dynamicVariables[name - 'a'],
                instruction.Position);
        }
        else if (name is >= 'A' and <= 'Z')
        {
            Push(
                _context.GetStaticVariable(name),
                instruction.Position);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private void PushStringLength(TermInfoStringLengthInstruction instruction)
    {
        TermInfoParameter value = Pop(instruction.Position);
        if (!value.IsString)
        {
            throw new TermInfoEvaluationException(
                "The %l operator requires a string value",
                instruction.Position);
        }

        Push(
            new TermInfoParameter((long)value.StringValue.Length),
            instruction.Position);
    }

    private void EvaluateBinary(TermInfoBinaryInstruction instruction)
    {
        long right = PopInteger(instruction.Position);
        long left = PopInteger(instruction.Position);

        long result = instruction.Operator switch
        {
            TermInfoBinaryOperator.Add =>
                Add(
                    left,
                    right,
                    instruction.Position),
            TermInfoBinaryOperator.Subtract =>
                Subtract(
                    left,
                    right,
                    instruction.Position),
            TermInfoBinaryOperator.Multiply =>
                Multiply(
                    left,
                    right,
                    instruction.Position),
            TermInfoBinaryOperator.Divide =>
                Divide(
                    left,
                    right,
                    instruction.Position),
            TermInfoBinaryOperator.Modulo =>
                Modulo(
                    left,
                    right,
                    instruction.Position),
            TermInfoBinaryOperator.BitwiseAnd => left & right,
            TermInfoBinaryOperator.BitwiseOr => left | right,
            TermInfoBinaryOperator.BitwiseXor => left ^ right,
            TermInfoBinaryOperator.Equal => left == right ? 1 : 0,
            TermInfoBinaryOperator.GreaterThan => left > right ? 1 : 0,
            TermInfoBinaryOperator.LessThan => left < right ? 1 : 0,
            TermInfoBinaryOperator.LogicalAnd => left != 0 && right != 0 ? 1 : 0,
            TermInfoBinaryOperator.LogicalOr => left != 0 || right != 0 ? 1 : 0,
            _ => throw new ArgumentOutOfRangeException(nameof(instruction)),
        };

        Push(
            new TermInfoParameter(result),
            instruction.Position);
    }

    private void EvaluateUnary(TermInfoUnaryInstruction instruction)
    {
        long value = PopInteger(instruction.Position);
        long result = instruction.Operator switch
        {
            TermInfoUnaryOperator.LogicalNot => value == 0 ? 1 : 0,
            TermInfoUnaryOperator.BitwiseNot => ~value,
            _ => throw new ArgumentOutOfRangeException(nameof(instruction)),
        };

        Push(
            new TermInfoParameter(result),
            instruction.Position);
    }

    private void IncrementParameters(TermInfoIncrementParametersInstruction instruction)
    {
        for (int i = 0; i < Math.Min(2, _parameters.Length); i++)
        {
            TermInfoParameter parameter = _parameters[i];
            if (!parameter.IsInteger)
            {
                throw new TermInfoEvaluationException(
                    $"The %i operator requires parameter p{i + 1} to be an integer",
                    instruction.Position);
            }

            _parameters[i] =
                new TermInfoParameter(
                    Increment(
                        parameter.IntegerValue,
                        instruction.Position));
        }
    }

    private void OutputCharacter(TermInfoCharacterOutputInstruction instruction)
    {
        TermInfoParameter parameter =
            PopOutputValue(instruction.Position);
        if (!parameter.IsInteger)
        {
            throw new TermInfoEvaluationException(
                "The %c conversion requires an integer value",
                instruction.Position);
        }

        long value = parameter.IntegerValue;
        if (value is < byte.MinValue or > byte.MaxValue)
        {
            throw new TermInfoEvaluationException(
                "The %c conversion requires a value from 0 through 255",
                instruction.Position);
        }

        AppendOutput(
            (char)value,
            instruction.Position);
    }

    private void OutputFormatted(TermInfoFormatInstruction instruction)
    {
        TermInfoParameter value =
            PopOutputValue(instruction.Position);
        string formatted =
            TermInfoFormatter.Format(
                instruction.Specification,
                value,
                instruction.Position);

        AppendOutput(
            formatted,
            instruction.Position);
    }

    private void EvaluateConditional(TermInfoConditionalInstruction instruction)
    {
        foreach (TermInfoConditionalBranch branch in instruction.Branches)
        {
            Execute(branch.Condition);
            long condition = PopInteger(instruction.Position);
            if (condition != 0)
            {
                Execute(branch.Body);
                return;
            }
        }

        Execute(instruction.ElseInstructions);
    }

    private void Push(
        TermInfoParameter value,
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (_stack.Count
            >= TermInfoParameterLimits.MaximumStackDepth)
        {
            throw new TermInfoEvaluationException(
                $"The terminfo parameter stack cannot exceed "
                + $"{TermInfoParameterLimits.MaximumStackDepth} values",
                position);
        }

        _stack.Add(value);
    }

    private TermInfoParameter Pop(int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (_stack.Count == 0)
        {
            throw new TermInfoEvaluationException(
                "The terminfo parameter stack is empty",
                position);
        }

        int last = _stack.Count - 1;
        TermInfoParameter value = _stack[last];
        _stack.RemoveAt(last);
        return value;
    }

    private TermInfoParameter PopOutputValue(int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (_stack.Count > 0)
        {
            return Pop(position);
        }

        if (!_analysis.UsesImplicitFormatParameters)
        {
            return Pop(position);
        }

        if (_implicitParameterIndex >= 9)
        {
            throw new TermInfoEvaluationException(
                "The terminfo parameter program requires more than nine parameters",
                position);
        }

        int index = _implicitParameterIndex;
        _implicitParameterIndex++;

        if (index >= _parameters.Length)
        {
            throw new TermInfoEvaluationException(
                $"Parameter p{index + 1} was not supplied",
                position);
        }

        return _parameters[index];
    }

    private long PopInteger(int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        TermInfoParameter value = Pop(position);
        if (!value.IsInteger)
        {
            throw new TermInfoEvaluationException(
                "This terminfo operation requires an integer value",
                position);
        }

        return value.IntegerValue;
    }

    private void AppendOutput(
        string value,
        int position)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (value.Length
            > TermInfoParameterLimits.MaximumOutputLength
                - _output.Length)
        {
            throw new TermInfoEvaluationException(
                $"Expanded terminfo output cannot exceed "
                + $"{TermInfoParameterLimits.MaximumOutputLength} characters",
                position);
        }

        _output.Append(value);
    }

    private void AppendOutput(
        char value,
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (_output.Length
            >= TermInfoParameterLimits.MaximumOutputLength)
        {
            throw new TermInfoEvaluationException(
                $"Expanded terminfo output cannot exceed "
                + $"{TermInfoParameterLimits.MaximumOutputLength} characters",
                position);
        }

        _output.Append(value);
    }

    private static long Add(
        long left,
        long right,
        int position)
    {
        return EvaluateChecked(
            () => checked(left + right),
            position);
    }

    private static long Subtract(
        long left,
        long right,
        int position)
    {
        return EvaluateChecked(
            () => checked(left - right),
            position);
    }

    private static long Multiply(
        long left,
        long right,
        int position)
    {
        return EvaluateChecked(
            () => checked(left * right),
            position);
    }

    private static long Increment(
        long value,
        int position)
    {
        return EvaluateChecked(
            () => checked(value + 1),
            position);
    }

    private static long Divide(
        long left,
        long right,
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (right == 0)
        {
            throw new TermInfoEvaluationException(
                "Division by zero",
                position);
        }

        if (left == long.MinValue && right == -1)
        {
            throw new TermInfoEvaluationException(
                "Terminfo arithmetic overflow",
                position);
        }

        return left / right;
    }

    private static long Modulo(
        long left,
        long right,
        int position)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (right == 0)
        {
            throw new TermInfoEvaluationException(
                "Modulo by zero",
                position);
        }

        if (left == long.MinValue && right == -1)
        {
            return 0;
        }

        return left % right;
    }

    private static long EvaluateChecked(
        Func<long> operation,
        int position)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        try
        {
            return operation();
        }
        catch (OverflowException)
        {
            throw new TermInfoEvaluationException(
                "Terminfo arithmetic overflow",
                position);
        }
    }
}
