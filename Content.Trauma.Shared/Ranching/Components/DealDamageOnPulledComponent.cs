using Content.Shared.Damage;
using Robust.Shared.GameStates;

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
