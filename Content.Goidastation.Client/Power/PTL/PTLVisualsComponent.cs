// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goidastation.Client.Power.PTL;

[RegisterComponent]
public sealed partial class PTLVisualsComponent : Component
{
    [DataField] public string ChargePrefix = "charge-";
    [DataField] public int MaxChargeStates = 6;
}
