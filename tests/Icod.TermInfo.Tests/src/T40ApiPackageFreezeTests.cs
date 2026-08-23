using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T40ApiPackageFreezeTests
{
	[Fact]
	public void CompiledParserSurfaceIsFrozen()
	{
		Assert.Equal(
			1_048_576,
			CompiledTermInfoParserOptions.DefaultMaximumEntrySize);
		Assert.Equal(
			16_777_216,
			CompiledTermInfoParserOptions.MaximumSupportedEntrySize);

		ConstructorInfo optionsConstructor =
			Assert.Single(
				typeof(CompiledTermInfoParserOptions)
					.GetConstructors(
						BindingFlags.Public
						| BindingFlags.Instance));
		ParameterInfo maximumEntrySize =
			Assert.Single(
				optionsConstructor.GetParameters());

		Assert.Equal(
			typeof(int),
			maximumEntrySize.ParameterType);
		Assert.True(
			maximumEntrySize.HasDefaultValue);
		Assert.Equal(
			CompiledTermInfoParserOptions.DefaultMaximumEntrySize,
			maximumEntrySize.DefaultValue);

		PropertyInfo maximum =
			Assert.Single(
				typeof(CompiledTermInfoParserOptions)
					.GetProperties(
						BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.DeclaredOnly));
		Assert.Equal(
			nameof(CompiledTermInfoParserOptions.MaximumEntrySize),
			maximum.Name);
		Assert.Equal(
			typeof(int),
			maximum.PropertyType);
		Assert.True(
			maximum.CanRead);
		Assert.False(
			maximum.CanWrite);

		Assert.True(
			typeof(CompiledTermInfoParser).IsAbstract);
		Assert.True(
			typeof(CompiledTermInfoParser).IsSealed);

		MethodInfo parse =
			Assert.Single(
				typeof(CompiledTermInfoParser)
					.GetMethods(
						BindingFlags.Public
						| BindingFlags.Static
						| BindingFlags.DeclaredOnly));
		Assert.Equal(
			nameof(CompiledTermInfoParser.Parse),
			parse.Name);
		Assert.Equal(
			typeof(TerminalDescription),
			parse.ReturnType);

		ParameterInfo[] parameters =
			parse.GetParameters();
		Assert.Equal(
			2,
			parameters.Length);
		Assert.Equal(
			typeof(ReadOnlySpan<byte>),
			parameters[0].ParameterType);
		Assert.Equal(
			typeof(CompiledTermInfoParserOptions),
			parameters[1].ParameterType);
		Assert.True(
			parameters[1].HasDefaultValue);
		Assert.Null(
			parameters[1].DefaultValue);
	}

	[Fact]
	public void CompiledFormatExceptionSurfaceIsFrozen()
	{
		Assert.True(
			typeof(FormatException).IsAssignableFrom(
				typeof(CompiledTermInfoFormatException)));
		Assert.True(
			typeof(CompiledTermInfoFormatException).IsSealed);

		string[] constructorShapes =
			typeof(CompiledTermInfoFormatException)
				.GetConstructors(
					BindingFlags.Public
					| BindingFlags.Instance)
				.Select(
					constructor =>
						string.Join(
							",",
							constructor
								.GetParameters()
								.Select(
									parameter =>
										parameter.ParameterType.Name)))
				.OrderBy(
					value => value,
					StringComparer.Ordinal)
				.ToArray();

		Assert.Equal(
			new[]
			{
				string.Empty,
				"String",
				"String,Exception",
			},
			constructorShapes);

		PropertyInfo[] properties =
			typeof(CompiledTermInfoFormatException)
				.GetProperties(
					BindingFlags.Public
					| BindingFlags.Instance
					| BindingFlags.DeclaredOnly);

		Assert.Equal(
			new[]
			{
				"Offset",
				"Section",
			},
			properties
				.Select(
					property => property.Name)
				.OrderBy(
					name => name,
					StringComparer.Ordinal)
				.ToArray());
		Assert.All(
			properties,
			property =>
			{
				Assert.True(
					property.CanRead);
				Assert.False(
					property.CanWrite);
			});
	}

	[Fact]
	public void DirectoryProviderSurfaceIsFrozen()
	{
		Assert.True(
			typeof(ITerminalDescriptionProvider).IsAssignableFrom(
				typeof(DirectoryTerminalDescriptionProvider)));

		ConstructorInfo constructor =
			Assert.Single(
				typeof(DirectoryTerminalDescriptionProvider)
					.GetConstructors(
						BindingFlags.Public
						| BindingFlags.Instance));
		ParameterInfo[] parameters =
			constructor.GetParameters();

		Assert.Equal(
			2,
			parameters.Length);
		Assert.Equal(
			typeof(string),
			parameters[0].ParameterType);
		Assert.Equal(
			typeof(CompiledTermInfoParserOptions),
			parameters[1].ParameterType);
		Assert.True(
			parameters[1].HasDefaultValue);
		Assert.Null(
			parameters[1].DefaultValue);

		PropertyInfo root =
			Assert.Single(
				typeof(DirectoryTerminalDescriptionProvider)
					.GetProperties(
						BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.DeclaredOnly));
		Assert.Equal(
			nameof(DirectoryTerminalDescriptionProvider.Root),
			root.Name);
		Assert.Equal(
			typeof(string),
			root.PropertyType);
		Assert.True(
			root.CanRead);
		Assert.False(
			root.CanWrite);

		AssertProviderTryLoadContract(
			typeof(DirectoryTerminalDescriptionProvider));
	}

	[Fact]
	public void SystemProviderAndOptionsSurfaceIsFrozen()
	{
		ConstructorInfo optionsConstructor =
			Assert.Single(
				typeof(SystemTerminalDescriptionProviderOptions)
					.GetConstructors(
						BindingFlags.Public
						| BindingFlags.Instance));
		ParameterInfo[] optionParameters =
			optionsConstructor.GetParameters();

		Assert.Equal(
			4,
			optionParameters.Length);
		Assert.Equal(
			new[]
			{
				typeof(bool),
				typeof(bool),
				typeof(bool),
				typeof(CompiledTermInfoParserOptions),
			},
			optionParameters
				.Select(
					parameter => parameter.ParameterType)
				.ToArray());
		Assert.All(
			optionParameters,
			parameter => Assert.True(
				parameter.HasDefaultValue));

		PropertyInfo[] optionProperties =
			typeof(SystemTerminalDescriptionProviderOptions)
				.GetProperties(
					BindingFlags.Public
					| BindingFlags.Instance
					| BindingFlags.DeclaredOnly);
		Assert.Equal(
			new[]
			{
				"ParserOptions",
				"UseEnvironment",
				"UseSystemDatabases",
				"UseUserDatabase",
			},
			optionProperties
				.Select(
					property => property.Name)
				.OrderBy(
					name => name,
					StringComparer.Ordinal)
				.ToArray());
		Assert.All(
			optionProperties,
			property =>
			{
				Assert.True(
					property.CanRead);
				Assert.False(
					property.CanWrite);
			});

		ConstructorInfo providerConstructor =
			Assert.Single(
				typeof(SystemTerminalDescriptionProvider)
					.GetConstructors(
						BindingFlags.Public
						| BindingFlags.Instance));
		ParameterInfo options =
			Assert.Single(
				providerConstructor.GetParameters());

		Assert.Equal(
			typeof(SystemTerminalDescriptionProviderOptions),
			options.ParameterType);
		Assert.True(
			options.HasDefaultValue);
		Assert.Null(
			options.DefaultValue);

		AssertProviderTryLoadContract(
			typeof(SystemTerminalDescriptionProvider));
	}

	[Fact]
	public void AcquisitionNullabilityContractsAreFrozen()
	{
		NullabilityInfoContext context =
			new();

		MethodInfo parse =
			Assert.Single(
				typeof(CompiledTermInfoParser)
					.GetMethods(
						BindingFlags.Public
						| BindingFlags.Static
						| BindingFlags.DeclaredOnly));
		ParameterInfo parserOptions =
			parse.GetParameters()[1];
		Assert.Equal(
			NullabilityState.Nullable,
			context.Create(parserOptions).ReadState);

		ConstructorInfo directoryConstructor =
			Assert.Single(
				typeof(DirectoryTerminalDescriptionProvider)
					.GetConstructors());
		Assert.Equal(
			NullabilityState.Nullable,
			context
				.Create(
					directoryConstructor.GetParameters()[1])
				.ReadState);

		ConstructorInfo systemConstructor =
			Assert.Single(
				typeof(SystemTerminalDescriptionProvider)
					.GetConstructors());
		Assert.Equal(
			NullabilityState.Nullable,
			context
				.Create(
					systemConstructor.GetParameters()[0])
				.ReadState);

		PropertyInfo section =
			typeof(CompiledTermInfoFormatException)
				.GetProperty(
					nameof(CompiledTermInfoFormatException.Section))!;
		Assert.Equal(
			NullabilityState.Nullable,
			context.Create(section).ReadState);
	}

	[Fact]
	public void AcquisitionProvidersExposeNoPublicCacheOrRefreshControl()
	{
		Type[] providers =
		[
			typeof(DirectoryTerminalDescriptionProvider),
			typeof(SystemTerminalDescriptionProvider),
		];
		string[] forbiddenFragments =
		[
			"Cache",
			"Clear",
			"Refresh",
			"Reload",
		];

		foreach (Type provider in providers)
		{
			MemberInfo[] members =
				provider.GetMembers(
					BindingFlags.Public
					| BindingFlags.Instance
					| BindingFlags.Static
					| BindingFlags.DeclaredOnly);

			Assert.DoesNotContain(
				members,
				member =>
					forbiddenFragments.Any(
						fragment =>
							member.Name.Contains(
								fragment,
								StringComparison.Ordinal)));
		}
	}

	[Fact]
	public void TerminalDatabaseCompositionContractIsFrozen()
	{
		Assert.True(
			typeof(ITerminalDescriptionProvider).IsAssignableFrom(
				typeof(TerminalDatabase)));

		ITerminalDescriptionProvider builtIn =
			TerminalDatabase.BuiltIn;
		Assert.True(
			builtIn.TryLoad(
				"xterm",
				out TerminalDescription? terminal));
		Assert.Same(
			TerminalProfiles.Xterm,
			terminal);
	}

	private static void AssertProviderTryLoadContract(
		Type providerType)
	{
		ArgumentNullException.ThrowIfNull(providerType);

		MethodInfo tryLoad =
			Assert.Single(
				providerType.GetMethods(
					BindingFlags.Public
					| BindingFlags.Instance
					| BindingFlags.DeclaredOnly),
				method =>
					string.Equals(
						method.Name,
						nameof(ITerminalDescriptionProvider.TryLoad),
						StringComparison.Ordinal));
		ParameterInfo[] parameters =
			tryLoad.GetParameters();

		Assert.Equal(
			typeof(bool),
			tryLoad.ReturnType);
		Assert.Equal(
			2,
			parameters.Length);
		Assert.Equal(
			typeof(string),
			parameters[0].ParameterType);
		Assert.Equal(
			typeof(TerminalDescription).MakeByRefType(),
			parameters[1].ParameterType);

		NotNullWhenAttribute? notNullWhen =
			parameters[1]
				.GetCustomAttribute<NotNullWhenAttribute>();
		Assert.NotNull(
			notNullWhen);
		Assert.True(
			notNullWhen!.ReturnValue);
	}
}
