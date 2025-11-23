using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ChangeFactionNearbyEffect : EntityEffect
{
    [DataField(required: true)] public ProtoId<NpcFactionPrototype> NewFaction;

    [DataField] public float Duration = 0f;

    [DataField] public float Radius = 5f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var lookupSys = args.EntityManager.System<EntityLookupSystem>();
        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
        {
            var effect = new ChangeFactionEntityEffect();
            effect.NewFaction = NewFaction;
            effect.Duration = Duration;

            effect.Effect(new(entity, args.EntityManager));
        }
    }
}
