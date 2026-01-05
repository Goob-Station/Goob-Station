// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Content.Shared._NF.Shuttles.Events;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class NavInterfaceState
{
    public float MaxRange;

    /// <summary>
    /// The relevant coordinates to base the radar around.
    /// </summary>
    public NetCoordinates? Coordinates;

    /// <summary>
    /// The relevant rotation to rotate the angle around.
    /// </summary>
    public Angle? Angle;

    public Dictionary<NetEntity, List<DockingPortState>> Docks;

    public bool RotateWithEntity = true;

    // Frontier fields

    /// <summary>
    /// Custom display names for network port buttons.
    /// Key is the port ID, value is the display name.
    /// </summary>
    public Dictionary<string, string> NetworkPortNames;

    /// <summary>
    /// Frontier - the state of the shuttle's inertial dampeners
    /// </summary>
    public InertiaDampeningMode DampeningMode;

    /// <summary>
    /// Frontier: settable maximum IFF range
    /// </summary>
    public float? MaxIffRange = null;

    /// <summary>
    /// Frontier: settable coordinate visibility
    /// </summary>
    public bool HideCoords = false;

    // End Frontier fields
    public NavInterfaceState(
        float maxRange,
        NetCoordinates? coordinates,
        Angle? angle,
        Dictionary<NetEntity, List<DockingPortState>> docks,
        InertiaDampeningMode dampeningMode, // Frontier
        Dictionary<string, string>? networkPortNames = null)
    {
        MaxRange = maxRange;
        Coordinates = coordinates;
        Angle = angle;
        Docks = docks;
        DampeningMode = dampeningMode; // Frontier
        NetworkPortNames = networkPortNames ?? new Dictionary<string, string>();
    }
}

[Serializable, NetSerializable]
public enum RadarConsoleUiKey : byte
{
    Key
}
