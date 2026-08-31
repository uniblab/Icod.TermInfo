using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo.Termcap;

/// <summary>
/// Supplies text readers for explicitly selected termcap database paths.
/// </summary>
/// <remarks>
/// A clean missing path returns <see langword="false"/> and a null reader.
/// Provider failures other than a clean miss propagate to the caller. A reader
/// returned on success is owned and disposed by the acquisition operation.
/// </remarks>
public interface ITermcapFileProvider
{
	/// <summary>
	/// Attempts to open one termcap database path for bounded parser input.
	/// </summary>
	bool TryOpenText(
		string path,
		[NotNullWhen( true )] out TextReader? reader
	);
}
