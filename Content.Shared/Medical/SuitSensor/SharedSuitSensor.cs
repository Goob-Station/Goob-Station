// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ahion <58528255+Ahion@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pspritechologist <81725545+Pspritechologist@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical.SuitSensor;

[Serializable, NetSerializable]
public sealed class SuitSensorStatus
{
    public SuitSensorStatus(NetEntity suitSensorUid, string name, string job, string jobIcon, List<string> jobDepartments)
    {
        SuitSensorUid = suitSensorUid;
        Name = name;
        Job = job;
        JobIcon = jobIcon;
        JobDepartments = jobDepartments;
    }

    public TimeSpan Timestamp;
    public NetEntity SuitSensorUid;
    public string Name;
    public string Job;
    public string JobIcon;
    public List<string> JobDepartments;
    public bool IsAlive;
    public int? TotalDamage;
    public int? TotalDamageThreshold;
    public float? DamagePercentage => TotalDamageThreshold == null || TotalDamage == null ? null : TotalDamage / (float) TotalDamageThreshold;
    public NetCoordinates? Coordinates;
}

[Serializable, NetSerializable]
public enum SuitSensorMode : byte
{
    /// <summary>
    /// Sensor doesn't send any information about owner
    /// </summary>
    SensorOff = 0,

    /// <summary>
    /// Sensor sends only binary status (alive/dead)
    /// </summary>
    SensorBinary = 1,

    /// <summary>
    /// Sensor sends health vitals status
    /// </summary>
    SensorVitals = 2,

    /// <summary>
    /// Sensor sends vitals status and GPS position
    /// </summary>
    SensorCords = 3
}

public static class SuitSensorConstants
{
    public const string NET_NAME = "name";
    public const string NET_JOB = "job";
    public const string NET_JOB_ICON = "jobIcon";
    public const string NET_JOB_DEPARTMENTS = "jobDepartments";
    public const string NET_IS_ALIVE = "alive";
    public const string NET_TOTAL_DAMAGE = "vitals";
    public const string NET_TOTAL_DAMAGE_THRESHOLD = "vitalsThreshold";
    public const string NET_COORDINATES = "coords";
    public const string NET_SUIT_SENSOR_UID = "uid";

    ///Used by the CrewMonitoringServerSystem to send the status of all connected suit sensors to each crew monitor
    public const string NET_STATUS_COLLECTION = "suit-status-collection";
}

[Serializable, NetSerializable]
public sealed partial class SuitSensorChangeDoAfterEvent : DoAfterEvent
{
    public SuitSensorMode Mode { get; private set; } = SuitSensorMode.SensorOff;

    public SuitSensorChangeDoAfterEvent(SuitSensorMode mode)
    {
        Mode = mode;
    }

    public override DoAfterEvent Clone() => this;
}