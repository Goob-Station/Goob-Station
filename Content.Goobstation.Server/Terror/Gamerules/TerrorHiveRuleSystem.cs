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

    private string? TryGetStationName(EntityUid uid)
    {
        var station = _stationSystem.GetStationInMap(Transform(uid).MapID);
        if (station == null)
            return null;

        return Name(station.Value);
    }
    private void OnSelectAntag(EntityUid uid, TerrorHiveRuleComponent rule, AfterAntagEntitySelectedEvent args)
    {
        rule.Queen ??= args.EntityUid;
    }

    private void OnWrappedCorpse(EntityUid uid, TerrorSpiderComponent spider, TerrorHiveWrappedEvent args)
    {
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();

        while (rules.MoveNext(out var ruleUid, out var rule, out _))
        {
            rule.TotalWrapped++;
            CheckThresholds(ruleUid, rule);
        }
    }

    private void CheckThresholds(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.HiveDefeated || rule.RoundWon)
            return;

        if (rule.TotalWrapped >= rule.RequiredWrapsForAnnouncement && !rule.InfestationAnnounced)
        {
            var announceFrom = rule.Queen ?? uid;
            _chat.DispatchStationAnnouncement(
                announceFrom,
                Loc.GetString("terror-hive-infestation-detected"),
                colorOverride: Color.Red,
                announcementSound: rule.DetectedAudio);

            rule.InfestationAnnounced = true;
        }

        if (rule.TotalWrapped < rule.RequiredWrapsForWin)
            return;

        if (GetLivingSpiders() > 0 && IsQueenAlive(rule.Queen))
            DoWinCondition(uid, rule);
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

        var announceFrom = rule.Queen ?? uid;
        _chat.DispatchStationAnnouncement(
            announceFrom,
            Loc.GetString("terror-hive-infestation-victory"),
            TryGetStationName(announceFrom),
            true,
            rule.CriticalAudio,
            Color.Red);
    }
    private void CheckLoseConditions(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.RoundWon || rule.HiveDefeated)
            return;

        if (GetLivingSpiders() > 0)
            return;

        rule.HiveDefeated = true;

        var announceFrom = rule.Queen ?? uid;
        _chat.DispatchStationAnnouncement(
            announceFrom,
            Loc.GetString("terror-hive-defeated"),
            TryGetStationName(announceFrom),
            true,
            rule.DetectedAudio,
            Color.Green);
    }
}
