using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component to call back to the cosmic cult ability system regarding a collision
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CosmicAstralNovaComponent : Component
{
    [DataField]
    public bool DoStun = true;

    [DataField]
    public DamageSpecifier CosmicNovaDamage = new()
    {
        DamageDict = new() {
            { "Asphyxiation", 13 }
        }
    };
}
