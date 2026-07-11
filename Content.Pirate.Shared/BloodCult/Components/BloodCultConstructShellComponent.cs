// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;
using Content.Shared._White.RadialSelector;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// A hollow shell awaiting a soul
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultConstructShellComponent : Component
{
	/// <summary>
	/// Empty for the dedicated juggernaut shell. Lesser shells present these forms for selection.
	/// </summary>
	[DataField]
	public List<RadialSelectorEntry> Constructs = new();
}
