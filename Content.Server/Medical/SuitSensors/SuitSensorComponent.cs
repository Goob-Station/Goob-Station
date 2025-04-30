// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Medical.SuitSensor;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Medical.SuitSensors;

/// <summary>
///     Tracking device, embedded in almost all uniforms and jumpsuits.
///     If enabled, will report to crew monitoring console owners position and status.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(SuitSensorSystem))]
public sealed partial class SuitSensorComponent : Component
{
    /// <summary>
    ///     Choose a random sensor mode when item is spawned.
    /// </summary>
    [DataField]
    public bool RandomMode = true;

    /// <summary>
    ///     If true user can't change suit sensor mode
    /// </summary>
    [DataField]
    public bool ControlsLocked = false;

    /// <summary>
    ///  How much time it takes to change another player's sensors
    /// </summary>
    [DataField]
    public float SensorsTime = 1.75f;

    /// <summary>
    ///     Current sensor mode. Can be switched by user verbs.
    /// </summary>
    [DataField]
    public SuitSensorMode Mode = SuitSensorMode.SensorOff;

    /// <summary>
    ///     Activate sensor if user wear it in this slot.
    /// </summary>
    [DataField]
    public string ActivationSlot = "jumpsuit";

    /// <summary>
    /// Activate sensor if user has this in a sensor-compatible container.
    /// </summary>
    [DataField]
    public string? ActivationContainer;

    /// <summary>
    ///     How often does sensor update its owners status (in seconds). Limited by the system update rate.
    /// </summary>
    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(2f);

    /// <summary>
    ///     Current user that wears suit sensor. Null if nobody wearing it.
    /// </summary>
    [ViewVariables]
    public EntityUid? User = null;

    /// <summary>
    ///     Next time when sensor updated owners status
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    ///     The station this suit sensor belongs to. If it's null the suit didn't spawn on a station and the sensor doesn't work.
    /// </summary>
    [DataField("station")]
    public EntityUid? StationId = null;

    /// <summary>
    ///     The server the suit sensor sends it state to.
    ///     The suit sensor will try connecting to a new server when no server is connected.
    ///     It does this by calling the servers entity system for performance reasons.
    /// </summary>
    [DataField("server")]
    public string? ConnectedServer = null;

    /// <summary>
    /// The previous mode of the suit. This is used to restore the state when an EMP effect ends.
    /// </summary>
    [DataField, ViewVariables]
    public SuitSensorMode PreviousMode = SuitSensorMode.SensorOff;

    /// <summary>
    ///  The previous locked status of the controls.  This is used to restore the state when an EMP effect ends.
    ///  This keeps prisoner jumpsuits/internal implants from becoming unlocked after an EMP.
    /// </summary>
    [DataField, ViewVariables]
    public bool PreviousControlsLocked = false;
}