namespace Icod.TermInfo;

internal enum SystemTerminalDatabaseLocationKind {
	EncodedTermInfo,
	TermInfoDirectory,
	UserDatabase,
	TermInfoDirsDirectory,
	PlatformDefaultDirectory,
}

internal sealed class SystemTerminalDatabaseLocation {
	internal SystemTerminalDatabaseLocation(
		SystemTerminalDatabaseLocationKind kind,
		string? path
	) {
		Kind = kind;
		Path = path;
	}

	internal SystemTerminalDatabaseLocationKind Kind {
		get;
	}

	internal string? Path {
		get;
	}
}
