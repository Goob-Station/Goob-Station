using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;

namespace Content.Goobstation.Server.Terror.Gamerules;

public sealed class TerrorHiveRuleSystem : GameRuleSystem<TerrorHiveRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorHiveRuleComponent, TerrorWrappedCorpseEvent>(OnWrappedCorpse);
        SubscribeLocalEvent<TerrorHiveRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }
    private void OnSelectAntag(
    EntityUid uid,
    TerrorHiveRuleComponent rule,
    AfterAntagEntitySelectedEvent args)
    {
        rule.Queen = args.EntityUid;

        Dirty(uid, rule);
    }

    private void OnWrappedCorpse(
        EntityUid uid,
        TerrorHiveRuleComponent rule,
        ref TerrorWrappedCorpseEvent args)
    {
        rule.TotalWrapped++;

        CheckThresholds(uid, rule);

        Dirty(uid, rule);
    }

    private void CheckThresholds(EntityUid uid, TerrorHiveRuleComponent rule)
    {
        if (rule.TotalWrapped >= rule.RequiredWrapsForAnnouncement && !rule.InfestationAnnounced)
        {
            DoInfestationAnnouncement(uid);
            rule.InfestationAnnounced = true;
        }

        if (rule.TotalWrapped >= rule.RequiredWrapsForWin)
        {
            DoWinCondition(uid);
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

private void DoWinCondition(EntityUid uid)
{
    var rules = QueryActiveRules();

    while (rules.MoveNext(out var ruleUid, out _, out var ruleComp, out _))
    {
        if (ruleComp is not TerrorHiveRuleComponent rule)
            continue;

        if (rule.RoundWon)
            continue;

        rule.RoundWon = true;

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString("terror-hive-infestation-victory"),
            Loc.GetString("Station"),
            true,
            rule.CriticalAudio,
            Color.Red);

        _roundEnd.RequestRoundEnd(null, false);

        Dirty(ruleUid, rule);
    }
}
}
