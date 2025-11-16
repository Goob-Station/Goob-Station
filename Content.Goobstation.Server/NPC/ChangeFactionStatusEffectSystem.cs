using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;

namespace Content.Goobstation.Server.NPC;
public sealed partial class ChangeFactionStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly NpcFactionSystem _npc = default!;

    public static readonly EntProtoId ChangeFactionStatusEffect = "ChangeFactionStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeFactionStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<ChangeFactionStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
    }

    public void ChangeFaction(EntityUid uid, ProtoId<NpcFactionPrototype> newFaction, out EntityUid? statusEffect, float durationInSeconds)
    {
        _status.TryAddStatusEffect(uid, ChangeFactionStatusEffect, out statusEffect, TimeSpan.FromSeconds(durationInSeconds));
        if (statusEffect != null && TryComp<ChangeFactionStatusEffectComponent>(statusEffect, out var f))
            f.NewFaction = newFaction;
    }

    private void OnStatusApplied(Entity<ChangeFactionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var npc = EnsureComp<NpcFactionMemberComponent>(args.Target);
        ent.Comp.OldFactions = npc.Factions;
        _npc.ClearFactions((args.Target, npc));
        _npc.AddFaction((args.Target, npc), ent.Comp.NewFaction);
    }

    private void OnStatusRemoved(Entity<ChangeFactionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var npc = EnsureComp<NpcFactionMemberComponent>(args.Target);
        _npc.ClearFactions((args.Target, npc));
        _npc.AddFactions((args.Target, npc), ent.Comp.OldFactions);
    }
}
