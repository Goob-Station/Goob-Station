using Content.Shared.Damage;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class RejuvenateOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist UndeadList = new();

    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public bool ReverseEffects;

    [DataField]
    public ProtoId<TagPrototype> SoulTappedTag = "SoulTapped";
}
