using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Icod.TermInfo;

/// <summary>
/// Provides conservative process-terminal environment inspection.
/// </summary>
public static class TerminalEnvironment
{
    // Linux asm-generic TIOCGWINSZ.
    private static readonly nuint LinuxGetWindowSizeRequest = 0x5413u;

    // Darwin _IOR('t', 104, struct winsize).
    private static readonly nuint MacOsGetWindowSizeRequest = 0x40087468u;

    private static readonly ITerminalSizeProvider? SizeProvider =
        CreateSizeProvider();

    /// <summary>
    /// Gets the current <c>TERM</c> value, or <see langword="null"/> when it is
    /// missing or blank.
    /// </summary>
    public static string? TerminalName
    {
        get
        {
            string? value =
                System.Environment.GetEnvironmentVariable("TERM");

            return (string.IsNullOrWhiteSpace(value))
                ? null
                : value
            ;
        }
    }

    /// <summary>
    /// Gets whether standard input is redirected.
    /// </summary>
    public static bool IsInputRedirected => Console.IsInputRedirected;

    /// <summary>
    /// Gets whether standard output is redirected.
    /// </summary>
    public static bool IsOutputRedirected => Console.IsOutputRedirected;

    /// <summary>
    /// Gets whether standard error is redirected.
    /// </summary>
    public static bool IsErrorRedirected => Console.IsErrorRedirected;

    /// <summary>
    /// Gets whether the specified standard stream is redirected.
    /// </summary>
    public static bool IsRedirected(TerminalStandardStream stream)
    {
        Validate(stream);

        return stream switch
        {
            TerminalStandardStream.Input => IsInputRedirected,
            TerminalStandardStream.Output => IsOutputRedirected,
            TerminalStandardStream.Error => IsErrorRedirected,
            _ => throw new ArgumentOutOfRangeException(nameof(stream)),
        };
    }

    /// <summary>
    /// Attempts to resolve the current <c>TERM</c> value using the supplied
    /// terminal database.
    /// </summary>
    public static bool TryResolve(
        TerminalDatabase database,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ArgumentNullException.ThrowIfNull(database);

        string? terminalName = TerminalName;
        if (terminalName is null)
        {
            terminal = null;
            return false;
        }

        return database.TryLoad(terminalName, out terminal);
    }

    /// <summary>
    /// Resolves the current <c>TERM</c> value or returns the caller-supplied
    /// fallback when the value is missing or unsupported.
    /// </summary>
    public static TerminalDescription Resolve(
        TerminalDatabase database,
        TerminalDescription fallback)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(fallback);

        return database.Resolve(TerminalName, fallback);
    }

    /// <summary>
    /// Attempts to query live terminal dimensions, preferring standard output,
    /// then standard error, then standard input.
    /// </summary>
    /// <remarks>
    /// A successful result always comes from the operating system. Environment
    /// variables and terminal-profile defaults are never substituted here.
    /// </remarks>
    public static bool TryGetLiveSize(out TerminalSize size)
    {
        if (TryGetLiveSize(
                TerminalStandardStream.Output,
                out size))
        {
            return true;
        }

        if (TryGetLiveSize(
                TerminalStandardStream.Error,
                out size))
        {
            return true;
        }

        if (TryGetLiveSize(
                TerminalStandardStream.Input,
                out size))
        {
            return true;
        }

        size = default;
        return false;
    }

    /// <summary>
    /// Attempts to query live terminal dimensions for one standard stream.
    /// </summary>
    /// <remarks>
    /// A redirected stream never reports a live terminal size. On Windows the
    /// screen-buffer API applies to output handles, so standard input returns
    /// no size through this overload.
    /// </remarks>
    public static bool TryGetLiveSize(
        TerminalStandardStream stream,
        out TerminalSize size)
    {
        Validate(stream);

        if (IsRedirected(stream) || (SizeProvider is null))
        {
            size = default;
            return false;
        }

        return SizeProvider.TryGetSize(stream, out size);
    }

    /// <summary>
    /// Attempts to read positive <c>COLUMNS</c> and <c>LINES</c> environment
    /// dimensions.
    /// </summary>
    /// <remarks>
    /// This is an explicit configured-size fallback and is distinct from a
    /// successful live terminal-size query.
    /// </remarks>
    public static bool TryGetEnvironmentSize(out TerminalSize size)
    {
        string? columnsText =
            System.Environment.GetEnvironmentVariable("COLUMNS");
        string? rowsText =
            System.Environment.GetEnvironmentVariable("LINES");

        if (!TryParsePositiveInteger(columnsText, out int columns)
            || !TryParsePositiveInteger(rowsText, out int rows))
        {
            size = default;
            return false;
        }

        size = new TerminalSize(columns, rows);
        return true;
    }

    /// <summary>
    /// Attempts to read default dimensions from a terminal profile.
    /// </summary>
    /// <remarks>
    /// This is an explicit profile fallback and is distinct from a successful
    /// live terminal-size query.
    /// </remarks>
    public static bool TryGetProfileSize(
        TerminalDescription terminal,
        out TerminalSize size)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        int? columns = terminal.GetNumber(NumericCapability.Columns);
        int? rows = terminal.GetNumber(NumericCapability.Lines);

        if ((columns is null)
            || (rows is null)
            || (columns.Value <= 0)
            || (rows.Value <= 0))
        {
            size = default;
            return false;
        }

        size = new TerminalSize(columns.Value, rows.Value);
        return true;
    }

    private static ITerminalSizeProvider? CreateSizeProvider()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsTerminalSizeProvider();
        }

        if (OperatingSystem.IsLinux())
        {
            return new UnixTerminalSizeProvider(
                LinuxGetWindowSizeRequest);
        }

        if (OperatingSystem.IsMacOS())
        {
            return new UnixTerminalSizeProvider(
                MacOsGetWindowSizeRequest);
        }

        return null;
    }

    private static bool TryParsePositiveInteger(
        string? text,
        out int value)
    {
        if (!int.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out value)
            || (value <= 0))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static void Validate(TerminalStandardStream stream)
    {
        if (!Enum.IsDefined(typeof(TerminalStandardStream), stream))
        {
            throw new ArgumentOutOfRangeException(nameof(stream));
        }
    }
}
