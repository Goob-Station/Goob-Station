// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Weapons.Ranged.Components;

public abstract partial class BatteryAmmoProviderComponent : AmmoProviderComponent
{
    /// <summary>
    /// How much battery it costs to fire once.
    /// </summary>
    [DataField("fireCost")] // Shitmed Change
    public float FireCost = 100;

    // Batteries aren't predicted which means we need to track the battery and manually count it ourselves woo!

    [ViewVariables(VVAccess.ReadWrite)]
    public int Shots;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Capacity;

    [DataField] public bool Examinable = true; // goob edit
}
