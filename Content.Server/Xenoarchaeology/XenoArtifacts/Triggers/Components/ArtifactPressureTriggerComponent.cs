// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
/// Triggers when a certain pressure threshold is hit
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactPressureTriggerComponent : Component
{
    /// <summary>
    /// The lower-end pressure threshold
    /// </summary>
    [DataField("minPressureThreshold")]
    public float? MinPressureThreshold;

    /// <summary>
    /// The higher-end pressure threshold
    /// </summary>
    [DataField("maxPressureThreshold")]
    public float? MaxPressureThreshold;
}