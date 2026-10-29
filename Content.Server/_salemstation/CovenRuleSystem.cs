using Content.Server.CovenRule;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.Chat;
using Content.Shared.CovenMember;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Content.Server.CovenInGameRules;

public sealed class CovenRuleSystem : GameRuleSystem<CovenRuleComponent>
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly Content.Server.Chat.Systems.ChatSystem _chatSystem = default!;
    [Dependency] private readonly Content.Server.Chat.Managers.IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    protected override void Started(EntityUid uid, CovenRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        component.ActiveAntags.Clear();
        component.WeWon = false;

        var sessions = _playerManager.Sessions
            .Where(session => session.AttachedEntity is not null)
            .ToList();

        if (sessions.Count == 0)
            return;

        var random = new Random();
        var playerPool = sessions
            .OrderBy(_ => random.Next())
            .ToList();

        foreach (var roleConfig in component.Roles)
        {
            var targetCount = Math.Min(roleConfig.Max, playerPool.Count);
            if (targetCount < roleConfig.Min && playerPool.Count > 0)
                targetCount = Math.Min(roleConfig.Min, playerPool.Count);

            for (var i = 0; i < targetCount; i++)
            {
                var session = playerPool[0];
                playerPool.RemoveAt(0);

                if (session.AttachedEntity is not { } mob)
                    continue;

                component.ActiveAntags.Add(mob);
                EnsureComp<CovenMemberComponent>(mob);
            }
        }

        SendCovenBriefing(component);
    }

    protected override void Ended(EntityUid uid, CovenRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        component.WeWon = false;
        component.ActiveAntags.Clear();
    }

    protected override void AppendRoundEndText(EntityUid uid, CovenRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        if (component.WeWon)
            args.AddLine("The Coven wins!");

        var covenMembers = component.ActiveAntags
            .Where(member => HasComp<CovenMemberComponent>(member))
            .Distinct()
            .ToList();

        if (covenMembers.Count == 0)
        {
            args.AddLine("Coven members: none");
            return;
        }

        args.AddLine("Coven members:");
        foreach (var member in covenMembers)
        {
            var sessionName = string.Empty;
            if (TryComp<ActorComponent>(member, out var actor) && actor.PlayerSession != null)
                sessionName = actor.PlayerSession.Name;

            var entityName = MetaData(member).EntityName;
            args.AddLine($"- {entityName} ({sessionName})");
        }
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        var query = EntityQueryEnumerator<CovenRuleComponent>();
        while (query.MoveNext(out var uid, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid))
                continue;

            CheckWinCondition(uid, rule);
        }
    }

    private void CheckWinCondition(EntityUid ruleUid, CovenRuleComponent rule)
    {
        if (rule.WeWon)
            return;

        var livingNonCovenPlayerFound = false;
        var mindQuery = EntityQueryEnumerator<MindComponent>();
        while (mindQuery.MoveNext(out var mindUid, out var mind))
        {
            if (mind.OwnedEntity == null)
                continue;

            var playerMob = mind.OwnedEntity.Value;
            if (_mobState.IsDead(playerMob))
                continue;

            if (HasComp<CovenMemberComponent>(playerMob))
                continue;

            livingNonCovenPlayerFound = true;
            break;
        }

        if (!livingNonCovenPlayerFound)
            TriggerFactionVictory(ruleUid, rule);
    }

    private void TriggerFactionVictory(EntityUid ruleUid, CovenRuleComponent rule)
    {
        rule.WeWon = true;

        _chatSystem.DispatchStationAnnouncement(
            ruleUid,
            "The station has fallen to sorcery and witchcraft. The Coven wins!",
            sender: "Central Command Warning"
        );

        _gameTicker.EndRound();
    }

    private void SendCovenBriefing(CovenRuleComponent component)
    {
        var message = "[color=purple][bold]You are part of the Coven!:[/bold]\n" +
                      "Your circle sent you to wipe out this station's crew with deception and witchcraft.\n" +
                      "Avenge your fallen brothers and sisters![/color]";

        foreach (var mob in component.ActiveAntags)
        {
            if (!TryComp<ActorComponent>(mob, out var actor) || actor.PlayerSession == null)
                continue;

            _chatManager.ChatMessageToOne(
                ChatChannel.Server,
                message,
                string.Empty,
                EntityUid.Invalid,
                false,
                actor.PlayerSession.Channel
            );
        }
    }
}
