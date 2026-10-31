using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Shared.StationEvents;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    /// <summary>
    /// Tries to spawn roundstart antags at the beginning of the round.
    /// </summary>
    private void TrySpawnRoundstartAntags(GameDirectorComponent scheduler)
    {
        scheduler.RoundstartAntags.Clear();
        if (scheduler.NoRoundstartAntags)
            return;

        var playerCount = GetPlayerCount();
        LogMessage($"Total player count: {playerCount}", false);

        var weightList = _prototypeManager.Index(scheduler.RoundStartAntagsWeightTable);
        var firstPick = weightList.Pick(_random);
        TryStartGameMode(scheduler, firstPick, playerCount);

        if (!scheduler.DualAntags)
            return;

        var availableWeights = GetCompatibleWeights(weightList.Weights, firstPick);
        if (availableWeights.Count > 0)
            TryStartGameMode(scheduler, _random.Pick(availableWeights), playerCount);
    }

    /// <summary>
    /// Gets weights filtered by compatibility with the given mode
    /// </summary>
    private Dictionary<string, float> GetCompatibleWeights(Dictionary<string, float> weights, string mode)
    {
        if (!_prototypeManager.TryIndex(mode, out IncompatibleGameModesPrototype? incompatible))
            return weights;

        return weights
            .Where(w => !incompatible.Modes.Contains(w.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Checks if a game mode can start with the given player count
    /// </summary>
    private bool CanStartGameMode(string protoId, int playerCount)
    {
        var proto = _prototypeManager.Index<EntityPrototype>(protoId);

        if (!proto.TryComp<GameRuleComponent>(out var gameRule, _factory))
            return false;

        return gameRule.MinPlayers <= playerCount;
    }

    /// <summary>
    /// Tries to start a game mode if player count requirements are met
    /// </summary>
    private void TryStartGameMode(GameDirectorComponent scheduler, string protoId, int playerCount)
    {
        if (!CanStartGameMode(protoId, playerCount))
        {
            LogMessage($"Not enough players for roundstart antag {protoId}");
            return;
        }

        LogMessage($"Roundstart antag chosen: {protoId}");
        RoundstartAntagsSelectedTotal.WithLabels(protoId).Inc();
        scheduler.RoundstartAntags.Add(protoId);
        GameTicker.AddGameRule(protoId);
    }
}
