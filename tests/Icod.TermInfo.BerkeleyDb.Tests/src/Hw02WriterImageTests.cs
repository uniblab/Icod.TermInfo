/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates deterministic HW02 Hash-v9 inline image construction.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Icod.TermInfo.Tests.Shared;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hw02WriterImageTests {
	[Fact]
	public void PreparedPublicationPreservesExactCompiledNamesSection() {
		byte[] compiled = Hdb07HashV9FixtureBuilder.CreateCompiledEntry(
			"hw02-primary",
			"HW02 caf\u00E9 description",
			"hw02-alias"
		);
		var entry = new BerkeleyDbTerminalDatabaseEntry(
			"hw02-primary",
			[ "hw02-alias" ],
			compiled
		);

		Array prepared = InvokePreparePublications( [ entry ] );
		object publication = Assert.Single( prepared.Cast<object>() );
		PropertyInfo? property = publication.GetType().GetProperty(
			"StorageKey",
			BindingFlags.Instance
				| BindingFlags.Public
				| BindingFlags.NonPublic
		);

		Assert.NotNull( property );
		Assert.Equal(
			Encoding.Latin1.GetBytes(
				"hw02-primary|hw02-alias|HW02 caf\u00E9 description"
			),
			Assert.IsType<byte[]>( property!.GetValue( publication ) )
		);
	}

	private static Array InvokePreparePublications(
		BerkeleyDbTerminalDatabaseEntry[] entries
	) {
		MethodInfo method = Assert.IsType<MethodInfo>(
			typeof( BerkeleyDbTerminalDatabaseWriter ).GetMethod(
				"PreparePublications",
				BindingFlags.Static
					| BindingFlags.Public
					| BindingFlags.NonPublic
			)
		);

		try {
			return Assert.IsAssignableFrom<Array>(
				method.Invoke(
					null,
					new object[] {
						entries,
						new BerkeleyDbTerminalDatabaseWriterOptions(),
						CancellationToken.None,
					}
				)
			);
		} catch (
			TargetInvocationException exception
		) when ( exception.InnerException is not null ) {
			ExceptionDispatchInfo.Capture( exception.InnerException ).Throw();
			throw;
		}
	}
}
