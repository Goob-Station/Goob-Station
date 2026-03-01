using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
using Content.Goobstation.Shared.Slasher.Objectives;
using Content.Goobstation.Shared.Slasher.Systems;
using Content.Server.AlertLevel;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Ghost;
using Content.Server.Light.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weather;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using FixedPoint2 = Content.Goobstation.Maths.FixedPoint.FixedPoint2;
using System.Linq;
using Content.Server.Light.Components;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Soul steal system for the slasher. Gives bonuses for stealing souls from incapacitated or dead targets.
/// </summary>
public sealed class SlasherSoulStealSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DevilContractSystem _devilContractSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly SlasherRegenerateSystem _regenerate = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSoulStealComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherSoulStealComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealEvent>(OnSoulSteal);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealDoAfterEvent>(OnSoulStealDoAfterComplete);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, ThrowDoHitEvent>(OnThrowHit);
        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSummonMacheteEvent>(OnSummonMachete);
        SubscribeLocalEvent<SlasherSoulStealComponent, DidEquipHandEvent>(OnDidEquipHand);
        SubscribeLocalEvent<SlasherSoulStealComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnMapInit(Entity<SlasherSoulStealComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherSoulStealComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }

    /// <summary>
    /// Handles the soul steal event
    /// </summary>
    /// <param name="ent">Owner of the SlasherSoulStealComponent</param>
    /// <param name="args">SlasherSoulStealEvent</param>
    private void OnSoulSteal(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealEvent args)
    {
        if (args.Handled || !args.Target.Valid)
            return;

        var user = ent.Owner;
        var target = args.Target;

        // Check rather our victim has a mind
        if (!_mindSystem.TryGetMind(target, out _, out MindComponent? _))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        // Can't steal soul from the same person multiple times
        if (HasComp<SoullessComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-no-mind"), user, user);
            args.Handled = true;
            return;
        }

        // Must be a valid mob
        if (!HasComp<MobStateComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-not-valid"), user, user);
            args.Handled = true;
            return;
        }

        // check if target is downed, incapacitated, or dead
        if (!CanStartSoulSteal(target))
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-fail-not-down"), user, user);
            args.Handled = true;
            return;
        }

        // DoAfter, starting the do-after to the next tick to avoid modifying ActiveDoAfterComponent when active.
        Timer.Spawn(_timing.TickPeriod, () =>
        {
            if (!Exists(user) || !Exists(target))
                return;

            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Soulstealdoafterduration,
                new SlasherSoulStealDoAfterEvent(), user, target: target)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                DistanceThreshold = 2f,
                RequireCanInteract = false
            });
        });

        // Popup for user
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start", ("target", target)), user, user);
        // Popup for victim only
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-start-victim", ("user", user)), target, target, PopupType.MediumCaution);
        args.Handled = true;
    }

    // Checks to ensure our target is valid (alive & not downed, incapacitated, or dead)
    private bool CanStartSoulSteal(EntityUid target)
    {
        return _mobState.IsCritical(target)
               || _mobState.IsIncapacitated(target)
               || _standing.IsDown(target)
               || _mobState.IsDead(target);
    }

    /// <summary>
    /// Slasher - Handles the soul steal do-after
    /// </summary>
    /// <param name="ent">SlasherSoulStealComponent</param>
    /// <param name="ev">SlasherSoulStealDoAfterEvent</param>
    private void OnSoulStealDoAfterComplete(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Args.Target == null)
            return;

        var user = ent.Owner;
        var target = ev.Args.Target.Value;
        var comp = ent.Comp;

        _audio.PlayPvs(ent.Comp.SoulStealSound, target);

        // Release ammonia gas into the atmosphere
        var tileMix = _atmos.GetTileMixture(target, excite: true);
        tileMix?.AdjustMoles(Gas.Ammonia, comp.MolesAmmonia);

        var alive = _mobState.IsAlive(target);

        var bruteBonus = alive ? comp.AliveBruteBonusPerSoul : comp.DeadBruteBonusPerSoul;
        var armorBonus = alive ? comp.AliveArmorPercentPerSoul : comp.DeadArmorPercentPerSoul;

        if (alive)
            comp.AliveSouls++;
        else
            comp.DeadSouls++;

        // Update absorb souls objective progress
        if (_mindSystem.TryGetMind(user, out var mindId, out var mind))
        {
            foreach (var objUid in mind.Objectives)
            {
                if (!TryComp<SlasherAbsorbSoulsConditionComponent>(objUid, out var absorbObj))
                    continue;

                absorbObj.Absorbed += 1;
                Dirty(objUid, absorbObj);
                break;
            }
        }

        // Apply devil clause downside
        _devilContractSystem.AddRandomNegativeClauseSlasher(target);

        // Used to prevent stealing from the same person multiple times
        EnsureComp<SoullessComponent>(target);

        //TryFlavorTwistLimbs(user, target); // TODO Originally intended to take off their limbs and replace them with limbs from random species but I couldn't get it working properly
        ApplyArmorBonus(user, armorBonus, comp);
        ApplyMacheteBonus(user, bruteBonus, comp);

        // Check for ascendance at 15 total souls
        var totalSouls = comp.AliveSouls + comp.DeadSouls;
        if (!comp.HasAscended && totalSouls >= comp.AscendanceSoulThreshold)
        {
            comp.HasAscended = true;

            // Initialize the light flicker timer when ascending
            comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;

            var station = _stationSystem.GetOwningStation(user);
            if (station != null)
            {
                // Set station to red alert
                _alertLevel.SetLevel(station.Value, "red", true, true, true, false);

                // Make it rain in space
                var xform = Transform(user);
                _weather.SetWeather(xform.MapID, _protoMan.Index<WeatherPrototype>("Storm"), null);

                // Make station announcement from Central Command
                _chatSystem.DispatchStationAnnouncement(
                    station.Value,
                    Loc.GetString("slasher-soulsteal-ascendance"),
                    sender: Loc.GetString("comms-console-announcement-title-centcom"),
                    playDefaultSound: false,
                    announcementSound: null,
                    colorOverride: Color.Red);

                _audio.PlayGlobal(comp.AscendanceSound, _stationSystem.GetInOwningStation(station.Value), true);
            }
        }

        // Grant a soul for regenerate
        _regenerate.GrantSoul(user);

        // Popup for user
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success", ("target", target)), user, user, PopupType.LargeCaution);
        // Popup for victim only
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success-victim", ("user", user)), target, target, PopupType.LargeCaution);
        Dirty(user, comp);
    }

    private void ApplyArmorBonus(EntityUid user, float percent, SlasherSoulStealComponent comp)
    {
        if (percent <= 0f)
            return;

        comp.ArmorReduction = MathF.Min(comp.ArmorReduction + percent, comp.ArmorCap);
    }

    private void OnDamageModify(Entity<SlasherSoulStealComponent> ent, ref DamageModifyEvent args)
    {
        var reduction = ent.Comp.ArmorReduction;
        if (reduction <= 0f || args.Damage.Empty)
            return;

        var pairs = args.Damage.DamageDict.ToArray();
        var factor = 1f - reduction;
        foreach (var kv in pairs)
        {
            var type = kv.Key;
            var val = kv.Value;
            if (val <= FixedPoint2.Zero)
                continue; // don't scale healing
            args.Damage.DamageDict[type] = val * factor;
        }
    }

    // Check machete to increase damage bonus
    private EntityUid? GetMachete(EntityUid user)
    {
        if (TryComp<SlasherSummonMacheteComponent>(user, out var summon)
            && summon.MacheteUid != null
            && Exists(summon.MacheteUid.Value))
            return summon.MacheteUid.Value;

        if (!TryComp<HandsComponent>(user, out var hands))
            return null;

        foreach (var held in _hands.EnumerateHeld((user, hands)))
        {
            if (HasComp<SlasherMassacreMacheteComponent>(held))
                return held;
        }

        return null;
    }

    // Apply brute bonus to machete
    private void ApplyMacheteBonus(EntityUid user, float bruteBonus, SlasherSoulStealComponent comp)
    {
        if (bruteBonus <= 0f)
            return;

        var machete = GetMachete(user);
        if (machete == null)
            return;

        var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete.Value);
        bonusComp.SlashBonus += bruteBonus;
        comp.TotalAppliedBruteBonus += bruteBonus;
        comp.LastMachete = machete.Value;
        Dirty(machete.Value, bonusComp);
    }

    /// <summary>
    /// Slasher - Handles the machete bonus damage from stealing souls
    /// </summary>
    /// <param name="ent">SlasherSoulStealMacheteBonusComponent</param>
    /// <param name="args">GetMeleeDamageEvent</param>
    private void OnGetMeleeDamage(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f)
            return;

        var add = new DamageSpecifier();

        add.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        args.Damage += add;
    }

    private void OnThrowHit(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f || TerminatingOrDeleted(args.Target))
            return;

        var dmgAdj = new DamageSpecifier();

        dmgAdj.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        _damageable.TryChangeDamage(args.Target, dmgAdj, true, origin: args.Component.Thrower);
    }

    /// <summary>
    /// Slasher - Handles summoning the Machete to the slasher (self)
    /// </summary>
    /// <param name="ent">SlasherSoulStealComponent</param>
    /// <param name="args">SlasherSummonMacheteEvent</param>
    private void OnSummonMachete(Entity<SlasherSoulStealComponent> ent, ref SlasherSummonMacheteEvent args)
    {
        var machete = GetMachete(ent.Owner);

        if (machete != null)
        {
            ent.Comp.LastMachete = machete.Value;

            if (ent.Comp.TotalAppliedBruteBonus > 0f)
            {
                var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(machete.Value);
                bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
                Dirty(machete.Value, bonusComp);
            }

            Dirty(ent);
        }
    }

    private void OnDidEquipHand(Entity<SlasherSoulStealComponent> ent, ref DidEquipHandEvent args)
    {
        if (!HasComp<SlasherMassacreMacheteComponent>(args.Equipped))
            return;

        ent.Comp.LastMachete = args.Equipped;

        if (ent.Comp.TotalAppliedBruteBonus > 0f)
        {
            var bonusComp = EnsureComp<SlasherSoulStealMacheteBonusComponent>(args.Equipped);
            bonusComp.SlashBonus = ent.Comp.TotalAppliedBruteBonus;
            Dirty(args.Equipped, bonusComp);
        }

        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherSoulStealComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.HasAscended)
                continue;

            // Don't start unless ascended
            if (_timing.CurTime < comp.NextLightFlicker)
                continue;

            // Trigger light flicker around the slasher
            FlickerLightsAround(uid, comp);

            // Schedule next flicker
            comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;
            Dirty(uid, comp);
        }
    }

    private void FlickerLightsAround(EntityUid slasher, SlasherSoulStealComponent comp)
    {
        var entities = _lookup.GetEntitiesInRange(slasher, comp.LightFlickerRadius).ToList();
        _random.Shuffle(entities);

        var flickerCounter = 0;
        foreach (var entity in entities)
        {
            if (!HasComp<PointLightComponent>(entity) && !HasComp<PoweredLightComponent>(entity))
                continue;

            var handled = false;

            // For powered lights, 50/50 chance to either flicker or destroy the bulb
            if (TryComp<PoweredLightComponent>(entity, out var lightComp) && _random.Prob(0.5f))
            {
                // Destroy the light bulb
                if (_light.TryDestroyBulb(entity, lightComp))
                    handled = true;
            }
            else
            {
                // Flicker the light via ghost boo event
                var ev = new GhostBooEvent();
                RaiseLocalEvent(entity, ev);
                handled = ev.Handled;
            }

            if (handled)
                flickerCounter++;

            if (flickerCounter >= comp.MaxLightsToFlicker)
                break;
        }
    }
}
