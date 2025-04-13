// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.TeleportOnStateChange;

/// <summary>
/// Teleports the entity to the given coordinates when changed to the specified state.
/// </summary>
[RegisterComponent]
public sealed partial class TeleportOnStateChangeComponent : Component
{

    /// <summary>
    /// The co-ordinates to teleport to.
    /// </summary>
    [DataField]
    public EntityCoordinates? Coordinates;

    /// <summary>
    /// What mob-state should trigger the teleportation?
    /// </summary>
    [DataField]
    public MobState MobStateTrigger = MobState.Critical;

    /// <summary>
    /// Should the component be removed on trigger?
    /// </summary>
    [DataField]
    public bool RemoveOnTrigger;
}
