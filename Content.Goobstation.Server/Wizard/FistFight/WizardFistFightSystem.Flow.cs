using Content.Goobstation.Shared.Wizard.FistFight;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Wizard.FistFight;

public sealed partial class WizardFistFightSystem
{
    protected override void ActiveTick(EntityUid uid, WizardFistFightRuleComponent comp, GameRuleComponent rule, float frameTime)
    {
        var now = Timing.CurTime;
        var stageOver = now >= comp.StageEnd;

        switch (comp.Stage)
        {
            case WizardFistFightStage.WaitingForChallenger:
                if (comp.Caster is not { } caster || !_mobState.IsAlive(caster))
                    CancelChallenge(uid, comp, "wizard-fistfight-challenge-cancelled");
                else if (stageOver)
                    CancelChallenge(uid, comp, "wizard-fistfight-challenge-unanswered");
                break;

            case WizardFistFightStage.Intro:
                if (!BothStanding(comp))
                    break;

                if (stageOver)
                    StartRound(comp, 1);
                break;

            case WizardFistFightStage.Round:
                if (BothStanding(comp))
                    UpdateRound(comp, stageOver);
                break;

            case WizardFistFightStage.Break:
                if (BothStanding(comp) && stageOver)
                    StartRound(comp, comp.CurrentRound + 1);
                break;

            case WizardFistFightStage.Verdict:
                if (stageOver)
                    Execute(comp);
                break;

            case WizardFistFightStage.Execution:
                if (stageOver)
                    WindDown(comp);
                break;

            case WizardFistFightStage.Return:
                if (stageOver)
                    GameTicker.EndGameRule(uid);
                break;
        }
    }

    private void CancelChallenge(EntityUid ruleUid, WizardFistFightRuleComponent comp, string messageKey)
    {
        if (comp.Spawner is { } spawner)
        {
            Del(spawner);
            comp.Spawner = null;
        }

        if (comp.Caster is { } caster && Exists(caster))
            Refund(caster, comp.WizCoinRefund, messageKey);

        GameTicker.EndGameRule(ruleUid);
    }

    private void StartRound(WizardFistFightRuleComponent comp, int round)
    {
        var suddenDeath = round > comp.Rounds;
        comp.CurrentRound = round;
        comp.Stage = WizardFistFightStage.Round;
        comp.StageEnd = Timing.CurTime + (suddenDeath ? comp.SuddenDeathDuration : comp.RoundDuration);

        foreach (var fighter in Fighters(comp))
        {
            SendToCorner(comp, fighter, fighter.Comp.Corner);
            _popup.PopupEntity(Loc.GetString("wizard-fistfight-round-popup"), fighter, PopupType.LargeCaution);
        }

        _audio.PlayGlobal(comp.BellSound, Filter.Broadcast(), true);
        _chat.DispatchGlobalAnnouncement(suddenDeath
            ? Loc.GetString("wizard-fistfight-sudden-death-start")
            : Loc.GetString("wizard-fistfight-round-start", ("round", round)),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);
    }

    private bool BothStanding(WizardFistFightRuleComponent comp)
    {
        var caster = comp.Caster!.Value;
        var challenger = comp.Challenger!.Value;
        var casterStanding = !TerminatingOrDeleted(caster) && Transform(caster).MapID == comp.ArenaMap;
        var challengerStanding = !TerminatingOrDeleted(challenger) && Transform(challenger).MapID == comp.ArenaMap;

        if (casterStanding && challengerStanding)
            return true;

        if (casterStanding == challengerStanding)
            DeclareDraw(comp);
        else
            DeclareWinner(comp, casterStanding ? caster : challenger, "wizard-fistfight-reason-out");

        return false;
    }

    private void UpdateRound(WizardFistFightRuleComponent comp, bool timeUp)
    {
        var caster = comp.Caster!.Value;
        var challenger = comp.Challenger!.Value;

        var casterDown = IsDown(caster);
        var challengerDown = IsDown(challenger);
        if (casterDown && challengerDown)
        {
            EndRound(comp, null, "wizard-fistfight-round-double-knockdown");
            return;
        }

        if (casterDown || challengerDown)
        {
            EndRound(comp, casterDown ? challenger : caster, "wizard-fistfight-round-knockdown");
            return;
        }

        if (!timeUp)
            return;

        var casterAway = !HasComp<ActorComponent>(caster);
        var challengerAway = !HasComp<ActorComponent>(challenger);
        if (casterAway && challengerAway)
        {
            _chat.DispatchGlobalAnnouncement(Loc.GetString("wizard-fistfight-voided"),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);
            Refund(caster, comp.WizCoinRefund, "wizard-fistfight-challenge-cancelled");
            _adminLog.Add(LogType.EventRan, LogImpact.High, $"Wizard fist fight voided: both fighters were SSD at the bell.");
            WindDown(comp);
            return;
        }

        if (casterAway || challengerAway)
        {
            EndRound(comp, casterAway ? challenger : caster, "wizard-fistfight-round-absent");
            return;
        }

        var staminaDiff = _stamina.GetStaminaDamage(caster) - _stamina.GetStaminaDamage(challenger);
        EntityUid? winner = MathF.Abs(staminaDiff) < 0.5f ? null : staminaDiff < 0 ? caster : challenger;
        EndRound(comp, winner, winner == null ? "wizard-fistfight-round-even" : "wizard-fistfight-round-decision");
    }

