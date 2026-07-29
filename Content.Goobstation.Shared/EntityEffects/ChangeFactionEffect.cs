using Content.Goobstation.Shared.NPC;
using Content.Shared.EntityEffects;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class ChangeFaction : EntityEffectBase<ChangeFaction>
{
    [DataField(required: true)]
    public ProtoId<NpcFactionPrototype> NewFaction;

    [DataField]
    public TimeSpan? Duration;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-change-faction", ("faction", NewFaction));
}

public sealed partial class ChangeFactionEffectSystem : EntityEffectSystem<NpcFactionMemberComponent, ChangeFaction>
{
    [Dependency] private ChangeFactionStatusEffectSystem _changeFaction = default!;

    protected override void Effect(Entity<NpcFactionMemberComponent> ent, ref EntityEffectEvent<ChangeFaction> args)
    {
        _changeFaction.TryChangeFaction(ent, args.Effect.NewFaction, args.Effect.Duration);
    }
}


