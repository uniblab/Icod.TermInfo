using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S06CancellationSemanticsTests
{
    [Fact]
    public void LocalCancellationCreatesStandardAndExtendedTombstones()
    {
        TermInfoSourceCapabilityState state =
            ParseState(
                "local-cancel|S06 local cancellation,\n"
                + "\tam,\n"
                + "\tcols#80,\n"
                + "\tclear=\\E[H,\n"
                + "\tVendorBool,\n"
                + "\tVendorNum#7,\n"
                + "\tVendorString=value,\n"
                + "\tam@,\n"
                + "\tcols@,\n"
                + "\tclear@,\n"
                + "\tVendorBool@,\n"
                + "\tVendorNum@,\n"
                + "\tVendorString@,\n");

        Assert.Empty(state.BooleanCapabilities);
        Assert.Empty(state.NumericCapabilities);
        Assert.Empty(state.StringCapabilities);
        Assert.Empty(state.ExtendedCapabilities);

        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            state.CanceledBooleanCapabilities);
        Assert.Contains(
            NumericCapability.Columns,
            state.CanceledNumericCapabilities);
        Assert.Contains(
            StringCapability.ClearScreen,
            state.CanceledStringCapabilities);
        Assert.Contains(
            "VendorBool",
            state.CanceledExtendedCapabilities);
        Assert.Contains(
            "VendorNum",
            state.CanceledExtendedCapabilities);
        Assert.Contains(
            "VendorString",
            state.CanceledExtendedCapabilities);
    }

    [Fact]
    public void LaterLocalDeclarationReplacesEarlierValueOrCancellation()
    {
        TermInfoSourceCapabilityState state =
            ParseState(
                "local-order|S06 local ordering,\n"
                + "\tam,\n"
                + "\tam@,\n"
                + "\tcols@,\n"
                + "\tcols#132,\n"
                + "\tclear@,\n"
                + "\tclear=done,\n"
                + "\tVendor=value,\n"
                + "\tVendor@,\n"
                + "\tVendor=final,\n");

        Assert.DoesNotContain(
            BooleanCapability.AutoRightMargin,
            state.BooleanCapabilities);
        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            state.CanceledBooleanCapabilities);

        Assert.Equal(
            132,
            state.NumericCapabilities[NumericCapability.Columns]);
        Assert.DoesNotContain(
            NumericCapability.Columns,
            state.CanceledNumericCapabilities);

        Assert.Equal(
            "done",
            state.StringCapabilities[StringCapability.ClearScreen]);
        Assert.DoesNotContain(
            StringCapability.ClearScreen,
            state.CanceledStringCapabilities);

        Assert.Equal(
            "final",
            state.ExtendedCapabilities["Vendor"].StringValue);
        Assert.DoesNotContain(
            "Vendor",
            state.CanceledExtendedCapabilities);
    }

    [Fact]
    public void LocalCancellationsBlockInheritedValues()
    {
        TermInfoSourceCapabilityState parent =
            ParseState(
                "parent|S06 parent,\n"
                + "\tam,\n"
                + "\tcols#80,\n"
                + "\tclear=parent,\n"
                + "\tVendor=parent,\n");
        TermInfoSourceCapabilityState child =
            ParseState(
                "child|S06 child,\n"
                + "\tam@,\n"
                + "\tcols@,\n"
                + "\tclear@,\n"
                + "\tVendor@,\n");

        child.Inherit(parent);

        Assert.Empty(child.BooleanCapabilities);
        Assert.Empty(child.NumericCapabilities);
        Assert.Empty(child.StringCapabilities);
        Assert.Empty(child.ExtendedCapabilities);
        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            child.CanceledBooleanCapabilities);
        Assert.Contains(
            NumericCapability.Columns,
            child.CanceledNumericCapabilities);
        Assert.Contains(
            StringCapability.ClearScreen,
            child.CanceledStringCapabilities);
        Assert.Contains(
            "Vendor",
            child.CanceledExtendedCapabilities);
    }

    [Fact]
    public void LongNameCancellationBlocksEquivalentShortNameInheritance()
    {
        TermInfoSourceCapabilityState parent =
            ParseState(
                "parent|S06 short-name parent,\n"
                + "\tam,\n"
                + "\tcols#80,\n"
                + "\tclear=parent,\n");
        TermInfoSourceCapabilityState child =
            ParseState(
                "child|S06 long-name cancellation,\n"
                + "\tauto_right_margin@,\n"
                + "\tcolumns@,\n"
                + "\tclear_screen@,\n");

        child.Inherit(parent);

        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            child.CanceledBooleanCapabilities);
        Assert.Contains(
            NumericCapability.Columns,
            child.CanceledNumericCapabilities);
        Assert.Contains(
            StringCapability.ClearScreen,
            child.CanceledStringCapabilities);
        Assert.Empty(child.BooleanCapabilities);
        Assert.Empty(child.NumericCapabilities);
        Assert.Empty(child.StringCapabilities);
    }

    [Fact]
    public void LocalValuesBlockInheritedCancellations()
    {
        TermInfoSourceCapabilityState parent =
            ParseState(
                "parent|S06 cancelled parent,\n"
                + "\tam@,\n"
                + "\tcols@,\n"
                + "\tclear@,\n"
                + "\tVendor@,\n");
        TermInfoSourceCapabilityState child =
            ParseState(
                "child|S06 local values,\n"
                + "\tam,\n"
                + "\tcols#132,\n"
                + "\tclear=child,\n"
                + "\tVendor=child,\n");

        child.Inherit(parent);

        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            child.BooleanCapabilities);
        Assert.Equal(
            132,
            child.NumericCapabilities[NumericCapability.Columns]);
        Assert.Equal(
            "child",
            child.StringCapabilities[StringCapability.ClearScreen]);
        Assert.Equal(
            "child",
            child.ExtendedCapabilities["Vendor"].StringValue);

        Assert.Empty(child.CanceledBooleanCapabilities);
        Assert.Empty(child.CanceledNumericCapabilities);
        Assert.Empty(child.CanceledStringCapabilities);
        Assert.Empty(child.CanceledExtendedCapabilities);
    }

    [Fact]
    public void RightToLeftParentCompositionLetsLeftwardParentOverrideEarlierImport()
    {
        TermInfoSourceCapabilityState rightmost =
            ParseState(
                "rightmost|S06 rightmost parent,\n"
                + "\tcols#90,\n"
                + "\tclear=rightmost,\n"
                + "\tVendor=rightmost,\n");
        TermInfoSourceCapabilityState leftmost =
            ParseState(
                "leftmost|S06 leftmost parent,\n"
                + "\tcols#80,\n"
                + "\tclear=leftmost,\n"
                + "\tVendor=leftmost,\n");
        TermInfoSourceCapabilityState parents =
            TermInfoSourceCapabilityState.CreateEmpty();
        TermInfoSourceCapabilityState child =
            TermInfoSourceCapabilityState.CreateEmpty();

        parents
            .OverlayHigherPriority(rightmost)
            .OverlayHigherPriority(leftmost);
        child.Inherit(parents);

        Assert.Equal(
            80,
            child.NumericCapabilities[NumericCapability.Columns]);
        Assert.Equal(
            "leftmost",
            child.StringCapabilities[StringCapability.ClearScreen]);
        Assert.Equal(
            "leftmost",
            child.ExtendedCapabilities["Vendor"].StringValue);
    }

    [Fact]
    public void LeftwardParentCancellationOverridesRightmostSourceValue()
    {
        TermInfoSourceCapabilityState rightmost =
            ParseState(
                "rightmost|S06 rightmost value,\n"
                + "\tcols#90,\n"
                + "\tVendor=rightmost,\n");
        TermInfoSourceCapabilityState leftmost =
            ParseState(
                "leftmost|S06 leftmost cancellation,\n"
                + "\tcols@,\n"
                + "\tVendor@,\n");
        TermInfoSourceCapabilityState parents =
            TermInfoSourceCapabilityState.CreateEmpty();

        parents
            .OverlayHigherPriority(rightmost)
            .OverlayHigherPriority(leftmost);

        Assert.DoesNotContain(
            NumericCapability.Columns,
            parents.NumericCapabilities.Keys);
        Assert.Contains(
            NumericCapability.Columns,
            parents.CanceledNumericCapabilities);
        Assert.DoesNotContain(
            "Vendor",
            parents.ExtendedCapabilities.Keys);
        Assert.Contains(
            "Vendor",
            parents.CanceledExtendedCapabilities);
    }

    [Fact]
    public void LeftwardParentValueOverridesRightmostSourceCancellation()
    {
        TermInfoSourceCapabilityState rightmost =
            ParseState(
                "rightmost|S06 rightmost cancellation,\n"
                + "\tcols@,\n"
                + "\tVendor@,\n");
        TermInfoSourceCapabilityState leftmost =
            ParseState(
                "leftmost|S06 leftmost value,\n"
                + "\tcols#80,\n"
                + "\tVendor=leftmost,\n");
        TermInfoSourceCapabilityState parents =
            TermInfoSourceCapabilityState.CreateEmpty();

        parents
            .OverlayHigherPriority(rightmost)
            .OverlayHigherPriority(leftmost);

        Assert.Equal(
            80,
            parents.NumericCapabilities[NumericCapability.Columns]);
        Assert.DoesNotContain(
            NumericCapability.Columns,
            parents.CanceledNumericCapabilities);
        Assert.Equal(
            "leftmost",
            parents.ExtendedCapabilities["Vendor"].StringValue);
        Assert.DoesNotContain(
            "Vendor",
            parents.CanceledExtendedCapabilities);
    }

    [Fact]
    public void DisabledAndSemanticallyInvalidFieldsDoNotEnterCapabilityState()
    {
        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                "ignored|S06 ignored fields,\n"
                + "\t.cols#99,\n"
                + "\t.Vendor=value,\n"
                + "\tam#1,\n"
                + "\tuse=dumb,\n");
        TermInfoSourceEntry entry =
            Assert.Single(result.Document.Entries);

        Assert.True(result.HasErrors);
        TermInfoSourceCapabilityState state =
            TermInfoSourceCapabilityState.CreateLocal(entry);

        Assert.Empty(state.BooleanCapabilities);
        Assert.Empty(state.NumericCapabilities);
        Assert.Empty(state.StringCapabilities);
        Assert.Empty(state.ExtendedCapabilities);
        Assert.Empty(state.CanceledBooleanCapabilities);
        Assert.Empty(state.CanceledNumericCapabilities);
        Assert.Empty(state.CanceledStringCapabilities);
        Assert.Empty(state.CanceledExtendedCapabilities);
    }

    [Fact]
    public void CloneCopiesValuesAndTombstonesWithoutSharingMutableState()
    {
        TermInfoSourceCapabilityState original =
            ParseState(
                "original|S06 clone source,\n"
                + "\tam,\n"
                + "\tcols@,\n"
                + "\tVendor=value,\n"
                + "\tOther@,\n");
        TermInfoSourceCapabilityState clone =
            original.Clone();
        TermInfoSourceCapabilityState lowerPriority =
            ParseState(
                "lower|S06 lower priority,\n"
                + "\tclear=lower,\n");

        clone.Inherit(lowerPriority);

        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            original.BooleanCapabilities);
        Assert.Contains(
            NumericCapability.Columns,
            original.CanceledNumericCapabilities);
        Assert.Equal(
            "value",
            original.ExtendedCapabilities["Vendor"].StringValue);
        Assert.Contains(
            "Other",
            original.CanceledExtendedCapabilities);
        Assert.DoesNotContain(
            StringCapability.ClearScreen,
            original.StringCapabilities.Keys);

        Assert.Equal(
            "lower",
            clone.StringCapabilities[StringCapability.ClearScreen]);
    }

    private static TermInfoSourceCapabilityState ParseState(
        string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        TermInfoSourceParseResult result =
            TermInfoSourceParser.Parse(
                source,
                "s06.ti");

        Assert.False(
            result.HasErrors,
            FormatDiagnostics(result.Diagnostics));
        return TermInfoSourceCapabilityState.CreateLocal(
            Assert.Single(result.Document.Entries));
    }

    private static string FormatDiagnostics(
        IEnumerable<TermInfoSourceDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        return string.Join(
            "; ",
            diagnostics.Select(
                diagnostic =>
                    diagnostic.Code
                    + " "
                    + diagnostic.Message));
    }
}
