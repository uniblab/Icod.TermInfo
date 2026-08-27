using Icod.TermInfo.Source;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Contains one independently loadable compiled terminfo entry produced from
/// source.
/// </summary>
public sealed class CompiledTermInfoSourceEntry {
	private readonly byte[] _data;

	internal CompiledTermInfoSourceEntry(
		TermInfoSourceEntry sourceEntry,
		byte[] data
	) {
		ArgumentNullException.ThrowIfNull( sourceEntry );
		ArgumentNullException.ThrowIfNull( data );

		CanonicalName = sourceEntry.CanonicalName;
		Aliases = sourceEntry.Aliases.ToArray();
		_data = (byte[])data.Clone();
	}

	/// <summary>
	/// Gets the canonical source entry name.
	/// </summary>
	public string CanonicalName { get; }

	/// <summary>
	/// Gets the source aliases in source order.
	/// </summary>
	public IReadOnlyList<string> Aliases { get; }

	/// <summary>
	/// Gets a copy of the complete compiled terminfo entry.
	/// </summary>
	/// <remarks>
	/// A new array is returned on every access so callers cannot mutate the
	/// compilation result retained by this object.
	/// </remarks>
	public byte[] Data {
		get {
			return (byte[])_data.Clone();
		}
	}
}
