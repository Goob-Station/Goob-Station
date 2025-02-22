using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedVoidCurseSystem : EntitySystem
{
    protected virtual void Cycle(Entity<VoidCurseComponent> ent)
    {

    }

    public void DoCurse(EntityUid uid)
    {
        if (!HasComp<MobStateComponent>(uid))
            return; // ignore non mobs because holy shit

        if (TryComp<HereticComponent>(uid, out var h) && h.CurrentPath == "Void" || HasComp<GhoulComponent>(uid))
            return;

        if (TryComp<VoidCurseComponent>(uid, out var curse))
        {
            if (!curse.Drain)
            {
                // we keep adding curse time until we reach ~1 minute
                // when the time is reached it can't add any more time to the curse and just locks itself out until it's gone
                // which is very balanced :+1:
                curse.Lifetime = Math.Clamp(curse.Lifetime + 10f, 0f, curse.MaxLifetime);
                if (curse.Lifetime >= curse.MaxLifetime)
                    curse.Drain = true;
            }
            curse.Stacks = Math.Clamp(curse.Stacks + 1, 0, curse.MaxStacks + 1);
            Dirty(uid, curse);
        }
        else EnsureComp<VoidCurseComponent>(uid);
    }
}
