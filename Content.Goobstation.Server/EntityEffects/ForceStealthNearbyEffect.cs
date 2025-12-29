using Content.Goobstation.Shared.Stealth;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ForceStealthNearbyEffect : EntityEffect
{
    [DataField] public float Radius = 5f;

    [DataField] public float Duration = 5f;

    [DataField] public float Chance = 1f;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-stealth-entities");

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var forceStealth = entityManager.System<ForcedStealthSystem>();
        var rand = IoCManager.Resolve<IRobustRandom>();

        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
            if (Chance >= 1f || rand.Prob(Chance))
                forceStealth.TryApplyForceStealth(entity, out _, Duration);
    }
}
