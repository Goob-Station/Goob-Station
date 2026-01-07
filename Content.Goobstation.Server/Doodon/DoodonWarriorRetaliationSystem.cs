using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Content.Shared.Damage;
using Content.Server.NPC.HTN;


namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonWarriorRetaliationSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly HTNSystem _htn = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DoodonWarriorComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(EntityUid uid, DoodonWarriorComponent comp, ref DamageChangedEvent args)
    {
        // Only react to taking damage (not healing)
        if (args.DamageDelta == null || args.DamageDelta.GetTotal() <= 0)
            return;

        // Need an attacker to retaliate against
        if (args.Origin is not EntityUid attacker || attacker == uid || Deleted(attacker))
            return;

        // Respect Stay: never retaliate
        if (comp.Orders == DoodonOrderType.Stay)
            return;

        // If papa is actively commanding an ordered attack, don’t override it (optional)
        if (comp.Orders == DoodonOrderType.AttackTarget)
            return;

        // Retaliation = "AttackTarget" using the ordered-target pipeline
        comp.Orders = DoodonOrderType.AttackTarget;
        Dirty(uid, comp);

        _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrders, DoodonOrderType.AttackTarget);
        _npc.SetBlackboard(uid, NPCBlackboard.CurrentOrderedTarget, attacker);

        if (!TryComp<HTNComponent>(uid, out var htn))
            return;

        if (htn.Plan != null)
            _htn.ShutdownPlan(htn);

        _htn.Replan(htn);
    }
}
