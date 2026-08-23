using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Icod.TermInfo;

/// <summary>
/// Loads compiled terminal descriptions from one caller-supplied directory
/// tree.
/// </summary>
public sealed class DirectoryTerminalDescriptionProvider
    : ITerminalDescriptionProvider
{
    private const int FileBufferSize = 4096;

    private readonly ConcurrentDictionary<string, Lazy<TerminalDescription?>> _cache =
        new(StringComparer.Ordinal);
    private readonly CompiledTermInfoParserOptions _parserOptions;

    /// <summary>
    /// Initializes a provider rooted at the specified terminfo directory.
    /// </summary>
    /// <param name="root">
    /// The directory containing conventional first-character terminfo
    /// subdirectories.
    /// </param>
    /// <param name="parserOptions">
    /// Optional parser resource limits. The values are snapshotted when the
    /// provider is constructed.
    /// </param>
    public DirectoryTerminalDescriptionProvider(
        string root,
        CompiledTermInfoParserOptions? parserOptions = null)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (string.IsNullOrWhiteSpace(root))
        {
            throw new ArgumentException(
                "The terminfo directory root cannot be empty or whitespace.",
                nameof(root));
        }

        Root = Path.GetFullPath(root);

        CompiledTermInfoParserOptions effectiveOptions =
            parserOptions ?? new CompiledTermInfoParserOptions();
        _parserOptions =
            new CompiledTermInfoParserOptions(
                effectiveOptions.MaximumEntrySize);
    }

    /// <summary>
    /// Gets the canonical absolute directory root owned by this provider.
    /// </summary>
    public string Root { get; }

    /// <inheritdoc/>
    public bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ValidateTerminalName(name);

        Lazy<TerminalDescription?> load =
            _cache.GetOrAdd(
                name,
                CreateLoad);

        try
        {
            terminal =
                load.Value;
        }
        catch
        {
            _cache.TryRemove(
                new KeyValuePair<string, Lazy<TerminalDescription?>>(
                    name,
                    load));
            throw;
        }

        if (terminal is null)
        {
            _cache.TryRemove(
                new KeyValuePair<string, Lazy<TerminalDescription?>>(
                    name,
                    load));
            return false;
        }

        return true;
    }

    private Lazy<TerminalDescription?> CreateLoad(
        string name)
    {
        return new Lazy<TerminalDescription?>(
            () => LoadUncached(
                name),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private TerminalDescription? LoadUncached(
        string name)
    {
        if (TryLoadUncached(
                name,
                out TerminalDescription? terminal))
        {
            return terminal;
        }

        return null;
    }

    private bool TryLoadUncached(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        string literalDirectory =
            name[0].ToString();
        string literalPath =
            Path.Combine(
                Root,
                literalDirectory,
                name);

        if (TryLoadCandidate(
                literalPath,
                name,
                out terminal))
        {
            return true;
        }

        if (TryGetHexDirectoryName(
                name[0],
                out string? hexDirectory))
        {
            string hexadecimalPath =
                Path.Combine(
                    Root,
                    hexDirectory,
                    name);

            if (TryLoadCandidate(
                    hexadecimalPath,
                    name,
                    out terminal))
            {
                return true;
            }
        }

        terminal = null;
        return false;
    }

    private bool TryLoadCandidate(
        string path,
        string requestedName,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        try
        {
            terminal =
                ReadTerminal(
                    path);
        }
        catch (FileNotFoundException)
        {
            terminal = null;
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            terminal = null;
            return false;
        }

        VerifyIdentity(
            requestedName,
            terminal,
            path);
        return true;
    }

    private TerminalDescription ReadTerminal(string path)
    {
        using FileStream stream =
            new(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.SequentialScan);

        long length = stream.Length;
        if (length > _parserOptions.MaximumEntrySize)
        {
            throw new CompiledTermInfoFormatException(
                $"The compiled entry is {length} bytes, exceeding the configured maximum of {_parserOptions.MaximumEntrySize} bytes.",
                -1,
                "entry");
        }

        byte[] entry =
            new byte[(int)length];
        stream.ReadExactly(entry);

        if (stream.ReadByte() != -1)
        {
            throw new IOException(
                $"Compiled terminfo entry '{path}' changed length while it was being read.");
        }

        return CompiledTermInfoParser.Parse(
            entry,
            _parserOptions);
    }

    private static void VerifyIdentity(
        string requestedName,
        TerminalDescription terminal,
        string path)
    {
        if (string.Equals(
                requestedName,
                terminal.Name,
                StringComparison.Ordinal))
        {
            return;
        }

        foreach (string alias in terminal.Aliases)
        {
            if (string.Equals(
                    requestedName,
                    alias,
                    StringComparison.Ordinal))
            {
                return;
            }
        }

        throw new InvalidDataException(
            $"Compiled terminfo entry '{path}' identifies terminal '{terminal.Name}' and does not declare requested name '{requestedName}'.");
    }

    private static bool TryGetHexDirectoryName(
        char firstCharacter,
        [NotNullWhen(true)] out string? directory)
    {
        if (firstCharacter > byte.MaxValue)
        {
            directory = null;
            return false;
        }

        directory =
            ((byte)firstCharacter).ToString(
                "x2",
                CultureInfo.InvariantCulture);
        return true;
    }

    internal static void ValidateTerminalName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The terminal name cannot be empty or whitespace.",
                nameof(name));
        }

        if (string.Equals(
                name,
                ".",
                StringComparison.Ordinal)
            || string.Equals(
                name,
                "..",
                StringComparison.Ordinal)
            || Path.IsPathRooted(name))
        {
            throw new ArgumentException(
                "The terminal name must be an exact non-rooted file name.",
                nameof(name));
        }

        foreach (char character in name)
        {
            if (character == '\0'
                || character == '/'
                || character == '\\'
                || char.IsControl(character)
                || char.IsSurrogate(character))
            {
                throw new ArgumentException(
                    "The terminal name contains unsafe path syntax.",
                    nameof(name));
            }
        }

        if (name.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException(
                "The terminal name contains a character which is invalid in a file name on this platform.",
                nameof(name));
        }

        if (OperatingSystem.IsWindows())
        {
            ValidateWindowsTerminalName(name);
        }
    }

    private static void ValidateWindowsTerminalName(string name)
    {
        if (name[^1] == '.'
            || name[^1] == ' ')
        {
            throw new ArgumentException(
                "The terminal name cannot end with a period or space on Windows.",
                nameof(name));
        }

        int dot = name.IndexOf('.');
        string stem =
            (dot < 0)
                ? name
                : name[..dot]
        ;

        if (IsWindowsDeviceStem(stem))
        {
            throw new ArgumentException(
                "The terminal name conflicts with a reserved Windows device name.",
                nameof(name));
        }
    }

    private static bool IsWindowsDeviceStem(string stem)
    {
        if (stem.Equals(
                "CON",
                StringComparison.OrdinalIgnoreCase)
            || stem.Equals(
                "PRN",
                StringComparison.OrdinalIgnoreCase)
            || stem.Equals(
                "AUX",
                StringComparison.OrdinalIgnoreCase)
            || stem.Equals(
                "NUL",
                StringComparison.OrdinalIgnoreCase)
            || stem.Equals(
                "CONIN$",
                StringComparison.OrdinalIgnoreCase)
            || stem.Equals(
                "CONOUT$",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (stem.Length != 4)
        {
            return false;
        }

        bool numberedDevice =
            stem.StartsWith(
                "COM",
                StringComparison.OrdinalIgnoreCase)
            || stem.StartsWith(
                "LPT",
                StringComparison.OrdinalIgnoreCase);

        return numberedDevice
            && stem[3] >= '1'
            && stem[3] <= '9';
    }
}
