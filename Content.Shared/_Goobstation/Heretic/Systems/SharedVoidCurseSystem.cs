using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Goobstation.Heretic.Systems;

[Virtual]
public partial class SharedVoidCurseSystem : EntitySystem
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
            curse.Lifetime += 10f;
            curse.Stacks = Math.Clamp(curse.Stacks + 1, 0, curse.MaxStacks + 1);
            Dirty<VoidCurseComponent>((uid, curse));
        }
        else EnsureComp<VoidCurseComponent>(uid);
    }
}
