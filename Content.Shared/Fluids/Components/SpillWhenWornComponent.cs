// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared.Fluids.Components;

/// <summary>
/// This entity will spill its contained solution onto the wearer when worn, and its
/// (empty) contents will be inaccessible while still worn.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpillWhenWornComponent : Component
{
    /// <summary>
    /// Name of the solution to spill.
    /// </summary>
    [DataField]
    public string Solution = "default";

    /// <summary>
    /// Tracks if this item is currently being worn.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsWorn;
}