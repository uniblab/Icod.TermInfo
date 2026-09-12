using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoOutputTests {
	[Fact]
	public void IgnoreModeStripsPaddingAndPreservesPayload() {
		StringWriter writer = new();

		TermInfoOutput.TPuts(
			"A$<5>B$<2.5/>C$<1*>D",
			affectedLines: 3,
			writer,
			PaddingMode.Ignore
		);

		Assert.Equal( "ABCD", writer.ToString() );
	}

	[Fact]
	public void SleepModeUsesDelayProviderAndMultiplier() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.TPuts(
			"A$<5>B$<2.5/>C$<1*>D",
			affectedLines: 3,
			writer,
			PaddingMode.Sleep,
			delayProvider
		);

		Assert.Equal( "ABCD", writer.ToString() );
		Assert.Equal(
			new[] {
				TimeSpan.FromMilliseconds( 5 ),
				TimeSpan.FromMilliseconds( 2.5 ),
				TimeSpan.FromMilliseconds( 3 ),
			},
			delayProvider.Delays
		);
	}

	[Fact]
	public void MultiplierWithZeroAffectedLinesProducesNoDelay() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.TPuts(
			"A$<10*>B",
			affectedLines: 0,
			writer,
			PaddingMode.Sleep,
			delayProvider
		);

		Assert.Equal( "AB", writer.ToString() );
		Assert.Empty( delayProvider.Delays );
	}

	[Theory]
	[InlineData( "unterminated $<5", "Unterminated padding annotation." )]
	[InlineData( "$<>" , "Padding duration is missing." )]
	[InlineData( "$<x>", "Padding duration is invalid." )]
	[InlineData( "$<1..2>", "Padding duration is invalid." )]
	[InlineData( "$<1**>", "Padding annotation has an invalid modifier." )]
	[InlineData( "$<1x>", "Padding annotation has an invalid modifier." )]
	public void MalformedPaddingIsRejected(
		string value,
		string expectedMessage
	) {
		StringWriter writer = new();

		TermInfoPaddingFormatException exception =
			Assert.Throws<TermInfoPaddingFormatException>(
				() => TermInfoOutput.TPuts(
					value,
					affectedLines: 1,
					writer,
					PaddingMode.Ignore
				)
			);

		Assert.Contains(
			expectedMessage,
			exception.Message,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void NonPaddingDollarSyntaxIsPreserved() {
		StringWriter writer = new();

		TermInfoOutput.TPuts(
			"price:$5;literal:$<not-padding?",
			affectedLines: 1,
			writer,
			PaddingMode.Ignore
		);

		Assert.Equal( "price:$5;literal:$<not-padding?", writer.ToString() );
	}

	[Fact]
	public void PutPUsesSingleAffectedLine() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.PutP(
			"A$<4*>B",
			writer,
			PaddingMode.Sleep,
			delayProvider
		);

		Assert.Equal( "AB", writer.ToString() );
		Assert.Single( delayProvider.Delays );
		Assert.Equal(
			TimeSpan.FromMilliseconds( 4 ),
			delayProvider.Delays[0]
		);
	}

	[Fact]
	public void CallbackOverloadPreservesPayload() {
		List<char> output = [];

		TermInfoOutput.TPuts(
			"A$<5>B",
			affectedLines: 2,
			output.Add,
			PaddingMode.Ignore
		);

		Assert.Equal( "AB", new string( output.ToArray() ) );
	}

	[Fact]
	public void CallbackOverloadValidatesCallback() {
		Assert.Throws<ArgumentNullException>(
			() => TermInfoOutput.TPuts(
				"value",
				affectedLines: 1,
				(Action<char>)null!,
				PaddingMode.Ignore
			)
		);
	}

	[Fact]
	public void AsyncWriterOverloadUsesAsyncDelayProvider() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.TPutsAsync(
			"A$<3>B$<2*>C",
			affectedLines: 4,
			writer,
			PaddingMode.Sleep,
			delayProvider
		).GetAwaiter().GetResult();

		Assert.Equal( "ABC", writer.ToString() );
		Assert.Equal(
			new[] {
				TimeSpan.FromMilliseconds( 3 ),
				TimeSpan.FromMilliseconds( 8 ),
			},
			delayProvider.AsyncDelays
		);
		Assert.Empty( delayProvider.Delays );
	}

	[Fact]
	public void AsyncPutPUsesSingleAffectedLine() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new();

		TermInfoOutput.PutPAsync(
			"A$<4*>B",
			writer,
			PaddingMode.Sleep,
			delayProvider
		).GetAwaiter().GetResult();

		Assert.Equal( "AB", writer.ToString() );
		Assert.Single( delayProvider.AsyncDelays );
		Assert.Equal(
			TimeSpan.FromMilliseconds( 4 ),
			delayProvider.AsyncDelays[0]
		);
	}

	[Fact]
	public void AsyncCancellationIsObservedBeforeOutput() {
		StringWriter writer = new();
		using CancellationTokenSource source = new();
		source.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => TermInfoOutput.TPutsAsync(
				"payload",
				affectedLines: 1,
				writer,
				PaddingMode.Ignore,
				cancellationToken: source.Token
			).GetAwaiter().GetResult()
		);

		Assert.Equal( string.Empty, writer.ToString() );
	}

	[Fact]
	public void AsyncSleepCancellationCanInterruptDelay() {
		StringWriter writer = new();
		RecordingDelayProvider delayProvider = new() {
			CancelAsync = true,
		};

		Assert.Throws<OperationCanceledException>(
			() => TermInfoOutput.TPutsAsync(
				"A$<10>B",
				affectedLines: 1,
				writer,
				PaddingMode.Sleep,
				delayProvider
			).GetAwaiter().GetResult()
		);

		Assert.Equal( "A", writer.ToString() );
		Assert.Single( delayProvider.AsyncDelays );
	}

	[Fact]
	public void SyncOutputFlushesBeforeDelay() {
		RecordingWriter writer = new();
		RecordingDelayProvider delayProvider = new() {
			OnDelay = () => Assert.Equal( 1, writer.FlushCount ),
		};

		TermInfoOutput.TPuts(
			"A$<1>B",
			affectedLines: 1,
			writer,
			PaddingMode.Sleep,
			delayProvider
		);

		Assert.Equal( "AB", writer.ToString() );
		Assert.Equal( 1, writer.FlushCount );
	}

	[Fact]
	public void AsyncOutputFlushesBeforeDelay() {
		RecordingWriter writer = new();
		RecordingDelayProvider delayProvider = new() {
			OnAsyncDelay = () => Assert.Equal( 1, writer.AsyncFlushCount ),
		};

		TermInfoOutput.TPutsAsync(
			"A$<1>B",
			affectedLines: 1,
			writer,
			PaddingMode.Sleep,
			delayProvider
		).GetAwaiter().GetResult();

		Assert.Equal( "AB", writer.ToString() );
		Assert.Equal( 1, writer.AsyncFlushCount );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( -42 )]
	public void NegativeAffectedLinesAreRejected( int affectedLines ) {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => TermInfoOutput.TPuts(
				"value",
				affectedLines,
				TextWriter.Null,
				PaddingMode.Ignore
			)
		);
	}

	[Fact]
	public void UndefinedPaddingModeIsRejected() {
		PaddingMode invalid = (PaddingMode)int.MaxValue;

		Assert.Throws<ArgumentOutOfRangeException>(
			() => TermInfoOutput.TPuts(
				"value",
				affectedLines: 1,
				TextWriter.Null,
				invalid
			)
		);
	}

	[Fact]
	public void NullInputsAreRejected() {
		Assert.Throws<ArgumentNullException>(
			() => TermInfoOutput.TPuts(
				null!,
				affectedLines: 1,
				TextWriter.Null,
				PaddingMode.Ignore
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoOutput.TPuts(
				"value",
				affectedLines: 1,
				(TextWriter)null!,
				PaddingMode.Ignore
			)
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoOutput.TPutsAsync(
				"value",
				affectedLines: 1,
				(TextWriter)null!,
				PaddingMode.Ignore
			).GetAwaiter().GetResult()
		);
	}

	private sealed class RecordingDelayProvider : ITermInfoDelayProvider {
		internal List<TimeSpan> Delays { get; } = [];

		internal List<TimeSpan> AsyncDelays { get; } = [];

		internal bool CancelAsync { get; init; }

		internal Action? OnDelay { get; init; }

		internal Action? OnAsyncDelay { get; init; }

		public void Delay( TimeSpan delay ) {
			Delays.Add( delay );
			OnDelay?.Invoke();
		}

		public ValueTask DelayAsync(
			TimeSpan delay,
			CancellationToken cancellationToken = default
		) {
			AsyncDelays.Add( delay );
			OnAsyncDelay?.Invoke();

			if ( CancelAsync ) {
				throw new OperationCanceledException( cancellationToken );
			}

			cancellationToken.ThrowIfCancellationRequested();
			return ValueTask.CompletedTask;
		}
	}

	private sealed class RecordingWriter : StringWriter {
		internal int FlushCount { get; private set; }

		internal int AsyncFlushCount { get; private set; }

		public override void Flush() {
			FlushCount++;
			base.Flush();
		}

		public override Task FlushAsync() {
			AsyncFlushCount++;
			return base.FlushAsync();
		}
	}
}
