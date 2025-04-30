// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Sound.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class EmitSoundOnCollideComponent : BaseEmitSoundComponent
{
    public static readonly TimeSpan CollideCooldown = TimeSpan.FromSeconds(0.2);

    /// <summary>
    /// Minimum velocity required for the sound to play.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("minVelocity")]
    public float MinimumVelocity = 3f;

    /// <summary>
    /// To avoid sound spam add a cooldown to it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("nextSound", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextSound;
}