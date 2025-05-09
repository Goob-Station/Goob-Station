// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.SensorMonitoring;
using Robust.Shared.Collections;

namespace Content.Server.SensorMonitoring;

[RegisterComponent]
public sealed partial class SensorMonitoringConsoleComponent : Component
{
    /// <summary>
    /// Used to assign network IDs for sensors and sensor streams.
    /// </summary>
    public int IdCounter;

    /// <summary>
    /// If enabled, additional data streams are shown intended to only be visible for debugging.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("debugStreams")]
    public bool DebugStreams = false;

    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<EntityUid, SensorData> Sensors = new();

    [DataField("retentionTime")]
    public TimeSpan RetentionTime = TimeSpan.FromMinutes(1);

    // UI update tracking stuff.
    public HashSet<EntityUid> InitialUIStateSent = new();
    public TimeSpan LastUIUpdate;
    public ValueList<int> RemovedSensors;

    public sealed class SensorData
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public int NetId;

        [ViewVariables(VVAccess.ReadWrite)]
        public SensorDeviceType DeviceType;

        [ViewVariables(VVAccess.ReadWrite)]
        public Dictionary<string, SensorStream> Streams = new();
    }

    public sealed class SensorStream
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public int NetId;

        [ViewVariables(VVAccess.ReadWrite)]
        public SensorUnit Unit;

        // Queue<T> is a ring buffer internally, and we can still iterate over it.
        // I don't wanna write a ring buffer myself, so this is pretty convenient!
        [ViewVariables]
        public Queue<SensorSample> Samples = new();
    }

    public sealed class ViewingPlayer
    {

    }
}
