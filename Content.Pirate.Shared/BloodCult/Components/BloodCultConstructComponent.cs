// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Manufactured constructs that work for the blood cult.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultConstructComponent : Component
{
	/// <summary>
	/// The body or soul stone anchoring the mind currently controlling this construct.
	/// </summary>
	[ViewVariables(VVAccess.ReadOnly)]
	public EntityUid? SourceEntity;

	[ViewVariables(VVAccess.ReadOnly)]
	public BloodCultConstructSourceKind SourceKind;

	[ViewVariables(VVAccess.ReadOnly)]
	public string? SourceContainerId;

	/// <summary>
	/// Juggernauts eject their source when entering critical condition. Lesser constructs eject it on death.
	/// </summary>
	[DataField]
	public bool EjectSourceOnCritical;
}

public enum BloodCultConstructSourceKind : byte
{
	Body,
	SoulStone,
}
