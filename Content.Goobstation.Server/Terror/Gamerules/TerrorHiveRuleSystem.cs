using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Terror.Gamerules;

public sealed class TerrorHiveRuleSystem : GameRuleSystem<TerrorHiveRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderComponent, TerrorSpiderDiedEvent>(OnSpiderDeath);
        SubscribeLocalEvent<TerrorWrappedCorpseEvent>(OnWrappedCorpse);
        SubscribeLocalEvent<TerrorHiveRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }
    private void OnSelectAntag(
    EntityUid uid,
    TerrorHiveRuleComponent rule,
    AfterAntagEntitySelectedEvent args)
    {
        rule.Queen ??= args.EntityUid;

        Dirty(uid, rule);
    }

    private void OnWrappedCorpse(TerrorWrappedCorpseEvent args)
    {
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();

        while (rules.MoveNext(out var ruleUid, out var rule, out _))
        {
            rule.TotalWrapped++;

            CheckThresholds(ruleUid, rule);

            Dirty(ruleUid, rule);
        }
    }

    private void CheckThresholds(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.HiveDefeated || rule.RoundWon)
            return;

        if (rule.TotalWrapped >= rule.RequiredWrapsForAnnouncement && !rule.InfestationAnnounced)
        {
            DoInfestationAnnouncement(uid);
            rule.InfestationAnnounced = true;
        }

        if (rule.TotalWrapped >= rule.RequiredWrapsForWin)
        {
            var livingSpiders = GetLivingSpiders();

            if (livingSpiders > 0 && IsQueenAlive(rule.Queen))
                DoWinCondition(uid, rule);
        }
    }
    private void DoInfestationAnnouncement(EntityUid uid)
    {
        var rules = QueryActiveRules();

        while (rules.MoveNext(out var ruleUid, out _, out var ruleComp, out _))
        {
            if (ruleComp is not TerrorHiveRuleComponent rule)
                continue;

            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("terror-hive-infestation-detected"),
                Loc.GetString("Station"),
                true,
                rule.DetectedAudio,
                Color.Red);
        }
    }

    private int GetLivingSpiders()
    {
        var count = 0;
        var query = EntityQueryEnumerator<TerrorSpiderComponent>();

        while (query.MoveNext(out var uid, out _))
        {
            if (!_mobState.IsDead(uid))
                count++;
        }

        return count;
    }

    private void OnSpiderDeath(
        EntityUid spiderUid,
        TerrorSpiderComponent spider,
        ref TerrorSpiderDiedEvent args)
    {
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();

        while (rules.MoveNext(out var ruleUid, out var rule, out _))
        {
            CheckLoseConditions(ruleUid, rule);
            Dirty(ruleUid, rule);
        }
    }

    private bool IsQueenAlive(EntityUid? queen)
    {
        if (queen == null)
            return false;

        if (!EntityManager.EntityExists(queen.Value))
            return false;

        return !_mobState.IsDead(queen.Value);
    }

    private void DoWinCondition(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.RoundWon || rule.HiveDefeated)
            return;

        rule.RoundWon = true;

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("terror-hive-infestation-victory"),
            Loc.GetString("Station"),
            true,
            rule.CriticalAudio,
            Color.Red);

        _roundEnd.RequestRoundEnd(null, false);

        Dirty(uid, rule);
    }
    private void CheckLoseConditions(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.RoundWon || rule.HiveDefeated)
            return;

        var livingSpiders = GetLivingSpiders();

        if (livingSpiders <= 0)
        {
            DoHiveDefeat(uid, rule);
        }
    }

    private void DoHiveDefeat(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.HiveDefeated)
            return;

        rule.HiveDefeated = true;

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("terror-hive-defeated"),
            Loc.GetString("Station"),
            true,
            rule.DetectedAudio,
            Color.Green);

        _roundEnd.RequestRoundEnd(null, false);

        Dirty(uid, rule);
    }
}
