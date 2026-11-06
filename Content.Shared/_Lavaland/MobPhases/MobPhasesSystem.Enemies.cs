using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.MobPhases;

public abstract partial class MobPhasesSystem
{
    [SubscribeLocalEvent]
    private void OnAggressorAdded(Entity<MobPhasesComponent> ent, ref AggressorAddedEvent args)
        => SyncEnemies(ent, ent.Comp.CurrentPhase, ent.Comp.CurrentPhase);

    [SubscribeLocalEvent]
    private void OnAggressorRemoved(Entity<MobPhasesComponent> ent, ref AggressorRemovedEvent args)
        => SyncEnemies(ent, ent.Comp.CurrentPhase, ent.Comp.CurrentPhase);

    [SubscribeLocalEvent]
    private void OnAggressiveState(Entity<AggressiveComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (TryComp<MobPhasesComponent>(ent, out var phases))
            SyncEnemies((ent, phases), phases.CurrentPhase, phases.CurrentPhase);
    }

    [SubscribeLocalEvent]
    private void OnAggressorStartup(Entity<AggressorComponent> ent, ref ComponentStartup args)
    {
        foreach (var boss in ent.Comp.Aggressives)
            if (TryComp<MobPhasesComponent>(boss, out var phases))
                SyncEnemies((boss, phases), phases.CurrentPhase, phases.CurrentPhase);
    }

    [SubscribeLocalEvent]
    private void OnKilled(Entity<MobPhasesComponent> ent, ref MegafaunaKilledEvent args)
        => ReleaseEnemies(ent);

    [SubscribeLocalEvent]
    private void OnPhasesShutdown(Entity<MobPhasesComponent> ent, ref ComponentShutdown args)
        => ReleaseEnemies(ent);

    private void SyncEnemies(Entity<MobPhasesComponent> ent, int oldPhase, int newPhase)
    {
        TryGetPhase(ent.Comp, oldPhase, out var old);
        TryGetPhase(ent.Comp, newPhase, out var next);
        TryComp<AggressiveComponent>(ent, out var aggressive);

        foreach (var (enemy, stash) in ent.Comp.Enemies)
        {
            if (aggressive?.Aggressors.Contains(enemy) == true)
            {
                if (oldPhase != newPhase)
                    SwapComponents(enemy, stash, old?.AddEnemyComp, old?.RemoveEnemyComp, next?.AddEnemyComp, next?.RemoveEnemyComp);

                continue;
            }

            SwapComponents(enemy, stash, old?.AddEnemyComp, old?.RemoveEnemyComp, null, null);
            ent.Comp.Enemies.Remove(enemy);
        }

        if (aggressive == null)
            return;

        foreach (var enemy in aggressive.Aggressors)
        {
            if (ent.Comp.Enemies.ContainsKey(enemy) || !Exists(enemy))
                continue;

            var stash = new ComponentRegistry();
            ent.Comp.Enemies[enemy] = stash;
            SwapComponents(enemy, stash, null, null, next?.AddEnemyComp, next?.RemoveEnemyComp);
        }
    }

    private void ReleaseEnemies(Entity<MobPhasesComponent> ent)
    {
        TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var phase);
        foreach (var (enemy, stash) in ent.Comp.Enemies)
            SwapComponents(enemy, stash, phase?.AddEnemyComp, phase?.RemoveEnemyComp, null, null);

        ent.Comp.Enemies.Clear();
    }
}
