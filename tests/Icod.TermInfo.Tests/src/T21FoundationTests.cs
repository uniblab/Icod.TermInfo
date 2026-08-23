using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T21FoundationTests
{
    private static readonly string[] Release07ExportedTypes =
    [
        "Icod.TermInfo.BooleanCapability",
        "Icod.TermInfo.ITerminalDescriptionProvider",
        "Icod.TermInfo.ITermInfoDelayProvider",
        "Icod.TermInfo.InMemoryTerminalDescriptionProvider",
        "Icod.TermInfo.NumericCapability",
        "Icod.TermInfo.PaddingMode",
        "Icod.TermInfo.StringCapability",
        "Icod.TermInfo.TerminalColorModel",
        "Icod.TermInfo.TerminalColorSupport",
        "Icod.TermInfo.TerminalColorTier",
        "Icod.TermInfo.TerminalColors",
        "Icod.TermInfo.TerminalDatabase",
        "Icod.TermInfo.TerminalDescription",
        "Icod.TermInfo.TerminalDescriptionBuilder",
        "Icod.TermInfo.TerminalEnvironment",
        "Icod.TermInfo.TerminalProfiles",
        "Icod.TermInfo.TerminalRgbColor",
        "Icod.TermInfo.TerminalRgbLayout",
        "Icod.TermInfo.TerminalSize",
        "Icod.TermInfo.TerminalStandardStream",
        "Icod.TermInfo.TermInfoCapabilityValue",
        "Icod.TermInfo.TermInfoCapabilityValueKind",
        "Icod.TermInfo.TermInfoCompatibility",
        "Icod.TermInfo.TermInfoDelay",
        "Icod.TermInfo.TermInfoEvaluationException",
        "Icod.TermInfo.TermInfoExpansionContext",
        "Icod.TermInfo.TermInfoFormatException",
        "Icod.TermInfo.TermInfoOutput",
        "Icod.TermInfo.TermInfoPaddingFormatException",
        "Icod.TermInfo.TermInfoParameter",
        "Icod.TermInfo.TermInfoParameterExpander",
        "Icod.TermInfo.TermInfoParameterProgram",
        "Icod.TermInfo.WindowsVirtualTerminal",
    ];
    private static readonly string[] Release07BooleanCapabilityNames =
    [
        "AutoRightMargin",
        "GenericType",
        "MoveStandoutMode",
        "EatNewlineGlitch",
        "XonXoff",
        "MoveInsertMode",
        "BackColorErase",
        "CanChangeColor",
        "HueLightnessSaturation",
        "HasMetaKey",
        "NoPadCharacter",
    ];
    private static readonly string[] Release07NumericCapabilityNames =
    [
        "Columns",
        "Lines",
        "Colors",
        "ColorPairs",
        "InitialTabWidth",
        "VirtualTerminal",
        "NoColorVideo",
    ];
    private static readonly string[] Release07StringCapabilityNames =
    [
        "Bell",
        "CarriageReturn",
        "CursorDownOne",
        "ScrollForward",
        "ClearScreen",
        "CursorAddress",
        "EnterBoldMode",
        "ExitAttributeMode",
        "SetForegroundColor",
        "SetBackgroundColor",
        "BackTab",
        "EnterBlinkMode",
        "EnterDimMode",
        "ChangeScrollRegion",
        "CursorLeft",
        "CursorLeftOne",
        "CursorDown",
        "CursorRight",
        "CursorRightOne",
        "CursorUp",
        "CursorUpOne",
        "DeleteCharacters",
        "DeleteCharacter",
        "DeleteLines",
        "DeleteLine",
        "ClearToEndOfScreen",
        "ClearToEndOfLine",
        "ClearToBeginningOfLine",
        "CursorHome",
        "ColumnAddress",
        "Tab",
        "SetTab",
        "InsertCharacters",
        "InsertCharacter",
        "InsertLines",
        "InsertLine",
        "EnterInvisibleMode",
        "OriginalColorPair",
        "RestoreCursor",
        "EnterReverseMode",
        "ScrollReverse",
        "ExitAlternateCharacterSetMode",
        "ExitAutomaticMargins",
        "ExitKeypadMode",
        "ExitStandoutMode",
        "ExitUnderlineMode",
        "SaveCursor",
        "SetAttributes",
        "EnterAlternateCharacterSetMode",
        "EnterAutomaticMargins",
        "EnterKeypadMode",
        "EnterStandoutMode",
        "EnterUnderlineMode",
        "RowAddress",
        "AlternateCharacterSet",
        "EnableAlternateCharacterSet",
        "KeyBackspace",
        "KeyCursorDown",
        "KeyCursorLeft",
        "KeyCursorRight",
        "KeyCursorUp",
        "KeyHome",
        "KeyF1",
        "KeyF2",
        "KeyF3",
        "KeyF4",
        "ResetString2",
        "EraseCharacters",
        "ClearAllTabs",
        "EnterCursorAddressingMode",
        "ExitCursorAddressingMode",
        "CursorInvisible",
        "CursorNormal",
        "CursorVeryVisible",
        "FlashScreen",
        "NewLine",
        "ScrollForwardLines",
        "ScrollReverseLines",
        "EnterInsertMode",
        "ExitInsertMode",
        "EnterMetaMode",
        "ExitMetaMode",
        "EnterItalicMode",
        "ExitItalicMode",
        "InitializeColor",
        "OriginalColors",
        "SetLegacyForegroundColor",
        "SetLegacyBackgroundColor",
        "InitString1",
        "InitString2",
        "InitString3",
        "ResetString1",
        "ResetString3",
        "KeyMouse",
        "MemoryLock",
        "MemoryUnlock",
        "RepeatCharacter",
        "PrintScreen",
        "PrinterOff",
        "PrinterOn",
        "KeyBackTab",
        "KeyBegin",
        "KeyDeleteCharacter",
        "KeyEnd",
        "KeyEnter",
        "KeyInsertCharacter",
        "KeyNextPage",
        "KeyPreviousPage",
        "KeyF5",
        "KeyF6",
        "KeyF7",
        "KeyF8",
        "KeyF9",
        "KeyF10",
        "KeyF11",
        "KeyF12",
        "KeyF13",
        "KeyF14",
        "KeyF15",
        "KeyF16",
        "KeyF17",
        "KeyF18",
        "KeyF19",
        "KeyF20",
        "KeyF21",
        "KeyF22",
        "KeyF23",
        "KeyF24",
        "KeyA1",
        "KeyA3",
        "KeyB2",
        "KeyC1",
        "KeyC3",
        "KeyScrollForward",
        "KeyScrollReverse",
        "KeyShiftDeleteCharacter",
        "KeyShiftEnd",
        "KeyShiftHome",
        "KeyShiftInsertCharacter",
        "KeyShiftLeft",
        "KeyShiftNextPage",
        "KeyShiftPreviousPage",
        "KeyShiftRight",
        "KeyFind",
        "KeyHelp",
        "KeyRedo",
        "KeySelect",
    ];
    [Fact]
    public void Release07ExportedTypesRemainAvailable()
    {
        HashSet<string> actual =
            typeof(TerminalDescription).Assembly
                .GetExportedTypes()
                .Select(type => type.FullName!)
                .ToHashSet(StringComparer.Ordinal);

        Assert.All(
            Release07ExportedTypes,
            expected => Assert.Contains(expected, actual));
    }

    [Fact]
    public void Release07BooleanCapabilityValuesRemainFrozen()
    {
        AssertRelease07EnumBaseline<BooleanCapability>(
            Release07BooleanCapabilityNames);
    }

    [Fact]
    public void Release07NumericCapabilityValuesRemainFrozen()
    {
        AssertRelease07EnumBaseline<NumericCapability>(
            Release07NumericCapabilityNames);
    }

    [Fact]
    public void Release07StringCapabilityValuesRemainFrozen()
    {
        AssertRelease07EnumBaseline<StringCapability>(
            Release07StringCapabilityNames);
    }

    [Fact]
    public void T21ContainsNoProductionDatabaseAcquisitionTypes()
    {
        string[] forbiddenTypeNames =
        [
            "CompiledTermInfoParser",
            "CompiledTermInfoReader",
            "DirectoryTermInfoProvider",
            "DirectoryTerminalDescriptionProvider",
            "SystemTermInfoProvider",
            "SystemTerminalDescriptionProvider",
        ];

        Type[] actualTypes =
            typeof(TerminalDescription).Assembly.GetTypes();

        foreach (string forbiddenTypeName in forbiddenTypeNames)
        {
            Assert.DoesNotContain(
                actualTypes,
                type => string.Equals(
                    type.Name,
                    forbiddenTypeName,
                    StringComparison.Ordinal));
        }
    }

    private static void AssertRelease07EnumBaseline<TEnum>(
        IReadOnlyList<string> release07Names)
        where TEnum : struct, Enum
    {
        HashSet<string> baselineNames =
            release07Names.ToHashSet(StringComparer.Ordinal);

        for (int expectedValue = 0;
            expectedValue < release07Names.Count;
            expectedValue++)
        {
            string name = release07Names[expectedValue];

            Assert.True(
                Enum.TryParse(name, out TEnum value),
                $"Release 0.7 enum member '{typeof(TEnum).Name}.{name}' is missing.");
            Assert.Equal(
                expectedValue,
                Convert.ToInt32(value));
        }

        FieldInfo[] fields =
            typeof(TEnum).GetFields(
                BindingFlags.Public
                | BindingFlags.Static);

        foreach (FieldInfo field in fields)
        {
            if (baselineNames.Contains(field.Name))
            {
                continue;
            }

            object? rawValue = field.GetValue(null);
            Assert.NotNull(rawValue);

            int numericValue = Convert.ToInt32(rawValue);
            Assert.True(
                numericValue >= release07Names.Count,
                $"New enum member '{typeof(TEnum).Name}.{field.Name}' uses value {numericValue}; release 0.8 members must be appended after the frozen 0.7 range.");
        }
    }
}
