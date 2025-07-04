// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Climbing.Components;

/// <summary>
/// Indicates that this entity is able to be placed on top of surfaces like tables.
/// Does not by itself allow the entity to carry out the action of climbing, unless
/// <see cref="CanClimb"/> is true. Use <see cref="CanForceClimb"/> to control whether
/// the entity can force other entities onto surfaces.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ClimbingComponent : Component
{
    /// <summary>
    /// Whether the owner is able to climb onto things by their own action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanClimb = true;

    /// <summary>
    /// Whether the owner is climbing on a climbable entity.
    /// </summary>
    [AutoNetworkedField, DataField]
    public bool IsClimbing;

    /// <summary>
    /// Whether the owner is being moved onto the climbed entity.
    /// </summary>
    [AutoNetworkedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan? NextTransition;

    /// <summary>
    /// Direction to move when transition.
    /// </summary>
    [AutoNetworkedField, DataField]
    public Vector2 Direction;

    /// <summary>
    /// How fast the entity is moved when climbing.
    /// </summary>
    [DataField]
    public float TransitionRate = 5f;

    [AutoNetworkedField, DataField]
    public Dictionary<string, int> DisabledFixtureMasks = new();
}