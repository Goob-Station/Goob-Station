using Content.Goobstation.Shared.Bloodsuckers.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class CoffinSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CoffinComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<CoffinComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(Entity<CoffinComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (HasComp<BloodsuckerComponent>(args.Entity))
            EnsureComp<InsideCoffinComponent>(args.Entity);
    }

    private void OnRemoved(Entity<CoffinComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemComp<InsideCoffinComponent>(args.Entity);
    }
}
