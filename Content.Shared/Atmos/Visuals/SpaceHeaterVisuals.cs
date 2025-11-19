// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
