// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ThunderBear2006 <100388962+ThunderBear2006@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Anomaly.Components;

/// <summary>
/// This component is used for handling anomalies that affect the temperature
/// </summary>
[RegisterComponent]
public sealed partial class TempAffectingAnomalyComponent : Component
{

    /// <summary>
    /// The the amount the tempurature should be modified by (negative for decreasing temp)
    /// </summary>
    [DataField("tempChangePerSecond")]
    public float TempChangePerSecond = 0;

    /// <summary>
    /// The minimum amount of severity required
    /// before the anomaly becomes a hotspot.
    /// </summary>
    [DataField("anomalyHotSpotThreshold")]
    public float AnomalyHotSpotThreshold = 0.6f;

    /// <summary>
    /// The temperature of the hotspot where the anomaly is
    /// </summary>
    [DataField("hotspotExposeTemperature")]
    public float HotspotExposeTemperature = 0;

    /// <summary>
    /// The volume of the hotspot where the anomaly is.
    /// </summary>
    [DataField("hotspotExposeVolume")]
    public float HotspotExposeVolume = 50;
}