    private void EndRound(WizardFistFightRuleComponent comp, EntityUid? roundWinner, string messageKey)
    {
        var caster = comp.Caster!.Value;
        var challenger = comp.Challenger!.Value;

        if (roundWinner is { } scorer)
            Comp<WizardFistFighterComponent>(scorer).Score++;

        var casterScore = Comp<WizardFistFighterComponent>(caster).Score;
        var challengerScore = Comp<WizardFistFighterComponent>(challenger).Score;

        _chat.DispatchGlobalAnnouncement(Loc.GetString(messageKey,
            ("round", comp.CurrentRound),
            ("winner", roundWinner is { } w ? Name(w) : string.Empty),
            ("loser", roundWinner is { } l ? Name(FindOpponent(comp, l)) : string.Empty),
            ("red", Name(challenger)),
            ("redScore", challengerScore),
            ("blue", Name(caster)),
            ("blueScore", casterScore)),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);

        if (comp.CurrentRound >= comp.Rounds)
        {
            if (casterScore != challengerScore)
            {
                DeclareWinner(comp, casterScore > challengerScore ? caster : challenger, "wizard-fistfight-reason-points");
                return;
            }

            if (comp.CurrentRound > comp.Rounds)
            {
                DeclareDraw(comp);
                return;
            }

            _chat.DispatchGlobalAnnouncement(Loc.GetString("wizard-fistfight-sudden-death-announce"),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);
        }

        comp.Stage = WizardFistFightStage.Break;
        comp.StageEnd = Timing.CurTime + comp.BreakDuration;
        foreach (var fighter in Fighters(comp))
            SendToCorner(comp, fighter, fighter.Comp.Corner);
    }

    private void DeclareWinner(WizardFistFightRuleComponent comp, EntityUid winner, string reasonKey)
    {
        var loser = FindOpponent(comp, winner);
        comp.Winner = winner;
        comp.Losers.Clear();
        comp.Losers.Add(loser);
        comp.Stage = WizardFistFightStage.Verdict;
        comp.StageEnd = Timing.CurTime + comp.ExecutionAfterVerdictDelay;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("wizard-fistfight-verdict",
            ("winner", Name(winner)),
            ("loser", Exists(loser) ? Name(loser) : Loc.GetString("wizard-fistfight-unknown-fighter")),
            ("reason", Loc.GetString(reasonKey))),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);

        if (Exists(winner))
            _popup.PopupEntity(Loc.GetString("wizard-fistfight-winner-popup"), winner, PopupType.LargeCaution);
        if (Exists(loser))
            _popup.PopupEntity(Loc.GetString("wizard-fistfight-loser-popup"), loser, PopupType.LargeCaution);

        _adminLog.Add(LogType.EventRan,
            LogImpact.High,
            $"Wizard fist fight decided: {ToPrettyString(winner):winner} beat {ToPrettyString(loser):loser} ({reasonKey}).");
    }

    private void DeclareDraw(WizardFistFightRuleComponent comp)
    {
        var caster = comp.Caster!.Value;
        var challenger = comp.Challenger!.Value;
        comp.Winner = null;
        comp.Losers.Clear();
        comp.Losers.Add(caster);
        comp.Losers.Add(challenger);
        comp.Stage = WizardFistFightStage.Verdict;
        comp.StageEnd = Timing.CurTime + comp.ExecutionAfterVerdictDelay;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("wizard-fistfight-draw",
            ("red", Exists(challenger) ? Name(challenger) : Loc.GetString("wizard-fistfight-unknown-fighter")),
            ("blue", Exists(caster) ? Name(caster) : Loc.GetString("wizard-fistfight-unknown-fighter"))),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);

        foreach (var loser in comp.Losers)
            if (Exists(loser))
                _popup.PopupEntity(Loc.GetString("wizard-fistfight-loser-popup"), loser, PopupType.LargeCaution);

        _adminLog.Add(LogType.EventRan,
            LogImpact.High,
            $"Wizard fist fight drawn: {ToPrettyString(caster):caster} and {ToPrettyString(challenger):challenger} both lose.");
    }

    private void Execute(WizardFistFightRuleComponent comp)
    {
        comp.Stage = WizardFistFightStage.Execution;
        comp.StageEnd = Timing.CurTime + comp.ReturnAfterGibDelay;

        if (comp.Winner is { } winner && TryComp<WizardFistFighterComponent>(winner, out var winnerFighter))
            RestoreFighter(comp, (winner, winnerFighter));

        foreach (var loser in comp.Losers)
        {
            if (!Exists(loser))
                continue;

            if (TryComp<WizardFistFighterComponent>(loser, out var fighter))
                RestoreFighter(comp, (loser, fighter));

            comp.Remains.UnionWith(_body.GibBody(loser));
        }
    }

    private void WindDown(WizardFistFightRuleComponent comp)
    {
        ReturnEveryone(comp);
        comp.Stage = WizardFistFightStage.Return;
        var songEnd = comp.FightStart + comp.MaxRuleLength;
        var earliest = Timing.CurTime + comp.CleanupAfterReturnDelay;
        comp.StageEnd = songEnd > earliest ? songEnd : earliest;
    }
}
