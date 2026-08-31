namespace Icod.TermInfo.Termcap;

/// <summary>
/// Reads termcap acquisition inputs from the current process environment when
/// explicitly supplied by the caller.
/// </summary>
public sealed class SystemTermcapEnvironmentProvider : ITermcapEnvironmentProvider
{
	/// <inheritdoc/>
	public string? GetEnvironmentVariable(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return Environment.GetEnvironmentVariable( name );
	}
}
