using Content.Shared._Lavaland.Aggression;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna;

public abstract class SharedMegafaunaSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);
    }

    private void OnAggressorAdded(Entity<MegafaunaAiComponent> ent, ref AggressorAddedEvent args)
    {
        if (ent.Comp.Active)
            return;

        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
        ent.Comp.Active = true;
    }

    private void OnAggressorRemoved(Entity<MegafaunaAiComponent> ent, ref AggressorRemovedEvent args)
    {
        if (!ent.Comp.Active
            || !TryComp<AggressiveComponent>(ent, out var aggressive)
            || aggressive.Aggressors.Count != 0)
            return;

        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());
        ent.Comp.Active = false;
    }
}
