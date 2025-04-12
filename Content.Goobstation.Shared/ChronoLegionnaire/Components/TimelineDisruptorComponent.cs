// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.ChronoLegionnaire.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.ChronoLegionnaire.Components;

/// <summary>
/// A component of a device that destroys a creature inside a stasis storage placed on that device.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTimelineDisruptorSystem))]
public sealed partial class TimelineDisruptorComponent : Component
{
    /// <summary>
    /// Determines if device working right now
    /// </summary>
    [DataField]
    public bool Disruption;

    /// <summary>
    /// ItemSlot that will be checked before disruption
    /// </summary>
    [DataField]
    public string DisruptionSlot = "disruptionSlot";

    /// <summary>
    /// Time where disruption will end
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan DisruptionEndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan NextSecond;

    /// <summary>
    /// Duration of disruption proccess
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DisruptionDuration = TimeSpan.FromSeconds(10);

    [DataField]
    public SoundSpecifier? DisruptionCompleteSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/ding.ogg");

    [DataField]
    public SoundSpecifier? DusruptionSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/ChronoLegionnaire/timeline_disruptor.ogg");

    [DataField]
    public EntityUid? DisruptionSoundStream;
}

[Serializable, NetSerializable]
public enum TimelineDisruptiorVisuals : byte
{
    Disrupting,
    ContainerInserted
}
