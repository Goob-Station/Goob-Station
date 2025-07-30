using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Actions;
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

        StartupMegafauna(ent);
    }

    private void OnAggressorRemoved(Entity<MegafaunaAiComponent> ent, ref AggressorRemovedEvent args)
    {
        if (!ent.Comp.Active
            || !TryComp<AggressiveComponent>(ent, out var aggressive)
            || aggressive.Aggressors.Count != 0)
            return;

        ShutdownMegafauna(ent);
    }

    private void OnStateChanged(Entity<MegafaunaAiComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.Active)
            return;

        ShutdownMegafauna(ent, true);
    }

    public void StartupMegafauna(Entity<MegafaunaAiComponent> ent)
    {
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
        ent.Comp.Active = true;
    }

    public void ShutdownMegafauna(Entity<MegafaunaAiComponent> ent, bool kill = false)
    {
        RaiseLocalEvent(ent, new MegafaunaShutdownEvent());

        if (kill)
            RaiseLocalEvent(ent, new MegafaunaKilledEvent());

        ent.Comp.Active = false;
    }

    /// <summary>
    /// Adds new megafauna action to MegafaunaAI's thread, so it will fire at the specified time.
    /// This will throw an error if thread with specified index doesn't exist.
    /// </summary>
    public void AddActionToThread(MegafaunaAiComponent comp, int index, MegafaunaActionSelector selector, float delay)
    {
        var time = Timing.CurTime + TimeSpan.FromSeconds(delay);
        comp.Threads[index].Actions.Add(time, selector);
    }

    /// <summary>Adds new thread to the megafauna.</summary>
    /// <returns>New thread's index.</returns>
    public int AddNewThread(MegafaunaAiComponent comp)
    {
        var thread = new MegafaunaActionThread(new Dictionary<TimeSpan, MegafaunaActionSelector>(), false);
        comp.Threads.Add(thread);
        return comp.Threads.Count - 1;
    }

    public void RemoveThread(MegafaunaAiComponent comp, int index)
    {
        comp.Threads.RemoveAt(index);
    }
}
