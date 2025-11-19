// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Xenoarchaeology.Artifact.XAT.Components;

/// <summary>
/// This is used for an artifact that activates when above or below a certain pressure.
/// </summary>
[RegisterComponent, Access(typeof(XATPressureSystem))]
public sealed partial class XATPressureComponent : Component
{
    /// <summary>
    /// The lower-end pressure threshold. Is not considered when null.
    /// </summary>
    [DataField]
    public float? MinPressureThreshold;

    /// <summary>
    /// The higher-end pressure threshold. Is not considered when null.
    /// </summary>
    [DataField]
    public float? MaxPressureThreshold;
}
