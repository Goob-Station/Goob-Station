// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// This is used for recharging all nearby batteries when activated.
/// </summary>
[RegisterComponent, Access(typeof(XAEChargeBatterySystem))]
public sealed partial class XAEChargeBatteryComponent : Component
{
    /// <summary>
    /// The radius of entities that will be affected.
    /// </summary>
    [DataField("radius")]
    public float Radius = 15f;
}
