using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wizard.FistFight;
using Content.Server.GameTicking.Rules;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Wizard.FistFight;

public sealed partial class WizardFistFightSystem
{
    [SubscribeLocalEvent]
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

    [SubscribeLocalEvent]
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

    [SubscribeLocalEvent]
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
        var count = 0;
        var query = EntityQueryEnumerator<ActorComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var mob, out var xform))
        {
            if (uid == caster || uid == challenger || xform.MapID == comp.ArenaMap)
                continue;

            if (!_mobState.IsAlive(uid, mob) || HasComp<StationAiHeldComponent>(uid))
                continue;

            count++;
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

        return count;
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
}
