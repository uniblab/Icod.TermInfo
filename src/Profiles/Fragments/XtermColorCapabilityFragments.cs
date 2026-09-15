/*
	Icod.TermInfo
	Provides managed terminfo runtime parsing, discovery, capabilities, and terminal profiles.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.TermInfo;

internal static class XtermColorCapabilityFragments {
	private const string LegacyForeground =
		"\u001b[3%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
		+ "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

	private const string LegacyBackground =
		"\u001b[4%?%p1%{1}%=%t4%e%p1%{3}%=%t6%e"
		+ "%p1%{4}%=%t1%e%p1%{6}%=%t3%e%p1%d%;m";

	private const string AixtermLegacyForeground =
		"%p1%{8}%/%{6}%*%{3}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
		+ "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

	private const string AixtermLegacyBackground =
		"%p1%{8}%/%{6}%*%{4}%+\u001b[%d%p1%{8}%m%Pa%?%ga%{1}%=%t4%e"
		+ "%ga%{3}%=%t6%e%ga%{4}%=%t1%e%ga%{6}%=%t3%e%ga%d%;m";

	internal static TerminalDescriptionBuilder ApplyXtermBasicEightColor(
		this TerminalDescriptionBuilder builder
	) {
		ArgumentNullException.ThrowIfNull( builder );

		return builder
			.ApplyAnsiIndexed( 8, 64 )
			.SetString(
				StringCapability.SetLegacyForegroundColor,
				LegacyForeground
			)
			.SetString(
				StringCapability.SetLegacyBackgroundColor,
				LegacyBackground
			);
	}

	internal static TerminalDescriptionBuilder ApplyXtermSixteenColor(
		this TerminalDescriptionBuilder builder
	) {
		ArgumentNullException.ThrowIfNull( builder );

		return builder
			.ApplyAixterm16( 256 )
			.ApplyXtermPaletteControls()
			.SetString(
				StringCapability.SetLegacyForegroundColor,
				AixtermLegacyForeground
			)
			.SetString(
				StringCapability.SetLegacyBackgroundColor,
				AixtermLegacyBackground
			);
	}
}
