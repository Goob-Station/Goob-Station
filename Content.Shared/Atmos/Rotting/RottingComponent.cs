// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Atmos.Rotting;

/// <summary>
/// Tracking component for stuff that has started to rot.
/// Only the current stage is networked to the client.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(SharedRottingSystem))]
public sealed partial class RottingComponent : Component
{
    /// <summary>
    /// Whether or not the rotting should deal damage
    /// </summary>
    [DataField]
    public bool DealDamage = true;

    /// <summary>
    /// When the next check will happen for rot progression + effects like damage and ammonia
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextRotUpdate = TimeSpan.Zero;

    /// <summary>
    /// How long in between each rot update.
    /// </summary>
    [DataField]
    public TimeSpan RotUpdateRate = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long has this thing been rotting?
    /// </summary>
    [DataField]
    public TimeSpan TotalRotTime = TimeSpan.Zero;

    /// <summary>
    /// The damage dealt by rotting.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 0.06 },
            { "Cellular", 0.06 }
        }
    };
}