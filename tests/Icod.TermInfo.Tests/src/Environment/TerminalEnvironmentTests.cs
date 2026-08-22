using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

[Collection(TerminalEnvironmentCollection.Name)]
public sealed class TerminalEnvironmentTests
{
    [Theory]
    [InlineData("ansi", "ansi")]
    [InlineData("vt100", "vt100")]
    [InlineData("vt100-am", "vt100-am")]
    [InlineData("dumb", "dumb")]
    public void TerminalNameReturnsExactNonBlankValue(
        string value,
        string expected)
    {
        using EnvironmentVariableScope scope =
            new("TERM", value);

        Assert.Equal(expected, TerminalEnvironment.TerminalName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TerminalNameTreatsMissingOrBlankValueAsUnavailable(
        string? value)
    {
        using EnvironmentVariableScope scope =
            new("TERM", value);

        Assert.Null(TerminalEnvironment.TerminalName);
    }

    [Theory]
    [InlineData("ansi")]
    [InlineData("vt100")]
    [InlineData("vt100-am")]
    [InlineData("vt102")]
    [InlineData("vt220")]
    [InlineData("vt200")]
    [InlineData("xterm")]
    [InlineData("xterm-16color")]
    [InlineData("xterm-88color")]
    [InlineData("xterm-256color")]
    [InlineData("dumb")]
    public void CurrentTermResolvesOnlyConfiguredBuiltInNames(string value)
    {
        using EnvironmentVariableScope scope =
            new("TERM", value);

        Assert.True(
            TerminalEnvironment.TryResolve(
                TerminalDatabase.BuiltIn,
                out TerminalDescription? terminal));
        Assert.NotNull(terminal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xterm-mono")]
    [InlineData("screen")]
    [InlineData("tmux")]
    public void MissingOrUnsupportedCurrentTermDoesNotResolve(string? value)
    {
        using EnvironmentVariableScope scope =
            new("TERM", value);

        Assert.False(
            TerminalEnvironment.TryResolve(
                TerminalDatabase.BuiltIn,
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("xterm-mono")]
    public void CurrentTermFallbackIsAlwaysExplicit(string? value)
    {
        using EnvironmentVariableScope scope =
            new("TERM", value);

        TerminalDescription terminal =
            TerminalEnvironment.Resolve(
                TerminalDatabase.BuiltIn,
                TerminalProfiles.Dumb);

        Assert.Same(TerminalProfiles.Dumb, terminal);
    }

    [Fact]
    public void ResolveRejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(
            () => TerminalEnvironment.TryResolve(null!, out _));
        Assert.Throws<ArgumentNullException>(
            () => TerminalEnvironment.Resolve(
                null!,
                TerminalProfiles.Dumb));
        Assert.Throws<ArgumentNullException>(
            () => TerminalEnvironment.Resolve(
                TerminalDatabase.BuiltIn,
                null!));
    }

    [Fact]
    public void RedirectionPropertiesAgreeWithSystemConsole()
    {
        Assert.Equal(
            Console.IsInputRedirected,
            TerminalEnvironment.IsInputRedirected);
        Assert.Equal(
            Console.IsOutputRedirected,
            TerminalEnvironment.IsOutputRedirected);
        Assert.Equal(
            Console.IsErrorRedirected,
            TerminalEnvironment.IsErrorRedirected);

        Assert.Equal(
            Console.IsInputRedirected,
            TerminalEnvironment.IsRedirected(TerminalStandardStream.Input));
        Assert.Equal(
            Console.IsOutputRedirected,
            TerminalEnvironment.IsRedirected(TerminalStandardStream.Output));
        Assert.Equal(
            Console.IsErrorRedirected,
            TerminalEnvironment.IsRedirected(TerminalStandardStream.Error));
    }

    [Fact]
    public void InvalidStandardStreamIsRejected()
    {
        TerminalStandardStream invalid =
            (TerminalStandardStream)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalEnvironment.IsRedirected(invalid));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TerminalEnvironment.TryGetLiveSize(invalid, out _));
    }

    [Fact]
    public void EnvironmentSizeUsesPositiveColumnsAndLines()
    {
        using EnvironmentVariableScope columns =
            new("COLUMNS", "132");
        using EnvironmentVariableScope lines =
            new("LINES", "43");

        Assert.True(
            TerminalEnvironment.TryGetEnvironmentSize(
                out TerminalSize size));
        Assert.Equal(new TerminalSize(132, 43), size);
    }

    [Theory]
    [InlineData(null, "24")]
    [InlineData("80", null)]
    [InlineData("", "24")]
    [InlineData("80", "")]
    [InlineData("0", "24")]
    [InlineData("80", "0")]
    [InlineData("-1", "24")]
    [InlineData("80", "-1")]
    [InlineData(" 80", "24")]
    [InlineData("80", "24 ")]
    [InlineData("not-a-number", "24")]
    [InlineData("80", "not-a-number")]
    [InlineData("99999999999999999999", "24")]
    public void InvalidEnvironmentSizeDoesNotProduceFallback(
        string? columnsValue,
        string? linesValue)
    {
        using EnvironmentVariableScope columns =
            new("COLUMNS", columnsValue);
        using EnvironmentVariableScope lines =
            new("LINES", linesValue);

        Assert.False(
            TerminalEnvironment.TryGetEnvironmentSize(
                out TerminalSize size));
        Assert.Equal(default(TerminalSize), size);
    }

    [Fact]
    public void ProfileSizeIsSeparateFromLiveSize()
    {
        Assert.True(
            TerminalEnvironment.TryGetProfileSize(
                TerminalProfiles.Ansi,
                out TerminalSize ansi));
        Assert.Equal(new TerminalSize(80, 24), ansi);

        Assert.True(
            TerminalEnvironment.TryGetProfileSize(
                TerminalProfiles.Vt100,
                out TerminalSize vt100));
        Assert.Equal(new TerminalSize(80, 24), vt100);

        Assert.False(
            TerminalEnvironment.TryGetProfileSize(
                TerminalProfiles.Dumb,
                out TerminalSize dumb));
        Assert.Equal(default(TerminalSize), dumb);
    }

    [Fact]
    public void InvalidProfileDimensionsAreNotReturned()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("invalid-size")
                .SetNumber(NumericCapability.Columns, 80)
                .SetNumber(NumericCapability.Lines, 0)
                .Build();

        Assert.False(
            TerminalEnvironment.TryGetProfileSize(
                terminal,
                out TerminalSize size));
        Assert.Equal(default(TerminalSize), size);
    }

    [Fact]
    public void ProfileSizeRejectsNullTerminal()
    {
        Assert.Throws<ArgumentNullException>(
            () => TerminalEnvironment.TryGetProfileSize(
                null!,
                out _));
    }

    [Fact]
    public void LiveSizeQueriesAreGracefulWithoutInteractiveTty()
    {
        foreach (TerminalStandardStream stream in
            Enum.GetValues<TerminalStandardStream>())
        {
            bool success =
                TerminalEnvironment.TryGetLiveSize(
                    stream,
                    out TerminalSize size);

            if (TerminalEnvironment.IsRedirected(stream))
            {
                Assert.False(success);
            }

            if (success)
            {
                Assert.True(size.Columns > 0);
                Assert.True(size.Rows > 0);
            }
            else
            {
                Assert.Equal(default(TerminalSize), size);
            }
        }
    }

    [Fact]
    public void DefaultLiveSizeQueryNeverSubstitutesProfileDefaults()
    {
        bool success =
            TerminalEnvironment.TryGetLiveSize(
                out TerminalSize size);

        if (success)
        {
            Assert.True(size.Columns > 0);
            Assert.True(size.Rows > 0);
        }
        else
        {
            Assert.Equal(default(TerminalSize), size);
        }
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _originalValue;
        private bool _disposed;

        internal EnvironmentVariableScope(
            string name,
            string? value)
        {
            ArgumentNullException.ThrowIfNull(name);

            _name = name;
            _originalValue =
                System.Environment.GetEnvironmentVariable(name);
            System.Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            System.Environment.SetEnvironmentVariable(
                _name,
                _originalValue);
            _disposed = true;
        }
    }
}
