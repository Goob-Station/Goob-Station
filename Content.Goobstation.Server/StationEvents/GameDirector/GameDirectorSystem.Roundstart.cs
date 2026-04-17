using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Shared.StationEvents;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random;
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
        if (scheduler.NoRoundstartAntags)
            return;

        var playerCount = GetPlayerCount();
        LogMessage($"Total player count: {playerCount}", false);

        var weightList = _prototypeManager.Index(scheduler.RoundStartAntagsWeightTable);

        if (!scheduler.DualAntags)
            SpawnSingleAntag(weightList, playerCount);
        else
            SpawnDualAntags(weightList, playerCount);
    }
    /// <summary>
    /// Spawns a single antag game mode
    /// </summary>
    private void SpawnSingleAntag(WeightedRandomPrototype weightList, int playerCount)
    {
        var pick = weightList.Pick(_random);
        TryStartGameMode(pick, playerCount);
    }

    /// <summary>
    /// Spawns two compatible antag game modes
    /// </summary>
    private void SpawnDualAntags(WeightedRandomPrototype weightList, int playerCount)
    {
        var firstPick = weightList.Pick(_random);
        var availableWeights = GetCompatibleWeights(weightList.Weights, firstPick);

        // If no compatible modes, just spawn the first pick
        if (availableWeights.Count == 0)
        {
            TryStartGameMode(firstPick, playerCount);
            return;
        }

        var secondPick = _random.Pick(availableWeights);

        // Validate both picks have enough players
        if (!CanStartGameMode(firstPick, playerCount) || !CanStartGameMode(secondPick, playerCount))
        {
            LogMessage("Not enough players for roundstart antags selected...");
            return;
        }

        // Start both game modes
        LogMessage("Choosing roundstart antags");
        StartGameMode(firstPick);
        StartGameMode(secondPick);
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

        if (!proto.TryGetComponent<GameRuleComponent>(out var gameRule, _factory))
            return false;

        return gameRule.MinPlayers <= playerCount;
    }

    /// <summary>
    /// Tries to start a game mode if player count requirements are met
    /// </summary>
    private void TryStartGameMode(string protoId, int playerCount)
    {
        if (!CanStartGameMode(protoId, playerCount))
        {
            LogMessage("Not enough players for roundstart antags selected...");
            return;
        }

        StartGameMode(protoId);
    }

    /// <summary>
    /// Starts a game mode and logs the selection
    /// </summary>
    private void StartGameMode(string protoId)
    {
        LogMessage($"Roundstart antag chosen: {protoId}");
        RoundstartAntagsSelectedTotal.WithLabels(protoId).Inc();
        GameTicker.AddGameRule(protoId);
    }
}
