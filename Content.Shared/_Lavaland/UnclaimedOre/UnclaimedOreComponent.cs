// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.UnclaimedOre;

/// <summary>
///     Component that holds information about ore that hasn't been processed yet.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnclaimedOreComponent : Component
{
    [DataField]
    public float MiningPoints;
}
