using System.Buffers.Binary;
using System.Text;
using Icod.TermInfo;

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

static byte[] CreateLegacyCompiledEntry(
    string name,
    string alias,
    short columns)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(alias);

    Require(
        StandardCapabilityCatalog
            .GetMetadata(NumericCapability.Columns)
            .BinaryIndex == 0,
        "The package smoke fixture assumes cols is compiled numeric index zero.");

    byte[] names =
        Encoding.ASCII.GetBytes(
            $"{name}|{alias}|Package smoke external entry\0");
    int numericOffset =
        12
        + names.Length;

    if ((numericOffset & 1) != 0)
    {
        numericOffset++;
    }

    byte[] entry =
        new byte[
            numericOffset
            + sizeof(short)];

    BinaryPrimitives.WriteUInt16LittleEndian(
        entry.AsSpan(0, sizeof(ushort)),
        0x011A);
    BinaryPrimitives.WriteUInt16LittleEndian(
        entry.AsSpan(2, sizeof(ushort)),
        checked((ushort)names.Length));
    BinaryPrimitives.WriteUInt16LittleEndian(
        entry.AsSpan(6, sizeof(ushort)),
        1);

    names.CopyTo(
        entry.AsSpan(12));
    BinaryPrimitives.WriteInt16LittleEndian(
        entry.AsSpan(
            numericOffset,
            sizeof(short)),
        columns);

    return entry;
}

static string WriteCompiledEntry(
    string root,
    string name,
    byte[] entry)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(root);
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentNullException.ThrowIfNull(entry);

    string directory =
        Path.Combine(
            root,
            name[0].ToString());
    Directory.CreateDirectory(
        directory);

    string path =
        Path.Combine(
            directory,
            name);
    File.WriteAllBytes(
        path,
        entry);
    return path;
}

TerminalDescription ansi = TerminalDatabase.BuiltIn.Load("ansi");
TerminalDescription vt100 = TerminalDatabase.BuiltIn.Load("vt100");
TerminalDescription vt100Alias = TerminalDatabase.BuiltIn.Load("vt100-am");
TerminalDescription xterm = TerminalDatabase.BuiltIn.Load("xterm");
TerminalDescription xterm16 = TerminalDatabase.BuiltIn.Load("xterm-16color");
TerminalDescription xterm88 = TerminalDatabase.BuiltIn.Load("xterm-88color");
TerminalDescription xterm256 = TerminalDatabase.BuiltIn.Load("xterm-256color");
TerminalDescription xtermDirect = TerminalDatabase.BuiltIn.Load("xterm-direct");
TerminalDescription xtermDirect16 = TerminalDatabase.BuiltIn.Load("xterm-direct16");
TerminalDescription xtermDirect256 = TerminalDatabase.BuiltIn.Load("xterm-direct256");
TerminalDescription winConsole = TerminalDatabase.BuiltIn.Load("winconsole");
TerminalDescription msTerminal = TerminalDatabase.BuiltIn.Load("ms-terminal");
TerminalDescription msTerminalDirect = TerminalDatabase.BuiltIn.Load("ms-terminal-direct");
TerminalDescription dumb = TerminalDatabase.BuiltIn.Load("dumb");

Require(ReferenceEquals(vt100, vt100Alias), "vt100-am must resolve to vt100.");
Require(
    StandardCapabilityCatalog.BooleanCapabilities.Count == 44
        && StandardCapabilityCatalog.NumericCapabilities.Count == 39
        && StandardCapabilityCatalog.StringCapabilities.Count == 414,
    "The complete standard capability catalog is not available from the package.");
StandardCapabilityMetadata<BooleanCapability> amMetadata =
    StandardCapabilityCatalog.GetMetadata(BooleanCapability.AutoRightMargin);
Require(
    amMetadata.ShortName == "am"
        && amMetadata.BinaryIndex == 1
        && (int)BooleanCapability.AutoRightMargin == 0,
    "Managed enum values must remain independent from compiled table indices.");
Require(
    !string.IsNullOrWhiteSpace(msTerminal.Description),
    "Verbose terminal descriptions must be available from the package.");
Require(
    msTerminal.NumericCapabilities.Any(
        pair =>
            pair.Key == NumericCapability.Colors
            && pair.Value == 256),
    "Per-description standard capability enumeration is not usable.");
Require(
    xterm.GetString(StringCapability.EnterCursorAddressingMode) is not null,
    "xterm must advertise cursor-addressing entry.");
Require(
    xterm.GetString(StringCapability.ExitCursorAddressingMode) is not null,
    "xterm must advertise cursor-addressing exit.");
