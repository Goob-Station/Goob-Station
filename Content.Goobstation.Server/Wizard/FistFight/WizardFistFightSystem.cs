using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wizard.FistFight;
using Content.Server.Administration.Logs;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.Components;
using Content.Server.Store.Systems;
using Content.Shared.Actions.Components;
using Content.Shared.Administration.Systems;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Magic.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Store.Components;
using Content.Shared.Throwing;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Wizard.FistFight;

public sealed partial class WizardFistFightSystem : GameRuleSystem<WizardFistFightRuleComponent>
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IAdminLogManager _adminLog = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedBuckleSystem _buckle = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedGodmodeSystem _godmode = default!;
    [Dependency] private RejuvenateSystem _rejuvenate = default!;
    [Dependency] private SharedCombatModeSystem _combat = default!;
    [Dependency] private StatusEffectsSystem _status = default!; // Used to check pax. Pax still uses old system so we do too.
    [Dependency] private BodySystem _body = default!;
    [Dependency] private StoreSystem _store = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedMapSystem _map = default!;

    private const string StorageContainerId = "wizard-fistfight-storage";
    private const string GlovesSlot = "gloves";
    private const string Currency = "WizCoin";
    private const string PacifiedStatus = "Pacified";

    private bool TryGetActiveRule(out EntityUid ruleUid, [NotNullWhen(true)] out WizardFistFightRuleComponent? comp)
    {
        var query = EntityQueryEnumerator<WizardFistFightRuleComponent, ActiveGameRuleComponent>();
        var found = query.MoveNext(out ruleUid, out comp, out _);
        if (!found)
            ruleUid = EntityUid.Invalid;
        return found;
    }


    [SubscribeLocalEvent]
    private void OnBeforeCastSpell(Entity<ActionComponent> ent, ref BeforeCastSpellEvent args)
    {
        if (args.Cancelled)
            return;

        if (!HasComp<WizardFistFighterComponent>(args.Performer) && !HasComp<WizardFistFightSpectatorComponent>(args.Performer))
            return;

        args.Cancelled = true;
        _popup.PopupEntity(Loc.GetString("wizard-fistfight-no-magic"), args.Performer, args.Performer, PopupType.MediumCaution);
    }

    [SubscribeLocalEvent]
    private void OnFighterInteractAttempt(Entity<WizardFistFighterComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnFighterUseAttempt(Entity<WizardFistFighterComponent> ent, ref UseAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterPickupAttempt(Entity<WizardFistFighterComponent> ent, ref PickupAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterDropAttempt(Entity<WizardFistFighterComponent> ent, ref DropAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterThrowAttempt(Entity<WizardFistFighterComponent> ent, ref ThrowAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterEquipAttempt(Entity<WizardFistFighterComponent> ent, ref IsEquippingAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterUnequipAttempt(Entity<WizardFistFighterComponent> ent, ref IsUnequippingAttemptEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnFighterPullAttempt(Entity<WizardFistFighterComponent> ent, ref PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
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

    /// <summary>
    /// There's probably more than a few ways someone can polymorph a wiz that's fighting.
    /// Not gonna tattle tho.
    /// </summary>
    [SubscribeLocalEvent]
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

    [SubscribeLocalEvent]
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
}
