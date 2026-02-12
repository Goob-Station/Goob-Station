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

    public void TryChangeFaction(EntityUid uid, ProtoId<NpcFactionPrototype> newFaction, out EntityUid? statusEffect, float durationInSeconds)
    {
        statusEffect = default;
        if (!TryComp<NpcFactionMemberComponent>(uid, out var npc))
            return;

        if (durationInSeconds <= 0)
        {
            SwapFactions((uid, npc), newFaction);
            return;
        }

        _status.TryAddStatusEffect(uid, ChangeFactionStatusEffect, out statusEffect, TimeSpan.FromSeconds(durationInSeconds));
        if (statusEffect.HasValue && TryComp<ChangeFactionStatusEffectComponent>(statusEffect, out var f))
        {
            f.NewFaction = newFaction;
            var args = new StatusEffectAppliedEvent(uid);
            OnStatusApplied((statusEffect.Value, f), ref args); // mango code
        }
    }

    private void OnStatusApplied(Entity<ChangeFactionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!ent.Comp.NewFaction.HasValue)
            return;

        var npc = EnsureComp<NpcFactionMemberComponent>(args.Target);
        ent.Comp.OldFactions = npc.Factions;
        SwapFactions((args.Target, npc), ent.Comp.NewFaction);
    }

    private void OnStatusRemoved(Entity<ChangeFactionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var npc = EnsureComp<NpcFactionMemberComponent>(args.Target);
        SwapFactions((args.Target, npc), ent.Comp.OldFactions);
    }

    private void SwapFactions(Entity<NpcFactionMemberComponent> ent, string faction)
    {
        _npc.ClearFactions((ent, ent.Comp));
        _npc.AddFaction((ent, ent.Comp), faction);
    }

    private void SwapFactions(Entity<NpcFactionMemberComponent> ent, HashSet<ProtoId<NpcFactionPrototype>> factions)
    {
        _npc.ClearFactions((ent, ent.Comp));
        _npc.AddFactions((ent, ent.Comp), factions);
    }
}
