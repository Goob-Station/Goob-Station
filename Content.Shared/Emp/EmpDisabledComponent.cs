// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Emp;

/// <summary>
/// While entity has this component it is "disabled" by EMP.
/// Add desired behaviour in other systems.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedEmpSystem))]
public sealed partial class EmpDisabledComponent : Component
{
    /// <summary>
    /// Moment of time when the component is removed and entity stops being "disabled".
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan DisabledUntil = TimeSpan.Zero;

    /// <summary>
    /// Default time between visual effect spawns.
    /// This gets a random multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EffectCooldown = TimeSpan.FromSeconds(3);

    /// <summary>
    /// When next effect will be spawned.
    /// TODO: Particle system.
    /// </summary>
    [AutoPausedField]
    public TimeSpan TargetTime = TimeSpan.Zero;
}