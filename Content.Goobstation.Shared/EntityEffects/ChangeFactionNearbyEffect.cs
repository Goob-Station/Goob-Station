using Content.Goobstation.Shared.NPC;
using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;
using Content.Shared.NPC.Components;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class ChangeFactionNearbyEffectSystem : EntityEffectSystem<NpcFactionMemberComponent, ChangeFactionNearbyEffect>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ChangeFactionStatusEffectSystem _faction = default!;

    protected override void Effect(Entity<NpcFactionMemberComponent> entity, ref EntityEffectEvent<ChangeFactionNearbyEffect> args)
    {
        foreach (var target in _lookup.GetEntitiesInRange(entity.Owner, args.Effect.Radius))
        {
            if (!HasComp<NpcFactionMemberComponent>(target))
                continue;

            _faction.TryChangeFaction(target, args.Effect.NewFaction, out _, args.Effect.Duration);
        }
    }
}

public sealed partial class ChangeFactionNearbyEffect : EntityEffectBase<ChangeFactionNearbyEffect>
{
    [DataField(required: true)] public ProtoId<NpcFactionPrototype> NewFaction;

    [DataField] public float Duration = 0f;

    [DataField] public float Radius = 5f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
