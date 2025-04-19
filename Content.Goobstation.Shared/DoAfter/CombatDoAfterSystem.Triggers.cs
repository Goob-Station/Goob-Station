using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.DoAfter;

public sealed partial class CombatDoAfterSystem
{
    private void InitializeTriggers()
    {
        SubscribeLocalEvent<CombatDoAfterComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<CombatDoAfterComponent> ent, ref MeleeHitEvent args)
    {
        if (!ent.Comp.MeleeHitTrigger || !args.IsHit)
            return;

        if (CheckDoAfter(ent, args.User))
        {
            var ev = ent.Comp.Trigger;
            ev.AffectedEntities = args.HitEntities;
            RaiseLocalEvent(ent, (object) ev);
        }

        if (ent.Comp is { DoAfterId: not null, DoAfterUser: not null })
        {
            _doAfter.Cancel(ent.Comp.DoAfterUser.Value, ent.Comp.DoAfterId.Value);
            ent.Comp.DoAfterId = null;
            ent.Comp.DoAfterUser = null;
            Dirty(ent);
        }
    }
}
