using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Mobs;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    private void InitializeHandle()
    {
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);
        SubscribeLocalEvent<MegafaunaAiComponent, MobStateChangedEvent>(OnStateChanged);
    }

    private void OnMegafaunaStartup(Entity<MegafaunaAiComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (_net.IsClient)
            return;

        var nextAction = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.StartingDelay);
        ent.Comp.Schedule.Add(nextAction, ent.Comp.Selector);
    }

    private void OnMegafaunaShutdown(Entity<MegafaunaAiComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (_net.IsClient)
            return;

        ent.Comp.Schedule.Clear();
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
        if (!ent.Comp.Active
            || !_mobState.IsDead(ent))
            return;

        KillMegafauna(ent);
    }
}
