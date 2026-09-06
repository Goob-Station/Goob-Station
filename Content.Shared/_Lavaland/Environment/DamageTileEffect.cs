using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Environment;

/// <summary>
/// Damages someone when stepping on a new tile that has this effect. Does not constantly apply, so if the person stays still it will be fine.
/// </summary>

public sealed partial class DamageTileEffectSystem : EntityEffectSystem<DamageableComponent, DamageTileEffect>
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    protected override void Effect(Entity<DamageableComponent> ent, ref EntityEffectEvent<DamageTileEffect> args)
    {
        _damageable.TryChangeDamage(ent.AsNullable(), args.Effect.Damage, true);
    }
}

public sealed partial class DamageTileEffect : EntityEffectBase<DamageTileEffect>
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototypeManager, IEntitySystemManager systemManager)
    {
        return null;
    }
}
