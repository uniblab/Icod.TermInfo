namespace Icod.TermInfo.Source;

/// <summary>
/// Configures bounded <c>use=</c> inheritance resolution.
/// </summary>
public sealed class TermInfoSourceResolverOptions
{
    /// <summary>
    /// The default maximum number of inheritance edges from the requested root.
    /// </summary>
    public const int DefaultMaximumInheritanceDepth = 64;

    /// <summary>
    /// The largest inheritance-depth limit accepted by the resolver.
    /// </summary>
    public const int MaximumSupportedInheritanceDepth = 256;

    /// <summary>
    /// Initializes resolver options.
    /// </summary>
    /// <param name="maximumInheritanceDepth">
    /// The maximum number of <c>use=</c> edges permitted from the requested
    /// root. Zero permits only entries which require no parent resolution.
    /// </param>
    public TermInfoSourceResolverOptions(
        int maximumInheritanceDepth = DefaultMaximumInheritanceDepth)
    {
        if (maximumInheritanceDepth < 0
            || maximumInheritanceDepth > MaximumSupportedInheritanceDepth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumInheritanceDepth),
                maximumInheritanceDepth,
                $"The maximum inheritance depth must be between 0 and {MaximumSupportedInheritanceDepth}, inclusive.");
        }

        MaximumInheritanceDepth = maximumInheritanceDepth;
    }

    /// <summary>
    /// Gets the maximum number of inheritance edges from the requested root.
    /// </summary>
    public int MaximumInheritanceDepth { get; }
}
