using System.Reflection;
using Icod.TermInfo;
using Icod.TermInfo.Source;

static void Require(
    bool condition,
    string message)
{
    ArgumentNullException.ThrowIfNull(message);

    if (!condition)
    {
        throw new InvalidOperationException(
            message);
    }
}

Assembly sourceAssembly =
    typeof(TermInfoSourceLexer).Assembly;
AssemblyName sourceName =
    sourceAssembly.GetName();

Require(
    sourceName.Name
        == "Icod.TermInfo.Source",
    "The source package assembly could not be loaded.");
Require(
    sourceName.Version
        == new Version(1, 0, 0, 0),
    "The source package must retain the stable 1.x assembly identity.");

TermInfoSourceLexResult lexed =
    TermInfoSourceLexer.Tokenize(
        "package-smoke|Package smoke source,\n"
        + "\tam,\n"
        + "\tuse=dumb,\n",
        "package-smoke.ti");
Require(
    !lexed.HasErrors,
    "The source package could not tokenize representative source.");
Require(
    lexed.Tokens.Any(
        token =>
            token.Kind == TermInfoSourceTokenKind.BooleanCapability
            && token.Text == "am"),
    "The source package did not expose Boolean capability lexing.");
Require(
    lexed.Tokens.Any(
        token =>
            token.Kind == TermInfoSourceTokenKind.UseReference
            && token.Span.SourceName == "package-smoke.ti"),
    "The source package did not expose use= lexing with source locations.");

Assembly runtimeAssembly =
    typeof(TerminalDescription).Assembly;
Require(
    runtimeAssembly.GetName().Name
        == "Icod.TermInfo",
    "The transitive Icod.TermInfo dependency is unavailable.");
Require(
    runtimeAssembly.GetName().Version
        == new Version(1, 0, 0, 0),
    "The runtime package must retain the stable 1.x assembly identity.");

TerminalDescription dumb =
    TerminalDatabase.BuiltIn.Load(
        "dumb");
Require(
    dumb.Name
        == "dumb",
    "The transitive runtime package is not usable.");

Console.WriteLine(
    "Icod.TermInfo.Source package smoke test passed.");
