using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Opens explicitly selected termcap database paths from the host filesystem.
/// </summary>
public sealed class SystemTermcapFileProvider : ITermcapFileProvider
{
	/// <inheritdoc/>
	public bool TryOpenText(
		string path,
		[NotNullWhen( true )] out TextReader? reader
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			reader = File.OpenText( path );
			return true;
		} catch ( FileNotFoundException ) {
			reader = null;
			return false;
		} catch ( DirectoryNotFoundException ) {
			reader = null;
			return false;
		}
	}
}
