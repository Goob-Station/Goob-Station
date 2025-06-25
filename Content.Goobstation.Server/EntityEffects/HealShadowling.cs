using Content.Shared._EE.Shadowling;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared._Shitmed.Targeting;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._EE.EntityEffects;


/// <summary>
/// HealthChange but unique to Shadowlings and Thralls
/// </summary>
[UsedImplicitly]
public sealed partial class HealShadowling : EntityEffect
{
    /// <inheritdoc/>
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool IgnoreResistances = true;

    [DataField]
    public bool ScaleByQuantity;
    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-heal-sling", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // If slings get custom organs, I will remove all of this code tbf
        if (!args.EntityManager.HasComponent<ShadowlingComponent>(args.TargetEntity) &&
            !args.EntityManager.HasComponent<ThrallComponent>(args.TargetEntity))
        {
            return;
        }

        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;
        }

        args.EntityManager.System<DamageableSystem>()
            .TryChangeDamage(
                args.TargetEntity,
                Damage * scale,
                IgnoreResistances,
                interruptsDoAfters: false,
                targetPart: TargetBodyPart.All,
                partMultiplier: 0.5f,
                canSever: false);
    }
}
