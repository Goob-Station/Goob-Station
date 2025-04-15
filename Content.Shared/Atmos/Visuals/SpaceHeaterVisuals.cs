// SPDX-FileCopyrightText: 2024 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Visuals;

/// <summary>
///     Used for the visualizer
/// </summary>
[Serializable, NetSerializable]
public enum SpaceHeaterVisualLayers : byte
{
    Main
}

[Serializable, NetSerializable]
public enum SpaceHeaterVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum SpaceHeaterState : byte
{
    Off,
    StandBy,
    Heating,
    Cooling,
}