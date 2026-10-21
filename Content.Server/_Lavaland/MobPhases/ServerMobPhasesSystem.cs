using Content.Server.Chat.Systems;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Chat;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.MobPhases;

public sealed partial class ServerMobPhasesSystem : MobPhasesSystem
{
    [Dependency] private ChatSystem _chat = default!;

    [SubscribeLocalEvent]
    private void OnPhaseChanged(Entity<MobPhasesComponent> ent, ref MobPhaseChangedEvent args)
    {
        TryGetPhase(ent.Comp, args.OldPhase, out var old);
        TryGetPhase(ent.Comp, args.NewPhase, out var next);

        if (args.NewPhase > args.OldPhase && next?.SpeechOnEnter is { } loc)
            _chat.TrySendInGameICMessage(ent, Loc.GetString(loc), InGameICChatType.Speak, false);

        var (oldAdd, oldRemove) = (old?.AddEnemyComp, old?.RemoveEnemyComp);
        var (nextAdd, nextRemove) = (next?.AddEnemyComp, next?.RemoveEnemyComp);

        TryComp<AggressiveComponent>(ent, out var aggressive);

        foreach (var (enemy, stash) in ent.Comp.Enemies)
        {
            if (aggressive?.Aggressors.Contains(enemy) == true)
            {
                SwapComponents(enemy, stash, oldAdd, oldRemove, nextAdd, nextRemove);
                continue;
            }

            SwapComponents(enemy, stash, oldAdd, oldRemove, null, null);
            ent.Comp.Enemies.Remove(enemy);
        }

        if (aggressive == null)
            return;

        foreach (var enemy in aggressive.Aggressors)
        {
            if (ent.Comp.Enemies.ContainsKey(enemy))
                continue;

            var stash = new ComponentRegistry();
            ent.Comp.Enemies[enemy] = stash;
            SwapComponents(enemy, stash, null, null, nextAdd, nextRemove);
        }
    }

    [SubscribeLocalEvent]
    private void OnAggressorAdded(Entity<MobPhasesComponent> ent, ref AggressorAddedEvent args)
    {
        if (ent.Comp.Enemies.ContainsKey(args.Aggressor))
            return;


        var stash = new ComponentRegistry();
        ent.Comp.Enemies[args.Aggressor] = stash;

        TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var phase);
        SwapComponents(args.Aggressor, stash, null, null, phase?.AddEnemyComp, phase?.RemoveEnemyComp);
    }

    [SubscribeLocalEvent]
    private void OnAggressorRemoved(Entity<MobPhasesComponent> ent, ref AggressorRemovedEvent args)
    {
        if (!ent.Comp.Enemies.Remove(args.Aggressor, out var stash))
            return;

        TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var phase);
        SwapComponents(args.Aggressor, stash, phase?.AddEnemyComp, phase?.RemoveEnemyComp, null, null);
    }

    [SubscribeLocalEvent]
    private void OnKilled(Entity<MobPhasesComponent> ent, ref MegafaunaKilledEvent args)
        => ReleaseEnemies(ent);

    [SubscribeLocalEvent]
    private void OnPhasesShutdown(Entity<MobPhasesComponent> ent, ref ComponentShutdown args)
        => ReleaseEnemies(ent);

    private void ReleaseEnemies(Entity<MobPhasesComponent> ent)
    {
        TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var phase);
        foreach (var (enemy, stash) in ent.Comp.Enemies)
            SwapComponents(enemy, stash, phase?.AddEnemyComp, phase?.RemoveEnemyComp, null, null);

        ent.Comp.Enemies.Clear();
    }
}
