using Content.Goobstation.Server.NPC;
using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ChangeFactionNearbyEffect : EntityEffect
{
    [DataField(required: true)] public ProtoId<NpcFactionPrototype> NewFaction;

    [DataField] public float Duration = 30f;

    [DataField] public float Radius = 5f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-faction");

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var cf = entityManager.System<ChangeFactionStatusEffectSystem>();
        var rand = IoCManager.Resolve<IRobustRandom>();

        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
            cf.ChangeFaction(entity, NewFaction, out _, Duration);
    }
}
