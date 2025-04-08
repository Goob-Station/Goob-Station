// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.TeleportOnStateChange;

/// <summary>
/// Teleports the entity to the given coordinates when changed to the specified state.
/// </summary>
/// <remarks>
/// Chooose **ONE** coordinate type to teleport to.
/// You don't need all of them.
/// </remarks>
[RegisterComponent]
public sealed partial class TeleportOnStateChangeComponent : Component
{
    /// <summary>
    /// The entity UID to teleport to.
    /// </summary>
    [DataField]
    public EntityUid? EntityTeleportTo;

    /// <summary>
    /// The co-ordinates to teleport to.
    /// </summary>
    [DataField]
    public EntityCoordinates? CoordinatesTeleportTo;

    /// <summary>
    /// The map co-ordinates to teleport to.
    /// </summary>
    [DataField]
    public MapCoordinates? MapCoordinatesTeleportTo;

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
