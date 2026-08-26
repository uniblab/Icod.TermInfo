using Icod.TermInfo;

namespace Icod.TermInfo.Source;

/// <summary>
/// Holds the mutable semantic capability state used while resolving source
/// entries.
/// </summary>
/// <remarks>
/// This type is intentionally internal. Source cancellation tombstones are
/// required while resolving <c>use=</c> inheritance, but they are not part of
/// the stable runtime <see cref="TerminalDescription"/> model.
/// </remarks>
internal sealed class TermInfoSourceCapabilityState
{
    private readonly HashSet<BooleanCapability> _booleanCapabilities = [];
    private readonly Dictionary<NumericCapability, int> _numericCapabilities = [];
    private readonly Dictionary<StringCapability, string> _stringCapabilities = [];
    private readonly Dictionary<string, TermInfoCapabilityValue> _extendedCapabilities =
        new(StringComparer.Ordinal);

    private readonly HashSet<BooleanCapability> _canceledBooleanCapabilities = [];
    private readonly HashSet<NumericCapability> _canceledNumericCapabilities = [];
    private readonly HashSet<StringCapability> _canceledStringCapabilities = [];
    private readonly HashSet<string> _canceledExtendedCapabilities =
        new(StringComparer.Ordinal);

    private TermInfoSourceCapabilityState()
    {
    }

    internal IReadOnlySet<BooleanCapability> BooleanCapabilities =>
        _booleanCapabilities;

    internal IReadOnlyDictionary<NumericCapability, int> NumericCapabilities =>
        _numericCapabilities;

    internal IReadOnlyDictionary<StringCapability, string> StringCapabilities =>
        _stringCapabilities;

    internal IReadOnlyDictionary<string, TermInfoCapabilityValue> ExtendedCapabilities =>
        _extendedCapabilities;

    internal IReadOnlySet<BooleanCapability> CanceledBooleanCapabilities =>
        _canceledBooleanCapabilities;

    internal IReadOnlySet<NumericCapability> CanceledNumericCapabilities =>
        _canceledNumericCapabilities;

    internal IReadOnlySet<StringCapability> CanceledStringCapabilities =>
        _canceledStringCapabilities;

    internal IReadOnlySet<string> CanceledExtendedCapabilities =>
        _canceledExtendedCapabilities;

    internal static TermInfoSourceCapabilityState CreateEmpty()
    {
        return new TermInfoSourceCapabilityState();
    }

    internal static TermInfoSourceCapabilityState CreateLocal(
        TermInfoSourceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        TermInfoSourceCapabilityState state =
            new();

        foreach (TermInfoSourceField field in entry.Fields)
        {
            state.ApplyLocalField(field);
        }

        return state;
    }

    internal TermInfoSourceCapabilityState Clone()
    {
        TermInfoSourceCapabilityState clone =
            new();

        clone._booleanCapabilities.UnionWith(_booleanCapabilities);
        CopyDictionary(
            _numericCapabilities,
            clone._numericCapabilities);
        CopyDictionary(
            _stringCapabilities,
            clone._stringCapabilities);
        CopyDictionary(
            _extendedCapabilities,
            clone._extendedCapabilities);

        clone._canceledBooleanCapabilities.UnionWith(
            _canceledBooleanCapabilities);
        clone._canceledNumericCapabilities.UnionWith(
            _canceledNumericCapabilities);
        clone._canceledStringCapabilities.UnionWith(
            _canceledStringCapabilities);
        clone._canceledExtendedCapabilities.UnionWith(
            _canceledExtendedCapabilities);

        return clone;
    }

