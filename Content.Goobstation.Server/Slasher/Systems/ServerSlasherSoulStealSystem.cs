using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Shared.Slasher;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Objectives;
using Content.Goobstation.Shared.Slasher.Systems;
using Content.Server.AlertLevel;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Ghost;
using Content.Server.Light.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Light.Components;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Station;
using Content.Shared.Throwing;
using Content.Shared.Weather;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server-side of the slasher's soul steal.
/// </summary>
public sealed class ServerSlasherSoulStealSystem : EntitySystem
{
    [Dependency] private readonly SlasherSoulStealSystem _soulSteal = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
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
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedStationSpawningSystem _spawning = default!;
    [Dependency] private readonly SlasherPrestigeSystem _prestige = default!;
    [Dependency] private readonly IdolSlasherCharmSystem _lovestruck = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealDoAfterEvent>(OnSoulStealDoAfterComplete);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    /// <summary>
    /// Slasher - Handles the soul steal do-after
    /// </summary>
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
        if (_mindSystem.TryGetMind(user, out _, out var mind))
            foreach (var objUid in mind.Objectives.ToList())
            {
                if (!TryComp<SlasherAbsorbSoulsConditionComponent>(objUid, out var absorbObj))
                    continue;

                absorbObj.Absorbed += 1;
                Dirty(objUid, absorbObj);
                break;
            }

        // Apply devil clause downside
        _devilContractSystem.AddRandomNegativeClauseSlasher(target);

        // Used to prevent stealing from the same person multiple times
        EnsureComp<SoullessComponent>(target);

        if (HasComp<SlasherIdolComponent>(user))
            _lovestruck.TryConvert(user, target);

        //TryFlavorTwistLimbs(user, target); // TODO Originally intended to take off their limbs and replace them with limbs from random species but I couldn't get it working properly
        _soulSteal.ApplyArmorBonus(user, armorBonus, comp);
        _soulSteal.ApplyMacheteBonus(user, bruteBonus, comp);

        var totalSouls = comp.AliveSouls + comp.DeadSouls;

        var specialUnlockHappened = false;

        // Check for possession unlock at 10 souls
        if (!comp.HasUnlockedPossession
            && totalSouls >= comp.PossessionSoulThreshold)
        {
            comp.HasUnlockedPossession = true;
            EnsureComp<SlasherPossessionComponent>(user);

            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-unlock-possession"), user, user, PopupType.LargeCaution);
            specialUnlockHappened = true;
        }

        // Check for ascendance at 15 total souls
        if (!comp.HasAscended
            && totalSouls >= comp.AscendanceSoulThreshold)
        {
            comp.HasAscended = true;

            // Record the prestige unlock for this player so they can pick gated variants later.
            if (comp.AscensionId != null && TryComp<ActorComponent>(user, out var actor))
                _prestige.GrantAscension(actor.PlayerSession.UserId, comp.AscensionId);

            // Initialize the light flicker timer when ascending
            comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;

            var station = _stationSystem.GetOwningStation(user);
            if (station != null)
            {
                _alertLevel.SetLevel(station.Value, "red", true, false, true, false);

                // Make it rain in space
                var xform = Transform(user);
                _weather.SetWeather(xform.MapID, _protoMan.Index<WeatherPrototype>("Storm"), null);

                // Swap clothing if the kit defines ascension gear
                if (comp.AscensionGear != null)
                    ApplyAscensionGear(user, comp.AscensionGear.Value);

                // Make station announcement from Central Command
                _chatSystem.DispatchStationAnnouncement(
                    station.Value,
                    Loc.GetString(comp.AscendanceAnnouncementKey),
                    sender: Loc.GetString("comms-console-announcement-title-centcom"),
                    playDefaultSound: false,
                    announcementSound: null,
                    colorOverride: Color.Red);

                _audio.PlayGlobal(comp.AscendanceSound, _stationSystem.GetInOwningStation(station.Value), true);
            }

            if (HasComp<SlasherIdolComponent>(user))
                _lovestruck.AnnounceAscension(user);
        }

        // Grant a soul for regenerate
        _regenerate.GrantSoul(user);

        // Popup for user only
        if (!specialUnlockHappened)
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success", ("target", target)), user, user, PopupType.LargeCaution);

        // Popup for victim only
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success-victim", ("user", user)), target, target, PopupType.LargeCaution);
        Dirty(user, comp);
    }

    /// <summary>
    /// Strips the slots covered by the ascension gear, then equips it.
    /// </summary>
    private void ApplyAscensionGear(EntityUid user, ProtoId<StartingGearPrototype> gearProto)
    {
        if (!_protoMan.TryIndex(gearProto, out var loadout))
            return;

        // Strip any slot the ascension gear will fill so items don't stack
        if (_inventory.TryGetSlots(user, out var slots))
            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(((IEquipmentLoadout) loadout).GetGear(slot.Name)))
                    continue;

                if (_inventory.TryGetSlotEntity(user, slot.Name, out var worn) && worn != null)
                {
                    _inventory.TryUnequip(user, slot.Name, silent: true, force: true);
                    QueueDel(worn.Value);
                }
            }

        _spawning.EquipStartingGear(user, loadout);
    }

    private void OnThrowHit(Entity<SlasherSoulStealMacheteBonusComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.SlashBonus <= 0f || TerminatingOrDeleted(args.Target))
            return;

        var dmgAdj = new DamageSpecifier();

        dmgAdj.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        _damageable.TryChangeDamage(args.Target, dmgAdj, true, origin: args.Component.Thrower);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherSoulStealComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.HasAscended)
                continue;

            if (_timing.CurTime < comp.NextLightFlicker)
                continue;

            FlickerLightsAround(uid, comp);

            comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;
        }
    }

    private void FlickerLightsAround(EntityUid slasher, SlasherSoulStealComponent comp)
    {
        var entities = _lookup.GetEntitiesInRange(slasher, comp.LightFlickerRadius).ToList();
        _random.Shuffle(entities);

        var flickerCounter = 0;
        foreach (var entity in entities)
        {
            if (!HasComp<PointLightComponent>(entity))
                continue;

            var handled = false;

            if (TryComp<PoweredLightComponent>(entity, out var lightComp)
                && _random.Prob(0.85f))
            {
                if (_random.Prob(0.2f) && _light.TryDestroyBulb(entity, lightComp))
                    handled = true;
                else
                {
                    var ev = new GhostBooEvent();
                    RaiseLocalEvent(entity, ev);
                    handled = ev.Handled;
                }
            }

            if (handled)
                flickerCounter++;

            if (flickerCounter >= comp.MaxLightsToFlicker)
                break;
        }
    }
}
