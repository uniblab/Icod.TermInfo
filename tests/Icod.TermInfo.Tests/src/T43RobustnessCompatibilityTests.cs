using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T43RobustnessCompatibilityTests
{
	private const ushort LegacyMagic = 0x011A;
	private const ushort ExtendedNumberMagic = 0x021E;

	[Fact]
	public void AssemblyIdentifiesT43DevelopmentVersion()
	{
		Assembly assembly =
			typeof(TerminalDescription).Assembly;
		AssemblyName assemblyName =
			assembly.GetName();
		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;

		Assert.Equal(
			new Version(1, 0, 0, 0),
			assemblyName.Version);
		Assert.NotNull(
			informationalVersion);

		string semanticVersion =
			informationalVersion!
				.Split(
					'+',
					2)[0];

		Assert.Equal(
			"1.0.0-beta.1",
			semanticVersion);
	}

	[Fact]
	public void GeneratedCompiledCorpusIsCultureIndependent()
	{
		CultureInfo originalCulture =
			CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture =
			CultureInfo.CurrentUICulture;

		try
		{
			TerminalSnapshot[]? baseline = null;

			foreach (
				CultureInfo culture
				in new[]
				{
					CultureInfo.InvariantCulture,
					CultureInfo.GetCultureInfo("tr-TR"),
					CultureInfo.GetCultureInfo("ar-SA"),
				})
			{
				CultureInfo.CurrentCulture =
					culture;
				CultureInfo.CurrentUICulture =
					culture;

				TerminalSnapshot[] current =
					Enumerable
						.Range(
							0,
							128)
						.Select(
							index =>
								Snapshot(
									CompiledTermInfoParser.Parse(
										CreateGeneratedEntry(
											index))))
						.ToArray();

				if (baseline is null)
				{
					baseline =
						current;
				}
				else
				{
					Assert.Equal(
						baseline,
						current);
				}
			}
		}
		finally
		{
			CultureInfo.CurrentCulture =
				originalCulture;
			CultureInfo.CurrentUICulture =
				originalUiCulture;
		}
	}

	[Fact]
	public void ExpandedDeterministicRandomCorpusNeverEscapesParserContract()
	{
		Random random =
			new(
				0x0043_1001);

		for (
			int iteration = 0;
			iteration < 4_096;
			iteration++)
		{
			byte[] entry =
				new byte[
					random.Next(
						0,
						4_097)];

			random.NextBytes(
				entry);

			if (entry.Length >= sizeof(ushort)
				&& iteration % 3 == 0)
			{
				ushort magic =
					(iteration % 2 == 0)
						? LegacyMagic
						: ExtendedNumberMagic
					;

				BinaryPrimitives.WriteUInt16LittleEndian(
					entry.AsSpan(
						0,
						sizeof(ushort)),
					magic);
			}

			Exception? exception =
				Record.Exception(
					() =>
						CompiledTermInfoParser.Parse(
							entry));

			if (exception is not null)
			{
				Assert.IsType<CompiledTermInfoFormatException>(
					exception);
			}
		}
	}

	[Theory]
	[InlineData("compiled/t29-legacy-minimal.bin", 0x4301)]
	[InlineData("compiled/t29-legacy-alignment.bin", 0x4302)]
	[InlineData("compiled/t29-legacy-edge.bin", 0x4303)]
	[InlineData("compiled/t29-extended.bin", 0x4304)]
	[InlineData("compiled/t29-extended32.bin", 0x4305)]
	public void ExpandedDeterministicMutationsNeverEscapeParserContract(
		string relativePath,
		int seed)
	{
		ArgumentNullException.ThrowIfNull(
			relativePath);

		byte[] original =
			ReadFixture(
				relativePath);
		Random random =
			new(
				seed);

		for (
			int iteration = 0;
			iteration < 512;
			iteration++)
		{
			byte[] entry =
				(byte[])original.Clone();
			int editCount =
				random.Next(
					1,
					9);

			for (
				int edit = 0;
				edit < editCount;
				edit++)
			{
				int offset =
					random.Next(
						entry.Length);
				int bit =
					random.Next(
						0,
						8);

				entry[offset] ^=
					(byte)(1 << bit);
			}

			Exception? exception =
				Record.Exception(
					() =>
						CompiledTermInfoParser.Parse(
							entry));

			if (exception is not null)
			{
				Assert.IsType<CompiledTermInfoFormatException>(
					exception);
			}
		}
	}

	[Fact]
	public async Task DirectoryProviderSingleFlightSurvivesConcurrentStress()
	{
		using TemporaryDirectory temporary =
			new();
		string name =
			"t29-legacy-minimal";

		WriteLiteralCandidate(
			temporary.Root,
			name,
			ReadFixture(
				"compiled/t29-legacy-minimal.bin"));

		DirectoryTerminalDescriptionProvider provider =
			new(
				temporary.Root);

		using ManualResetEventSlim gate =
			new(
				initialState: false);

		Task<TerminalDescription>[] tasks =
			Enumerable
				.Range(
					0,
					256)
				.Select(
					_ =>
						Task.Run(
							() =>
							{
								gate.Wait();
								return Load(
									provider,
									name);
							}))
				.ToArray();

		gate.Set();

		TerminalDescription[] terminals =
			await Task.WhenAll(
				tasks);

		TerminalDescription first =
			terminals[0];

		Assert.All(
			terminals,
			terminal =>
				Assert.Same(
					first,
					terminal));
	}

	[Fact]
	public void ParserMaximumEntrySizeBoundaryIsExact()
	{
		byte[] entry =
			ReadFixture(
				"compiled/t29-legacy-minimal.bin");

		CompiledTermInfoParserOptions exact =
			new(
				maximumEntrySize: entry.Length);

		TerminalDescription terminal =
			CompiledTermInfoParser.Parse(
				entry,
				exact);

		Assert.Equal(
			"t29-legacy-minimal",
			terminal.Name);

		CompiledTermInfoParserOptions tooSmall =
			new(
				maximumEntrySize:
					entry.Length - 1);

		Assert.Throws<CompiledTermInfoFormatException>(
			() =>
				CompiledTermInfoParser.Parse(
					entry,
					tooSmall));
	}

	private static byte[] CreateGeneratedEntry(
		int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(
				nameof(index));
		}

		ushort magic =
			(index % 2 == 0)
				? LegacyMagic
				: ExtendedNumberMagic
			;
		string name =
			$"t43-generated-{index:D3}";
		string alias =
			$"t43-alias-{index:D3}";
		byte[] names =
			Encoding.ASCII.GetBytes(
				$"{name}|{alias}|T43 generated entry {index:D3}\0");
		int booleanCount =
			index % 9;
		int numericWidth =
			(magic == ExtendedNumberMagic)
				? sizeof(int)
				: sizeof(short)
			;

		int numericOffset =
			12
			+ names.Length
			+ booleanCount;

		if ((numericOffset & 1) != 0)
		{
			numericOffset++;
		}

		byte[] entry =
			new byte[
				numericOffset
				+ numericWidth];

		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				0,
				sizeof(ushort)),
			magic);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				2,
				sizeof(ushort)),
			checked((ushort)names.Length));
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				4,
				sizeof(ushort)),
			checked((ushort)booleanCount));
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				6,
				sizeof(ushort)),
			1);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				8,
				sizeof(ushort)),
			0);
		BinaryPrimitives.WriteUInt16LittleEndian(
			entry.AsSpan(
				10,
				sizeof(ushort)),
			0);

		names
			.AsSpan()
			.CopyTo(
				entry.AsSpan(12));

		for (
			int boolean = 0;
			boolean < booleanCount;
			boolean++)
		{
			entry[
				12
				+ names.Length
				+ boolean] =
				(byte)(boolean % 2);
		}

		int columns =
			(magic == ExtendedNumberMagic)
				? 100_000 + index
				: 80 + index
			;

		if (magic == ExtendedNumberMagic)
		{
			BinaryPrimitives.WriteInt32LittleEndian(
				entry.AsSpan(
					numericOffset,
					sizeof(int)),
				columns);
		}
		else
		{
			BinaryPrimitives.WriteInt16LittleEndian(
				entry.AsSpan(
					numericOffset,
					sizeof(short)),
				checked((short)columns));
		}

		return entry;
	}

	private static TerminalSnapshot Snapshot(
		TerminalDescription terminal)
	{
		ArgumentNullException.ThrowIfNull(
			terminal);

		return new TerminalSnapshot(
			terminal.Name,
			terminal.Description,
			string.Join(
				"\u001F",
				terminal.Aliases),
			terminal.GetNumber(
				NumericCapability.Columns),
			terminal.BooleanCapabilities.Count,
			terminal.NumericCapabilities.Count,
			terminal.StringCapabilities.Count,
			terminal.ExtendedCapabilities.Count);
	}

	private static TerminalDescription Load(
		ITerminalDescriptionProvider provider,
		string name)
	{
		ArgumentNullException.ThrowIfNull(
			provider);
		ArgumentNullException.ThrowIfNull(
			name);

		if (!provider.TryLoad(
				name,
				out TerminalDescription? terminal))
		{
			throw new InvalidOperationException(
				$"Expected provider to load '{name}'.");
		}

		return terminal;
	}

	private static byte[] ReadFixture(
		string relativePath)
	{
		ArgumentNullException.ThrowIfNull(
			relativePath);

		return File.ReadAllBytes(
			Path.Combine(
				AppContext.BaseDirectory,
				"fixtures",
				"compiled-terminfo",
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar)));
	}

	private static string WriteLiteralCandidate(
		string root,
		string name,
		byte[] entry)
	{
		ArgumentNullException.ThrowIfNull(
			root);
		ArgumentNullException.ThrowIfNull(
			name);
		ArgumentNullException.ThrowIfNull(
			entry);

		string directory =
			Path.Combine(
				root,
				name[0].ToString());

		Directory.CreateDirectory(
			directory);

		string path =
			Path.Combine(
				directory,
				name);

		File.WriteAllBytes(
			path,
			entry);

		return path;
	}

	private readonly record struct TerminalSnapshot(
		string Name,
		string? Description,
		string Aliases,
		int? Columns,
		int BooleanCount,
		int NumericCount,
		int StringCount,
		int ExtendedCount);

	private sealed class TemporaryDirectory : IDisposable
	{
		internal TemporaryDirectory()
		{
			Root =
				Path.Combine(
					Path.GetTempPath(),
					"icod-terminfo-t43-"
					+ Guid.NewGuid().ToString("N"));

			Directory.CreateDirectory(
				Root);
		}

		internal string Root
		{
			get;
		}

		public void Dispose()
		{
			if (Directory.Exists(
					Root))
			{
				Directory.Delete(
					Root,
					recursive: true);
			}
		}
	}
}
