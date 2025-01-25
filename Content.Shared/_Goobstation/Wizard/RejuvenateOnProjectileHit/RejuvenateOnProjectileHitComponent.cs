using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.RejuvenateOnProjectileHit;

[RegisterComponent, NetworkedComponent]
public sealed partial class RejuvenateOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist UndeadList = new();

    [DataField]
    public DamageSpecifier UndeadDamage = new();
}
