namespace Icod.TermInfo.Termcap;

/// <summary>
/// Selects whether termcap acquisition may append a conventional implicit
/// database path set after explicitly supplied sources.
/// </summary>
public enum TermcapDefaultPathPolicy
{
	/// <summary>Do not add any implicit termcap database paths.</summary>
	None = 0,

	/// <summary>
	/// Append the conventional ncurses-compatible path order
	/// <c>/etc/termcap</c>, <c>/usr/share/misc/termcap</c>, and, when a home
	/// directory was supplied, <c>$HOME/.termcap</c>.
	/// </summary>
	Ncurses = 1,
}