Require(
    xterm.TryGetExtendedString("XM", out _),
    "xterm must carry XM mouse-mode metadata.");
Require(
    xterm.ExpandExtendedString("XM", 1) == "\x1b[?1006;1000h",
    "Extended string expansion changed.");
Require(
    xterm.TryGetExtendedString("BE", out string? pasteEnable)
        && pasteEnable == "\x1b[?2004h",
    "xterm bracketed-paste enable metadata changed.");
Require(
    xterm.TryGetExtendedString("fe", out string? focusEnable)
        && focusEnable == "\x1b[?1004h",
    "xterm focus-enable metadata changed.");
Require(
    xterm.TryGetExtendedString("Ms", out string? clipboard),
    "xterm must carry clipboard metadata.");
Require(
    TermInfoParameterExpander.Expand(clipboard!, "c", "YWJj")
        == "\x1b]52;c;YWJj\x1b\\",
    "xterm clipboard metadata expansion changed.");
Require(ansi.GetNumber(NumericCapability.Colors) == 8, "ANSI must advertise eight colors.");
Require(vt100.GetNumber(NumericCapability.Colors) is null, "VT100 must remain monochrome.");
Require(xterm16.GetNumber(NumericCapability.Colors) == 16, "xterm-16color must advertise 16 colors.");
Require(xterm88.GetNumber(NumericCapability.Colors) == 88, "xterm-88color must advertise 88 colors.");
Require(xterm88.GetNumber(NumericCapability.ColorPairs) == 7744, "xterm-88color must advertise 7744 pairs.");
Require(xterm256.GetNumber(NumericCapability.Colors) == 256, "xterm-256color must advertise 256 colors.");
Require(xterm256.GetNumber(NumericCapability.ColorPairs) == 65536, "xterm-256color must advertise 65536 pairs.");
Require(
    TerminalColors.ExpandForeground(xterm256, 255) == "\x1b[38;5;255m",
    "xterm-256color foreground expansion changed.");
Require(
    xtermDirect.GetNumber(NumericCapability.Colors) == (1 << 24),
    "xterm-direct must advertise the direct RGB color space.");
Require(
    TerminalColors.GetColorSupport(xtermDirect256).Model == TerminalColorModel.DirectRgb,
    "xterm-direct256 must classify as direct RGB.");
Require(
    TerminalColors.GetColorSupport(xtermDirect256).IndexedColorCount == 256,
    "xterm-direct256 must retain 256 indexed colors.");
Require(
    TerminalColors.ExpandForeground(xtermDirect16, 15) == "\x1b[97m",
    "xterm-direct16 indexed foreground expansion changed.");
Require(
    TerminalColors.ExpandForeground(
        xtermDirect256,
        new TerminalRgbColor(0x12, 0x34, 0x56))
        == "\x1b[38:2::18:52:86m",
    "xterm-direct256 RGB foreground expansion changed.");
Require(winConsole.Name == "winconsole", "winconsole must be available.");
Require(msTerminal.Name == "ms-terminal", "ms-terminal must be available.");
Require(
    TerminalColors.GetColorSupport(msTerminal).Model
        == TerminalColorModel.Indexed,
    "ms-terminal must retain indexed-color semantics.");
Require(
    TerminalColors.GetColorSupport(msTerminalDirect).Model
        == TerminalColorModel.DirectRgb,
    "ms-terminal-direct must retain direct-RGB semantics.");
Require(
    !ReferenceEquals(msTerminal, xterm256),
    "Windows Terminal must not be an xterm alias.");
Require(dumb.Name == "dumb", "The dumb fallback profile must be available.");
Require(
    !TerminalDatabase.BuiltIn.TryLoad("xterm-mono", out _),
    "Unselected terminal names must not silently resolve.");
Require(
    !TerminalDatabase.BuiltIn.TryLoad("linux", out _),
    "0.8 must not pretend to provide arbitrary system terminfo identities.");

TermInfoParameterProgram program =
    TermInfoParameterProgram.Parse("%p1%{1}%+%d");
Require(program.Source == "%p1%{1}%+%d", "Parsed program source changed.");
Require(program.Expand(41) == "42", "Reusable parsed-program expansion failed.");

string cup = ansi.Expand(StringCapability.CursorAddress, 0, 0);
Require(cup == "\x1b[1;1H", "ANSI cursor addressing expansion changed.");

using StringWriter writer = new();
TermInfoOutput.PutP(cup, writer);
Require(writer.ToString() == cup, "Padding-free output changed unexpectedly.");

writer.GetStringBuilder().Clear();
TermInfoOutput.PutP(
    vt100.GetRequiredString(StringCapability.ClearScreen),
    writer);
