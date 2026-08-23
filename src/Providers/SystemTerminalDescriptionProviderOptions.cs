namespace Icod.TermInfo;

/// <summary>
/// Configures which system terminfo discovery sources a system provider is
/// permitted to consult.
/// </summary>
public sealed class SystemTerminalDescriptionProviderOptions
{
    /// <summary>
    /// Initializes immutable system-discovery options.
    /// </summary>
    /// <param name="useEnvironment">
    /// Whether environment-controlled discovery inputs may be consulted.
    /// </param>
    /// <param name="useUserDatabase">
    /// Whether user-local terminfo discovery may be consulted.
    /// </param>
    /// <param name="useSystemDatabases">
    /// Whether platform system terminfo databases may be consulted.
    /// </param>
    /// <param name="parserOptions">
    /// Optional compiled-entry parser limits. Values are snapshotted by this
    /// options instance.
    /// </param>
    public SystemTerminalDescriptionProviderOptions(
        bool useEnvironment = true,
        bool useUserDatabase = true,
        bool useSystemDatabases = true,
        CompiledTermInfoParserOptions? parserOptions = null)
    {
        UseEnvironment = useEnvironment;
        UseUserDatabase = useUserDatabase;
        UseSystemDatabases = useSystemDatabases;

        CompiledTermInfoParserOptions effectiveParserOptions =
            parserOptions ?? new CompiledTermInfoParserOptions();
        ParserOptions =
            new CompiledTermInfoParserOptions(
                effectiveParserOptions.MaximumEntrySize);
    }

    /// <summary>
    /// Gets whether environment-controlled discovery inputs may be consulted.
    /// </summary>
    public bool UseEnvironment { get; }

    /// <summary>
    /// Gets whether user-local terminfo discovery may be consulted.
    /// </summary>
    public bool UseUserDatabase { get; }

    /// <summary>
    /// Gets whether platform system terminfo databases may be consulted.
    /// </summary>
    public bool UseSystemDatabases { get; }

    /// <summary>
    /// Gets the immutable compiled-entry parser limits.
    /// </summary>
    public CompiledTermInfoParserOptions ParserOptions { get; }
}
