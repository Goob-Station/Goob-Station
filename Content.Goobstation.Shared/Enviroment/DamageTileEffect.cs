using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.Tiles;

[DataDefinition]
public sealed partial class DamageTileEffect : EntityEffect
{
    /// <summary>
    /// Damages someone when stepping on a new tile that has this effect. Does not constantly apply, so if the person stays still it will be fine.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out DamageableComponent? damageable))
            return;

        args.EntityManager.System<DamageableSystem>()
            .TryChangeDamage(args.TargetEntity, Damage, true);
    }
    protected override string? ReagentEffectGuidebookText(
        IPrototypeManager prototypeManager,
        IEntitySystemManager systemManager)
    {
        // Not a reagent effect; do not appear in the guidebook
        return null;
    }
}
