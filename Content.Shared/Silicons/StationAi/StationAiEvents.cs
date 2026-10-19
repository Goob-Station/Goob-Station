// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Medical.SuitSensor;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Instant action event to toggle whether the AI eye appears on crew monitoring.
/// </summary>
public sealed partial class ToggleAiEyeVisibilityEvent : InstantActionEvent
{
}

/// <summary>
/// Client requests immediate crew monitoring sensor data from the server.
/// </summary>
[Serializable, NetSerializable]
public sealed class StationAiRequestCrewMonitoringSensorsEvent : EntityEventArgs
{
}

/// <summary>
/// Server sends crew monitoring sensor data to the AI client.
/// </summary>
[Serializable, NetSerializable]
public sealed class StationAiCrewMonitoringSensorsEvent : EntityEventArgs
{
    public readonly List<SuitSensorStatus> Sensors;

    public StationAiCrewMonitoringSensorsEvent(List<SuitSensorStatus> sensors)
    {
        Sensors = sensors;
    }
}

/// <summary>
/// Sent from client to server to teleport the AI eye to a target (chat speaker or crew monitoring member).
/// </summary>
[Serializable, NetSerializable]
public sealed class StationAiTeleportToTargetEvent : EntityEventArgs
{
    public readonly NetEntity Target;

    public StationAiTeleportToTargetEvent(NetEntity target)
    {
        Target = target;
    }
}