Require(
    !writer.ToString().Contains("$<", StringComparison.Ordinal),
    "Padding annotations must never be emitted literally in ignore mode.");

using MemoryStream capabilityBytes = new();
TermInfoOutput.TPuts(
    "\u0080",
    1,
    capabilityBytes,
    Encoding.Latin1);
byte[] rawCapabilityBytes = capabilityBytes.ToArray();
Require(
    rawCapabilityBytes.Length == 1
        && rawCapabilityBytes[0] == 0x80,
    "Latin-1 capability-byte round-trip changed.");

TerminalDescription xonTerminal =
    new TerminalDescriptionBuilder("package-smoke-xon")
        .SetBoolean(BooleanCapability.XonXoff)
        .Build();
writer.GetStringBuilder().Clear();
TermInfoOutput.TPuts(
    "before$<1>after",
    1,
    writer,
    new TermInfoOutputOptions(xonTerminal));
Require(
    writer.ToString() == "beforeafter",
    "Terminal-aware xon padding suppression changed.");

Require(
    TermInfoCompatibility.TiGetNum(ansi, "cols") == 80,
    "Compatibility capability lookup changed.");

string externalName =
    "package-smoke-external";
string externalAlias =
    "package-smoke-external-alias";
byte[] compiledEntry =
    CreateLegacyCompiledEntry(
        externalName,
        externalAlias,
        123);

TerminalDescription parsed =
    CompiledTermInfoParser.Parse(
        compiledEntry);
Require(
    parsed.Name == externalName
        && parsed.Aliases.Contains(
            externalAlias,
            StringComparer.Ordinal)
        && parsed.GetNumber(NumericCapability.Columns) == 123,
    "Fresh package consumer could not parse caller-supplied compiled bytes.");

string acquisitionRoot =
    Path.Combine(
        Path.GetTempPath(),
        "Icod.TermInfo-package-smoke-"
        + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(
    acquisitionRoot);

string? originalTermInfo =
    Environment.GetEnvironmentVariable(
        "TERMINFO");
string? originalTermInfoDirs =
    Environment.GetEnvironmentVariable(
        "TERMINFO_DIRS");

try
{
    WriteCompiledEntry(
        acquisitionRoot,
        externalName,
        compiledEntry);

    DirectoryTerminalDescriptionProvider explicitProvider =
        new(
            acquisitionRoot);
    Require(
        explicitProvider.TryLoad(
            externalName,
            out TerminalDescription? explicitTerminal)
            && explicitTerminal.GetNumber(NumericCapability.Columns) == 123,
        "Fresh package consumer could not load an explicit terminfo root.");

    SystemTerminalDescriptionProvider restrictedSystem =
        new(
            new SystemTerminalDescriptionProviderOptions(
                useEnvironment: false,
                useUserDatabase: false,
                useSystemDatabases: false));
    Require(
        !restrictedSystem.TryLoad(
            externalName,
            out _),
        "A fully restricted system provider unexpectedly found an entry.");

    Environment.SetEnvironmentVariable(
        "TERMINFO",
        acquisitionRoot);
    Environment.SetEnvironmentVariable(
        "TERMINFO_DIRS",
        null);

    SystemTerminalDescriptionProvider system =
        new(
            new SystemTerminalDescriptionProviderOptions(
                useEnvironment: true,
                useUserDatabase: false,
                useSystemDatabases: false));
    Require(
        system.TryLoad(
            externalName,
            out TerminalDescription? systemTerminal)
            && systemTerminal.GetNumber(NumericCapability.Columns) == 123,
        "Fresh package consumer could not load through system TERMINFO discovery.");

    TerminalDatabase composed =
        new(
            new ITerminalDescriptionProvider[]
            {
                system,
                TerminalDatabase.BuiltIn,
            });
    Require(
        ReferenceEquals(
            composed.Load("xterm"),
            TerminalProfiles.Xterm),
        "System-to-built-in fallback composition failed.");
    Require(
        ReferenceEquals(
            composed.Load(externalName),
            systemTerminal),
        "Composed database did not preserve system-provider precedence.");
}
finally
{
    Environment.SetEnvironmentVariable(
        "TERMINFO",
        originalTermInfo);
    Environment.SetEnvironmentVariable(
        "TERMINFO_DIRS",
        originalTermInfoDirs);

    if (Directory.Exists(acquisitionRoot))
    {
        Directory.Delete(
            acquisitionRoot,
            recursive: true);
    }
}

Console.WriteLine("Icod.TermInfo package smoke test passed.");
