using System.Diagnostics;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Vt100OutputTests {
	[Fact]
	public void Vt100RawCapabilitiesStillContainPaddingAnnotations() {
		Assert.Equal(
			"\x1b[H\x1b[J$<50>",
			TerminalProfiles.Vt100.GetRequiredString(
				StringCapability.ClearScreen
			)
		);
		Assert.Equal(
			"\x1b[1m$<2>",
			TerminalProfiles.Vt100.GetRequiredString(
				StringCapability.EnterBoldMode
			)
		);
		Assert.Equal(
			"\x1b[C$<2>",
			TerminalProfiles.Vt100.GetRequiredString(
				StringCapability.CursorRightOne
			)
		);
	}

	[Fact]
	public void IgnoreModeEmitsVt100PayloadWithoutLiteralPaddingSyntax() {
		string raw =
			TerminalProfiles.Vt100.GetRequiredString(
				StringCapability.ClearScreen
			);
		StringWriter writer = new();

		TermInfoOutput.TPuts(
			raw,
			affectedLines: 1,
			writer,
			PaddingMode.Ignore
		);

		Assert.Equal( "\x1b[H\x1b[J", writer.ToString() );
	}

	[Fact]
	public void SleepModeSchedulesVt100PaddingAnnotation() {
		string raw =
			TerminalProfiles.Vt100.GetRequiredString(
				StringCapability.CursorRightOne
			);
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.TPuts(
			raw,
			affectedLines: 1,
			writer,
			PaddingMode.Sleep,
			delayProvider
		);

		Assert.Equal( "\x1b[C", writer.ToString() );
		Assert.Single( delayProvider.Delays );
		Assert.Equal(
			TimeSpan.FromMilliseconds( 2 ),
			delayProvider.Delays[0]
		);
	}

	[Fact]
	public void DelayModeRemainsObservablySlowerThanIgnoreMode() {
		const string Value = "A$<20>B";

		Stopwatch ignore = Stopwatch.StartNew();
		TermInfoOutput.TPuts(
			Value,
			affectedLines: 1,
			TextWriter.Null,
			PaddingMode.Ignore
		);
		ignore.Stop();

		Stopwatch sleep = Stopwatch.StartNew();
		TermInfoOutput.TPuts(
			Value,
			affectedLines: 1,
			TextWriter.Null,
			PaddingMode.Sleep
		);
		sleep.Stop();

		Assert.True(
			sleep.Elapsed >= TimeSpan.FromMilliseconds( 10 ),
			$"Expected observable delay; measured {sleep.Elapsed}."
		);
		Assert.True(
			sleep.Elapsed > ignore.Elapsed,
			$"Sleep mode {sleep.Elapsed} should exceed ignore mode {ignore.Elapsed}."
		);
	}

	private sealed class RecordingDelayProvider : ITermInfoDelayProvider {
		internal List<TimeSpan> Delays { get; } = [];

		public void Delay( TimeSpan delay ) {
			Delays.Add( delay );
		}

		public ValueTask DelayAsync(
			TimeSpan delay,
			CancellationToken cancellationToken = default
		) {
			cancellationToken.ThrowIfCancellationRequested();
			Delays.Add( delay );
			return ValueTask.CompletedTask;
		}
	}
}
