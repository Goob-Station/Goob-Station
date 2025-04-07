// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 to4no_fix <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server.Explosion.Components;

/// <summary>
/// A component that electrocutes an entity having this component when a trigger is triggered.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(TriggerSystem))]
public sealed partial class ShockOnTriggerComponent : Component
{
    /// <summary>
    /// The force of an electric shock when the trigger is triggered.
    /// </summary>
    [DataField]
    public int Damage = 5;

    /// <summary>
    /// Duration of electric shock when the trigger is triggered.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The minimum delay between repeating triggers.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    /// When can the trigger run again?
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextTrigger = TimeSpan.Zero;
}