    /// <summary>
    /// Merges a lower-priority, already-resolved source state into this state.
    /// </summary>
    /// <remarks>
    /// Values and cancellations already present in this state win. S07 will use
    /// this operation to apply a completed parent aggregate beneath the child's
    /// explicit local state.
    /// </remarks>
    internal TermInfoSourceCapabilityState Inherit(
        TermInfoSourceCapabilityState source)
    {
        ArgumentNullException.ThrowIfNull(source);

        foreach (BooleanCapability capability in source._booleanCapabilities)
        {
            if (!_booleanCapabilities.Contains(capability)
                && !_canceledBooleanCapabilities.Contains(capability))
            {
                _booleanCapabilities.Add(capability);
            }
        }

        foreach (
            KeyValuePair<NumericCapability, int> pair
            in source._numericCapabilities)
        {
            if (!_numericCapabilities.ContainsKey(pair.Key)
                && !_canceledNumericCapabilities.Contains(pair.Key))
            {
                _numericCapabilities.Add(
                    pair.Key,
                    pair.Value);
            }
        }

        foreach (
            KeyValuePair<StringCapability, string> pair
            in source._stringCapabilities)
        {
            if (!_stringCapabilities.ContainsKey(pair.Key)
                && !_canceledStringCapabilities.Contains(pair.Key))
            {
                _stringCapabilities.Add(
                    pair.Key,
                    pair.Value);
            }
        }

        foreach (
            KeyValuePair<string, TermInfoCapabilityValue> pair
            in source._extendedCapabilities)
        {
            if (!_extendedCapabilities.ContainsKey(pair.Key)
                && !_canceledExtendedCapabilities.Contains(pair.Key))
            {
                _extendedCapabilities.Add(
                    pair.Key,
                    pair.Value);
            }
        }

        foreach (
            BooleanCapability capability
            in source._canceledBooleanCapabilities)
        {
            if (!_booleanCapabilities.Contains(capability)
                && !_canceledBooleanCapabilities.Contains(capability))
            {
                _canceledBooleanCapabilities.Add(capability);
            }
        }

        foreach (
            NumericCapability capability
            in source._canceledNumericCapabilities)
        {
            if (!_numericCapabilities.ContainsKey(capability)
                && !_canceledNumericCapabilities.Contains(capability))
            {
                _canceledNumericCapabilities.Add(capability);
            }
        }

        foreach (
            StringCapability capability
            in source._canceledStringCapabilities)
        {
            if (!_stringCapabilities.ContainsKey(capability)
                && !_canceledStringCapabilities.Contains(capability))
            {
                _canceledStringCapabilities.Add(capability);
            }
        }

        foreach (string name in source._canceledExtendedCapabilities)
        {
            if (!_extendedCapabilities.ContainsKey(name)
                && !_canceledExtendedCapabilities.Contains(name))
            {
                _canceledExtendedCapabilities.Add(name);
            }
        }

        return this;
    }

    /// <summary>
    /// Overlays a higher-priority resolved parent state onto this parent
    /// aggregate.
    /// </summary>
    /// <remarks>
    /// The source state wins collisions. S07 will process <c>use=</c> parents
    /// from right to left, calling this operation as it moves leftward.
    /// </remarks>
    internal TermInfoSourceCapabilityState OverlayHigherPriority(
        TermInfoSourceCapabilityState source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (ReferenceEquals(
                this,
                source))
        {
            return this;
        }

        foreach (BooleanCapability capability in source._booleanCapabilities)
        {
            SetBoolean(capability);
        }

        foreach (
            KeyValuePair<NumericCapability, int> pair
            in source._numericCapabilities)
        {
            SetNumber(
                pair.Key,
                pair.Value);
        }

        foreach (
            KeyValuePair<StringCapability, string> pair
            in source._stringCapabilities)
        {
            SetString(
                pair.Key,
                pair.Value);
        }

        foreach (
            KeyValuePair<string, TermInfoCapabilityValue> pair
            in source._extendedCapabilities)
        {
            SetExtended(
                pair.Key,
                pair.Value);
        }

        foreach (
            BooleanCapability capability
            in source._canceledBooleanCapabilities)
        {
            CancelBoolean(capability);
        }

        foreach (
            NumericCapability capability
            in source._canceledNumericCapabilities)
        {
            CancelNumber(capability);
        }

        foreach (
            StringCapability capability
            in source._canceledStringCapabilities)
        {
            CancelString(capability);
        }

        foreach (string name in source._canceledExtendedCapabilities)
        {
            CancelExtended(name);
        }

        return this;
    }

