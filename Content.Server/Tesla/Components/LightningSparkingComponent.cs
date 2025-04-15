// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Tesla.EntitySystems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Tesla.Components;

/// <summary>
/// The component changes the visual of an object after it is struck by lightning
/// </summary>
[RegisterComponent, Access(typeof(LightningSparkingSystem)), AutoGenerateComponentPause]
public sealed partial class LightningSparkingComponent : Component
{
    /// <summary>
    /// Spark duration.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LightningTime = 4;

    /// <summary>
    /// When the spark visual should turn off.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan LightningEndTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool IsSparking;
}