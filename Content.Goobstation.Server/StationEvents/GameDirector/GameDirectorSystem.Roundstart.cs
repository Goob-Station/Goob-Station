using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    private int EstimateRoundstartPlayers(int connected)
    {
        var ready = 0;
        foreach (var status in GameTicker.PlayerGameStatuses.Values)
        {
            if (status == PlayerGameStatus.ReadyToPlay)
                ready++;
        }

        return ready > 0 ? ready + _event.PlayerCountBias : connected;
    }

    private void PlanRoundstart(GameDirectorComponent scheduler, StoryPrototype story, int players)
    {
        scheduler.RoundstartPopulation = players;
        var scale = story.RoundstartThreat * scheduler.RoundstartThreatScale;
        if (scale <= 0 || story.Roundstart.Count == 0)
        {
            LogMessage($"No roundstart antags for {story.ID}");
            return;
        }

        var budget = MathF.Max(scheduler.ThreatPerPlayer * story.Threat * scale * players, scheduler.MinRoundstartThreat);
        var remaining = budget;
        var chosen = new List<EntProtoId>();
        var now = _timing.CurTime;

        for (var i = 0; i < story.Roundstart.Count; i++)
        {
            var slot = story.Roundstart[i];
            if (!_random.Prob(slot.Prob))
                continue;

            var slotBudget = slot.Share is { } share
                ? MathF.Min(budget * share, remaining)
                : remaining;

            var picks = new List<AntagPick>();
            foreach (var entry in _prototypeManager.Index(slot.Pool).Rules)
            {
                if (chosen.Contains(entry.Id)
                    || !_prototypeManager.TryIndex(entry.Id, out var proto)
                    || !CompatibleWith(entry.Id, chosen)
                    || proto.TryComp<GameRuleComponent>(out var gameRule, _factory) && gameRule.MinPlayers > players)
                    continue;

                if (EvaluateAntag(scheduler, proto, entry, slotBudget, int.MaxValue, false) is { } pick)
                    picks.Add(pick);
            }

            if (picks.Count == 0)
                continue;

            var picked = PickWeighted(picks, p => p.Score);
            StartPurchase(scheduler, picked, true, now);
            chosen.Add(picked.Proto);
            remaining -= picked.Count * picked.Threat;
            RoundstartAntagsSelectedTotal.WithLabels(picked.Proto).Set(picked.Count);
            LogMessage($"Roundstart antag chosen: {picked.Proto} x{picked.Count} " +
                       $"({picked.Count * picked.Threat:F1} of {budget:F1} threat for {players} players)");
        }
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent args)
    {
        var players = args.PlayerPool.Count + _event.PlayerCountBias;
        if (players <= 0)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var scheduler, out _))
        {
            if (scheduler.RoundstartPopulation <= 0)
                continue;

            var scale = (float) players / scheduler.RoundstartPopulation;
            scheduler.RoundstartPopulation = players;

            foreach (var purchase in scheduler.Purchases)
            {
                if (!purchase.Roundstart || !TryComp<AntagSelectionComponent>(purchase.Rule, out var selection))
                    continue;

                var count = Math.Clamp((int) MathF.Round(purchase.Count * scale), purchase.MinCount, purchase.MaxCount);
                if (count == purchase.Count)
                    continue;

                ApplyAntagCount((purchase.Rule, selection), count);
                RoundstartAntagsSelectedTotal.WithLabels(purchase.Proto).Set(count);
                LogMessage($"Roundstart antag {purchase.Proto} adjusted from x{purchase.Count} to x{count} for {players} ready players", false);
                purchase.Count = count;
            }
        }
    }
}
