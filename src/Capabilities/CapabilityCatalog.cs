namespace Icod.TermInfo;

internal static class CapabilityCatalog
{
    internal static bool IsStandardName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return StandardCapabilityCatalog.IsStandardShortName(name);
    }

    internal static bool TryGetBoolean(
        string name,
        out BooleanCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (StandardCapabilityCatalog.TryGetBoolean(
                name,
                out StandardCapabilityMetadata<BooleanCapability>? metadata))
        {
            capability = metadata.Capability;
            return true;
        }

        capability = default;
        return false;
    }

    internal static bool TryGetNumeric(
        string name,
        out NumericCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (StandardCapabilityCatalog.TryGetNumeric(
                name,
                out StandardCapabilityMetadata<NumericCapability>? metadata))
        {
            capability = metadata.Capability;
            return true;
        }

        capability = default;
        return false;
    }

    internal static bool TryGetString(
        string name,
        out StringCapability capability)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (StandardCapabilityCatalog.TryGetString(
                name,
                out StandardCapabilityMetadata<StringCapability>? metadata))
        {
            capability = metadata.Capability;
            return true;
        }

        capability = default;
        return false;
    }
}
