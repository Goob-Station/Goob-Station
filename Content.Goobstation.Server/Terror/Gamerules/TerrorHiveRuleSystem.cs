using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Terror.Gamerules;

public sealed class TerrorHiveRuleSystem : GameRuleSystem<TerrorHiveRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderComponent, TerrorSpiderDiedEvent>(OnSpiderDeath);
        SubscribeLocalEvent<TerrorSpiderComponent, TerrorHiveWrappedEvent>(OnWrappedCorpse);
        SubscribeLocalEvent<TerrorHiveRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    private string? GetStationName() // Because just putting station announcement as the string is somehow bad, apparently.
    {
        var stations = _stationSystem.GetStations();
        if (stations.Count == 0)
            return null;
        return Name(stations[0]);
    }
    private void OnSelectAntag(EntityUid uid, TerrorHiveRuleComponent rule, AfterAntagEntitySelectedEvent args)
    {
        rule.Queen ??= args.EntityUid;

        Dirty(uid, rule);
    }

    private void OnWrappedCorpse(EntityUid uid, TerrorSpiderComponent spider, TerrorHiveWrappedEvent args)
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
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("terror-hive-infestation-detected"), GetStationName(),
                true,
                rule.DetectedAudio,
                Color.Red);

            rule.InfestationAnnounced = true;
        }

        if (rule.TotalWrapped >= rule.RequiredWrapsForWin)
        {
            if (GetLivingSpiders() > 0 && IsQueenAlive(rule.Queen))
                DoWinCondition(uid, rule);
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

    private void OnSpiderDeath(EntityUid spiderUid, TerrorSpiderComponent spider, TerrorSpiderDiedEvent args)
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
            Loc.GetString("terror-hive-infestation-victory"), GetStationName(),
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
            Loc.GetString("terror-hive-defeated"), GetStationName(),
            true,
            rule.DetectedAudio,
            Color.Green);

        _roundEnd.RequestRoundEnd(null, false);

        Dirty(uid, rule);
    }
}
