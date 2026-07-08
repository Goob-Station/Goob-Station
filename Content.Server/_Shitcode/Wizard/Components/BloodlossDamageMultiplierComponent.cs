// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class BloodlossDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;
}