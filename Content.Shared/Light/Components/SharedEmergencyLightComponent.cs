// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Light.Components;

/// <summary>
/// Handles station alert level and power changes for emergency lights.
/// All logic is serverside, animation is handled by <see cref="RotatingLightComponent"/>.
/// </summary>
[Access(typeof(SharedEmergencyLightSystem))]
public abstract partial class SharedEmergencyLightComponent : Component
{
}

[Serializable, NetSerializable]
public enum EmergencyLightVisuals
{
    On,
    Color
}

public enum EmergencyLightVisualLayers
{
    Base,
    LightOff,
    LightOn,
}