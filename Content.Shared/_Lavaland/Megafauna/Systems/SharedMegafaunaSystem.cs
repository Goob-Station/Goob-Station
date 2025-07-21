using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared.Mobs;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public abstract class SharedMegafaunaSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);
        SubscribeLocalEvent<MegafaunaAiComponent, MobStateChangedEvent>(OnStateChanged);
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

    private void OnStateChanged(Entity<MegafaunaAiComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.Active)
            return;

        RaiseLocalEvent(ent, new MegafaunaShutdownEvent()); // Proper cleanup
        RaiseLocalEvent(ent, new MegafaunaKilledEvent()); // Specific handling
        ent.Comp.Active = false;
    }
}
