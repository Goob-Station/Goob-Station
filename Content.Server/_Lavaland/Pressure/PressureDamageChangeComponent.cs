// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureDamageChangeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Enabled = true;

    [DataField]
    public float LowerBound = Atmospherics.OneAtmosphere * 0.2f;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public bool ApplyWhenInRange = true;

    [DataField]
    public float AppliedModifier = 2f; // Becomes 2 times better when in lavaland pressure environment

    [DataField]
    public bool ApplyToMelee = true;

    [DataField]
    public bool ApplyToProjectiles = true;
}
