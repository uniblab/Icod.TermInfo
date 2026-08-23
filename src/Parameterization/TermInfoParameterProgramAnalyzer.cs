namespace Icod.TermInfo;

internal static class TermInfoParameterProgramAnalyzer
{
    internal static TermInfoParameterProgramAnalysis Analyze(
        IReadOnlyList<TermInfoInstruction> instructions)
    {
        ArgumentNullException.ThrowIfNull(instructions);

        StructuralInspection inspection = new();
        InspectStructure(
            instructions,
            0,
            inspection);

        bool usesImplicitFormatParameters =
            !inspection.ContainsExplicitParameterPush;

        AnalysisEngine engine =
            new(usesImplicitFormatParameters);
        AnalysisState initial = new();

        engine.AnalyzeSequence(
            instructions,
            [initial]);

        return engine.CreateAnalysis(
            inspection.InstructionCount,
            inspection.MaximumConditionalNesting);
    }

    private static void InspectStructure(
        IReadOnlyList<TermInfoInstruction> instructions,
        int conditionalNesting,
        StructuralInspection inspection)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentOutOfRangeException.ThrowIfNegative(conditionalNesting);

        foreach (TermInfoInstruction instruction in instructions)
        {
            inspection.InstructionCount++;
            if (inspection.InstructionCount
                > TermInfoParameterLimits.MaximumInstructionCount)
            {
                throw new TermInfoFormatException(
                    $"Parameter programs cannot exceed "
                    + $"{TermInfoParameterLimits.MaximumInstructionCount} "
                    + "parsed instructions",
                    instruction.Position);
            }

            if (instruction is TermInfoPushParameterInstruction)
            {
                inspection.ContainsExplicitParameterPush = true;
            }

            if (instruction is not TermInfoConditionalInstruction conditional)
            {
                continue;
            }

            int nestedLevel = conditionalNesting + 1;
            if (nestedLevel
                > TermInfoParameterLimits.MaximumConditionalNesting)
            {
                throw new TermInfoFormatException(
                    $"Conditional nesting cannot exceed "
                    + $"{TermInfoParameterLimits.MaximumConditionalNesting}",
                    conditional.Position);
            }

            inspection.MaximumConditionalNesting =
                Math.Max(
                    inspection.MaximumConditionalNesting,
                    nestedLevel);

            foreach (TermInfoConditionalBranch branch in conditional.Branches)
            {
                InspectStructure(
                    branch.Condition,
                    nestedLevel,
                    inspection);
                InspectStructure(
                    branch.Body,
                    nestedLevel,
                    inspection);
            }

            InspectStructure(
                conditional.ElseInstructions,
                nestedLevel,
                inspection);
        }
    }

    private sealed class StructuralInspection
    {
        internal int InstructionCount { get; set; }

        internal int MaximumConditionalNesting { get; set; }

        internal bool ContainsExplicitParameterPush { get; set; }
    }

    private sealed class AnalysisEngine
    {
        private readonly bool _usesImplicitFormatParameters;
        private readonly bool[] _referencedParameters = new bool[9];
        private readonly TermInfoParameterUsage[] _parameterUsages =
            new TermInfoParameterUsage[9];
        private readonly HashSet<char> _dynamicVariables = new();
        private readonly HashSet<char> _staticVariables = new();
        private int _maximumStackDepth;
        private bool _mayUnderflow;

        internal AnalysisEngine(bool usesImplicitFormatParameters)
        {
            _usesImplicitFormatParameters = usesImplicitFormatParameters;
        }

        internal TermInfoParameterProgramAnalysis CreateAnalysis(
            int instructionCount,
            int maximumConditionalNesting)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(instructionCount);
            ArgumentOutOfRangeException.ThrowIfNegative(
                maximumConditionalNesting);

            int[] referenced =
                Enumerable
                    .Range(0, _referencedParameters.Length)
                    .Where(index => _referencedParameters[index])
                    .ToArray();
            int highest =
                referenced.Length == 0
                    ? -1
                    : referenced[^1];

            return new TermInfoParameterProgramAnalysis(
                _usesImplicitFormatParameters,
                referenced,
                highest,
                _parameterUsages,
                _dynamicVariables
                    .OrderBy(value => value)
                    .ToArray(),
                _staticVariables
                    .OrderBy(value => value)
                    .ToArray(),
                instructionCount,
                maximumConditionalNesting,
                _maximumStackDepth,
                _mayUnderflow);
        }

        internal List<AnalysisState> AnalyzeSequence(
            IReadOnlyList<TermInfoInstruction> instructions,
            List<AnalysisState> states)
        {
            ArgumentNullException.ThrowIfNull(instructions);
            ArgumentNullException.ThrowIfNull(states);

            List<AnalysisState> current =
                MergeStates(states);

            foreach (TermInfoInstruction instruction in instructions)
            {
                if (current.Count == 0)
                {
                    break;
                }

                current =
                    instruction is TermInfoConditionalInstruction conditional
                        ? AnalyzeConditional(
                            conditional,
                            current)
                        : AnalyzeSimple(
                            instruction,
                            current);
            }

            return current;
        }

        private List<AnalysisState> AnalyzeSimple(
            TermInfoInstruction instruction,
            List<AnalysisState> states)
        {
            ArgumentNullException.ThrowIfNull(instruction);
            ArgumentNullException.ThrowIfNull(states);

            List<AnalysisState> survivors = [];

            foreach (AnalysisState state in states)
            {
                if (ApplySimple(
                        instruction,
                        state))
                {
                    survivors.Add(state);
                }
            }

            return MergeStates(survivors);
        }

        private bool ApplySimple(
            TermInfoInstruction instruction,
            AnalysisState state)
        {
            ArgumentNullException.ThrowIfNull(instruction);
            ArgumentNullException.ThrowIfNull(state);

            switch (instruction)
            {
                case TermInfoLiteralInstruction:
                    return true;

                case TermInfoPushParameterInstruction pushParameter:
                    RecordParameterReference(
                        pushParameter.ParameterIndex);
                    return Push(
                        state,
                        new AbstractValue(
                            1 << pushParameter.ParameterIndex),
                        pushParameter.Position);

                case TermInfoSetVariableInstruction setVariable:
                    if (!TryPop(
                            state,
                            setVariable.Position,
                            out AbstractValue setValue))
                    {
                        return false;
                    }

                    RecordReferences(setValue);
                    SetVariable(
                        state,
                        setVariable.VariableName,
                        setValue);
                    return true;

                case TermInfoGetVariableInstruction getVariable:
                    return Push(
                        state,
                        GetVariable(
                            state,
                            getVariable.VariableName),
                        getVariable.Position);

                case TermInfoPushIntegerInstruction pushInteger:
                    return Push(
                        state,
                        AbstractValue.None,
                        pushInteger.Position);

                case TermInfoPushCharacterInstruction pushCharacter:
                    return Push(
                        state,
                        AbstractValue.None,
                        pushCharacter.Position);

                case TermInfoStringLengthInstruction length:
                    if (!TryPopRequired(
                            state,
                            length.Position,
                            TermInfoParameterUsage.String,
                            out _))
                    {
                        return false;
                    }

                    return Push(
                        state,
                        AbstractValue.None,
                        length.Position);

                case TermInfoBinaryInstruction binary:
                    if (!TryPopRequired(
                            state,
                            binary.Position,
                            TermInfoParameterUsage.Integer,
                            out _)
                        || !TryPopRequired(
                            state,
                            binary.Position,
                            TermInfoParameterUsage.Integer,
                            out _))
                    {
                        return false;
                    }

                    return Push(
                        state,
                        AbstractValue.None,
                        binary.Position);

                case TermInfoUnaryInstruction unary:
                    if (!TryPopRequired(
                            state,
                            unary.Position,
                            TermInfoParameterUsage.Integer,
                            out _))
                    {
                        return false;
                    }

                    return Push(
                        state,
                        AbstractValue.None,
                        unary.Position);

                case TermInfoIncrementParametersInstruction:
                    RecordParameterUse(
                        0,
                        TermInfoParameterUsage.Integer);
                    RecordParameterUse(
                        1,
                        TermInfoParameterUsage.Integer);
                    return true;

                case TermInfoCharacterOutputInstruction characterOutput:
                    return TryPopOutputRequired(
                        state,
                        characterOutput.Position,
                        TermInfoParameterUsage.Integer);

                case TermInfoFormatInstruction format:
                    TermInfoParameterUsage usage =
                        format.Specification.Conversion == 's'
                            ? TermInfoParameterUsage.String
                            : TermInfoParameterUsage.Integer;
                    return TryPopOutputRequired(
                        state,
                        format.Position,
                        usage);

                default:
                    throw new InvalidOperationException(
                        $"Unsupported parameter instruction "
                        + $"'{instruction.GetType().FullName}'.");
            }
        }

        private List<AnalysisState> AnalyzeConditional(
            TermInfoConditionalInstruction conditional,
            List<AnalysisState> states)
        {
            ArgumentNullException.ThrowIfNull(conditional);
            ArgumentNullException.ThrowIfNull(states);

            List<AnalysisState> remaining =
                CloneStates(states);
            List<AnalysisState> outputs = [];

            foreach (TermInfoConditionalBranch branch in conditional.Branches)
            {
                List<AnalysisState> conditionStates =
                    AnalyzeSequence(
                        branch.Condition,
                        remaining);
                List<AnalysisState> postCondition = [];

                foreach (AnalysisState state in conditionStates)
                {
                    if (TryPopRequired(
                            state,
                            conditional.Position,
                            TermInfoParameterUsage.Integer,
                            out _))
                    {
                        postCondition.Add(state);
                    }
                }

                outputs.AddRange(
                    AnalyzeSequence(
                        branch.Body,
                        CloneStates(postCondition)));

                remaining = postCondition;
            }

            outputs.AddRange(
                AnalyzeSequence(
                    conditional.ElseInstructions,
                    remaining));

            return MergeStates(outputs);
        }

        private bool Push(
            AnalysisState state,
            AbstractValue value,
            int position)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentOutOfRangeException.ThrowIfNegative(position);

            if (state.Stack.Count
                >= TermInfoParameterLimits.MaximumStackDepth)
            {
                throw new TermInfoFormatException(
                    $"Parameter-program stack depth cannot exceed "
                    + $"{TermInfoParameterLimits.MaximumStackDepth}",
                    position);
            }

            state.Stack.Add(value);
            _maximumStackDepth =
                Math.Max(
                    _maximumStackDepth,
                    state.Stack.Count);
            return true;
        }

        private bool TryPop(
            AnalysisState state,
            int position,
            out AbstractValue value)
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentOutOfRangeException.ThrowIfNegative(position);

            if (state.Stack.Count == 0)
            {
                _mayUnderflow = true;
                value = AbstractValue.None;
                return false;
            }

            int last = state.Stack.Count - 1;
            value = state.Stack[last];
            state.Stack.RemoveAt(last);
            return true;
        }

        private bool TryPopRequired(
            AnalysisState state,
            int position,
            TermInfoParameterUsage usage,
            out AbstractValue value)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (!TryPop(
                    state,
                    position,
                    out value))
            {
                return false;
            }

            RecordUsage(
                value,
                usage);
            return true;
        }

        private bool TryPopOutputRequired(
            AnalysisState state,
            int position,
            TermInfoParameterUsage usage)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (state.Stack.Count > 0)
            {
                return TryPopRequired(
                    state,
                    position,
                    usage,
                    out _);
            }

            if (!_usesImplicitFormatParameters)
            {
                _mayUnderflow = true;
                return false;
            }

            if (state.ImplicitParameterIndex >= 9)
            {
                _mayUnderflow = true;
                return false;
            }

            int parameterIndex =
                state.ImplicitParameterIndex;
            state.ImplicitParameterIndex++;

            RecordParameterUse(
                parameterIndex,
                usage);
            return true;
        }

        private void SetVariable(
            AnalysisState state,
            char name,
            AbstractValue value)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (name is >= 'a' and <= 'z')
            {
                _dynamicVariables.Add(name);
                state.DynamicVariables[name - 'a'] = value;
                return;
            }

            if (name is >= 'A' and <= 'Z')
            {
                _staticVariables.Add(name);
                state.StaticVariables[name - 'A'] = value;
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }

        private AbstractValue GetVariable(
            AnalysisState state,
            char name)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (name is >= 'a' and <= 'z')
            {
                _dynamicVariables.Add(name);
                return state.DynamicVariables[name - 'a'];
            }

            if (name is >= 'A' and <= 'Z')
            {
                _staticVariables.Add(name);
                return state.StaticVariables[name - 'A'];
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }

        private void RecordUsage(
            AbstractValue value,
            TermInfoParameterUsage usage)
        {
            if (usage == TermInfoParameterUsage.None)
            {
                throw new ArgumentOutOfRangeException(nameof(usage));
            }

            RecordReferences(value);

            for (int index = 0; index < 9; index++)
            {
                if ((value.ParameterMask & (1 << index)) == 0)
                {
                    continue;
                }

                _parameterUsages[index] |= usage;
            }
        }

        private void RecordReferences(AbstractValue value)
        {
            for (int index = 0; index < 9; index++)
            {
                if ((value.ParameterMask & (1 << index)) != 0)
                {
                    RecordParameterReference(index);
                }
            }
        }

        private void RecordParameterUse(
            int index,
            TermInfoParameterUsage usage)
        {
            RecordParameterReference(index);
            _parameterUsages[index] |= usage;
        }

        private void RecordParameterReference(int index)
        {
            if (index is < 0 or > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _referencedParameters[index] = true;
        }

        private static List<AnalysisState> CloneStates(
            IEnumerable<AnalysisState> states)
        {
            ArgumentNullException.ThrowIfNull(states);

            return states
                .Select(state => state.Clone())
                .ToList();
        }

        private static List<AnalysisState> MergeStates(
            IEnumerable<AnalysisState> states)
        {
            ArgumentNullException.ThrowIfNull(states);

            Dictionary<(int StackDepth, int ImplicitIndex), AnalysisState>
                byShape = new();

            foreach (AnalysisState state in states)
            {
                (int StackDepth, int ImplicitIndex) key =
                    (
                        state.Stack.Count,
                        state.ImplicitParameterIndex);

                if (!byShape.TryGetValue(
                        key,
                        out AnalysisState? existing))
                {
                    byShape.Add(
                        key,
                        state);
                    continue;
                }

                existing.MergeFrom(state);
            }

            return byShape
                .OrderBy(pair => pair.Key.StackDepth)
                .ThenBy(pair => pair.Key.ImplicitIndex)
                .Select(pair => pair.Value)
                .ToList();
        }
    }

    private readonly record struct AbstractValue(int ParameterMask)
    {
        internal static AbstractValue None => new(0);

        internal AbstractValue Merge(AbstractValue other)
        {
            return new AbstractValue(
                ParameterMask
                | other.ParameterMask);
        }
    }

    private sealed class AnalysisState
    {
        internal List<AbstractValue> Stack { get; } = [];

        internal AbstractValue[] DynamicVariables { get; } =
            new AbstractValue[26];

        internal AbstractValue[] StaticVariables { get; } =
            new AbstractValue[26];

        internal int ImplicitParameterIndex { get; set; }

        internal AnalysisState Clone()
        {
            AnalysisState clone =
                new()
                {
                    ImplicitParameterIndex =
                        ImplicitParameterIndex,
                };

            clone.Stack.AddRange(Stack);
            Array.Copy(
                DynamicVariables,
                clone.DynamicVariables,
                DynamicVariables.Length);
            Array.Copy(
                StaticVariables,
                clone.StaticVariables,
                StaticVariables.Length);

            return clone;
        }

        internal void MergeFrom(AnalysisState other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (Stack.Count != other.Stack.Count
                || ImplicitParameterIndex
                    != other.ImplicitParameterIndex)
            {
                throw new ArgumentException(
                    "Only analysis states with the same execution shape can be merged.",
                    nameof(other));
            }

            for (int index = 0; index < Stack.Count; index++)
            {
                Stack[index] =
                    Stack[index].Merge(
                        other.Stack[index]);
            }

            for (int index = 0;
                index < DynamicVariables.Length;
                index++)
            {
                DynamicVariables[index] =
                    DynamicVariables[index].Merge(
                        other.DynamicVariables[index]);
                StaticVariables[index] =
                    StaticVariables[index].Merge(
                        other.StaticVariables[index]);
            }
        }
    }
}
