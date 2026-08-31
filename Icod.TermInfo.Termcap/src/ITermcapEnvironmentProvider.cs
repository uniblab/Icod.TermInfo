namespace Icod.TermInfo.Termcap;

/// <summary>
/// Supplies explicitly requested process-environment values to termcap
/// acquisition.
/// </summary>
/// <remarks>
/// TC06 uses this abstraction so environment-dependent discovery remains an
/// opt-in caller decision and can be tested without mutating process-global
/// environment state.
/// </remarks>
public interface ITermcapEnvironmentProvider
{
	/// <summary>
	/// Gets one environment variable value, or <see langword="null"/> when it is
	/// not defined.
	/// </summary>
	string? GetEnvironmentVariable(
		string name
	);
}
