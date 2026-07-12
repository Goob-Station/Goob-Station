// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Shared.GameTicking.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.GameTicking.Rules;

public abstract partial class GameRuleSystem<T> : EntitySystem where T : IComponent
{
    [Dependency] protected readonly IRobustRandom RobustRandom = default!;
    [Dependency] protected readonly IChatManager ChatManager = default!;
    [Dependency] protected readonly GameTicker GameTicker = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    // Not protected, just to be used in utility methods
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly MapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnStartAttempt);
        SubscribeLocalEvent<T, GameRuleAddedEvent>(OnGameRuleAdded);
        SubscribeLocalEvent<T, GameRuleStartedEvent>(OnGameRuleStarted);
        SubscribeLocalEvent<T, GameRuleEndedEvent>(OnGameRuleEnded);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnStartAttempt(RoundStartAttemptEvent args)
    {
        if (args.Forced || args.Cancelled)
            return;

        var playerCount = _playerManager.PlayerCount;
        var query = QueryAllRules();
        while (query.MoveNext(out var uid, out _, out var gameRule))
        {
            // Harmony: skip ended rules (e.g. from a failed start during fallback)
            if (HasComp<EndedGameRuleComponent>(uid))
            {
                Log.Debug($"Ended gamerule ignored from startup check: {ToPrettyString(uid)}");
                continue;
            }

            var minPlayers = gameRule.MinPlayers;
            var name = ToPrettyString(uid);

            if (playerCount >= minPlayers)
                continue;

            if (gameRule.CancelPresetOnTooFewPlayers)
            {
                ChatManager.SendAdminAnnouncement(Loc.GetString("preset-not-enough-ready-players",
                    ("readyPlayersCount", playerCount),
                    ("minimumPlayers", minPlayers),
                    ("presetName", name)));
                args.Cancel();
                //TODO remove this once announcements are logged
                Log.Info($"Rule '{name}' requires {minPlayers} players, but only {playerCount} are ready.");
            }
            else
            {
                ForceEndSelf(uid, gameRule);
            }
        }
    }

    private void OnGameRuleAdded(EntityUid uid, T component, ref GameRuleAddedEvent args)
    {
        if (!TryComp<GameRuleComponent>(uid, out var ruleData))
            return;
        Added(uid, component, ruleData, args);
    }

    private void OnGameRuleStarted(EntityUid uid, T component, ref GameRuleStartedEvent args)
    {
        if (!TryComp<GameRuleComponent>(uid, out var ruleData))
            return;
        Started(uid, component, ruleData, args);
    }

    private void OnGameRuleEnded(EntityUid uid, T component, ref GameRuleEndedEvent args)
    {
        if (!TryComp<GameRuleComponent>(uid, out var ruleData))
            return;
        Ended(uid, component, ruleData, args);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var query = AllEntityQuery<T>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!TryComp<GameRuleComponent>(uid, out var ruleData))
                continue;

            // Harmony: skip ended rules (e.g. from a failed start during fallback)
            if (HasComp<EndedGameRuleComponent>(uid))
            {
                Log.Debug($"Ended gamerule ignored from round end: {ToPrettyString(uid)}");
                continue;
            }

            AppendRoundEndText(uid, comp, ruleData, ref ev);
        }
    }

    /// <summary>
    /// Called when the gamerule is added
    /// </summary>
    protected virtual void Added(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {

    }

    /// <summary>
    /// Called when the gamerule begins
    /// </summary>
    protected virtual void Started(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {

    }

    /// <summary>
    /// Called when the gamerule ends
    /// </summary>
    protected virtual void Ended(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {

    }

    /// <summary>
    /// Called at the end of a round when text needs to be added for a game rule.
    /// </summary>
    protected virtual void AppendRoundEndText(EntityUid uid, T component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {

    }

    /// <summary>
    /// Called on an active gamerule entity in the Update function
    /// </summary>
    protected virtual void ActiveTick(EntityUid uid, T component, GameRuleComponent gameRule, float frameTime)
    {

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<T, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var comp1, out var comp2))
        {
            if (!GameTicker.IsGameRuleActive(uid, comp2))
                continue;

            ActiveTick(uid, comp1, comp2, frameTime);
        }
    }
}
