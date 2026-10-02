using System.Linq;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Administration.Logs;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.Components;
using Content.Server.Store.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Shitcode.Wizard.FistFight;
using Content.Shared.Actions.Components;
using Content.Shared.Administration.Systems;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Magic.Events;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusEffect;
using Content.Shared.Store.Components;
using Content.Shared.Throwing;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Shitcode.Wizard.FistFight;

public sealed class WizardFistFightSystem : GameRuleSystem<WizardFistFightRuleComponent>
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!; // Used to check pax. Pax still uses old system so we do too.
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private const string StorageContainerId = "wizard-fistfight-storage";
    private const string GlovesSlot = "gloves";
    private const string Currency = "WizCoin";
    private const string PacifiedStatus = "Pacified";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, WizardFistFightEvent>(OnPurchased);
        SubscribeLocalEvent<WizardFistFightRuleComponent, RuleLoadedGridsEvent>(OnGridsLoaded);
        SubscribeLocalEvent<WizardFistFightChallengerComponent, MindAddedMessage>(OnChallengerMindAdded);

        SubscribeLocalEvent<ActionComponent, BeforeCastSpellEvent>(OnBeforeCastSpell);

        SubscribeLocalEvent<WizardFistFighterComponent, InteractionAttemptEvent>(OnFighterInteractAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, UseAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, PickupAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, DropAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, ThrowAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, IsEquippingAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, IsUnequippingAttemptEvent>(OnFighterBlockedAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, PullAttemptEvent>(OnFighterPullAttempt);
        SubscribeLocalEvent<WizardFistFighterComponent, AttackAttemptEvent>(OnFighterAttackAttempt);

        SubscribeLocalEvent<WizardFistFighterComponent, PolymorphedEvent>(OnFighterPolymorphed);
        SubscribeLocalEvent<WizardFistFightSpectatorComponent, PolymorphedEvent>(OnSpectatorPolymorphed);
    }

    #region Setup

    private void OnPurchased(Entity<MindContainerComponent> ent, ref WizardFistFightEvent args)
    {
        var buyer = ent.Owner;

        if (TryGetActiveRule(out _, out _))
        {
            Refund(buyer, args.WizCoinRefund, "wizard-fistfight-already-active");
            return;
        }

        if (!GameTicker.StartGameRule(args.GameRule, out var ruleUid)
            || !TryComp<WizardFistFightRuleComponent>(ruleUid, out var comp)
            || comp.ArenaMap == null || comp.RedCorner == null || comp.BlueCorner == null || comp.SpectatorSpawns.Count == 0)
        {
            Log.Error($"Wizard fist fight could not start: rule {args.GameRule} failed to load its arena or is missing its markers.");
            Refund(buyer, args.WizCoinRefund, "wizard-fistfight-failed");
            if (Exists(ruleUid))
                GameTicker.EndGameRule(ruleUid);
            return;
        }

        comp.Caster = buyer;
        comp.WizCoinPrize = args.WizCoinPrize;
        comp.WizCoinRefund = args.WizCoinRefund;
        comp.StageEnd = Timing.CurTime + args.ChallengeTimeout;
        comp.Spawner = Spawn(args.ChallengerSpawner, Transform(buyer).Coordinates);

        _audio.PlayPvs(args.AnnouncementSound, buyer);
        _popup.PopupEntity(Loc.GetString("wizard-fistfight-challenge-popup"), buyer, buyer, PopupType.Large);
        _adminLog.Add(LogType.EventRan, LogImpact.High, $"{ToPrettyString(buyer):player} started the wizard fist fight event.");
    }

    private void OnGridsLoaded(Entity<WizardFistFightRuleComponent> ent, ref RuleLoadedGridsEvent args)
    {
        var comp = ent.Comp;
        comp.ArenaMap = args.Map;

        var corners = EntityQueryEnumerator<WizardFistFightCornerComponent, TransformComponent>();
        while (corners.MoveNext(out _, out var corner, out var xform))
        {
            if (xform.MapID != args.Map)
                continue;

            if (corner.Corner == FistFightCorner.Red)
                comp.RedCorner = xform.Coordinates;
            else
                comp.BlueCorner = xform.Coordinates;
        }

        var spawns = EntityQueryEnumerator<WizardFistFightSpectatorSpawnComponent, TransformComponent>();
        while (spawns.MoveNext(out _, out _, out var xform))
        {
            if (xform.MapID == args.Map)
                comp.SpectatorSpawns.Add(xform.Coordinates);
        }
    }

    private void OnChallengerMindAdded(Entity<WizardFistFightChallengerComponent> ent, ref MindAddedMessage args)
    {
        if (!TryGetActiveRule(out var ruleUid, out var comp)
            || comp.Stage != WizardFistFightStage.WaitingForChallenger
            || comp.Challenger != null)
        {
            Log.Warning($"Wizard fist fight challenger {ToPrettyString(ent)} arrived with broken gamerule.");
            QueueDel(ent);
            return;
        }

        comp.Challenger = ent.Owner;
        RemCompDeferred<WizardFistFightChallengerComponent>(ent);
        BeginFight(ruleUid, comp);
    }

    private void BeginFight(EntityUid ruleUid, WizardFistFightRuleComponent comp)
    {
        var challenger = comp.Challenger!.Value;

        if (comp.Caster is not { } caster || !_mobState.IsAlive(caster))
        {
            comp.Winner = challenger;
            if (comp.Caster is { } forfeited)
                comp.Losers.Add(forfeited);
            _chat.DispatchGlobalAnnouncement(Loc.GetString("wizard-fistfight-forfeit", ("winner", Name(challenger))),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);
            Execute(comp);
            _adminLog.Add(LogType.EventRan, LogImpact.High, $"Wizard fist fight ended by forfeit: {ToPrettyString(challenger):winner} wins.");
            GameTicker.EndGameRule(ruleUid);
            return;
        }

        var now = Timing.CurTime;
        comp.FightStart = now;
        comp.Stage = WizardFistFightStage.Intro;
        comp.StageEnd = now + comp.IntroDuration;
        comp.Storage = _container.EnsureContainer<Container>(ruleUid, StorageContainerId);

        comp.Home = SaveLocation(caster);

        var spectators = GatherSpectators(comp, caster, challenger);
        PrepareFighter(comp, caster, FistFightCorner.Blue);
        PrepareFighter(comp, challenger, FistFightCorner.Red);

        comp.MusicEntity = _audio.PlayGlobal(comp.Music, Filter.Broadcast(), true)?.Entity;
        _chat.DispatchGlobalAnnouncement(Loc.GetString(comp.IntroAnnouncement,
            ("red", Name(challenger)),
            ("blue", Name(caster)),
            ("rounds", comp.Rounds)),
            Loc.GetString(comp.AnnouncerName),
            playSound: false,
            colorOverride: comp.AnnouncerColor);
        _adminLog.Add(LogType.EventRan, LogImpact.Extreme,
            $"Wizard fist fight started: {ToPrettyString(caster):caster} vs {ToPrettyString(challenger):challenger}, {spectators} spectators teleported to ring.");
    }

    private int GatherSpectators(WizardFistFightRuleComponent comp, EntityUid caster, EntityUid challenger)
    {
        var candidates = new List<EntityUid>();
        var query = EntityQueryEnumerator<ActorComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var mob, out var xform))
        {
            if (uid == caster || uid == challenger || xform.MapID == comp.ArenaMap)
                continue;

            if (!_mobState.IsAlive(uid, mob) || HasComp<StationAiHeldComponent>(uid))
                continue;

            candidates.Add(uid);
        }

        foreach (var uid in candidates)
        {
            var spectator = EnsureComp<WizardFistFightSpectatorComponent>(uid);
            spectator.Home = SaveLocation(uid);
            if (_status.TryGetTime(uid, PacifiedStatus, out var pacifiedTime))
            {
                spectator.PacifiedUntil = pacifiedTime.Value.Item2;
                _status.TryRemoveStatusEffect(uid, PacifiedStatus);
            }
            else
            {
                spectator.WasPacified = HasComp<PacifiedComponent>(uid);
            }

            spectator.WasGodmoded = HasComp<GodmodeComponent>(uid);
            ApplySpectatorProtection(uid);

            var jitter = new Vector2(RobustRandom.NextFloat(-0.35f, 0.35f), RobustRandom.NextFloat(-0.35f, 0.35f));
            Teleport(uid, RobustRandom.Pick(comp.SpectatorSpawns).Offset(jitter));
            _popup.PopupEntity(Loc.GetString("wizard-fistfight-spectator-popup"), uid, uid, PopupType.Medium);
        }

        return candidates.Count;
    }

    private void ApplySpectatorProtection(EntityUid uid)
    {
        EnsureComp<PacifiedComponent>(uid);
        _godmode.EnableGodmode(uid); // Surely won't cause problems
    }

    private void PrepareFighter(WizardFistFightRuleComponent comp, EntityUid uid, FistFightCorner corner)
    {
        var fighter = EnsureComp<WizardFistFighterComponent>(uid);
        fighter.Corner = corner;
        Dirty(uid, fighter);

        if (_status.TryGetTime(uid, PacifiedStatus, out var pacifiedTime))
        {
            fighter.PacifiedUntil = pacifiedTime.Value.Item2;
            _status.TryRemoveStatusEffect(uid, PacifiedStatus);
        }
        else
        {
            fighter.WasPacified = HasComp<PacifiedComponent>(uid);
        }

        RemComp<PacifiedComponent>(uid);
        _combat.SetInCombatMode(uid, true);

        foreach (var held in _hands.EnumerateHeld(uid).ToList())
            if (_container.Insert(held, comp.Storage!))
                fighter.StoredItems.Add(held);

        if (_inventory.TryUnequip(uid, GlovesSlot, out var oldGloves, silent: true, force: true)
            && _container.Insert(oldGloves.Value, comp.Storage!))
            fighter.OriginalGloves = oldGloves;

        var gloves = Spawn(corner == FistFightCorner.Red ? comp.RedGloves : comp.BlueGloves, Transform(uid).Coordinates);
        if (!_inventory.TryEquip(uid, gloves, GlovesSlot, silent: true, force: true)
            && !_hands.TryForcePickupAnyHand(uid, gloves))
        {
            QueueDel(gloves);
            gloves = EntityUid.Invalid;
        }

        if (gloves.IsValid())
            fighter.Gloves = gloves;

        var cornerCoords = FindCornerCoordinates(comp, corner);
        SendToCorner(comp, uid, corner);
        Spawn(comp.TeleportEffect, cornerCoords);
        _audio.PlayPvs(comp.TeleportSound, cornerCoords);
        _popup.PopupEntity(Loc.GetString("wizard-fistfight-fighter-popup"), uid, uid, PopupType.LargeCaution);
    }

    #endregion

    #region Flow

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

    #endregion

    #region Teardown

    private void ReturnEveryone(WizardFistFightRuleComponent comp)
    {
        foreach (var participant in new[] { comp.Caster, comp.Challenger })
        {
            if (participant is not { } uid || !Exists(uid))
                continue;

            if (TryComp<WizardFistFighterComponent>(uid, out var fighter))
                RestoreFighter(comp, (uid, fighter));

            if (comp.Home is { } home)
                TeleportHome(comp, uid, home);
        }

        var spectators = new List<Entity<WizardFistFightSpectatorComponent>>();
        var query = EntityQueryEnumerator<WizardFistFightSpectatorComponent>();
        while (query.MoveNext(out var uid, out var spectator))
        {
            spectators.Add((uid, spectator));
        }

        var now = Timing.CurTime;
        foreach (var (uid, spectator) in spectators)
        {
            RemComp<WizardFistFightSpectatorComponent>(uid);
            if (!spectator.WasPacified)
                RemComp<PacifiedComponent>(uid);
            if (spectator.PacifiedUntil is { } until && until > now)
                _status.TryAddStatusEffect(uid, PacifiedStatus, until - now, true, PacifiedStatus);
            if (!spectator.WasGodmoded)
                _godmode.DisableGodmode(uid);

            TeleportHome(comp, uid, spectator.Home);
        }

        DumpLeftovers(comp);

        if (!comp.PrizePaid && comp.Winner is { } winner && Exists(winner))
        {
            comp.PrizePaid = true;
            RewardWinner(comp, winner);
        }
    }

    private void DumpLeftovers(WizardFistFightRuleComponent comp)
    {
        EntityCoordinates? destination = null;
        if (comp.Home is { } home)
            destination = IsUsable(comp, home) ? home : RandomStationSpawn(comp);
        destination ??= RandomStationSpawn(comp);

        if (destination is not { } spot)
        {
            Log.Warning("Wizard fist fight could not find anywhere on the station to dump the leftovers.");
            return;
        }

        foreach (var uid in comp.Remains)
        {
            if (!Exists(uid) || Transform(uid).MapID != comp.ArenaMap || _container.IsEntityInContainer(uid))
                continue;

            _transform.SetCoordinates(uid, spot.Offset(Scatter()));
            _transform.AttachToGridOrMap(uid);
        }

        comp.Remains.Clear();

        if (comp.Storage is not { } storage)
            return;

        foreach (var item in storage.ContainedEntities.ToList())
            _container.Remove(item, storage, destination: spot.Offset(Scatter()));
    }

    /// <summary>
    /// There shouldn't really be any way to still exist in the arena after the event is over but just in case
    /// we teleport them to the station intead of atomizing them with the map.
    /// </summary>
    private void EvacuateArena(WizardFistFightRuleComponent comp)
    {
        if (comp.ArenaMap is not { } arena)
            return;

        var stranded = new List<EntityUid>();
        var query = EntityQueryEnumerator<MindContainerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mind, out var xform))
        {
            if (mind.HasMind && xform.MapID == arena)
                stranded.Add(uid);
        }

        foreach (var uid in stranded)
        {
            if (RandomStationSpawn(comp) is { } spawn)
                Teleport(uid, spawn);
            else
                Log.Warning($"Wizard fist fight could not find anywhere on the station to evacuate {ToPrettyString(uid)} to.");
        }
    }

    private void RestoreFighter(WizardFistFightRuleComponent comp, Entity<WizardFistFighterComponent> ent)
    {
        var (uid, fighter) = ent;

        RemComp<WizardFistFighterComponent>(uid);

        if (fighter.Gloves is { } gloves && Exists(gloves))
        {
            _inventory.TryUnequip(uid, GlovesSlot, out _, silent: true, force: true);
            QueueDel(gloves);
        }

        if (fighter.OriginalGloves is { } original && Exists(original)
            && !_inventory.TryEquip(uid, original, GlovesSlot, silent: true, force: true))
            GiveOrDrop(comp, uid, original);

        foreach (var item in fighter.StoredItems)
            if (Exists(item))
                GiveOrDrop(comp, uid, item);

        _rejuvenate.PerformRejuvenate(uid);

        var now = Timing.CurTime;
        if (fighter.PacifiedUntil is { } until && until > now)
            _status.TryAddStatusEffect(uid, PacifiedStatus, until - now, true, PacifiedStatus);
        else if (fighter.WasPacified)
            EnsureComp<PacifiedComponent>(uid);
    }

    private void GiveOrDrop(WizardFistFightRuleComponent comp, EntityUid owner, EntityUid item)
    {
        if (_hands.TryForcePickupAnyHand(owner, item))
            return;

        _container.Remove(item, comp.Storage!, destination: Transform(owner).Coordinates);
    }

    protected override void Ended(EntityUid uid, WizardFistFightRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, comp, gameRule, args);

        if (comp.Spawner is { } spawner)
            QueueDel(spawner);

        ReturnEveryone(comp);
        EvacuateArena(comp);

        if (comp.MusicEntity is { } music)
            _audio.Stop(music);

        if (comp.ArenaMap is { } map && _map.MapExists(map))
            _map.DeleteMap(map);
    }

    #endregion

    #region Polymorph

    /// <summary>
    /// There's probably more than a few ways someone can polymorph a wiz that's fighting.
    /// Not gonna tattle tho.
    /// </summary>
    private void OnFighterPolymorphed(Entity<WizardFistFighterComponent> ent, ref PolymorphedEvent args)
    {
        if (args.OldEntity != ent.Owner || !TryGetActiveRule(out _, out var comp))
            return;

        var old = ent.Comp;
        var fresh = EnsureComp<WizardFistFighterComponent>(args.NewEntity);
        fresh.Corner = old.Corner;
        fresh.Score = old.Score;
        fresh.Gloves = old.Gloves;
        fresh.OriginalGloves = old.OriginalGloves;
        fresh.StoredItems = old.StoredItems;
        fresh.WasPacified = old.WasPacified;
        fresh.PacifiedUntil = old.PacifiedUntil;
        Dirty(args.NewEntity, fresh);
        RemComp<WizardFistFighterComponent>(ent);

        if (comp.Caster == ent.Owner)
            comp.Caster = args.NewEntity;
        else if (comp.Challenger == ent.Owner)
            comp.Challenger = args.NewEntity;

        if (comp.Winner == ent.Owner)
            comp.Winner = args.NewEntity;

        var loserIndex = comp.Losers.IndexOf(ent.Owner);
        if (loserIndex >= 0)
            comp.Losers[loserIndex] = args.NewEntity;

        RemComp<PacifiedComponent>(args.NewEntity);
        _combat.SetInCombatMode(args.NewEntity, true);
        Teleport(args.NewEntity, FindCornerCoordinates(comp, fresh.Corner));
    }

    private void OnSpectatorPolymorphed(Entity<WizardFistFightSpectatorComponent> ent, ref PolymorphedEvent args)
    {
        if (args.OldEntity != ent.Owner)
            return;

        var old = ent.Comp;
        var fresh = EnsureComp<WizardFistFightSpectatorComponent>(args.NewEntity);
        fresh.Home = old.Home;
        fresh.WasPacified = old.WasPacified;
        fresh.WasGodmoded = old.WasGodmoded;
        fresh.PacifiedUntil = old.PacifiedUntil;
        RemComp<WizardFistFightSpectatorComponent>(ent);

        ApplySpectatorProtection(args.NewEntity);
    }

    #endregion


    private bool TryGetActiveRule(out EntityUid ruleUid, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out WizardFistFightRuleComponent? comp)
    {
        var query = EntityQueryEnumerator<WizardFistFightRuleComponent, ActiveGameRuleComponent>();
        var found = query.MoveNext(out ruleUid, out comp, out _);
        if (!found)
            ruleUid = EntityUid.Invalid;
        return found;
    }

    private void OnBeforeCastSpell(Entity<ActionComponent> ent, ref BeforeCastSpellEvent args)
    {
        if (args.Cancelled)
            return;

        if (!HasComp<WizardFistFighterComponent>(args.Performer) && !HasComp<WizardFistFightSpectatorComponent>(args.Performer))
            return;

        args.Cancelled = true;
        _popup.PopupEntity(Loc.GetString("wizard-fistfight-no-magic"), args.Performer, args.Performer, PopupType.MediumCaution);
    }

    private void OnFighterInteractAttempt(Entity<WizardFistFighterComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnFighterBlockedAttempt(EntityUid uid, WizardFistFighterComponent comp, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnFighterPullAttempt(Entity<WizardFistFighterComponent> ent, ref PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnFighterAttackAttempt(Entity<WizardFistFighterComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Disarm)
        {
            args.Cancel();
            return;
        }

        if (args.Weapon is { } weapon
            && weapon.Owner != ent.Owner
            && weapon.Owner != ent.Comp.Gloves
            && !_hands.IsHolding(ent.Owner, weapon.Owner))
        {
            args.Cancel();
            return;
        }

        if (args.Target is { } target && (target == ent.Owner || !HasComp<WizardFistFighterComponent>(target)))
            args.Cancel();
    }

    private static EntityUid FindOpponent(WizardFistFightRuleComponent comp, EntityUid fighter)
    {
        return fighter == comp.Caster ? comp.Challenger!.Value : comp.Caster!.Value;
    }

    private static EntityCoordinates FindCornerCoordinates(WizardFistFightRuleComponent comp, FistFightCorner corner)
    {
        return corner == FistFightCorner.Red ? comp.RedCorner!.Value : comp.BlueCorner!.Value;
    }

    private Vector2 Scatter()
    {
        return new Vector2(RobustRandom.NextFloat(-1f, 1f), RobustRandom.NextFloat(-1f, 1f));
    }

    private void SendToCorner(WizardFistFightRuleComponent comp, EntityUid uid, FistFightCorner corner)
    {
        if (!Exists(uid))
            return;

        Teleport(uid, FindCornerCoordinates(comp, corner));
        _rejuvenate.PerformRejuvenate(uid);
    }

    private IEnumerable<Entity<WizardFistFighterComponent>> Fighters(WizardFistFightRuleComponent comp)
    {
        if (comp.Caster is { } caster && TryComp<WizardFistFighterComponent>(caster, out var casterComp))
            yield return (caster, casterComp);

        if (comp.Challenger is { } challenger && TryComp<WizardFistFighterComponent>(challenger, out var challengerComp))
            yield return (challenger, challengerComp);
    }

    private bool IsDown(EntityUid uid)
    {
        return _mobState.IsIncapacitated(uid) || TryComp<StaminaComponent>(uid, out var stamina) && stamina.Critical;
    }

    private EntityCoordinates SaveLocation(EntityUid uid)
    {
        _container.TryRemoveFromContainer(uid, true);
        return Transform(uid).Coordinates;
    }

    private void Teleport(EntityUid uid, EntityCoordinates coords)
    {
        _container.TryRemoveFromContainer(uid, true);
        _pulling.StopAllPulls(uid);
        if (TryComp<BuckleComponent>(uid, out var buckle) && buckle.Buckled)
            _buckle.Unbuckle((uid, buckle), null);

        _transform.SetCoordinates(uid, coords);
        _transform.AttachToGridOrMap(uid);
    }

    private void TeleportHome(WizardFistFightRuleComponent comp, EntityUid uid, EntityCoordinates home)
    {
        if (IsUsable(comp, home))
            Teleport(uid, home);
        else if (RandomStationSpawn(comp) is { } spawn)
            Teleport(uid, spawn);
        else
            Log.Warning($"Wizard fist fight could not find anywhere to send {ToPrettyString(uid)} home.");
    }

    private EntityCoordinates? RandomStationSpawn(WizardFistFightRuleComponent comp)
    {
        var spawns = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out _, out var spawn, out var xform))
        {
            if (spawn.SpawnType == SpawnPointType.LateJoin && xform.MapID != comp.ArenaMap)
                spawns.Add(xform.Coordinates);
        }

        return spawns.Count > 0 ? RobustRandom.Pick(spawns) : null;
    }

    private bool IsUsable(WizardFistFightRuleComponent comp, EntityCoordinates coords)
    {
        if (TerminatingOrDeleted(coords.EntityId))
            return false;

        var map = Transform(coords.EntityId).MapID;
        return map != MapId.Nullspace && map != comp.ArenaMap && _map.MapExists(map);
    }

    private void RewardWinner(WizardFistFightRuleComponent comp, EntityUid winner)
    {
        if (FindSpellbook(winner) is { } book)
        { // Evelyne larp approved
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { Currency, comp.WizCoinPrize } }, book);
        }
        else
        {
            book = Spawn(comp.VictoryItem, Transform(winner).Coordinates);
            if (TryComp<StoreComponent>(book, out var store))
            {
                store.Balance.Clear();
                store.Balance[Currency] = comp.WizCoinPrize;
            }

            _hands.TryForcePickupAnyHand(winner, book);
        }

        _popup.PopupEntity(Loc.GetString("wizard-fistfight-prize-popup", ("amount", comp.WizCoinPrize)), winner, winner, PopupType.Large);
    }

    private void Refund(EntityUid user, FixedPoint2 amount, string messageKey)
    {
        if (amount > FixedPoint2.Zero && FindSpellbook(user) is { } book)
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { Currency, amount } }, book);

        _popup.PopupEntity(Loc.GetString(messageKey), user, user, PopupType.MediumCaution);
    }

    private EntityUid? FindSpellbook(EntityUid user)
    {
        var query = EntityQueryEnumerator<StoreComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var store, out var xform))
        {
            if (!store.CurrencyWhitelist.Contains(Currency))
                continue;

            var parent = xform.ParentUid;
            for (var depth = 0; parent.IsValid() && depth < 8; depth++)
            {
                if (parent == user)
                    return uid;

                parent = Transform(parent).ParentUid;
            }
        }

        return null;
    }
}
