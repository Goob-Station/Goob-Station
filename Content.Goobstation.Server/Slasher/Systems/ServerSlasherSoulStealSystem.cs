using System.Linq;
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

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server half of slashers soul steal system.
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
    [Dependency] private readonly SlasherPrestigeManager _prestige = default!;

    private static readonly ProtoId<WeatherPrototype> AscensionWeather = "Storm";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherSoulStealComponent, SlasherSoulStealDoAfterEvent>(OnSoulStealDoAfterComplete);
        SubscribeLocalEvent<SlasherSoulStealMacheteBonusComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnSoulStealDoAfterComplete(Entity<SlasherSoulStealComponent> ent, ref SlasherSoulStealDoAfterEvent ev)
    {
        if (ev.Cancelled || ev.Args.Target is not { } target)
            return;

        var user = ent.Owner;
        var comp = ent.Comp;

        _audio.PlayPvs(comp.SoulStealSound, target);

        var tileMix = _atmos.GetTileMixture(target, excite: true);
        tileMix?.AdjustMoles(Gas.Ammonia, comp.MolesAmmonia);

        var alive = _mobState.IsAlive(target);
        if (alive)
            comp.AliveSouls++;
        else
            comp.DeadSouls++;

        AdvanceAbsorbObjective(user);

        _devilContractSystem.AddRandomNegativeClauseSlasher(target);
        EnsureComp<SoullessComponent>(target);

        _soulSteal.ApplyArmorBonus(ent, alive ? comp.AliveArmorPercentPerSoul : comp.DeadArmorPercentPerSoul);
        _soulSteal.ApplyMacheteBonus(ent, alive ? comp.AliveBruteBonusPerSoul : comp.DeadBruteBonusPerSoul);

        var totalSouls = comp.AliveSouls + comp.DeadSouls;
        var unlockedPossession = TryUnlockPossession(ent, totalSouls);
        TryAscend(ent, totalSouls);

        _regenerate.GrantSoul(user);

        if (!unlockedPossession)
        {
            _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success", ("target", target)),
                user,
                user,
                PopupType.LargeCaution);
        }

        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-success-victim", ("user", user)),
            target,
            target,
            PopupType.LargeCaution);
        Dirty(ent);
    }

    private void AdvanceAbsorbObjective(EntityUid user)
    {
        if (!_mindSystem.TryGetMind(user, out _, out var mind))
            return;

        foreach (var objUid in mind.Objectives.ToList())
        {
            if (!TryComp<SlasherAbsorbSoulsConditionComponent>(objUid, out var absorbObj))
                continue;

            absorbObj.Absorbed += 1;
            Dirty(objUid, absorbObj);
            break;
        }
    }

    private bool TryUnlockPossession(Entity<SlasherSoulStealComponent> ent, int totalSouls)
    {
        if (ent.Comp.HasUnlockedPossession || totalSouls < ent.Comp.PossessionSoulThreshold)
            return false;

        ent.Comp.HasUnlockedPossession = true;
        EnsureComp<SlasherPossessionComponent>(ent);
        _popup.PopupEntity(Loc.GetString("slasher-soulsteal-unlock-possession"), ent, ent, PopupType.LargeCaution);
        return true;
    }

    private void TryAscend(Entity<SlasherSoulStealComponent> ent, int totalSouls)
    {
        var comp = ent.Comp;
        if (comp.HasAscended || totalSouls < comp.AscendanceSoulThreshold)
            return;

        comp.HasAscended = true;

        if (comp.AscensionId != null && TryComp<ActorComponent>(ent, out var actor))
            _prestige.GrantAscension(actor.PlayerSession.UserId, comp.AscensionId);

        comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;

        if (comp.AscensionGear != null)
            ApplyAscensionGear(ent, comp.AscensionGear.Value);

        if (_stationSystem.GetOwningStation(ent) is { } station)
            WarnStation(ent, station);

        RaiseLocalEvent(new SlasherAscendedEvent(ent));
    }

    private void WarnStation(Entity<SlasherSoulStealComponent> ent, EntityUid station)
    {
        var comp = ent.Comp;
        _alertLevel.SetLevel(station, "red", playSound: true, announce: false, force: true);
        _weather.SetWeather(Transform(ent).MapID, _protoMan.Index(AscensionWeather), null);

        _chatSystem.DispatchStationAnnouncement(
            station,
            Loc.GetString(comp.AscendanceAnnouncementKey),
            sender: Loc.GetString("comms-console-announcement-title-centcom"),
            playDefaultSound: false,
            announcementSound: null,
            colorOverride: Color.Red);

        _audio.PlayGlobal(comp.AscendanceSound, _stationSystem.GetInOwningStation(station), true);
    }

    private void ApplyAscensionGear(EntityUid user, ProtoId<StartingGearPrototype> gearProto)
    {
        if (!_protoMan.TryIndex(gearProto, out var loadout))
            return;

        if (_inventory.TryGetSlots(user, out var slots))
        {
            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(((IEquipmentLoadout) loadout).GetGear(slot.Name)))
                    continue;

                if (!_inventory.TryGetSlotEntity(user, slot.Name, out var worn))
                    continue;

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

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Slash", ent.Comp.SlashBonus);
        _damageable.TryChangeDamage(args.Target, damage, true, origin: args.Component.Thrower);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherSoulStealComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.HasAscended || _timing.CurTime < comp.NextLightFlicker)
                continue;

            FlickerLightsAround(uid, comp);
            comp.NextLightFlicker = _timing.CurTime + comp.LightFlickerInterval;
        }
    }

    private void FlickerLightsAround(EntityUid slasher, SlasherSoulStealComponent comp)
    {
        var entities = _lookup.GetEntitiesInRange(slasher, comp.LightFlickerRadius).ToList();
        _random.Shuffle(entities);

        var flickered = 0;
        foreach (var entity in entities)
        {
            if (!HasComp<PointLightComponent>(entity)
                || !TryComp<PoweredLightComponent>(entity, out var lightComp)
                || !_random.Prob(0.85f))
                continue;

            bool handled;
            if (_random.Prob(0.2f) && _light.TryDestroyBulb(entity, lightComp))
                handled = true;
            else
            {
                var ev = new GhostBooEvent();
                RaiseLocalEvent(entity, ev);
                handled = ev.Handled;
            }

            if (handled && ++flickered >= comp.MaxLightsToFlicker)
                break;
        }
    }
}
