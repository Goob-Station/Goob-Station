// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._White.Blocking;

[RegisterComponent]
public sealed partial class RechargeableBlockingComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DischargedRechargeRate = 1.33f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ChargedRechargeRate = 2f;

    // Percentage of maxCharge to be able to activate item again.
    [DataField]
    public float RechargePercentage = 0.1f;

    [ViewVariables]
    public bool Discharged;
}
