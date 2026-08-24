// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Weapons.ChangeTemperatureOnHit;

[RegisterComponent]
public sealed partial class ChangeTemperatureOnHitComponent : Component
{
    [DataField]
    public float Heat;

    [DataField]
    public bool IgnoreResistances = true;
}