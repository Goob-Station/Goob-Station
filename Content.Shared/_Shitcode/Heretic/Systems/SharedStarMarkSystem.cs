using Content.Shared._Shitcode.Heretic.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarMarkSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicFieldComponent, PreventCollideEvent>(OnPreventColliede);
    }

    private void OnPreventColliede(Entity<CosmicFieldComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<StarMarkComponent>(args.OtherEntity))
            args.Cancelled = true;
    }
}
