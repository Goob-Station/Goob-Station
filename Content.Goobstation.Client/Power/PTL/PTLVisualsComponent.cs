// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.Power.PTL;

[RegisterComponent]
public sealed partial class PTLVisualsComponent : Component
{
    [DataField] public string ChargePrefix = "charge-";
    [DataField] public int MaxChargeStates = 6;
}
