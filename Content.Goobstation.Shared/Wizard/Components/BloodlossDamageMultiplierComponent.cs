// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Wizard.Components;

[RegisterComponent]
public sealed partial class BloodlossDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;
}