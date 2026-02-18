using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.Mimery;

[RegisterComponent]
public sealed partial class EntityEffectOnProjectileHitComponent : Component
{
    [DataField]
    public List<EntityEffect> Effects = new();
}