    private void ApplyLocalField(
        TermInfoSourceField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.Kind == TermInfoSourceFieldKind.UseReference
            || field.Kind == TermInfoSourceFieldKind.DisabledCapability
            || field.CapabilityClassification is null
            || field.CapabilityClassification
                == TermInfoSourceCapabilityClassification.Invalid)
        {
            return;
        }

        if (field.CapabilityClassification
            == TermInfoSourceCapabilityClassification.Standard)
        {
            ApplyStandardField(field);
            return;
        }

        ApplyExtendedField(field);
    }

    private void ApplyStandardField(
        TermInfoSourceField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        switch (field.Kind)
        {
            case TermInfoSourceFieldKind.BooleanCapability:
                if (field.StandardBooleanCapability is BooleanCapability boolean)
                {
                    SetBoolean(boolean);
                }
                break;

            case TermInfoSourceFieldKind.NumericCapability:
                if (field.StandardNumericCapability is NumericCapability numeric
                    && field.NumericValue is int number)
                {
                    SetNumber(
                        numeric,
                        number);
                }
                break;

            case TermInfoSourceFieldKind.StringCapability:
                if (field.StandardStringCapability is StringCapability text
                    && field.StringValue is string value)
                {
                    SetString(
                        text,
                        value);
                }
                break;

            case TermInfoSourceFieldKind.CancelledCapability:
                if (field.StandardBooleanCapability is BooleanCapability canceledBoolean)
                {
                    CancelBoolean(canceledBoolean);
                }
                else if (field.StandardNumericCapability is NumericCapability canceledNumeric)
                {
                    CancelNumber(canceledNumeric);
                }
                else if (field.StandardStringCapability is StringCapability canceledString)
                {
                    CancelString(canceledString);
                }
                break;
        }
    }

    private void ApplyExtendedField(
        TermInfoSourceField field)
    {
        ArgumentNullException.ThrowIfNull(field);

        string? name =
            field.CanonicalCapabilityName;
        if (name is null)
        {
            return;
        }

        switch (field.Kind)
        {
            case TermInfoSourceFieldKind.BooleanCapability:
                SetExtended(
                    name,
                    new TermInfoCapabilityValue(true));
                break;

            case TermInfoSourceFieldKind.NumericCapability:
                if (field.NumericValue is int number)
                {
                    SetExtended(
                        name,
                        new TermInfoCapabilityValue(number));
                }
                break;

            case TermInfoSourceFieldKind.StringCapability:
                if (field.StringValue is string value)
                {
                    SetExtended(
                        name,
                        new TermInfoCapabilityValue(value));
                }
                break;

            case TermInfoSourceFieldKind.CancelledCapability:
                CancelExtended(name);
                break;
        }
    }

    private void SetBoolean(
        BooleanCapability capability)
    {
        _canceledBooleanCapabilities.Remove(capability);
        _booleanCapabilities.Add(capability);
    }

    private void SetNumber(
        NumericCapability capability,
        int value)
    {
        _canceledNumericCapabilities.Remove(capability);
        _numericCapabilities[capability] = value;
    }

    private void SetString(
        StringCapability capability,
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _canceledStringCapabilities.Remove(capability);
        _stringCapabilities[capability] = value;
    }

    private void SetExtended(
        string name,
        TermInfoCapabilityValue value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _canceledExtendedCapabilities.Remove(name);
        _extendedCapabilities[name] = value;
    }

    private void CancelBoolean(
        BooleanCapability capability)
    {
        _booleanCapabilities.Remove(capability);
        _canceledBooleanCapabilities.Add(capability);
    }

    private void CancelNumber(
        NumericCapability capability)
    {
        _numericCapabilities.Remove(capability);
        _canceledNumericCapabilities.Add(capability);
    }

    private void CancelString(
        StringCapability capability)
    {
        _stringCapabilities.Remove(capability);
        _canceledStringCapabilities.Add(capability);
    }

    private void CancelExtended(
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _extendedCapabilities.Remove(name);
        _canceledExtendedCapabilities.Add(name);
    }

    private static void CopyDictionary<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> source,
        IDictionary<TKey, TValue> destination)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        foreach (KeyValuePair<TKey, TValue> pair in source)
        {
            destination.Add(
                pair.Key,
                pair.Value);
        }
    }
}
