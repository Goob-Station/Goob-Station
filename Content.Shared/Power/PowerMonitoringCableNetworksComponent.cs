// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Power;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedPowerMonitoringConsoleSystem))]
public sealed partial class PowerMonitoringCableNetworksComponent : Component
{
    /// <summary>
    /// A dictionary of the all the nav map chunks that contain anchored power cables
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Vector2i, PowerCableChunk> AllChunks = new();

    /// <summary>
    /// A dictionary of the all the nav map chunks that contain anchored power cables
    /// that are directly connected to the console's current focus
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<Vector2i, PowerCableChunk> FocusChunks = new();
}

[Serializable, NetSerializable]
public struct PowerCableChunk
{
    public readonly Vector2i Origin;

    /// <summary>
    /// Bitmask dictionary for power cables, 1 for occupied and 0 for empty.
    /// </summary>
    public int[] PowerCableData;

    public PowerCableChunk(Vector2i origin)
    {
        Origin = origin;
        PowerCableData = new int[3];
    }
}