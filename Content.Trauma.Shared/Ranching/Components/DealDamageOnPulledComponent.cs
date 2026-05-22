// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Gibs the entity when its grabbed
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DealDamageOnPulledComponent : Component
{
    /// <summary>
    /// The damage to deal
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            ["Blunt"] = 30,
        }
    };
}
