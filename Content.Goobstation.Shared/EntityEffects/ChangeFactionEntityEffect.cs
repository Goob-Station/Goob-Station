using Content.Goobstation.Shared.NPC;
using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class ChangeFactionEntityEffectSystem : EntityEffectSystem<NpcFactionMemberComponent, ChangeFactionEntityEffect>
{
    [Dependency] private readonly ChangeFactionStatusEffectSystem _faction = default!;

    protected override void Effect(Entity<NpcFactionMemberComponent> entity, ref EntityEffectEvent<ChangeFactionEntityEffect> args)
    {
        _faction.TryChangeFaction(entity.Owner, args.Effect.NewFaction, out _, args.Effect.Duration);
    }
}

public sealed partial class ChangeFactionEntityEffect : EntityEffectBase<ChangeFactionEntityEffect>
{
    [DataField(required: true)] public ProtoId<NpcFactionPrototype> NewFaction;

    [DataField] public float Duration = 0f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-faction", ("faction", NewFaction));
}
