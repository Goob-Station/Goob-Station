using System.Linq;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Content.Shared.Atmos;
using Content.Shared.Interaction;
using Content.Shared.Projectiles;
using Content.Shared.Tag;
using Content.Shared.Mobs.Components;
using Content.Shared.Radiation.Components;
using Content.Server.Audio;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Supermatter.Components;
using Content.Shared.Supermatter.Systems;
using Content.Server.Lightning;
using Content.Server.AlertLevel;
using Content.Server.Station.Systems;
using System.Text;
using Content.Server.Kitchen.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Server.DoAfter;
using Content.Server.Popups;

namespace Content.Server.Supermatter.Systems;

public sealed class SupermatterSystem : SharedSupermatterSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly AlertLevelSystem _alert = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private DelamType _delamType = DelamType.Explosion;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SupermatterComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<SupermatterComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<SupermatterComponent, StartCollideEvent>(OnCollideEvent);
        SubscribeLocalEvent<SupermatterComponent, InteractHandEvent>(OnHandInteract);
        SubscribeLocalEvent<SupermatterComponent, InteractUsingEvent>(OnItemInteract);
        SubscribeLocalEvent<SupermatterComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<SupermatterComponent, SupermatterDoAfterEvent>(OnGetSliver);
    }

    private void OnComponentRemove(EntityUid uid, SupermatterComponent component, ComponentRemove args)
    {
        // turn off any ambient if component is removed (ex. entity deleted)
        _ambient.SetAmbience(uid, false);
        component.AudioStream = _audio.Stop(component.AudioStream);
    }

    private void OnMapInit(EntityUid uid, SupermatterComponent component, MapInitEvent args)
    {
        // Set the Sound
        _ambient.SetAmbience(uid, true);

        //Add Air to the initialized SM in the Map so it doesnt delam on default
        var mix = _atmosphere.GetContainingMixture(uid, true, true);
        mix?.AdjustMoles(Gas.Oxygen, Atmospherics.OxygenMolesStandard);
        mix?.AdjustMoles(Gas.Nitrogen, Atmospherics.NitrogenMolesStandard);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        foreach (var sm in EntityManager.EntityQuery<SupermatterComponent>())
        {
            if (!sm.Activated)
                continue;

            var uid = sm.Owner;
            sm.UpdateAccumulator += frameTime;

            if (sm.UpdateAccumulator >= sm.UpdateTimer)
            {
                sm.UpdateAccumulator -= sm.UpdateTimer;
                Cycle(uid, sm);
            }
        }
    }

    public void Cycle(EntityUid uid, SupermatterComponent sm)
    {
        sm.ZapAccumulator++;
        sm.YellAccumulator++;

        ProcessAtmos(uid, sm);
        HandleDamage(uid, sm);

        if (sm.Damage >= sm.DelaminationPoint || sm.Delamming)
            HandleDelamination(uid, sm);

        HandleSoundLoop(uid, sm);

        if (sm.ZapAccumulator >= sm.ZapTimer)
        {
            sm.ZapAccumulator -= sm.ZapTimer;
            SupermatterZap(uid, sm);
        }

        if (sm.YellAccumulator >= sm.YellTimer)
        {
            sm.YellAccumulator -= sm.YellTimer;
            HandleAnnouncements(uid, sm);
        }
    }

    #region Processing

    /// <summary>
    ///     Handle power and radiation output depending on atmospheric things.
    /// </summary>
    private void ProcessAtmos(EntityUid uid, SupermatterComponent sm)
    {
        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        if (mix is not { })
            return;

        var absorbedGas = mix.Remove(sm.GasEfficiency * mix.TotalMoles);
        var moles = absorbedGas.TotalMoles;

        if (!(moles > 0f))
            return;

        var gases = sm.GasStorage;
        var facts = sm.GasDataFields;

        //Lets get the proportions of the gasses in the mix for scaling stuff later
        //They range between 0 and 1
        gases = gases.ToDictionary(
            gas => gas.Key,
            gas => Math.Clamp(absorbedGas.GetMoles(gas.Key) / moles, 0, 1)
        );

        //No less then zero, and no greater then one, we use this to do explosions and heat to power transfer.
        var powerRatio = gases.Sum(gas => gases[gas.Key] * facts[gas.Key].PowerMixRatio);

        // Minimum value of -10, maximum value of 23. Affects plasma, o2 and heat output.
        var heatModifier = gases.Sum(gas => gases[gas.Key] * facts[gas.Key].HeatPenalty);

        // Minimum value of -10, maximum value of 23. Affects plasma, o2 and heat output.
        var transmissionBonus = gases.Sum(gas => gases[gas.Key] * facts[gas.Key].TransmitModifier);

        var h2OBonus = 1 - gases[Gas.WaterVapor] * 0.25f;

        powerRatio = Math.Clamp(powerRatio, 0, 1);
        heatModifier = Math.Max(heatModifier, 0.5f);
        transmissionBonus *= h2OBonus;

        // Effects the damage heat does to the crystal
        sm.DynamicHeatResistance = 1f;

        // more moles of gases are harder to heat than fewer,
        // so let's scale heat damage around them
        sm.MoleHeatPenaltyThreshold = (float) Math.Max(moles * sm.MoleHeatPenalty, 0.25);

        // Ramps up or down in increments of 0.02 up to the proportion of co2
        // Given infinite time, powerloss_dynamic_scaling = co2comp
        // Some value between 0 and 1
        if (moles > sm.PowerlossInhibitionMoleThreshold && gases[Gas.CarbonDioxide] > sm.PowerlossInhibitionGasThreshold)
        {
            var co2powerloss = Math.Clamp(gases[Gas.CarbonDioxide] - sm.PowerlossDynamicScaling, -0.02f, 0.02f);
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling + co2powerloss, 0f, 1f);
        }
        else
        {
            sm.PowerlossDynamicScaling = Math.Clamp(sm.PowerlossDynamicScaling - 0.05f, 0f, 1f);
        }

        // Ranges from 0 to 1(1-(value between 0 and 1 * ranges from 1 to 1.5(mol / 500)))
        // We take the mol count, and scale it to be our inhibitor
        var powerlossInhibitor =
            Math.Clamp(
                1 - sm.PowerlossDynamicScaling *
                Math.Clamp(moles / sm.PowerlossInhibitionMoleBoostThreshold, 1f, 1.5f),
                0f, 1f);

        if (sm.MatterPower != 0) //We base our removed power off one 10th of the matter_power.
        {
            var removedMatter = Math.Max(sm.MatterPower / sm.MatterPowerConversion, 40);
            //Adds at least 40 power
            sm.Power = Math.Max(sm.Power + removedMatter, 0);
            //Removes at least 40 matter power
            sm.MatterPower = Math.Max(sm.MatterPower - removedMatter, 0);
        }

        //based on gas mix, makes the power more based on heat or less effected by heat
        var tempFactor = powerRatio > 0.8 ? 50f : 30f;

        //if there is more pluox and n2 then anything else, we receive no power increase from heat
        sm.Power = Math.Max(absorbedGas.Temperature * tempFactor / Atmospherics.T0C * powerRatio + sm.Power, 0);

        //Radiate stuff
        if (TryComp<RadiationSourceComponent>(uid, out var rad))
            rad.Intensity = sm.Power * Math.Max(0, 1f + transmissionBonus / 10f) * 0.003f;

        //Power * 0.55 * a value between 1 and 0.8
        var energy = sm.Power * sm.ReactionPowerModifier;

        // Keep in mind we are only adding this temperature to (efficiency)% of the one tile the rock
        // is on. An increase of 4*C @ 25% efficiency here results in an increase of 1*C / (#tilesincore) overall.
        // Power * 0.55 * (some value between 1.5 and 23) / 5
        absorbedGas.Temperature += energy * heatModifier * sm.ThermalReleaseModifier;
        absorbedGas.Temperature = Math.Max(0,
            Math.Min(absorbedGas.Temperature, sm.HeatThreshold * heatModifier));

        // Assmos - /tg/ gases
        // Checks for carbon dioxide and spits out pluoxium if both CO2 and oxygen are present.
        if (mix.GetMoles(Gas.CarbonDioxide) > 0.01f)
        {
            var co2PP = absorbedGas.Pressure * ((mix.GetMoles(Gas.CarbonDioxide) / mix.TotalMoles) * 100);
            var co2Ratio = Math.Clamp(0.5f * (co2PP - (101.325f*0.01f)) / (co2PP + (101.325f*0.25f)), 0, 1);
            var consumedCO2 = absorbedGas.GetMoles(Gas.CarbonDioxide) * co2Ratio;
            consumedCO2 = Math.Min(consumedCO2, Math.Min(absorbedGas.GetMoles(Gas.Oxygen), absorbedGas.GetMoles(Gas.CarbonDioxide)));

            if (consumedCO2 > 0)
            {
                absorbedGas.AdjustMoles(Gas.CarbonDioxide, -consumedCO2);
                absorbedGas.AdjustMoles(Gas.Oxygen, -consumedCO2);
                absorbedGas.AdjustMoles(Gas.Pluoxium, consumedCO2);
            }
        }
        // Assmos - /tg/ gases end

        // Release the waste
        absorbedGas.AdjustMoles(Gas.Plasma, Math.Max(energy * heatModifier * sm.PlasmaReleaseModifier, 0f));
        absorbedGas.AdjustMoles(Gas.Oxygen, Math.Max((energy + absorbedGas.Temperature * heatModifier - Atmospherics.T0C) * sm.OxygenReleaseEfficiencyModifier, 0f));

        _atmosphere.Merge(mix, absorbedGas);

        var powerReduction = (float) Math.Pow(sm.Power / 500, 3);

        // After this point power is lowered
        // This wraps around to the begining of the function
        sm.Power = Math.Max(sm.Power - Math.Min(powerReduction * powerlossInhibitor, sm.Power * 0.83f * powerlossInhibitor), 0f);
    }

    /// <summary>
    ///     Shoot lightning bolts depensing on accumulated power.
    /// </summary>
    private void SupermatterZap(EntityUid uid, SupermatterComponent sm)
    {
        // Divide power by it's threshold to get a value from 0 to 1, then multiply by the amount of possible lightnings
        // Makes it pretty obvious that if SM is shooting out red lightnings something is wrong.
        // And if it shoots too weak lightnings it means that it's underfed. Feed the SM :godo:
        var zapPower = sm.Power / sm.PowerPenaltyThreshold * sm.LightningPrototypes.Length;
        var zapPowerNorm = (int) Math.Clamp(zapPower, 0, sm.LightningPrototypes.Length - 1);
        _lightning.ShootRandomLightnings(uid, 3.5f, sm.Power > sm.PowerPenaltyThreshold ? 3 : 1, sm.LightningPrototypes[zapPowerNorm]);
    }

    /// <summary>
    ///     Handles environmental damage.
    /// </summary>
    private void HandleDamage(EntityUid uid, SupermatterComponent sm)
    {
        var xform = Transform(uid);
        var indices = _xform.GetGridOrMapTilePosition(uid, xform);

        sm.DamageArchived = sm.Damage;

        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        // We're in space or there is no gas to process
        if (!xform.GridUid.HasValue || mix is not { } || mix.TotalMoles == 0f)
        {
            sm.Damage += Math.Max(sm.Power / 1000 * sm.DamageIncreaseMultiplier, 0.1f);
            return;
        }

        // Absorbed gas from surrounding area
        var absorbedGas = mix.Remove(sm.GasEfficiency * mix.TotalMoles);
        var moles = absorbedGas.TotalMoles;

        var totalDamage = 0f;

        var tempThreshold = Atmospherics.T0C + sm.HeatPenaltyThreshold;

        // Temperature start to have a positive effect on damage after 350
        var tempDamage = Math.Max(Math.Clamp(moles / 200f, .5f, 1f) * absorbedGas.Temperature - tempThreshold * sm.DynamicHeatResistance, 0f) * sm.MoleHeatThreshold / 150f * sm.DamageIncreaseMultiplier;
        totalDamage += tempDamage;

        // Power only starts affecting damage when it is above 5000
        var powerDamage = Math.Max(sm.Power - sm.PowerPenaltyThreshold, 0f) / 500f * sm.DamageIncreaseMultiplier;
        totalDamage += powerDamage;

        // Molar count only starts affecting damage when it is above 1800
        var moleDamage = Math.Max(moles - sm.MolePenaltyThreshold, 0) / 80 * sm.DamageIncreaseMultiplier;
        totalDamage += moleDamage;

        // Healing damage
        if (moles < sm.MolePenaltyThreshold)
        {
            // left there a very small float value so that it doesn't eventually divide by 0.
            var healHeatDamage = Math.Min(absorbedGas.Temperature - tempThreshold, 0.001f) / 150;
            totalDamage += healHeatDamage;
        }

        // Check for space tiles next to SM
        // TODO: change moles out for checking if adjacent tiles exist
        var enumerator = _atmosphere.GetAdjacentTileMixtures(xform.GridUid.Value, indices, false, false);
        while (enumerator.MoveNext(out var ind))
        {
            if (ind.TotalMoles != 0)
                continue;

            var integrity = GetIntegrity(sm);

            // this is some magic number shit
            var factor = integrity switch
            {
                < 10 => 0.0005f,
                < 25 => 0.0009f,
                < 45 => 0.005f,
                < 75 => 0.002f,
                _ => 0f
            };

            totalDamage += Math.Clamp(sm.Power * factor * sm.DamageIncreaseMultiplier, 0, sm.MaxSpaceExposureDamage);

            break;
        }

        sm.Damage = Math.Min(sm.DamageArchived + sm.DamageHardcap * sm.DelaminationPoint, totalDamage);
    }

    /// <summary>
    ///     Handles announcements.
    /// </summary>
    private void HandleAnnouncements(EntityUid uid, SupermatterComponent sm)
    {
        var message = string.Empty;
        var global = false;

        var integrity = GetIntegrity(sm).ToString("0.00");

        // Special cases
        if (sm.Damage < sm.DelaminationPoint && sm.Delamming)
        {
            message = Loc.GetString("supermatter-delam-cancel", ("integrity", integrity));
            sm.DelamAnnounced = false;
            global = true;
        }
        if (sm.Delamming && !sm.DelamAnnounced)
        {
            var sb = new StringBuilder();
            var loc = string.Empty;
            var alertLevel = "yellow";

            switch (_delamType)
            {
                case DelamType.Explosion:
                default:
                    loc = "supermatter-delam-explosion";
                    break;

                case DelamType.Singulo:
                    loc = "supermatter-delam-overmass";
                    alertLevel = "delta";
                    break;

                case DelamType.Tesla:
                    loc = "supermatter-delam-tesla";
                    alertLevel = "delta";
                    break;

                case DelamType.Cascade:
                    loc = "supermatter-delam-cascade";
                    alertLevel = "delta";
                    break;
            }

            var station = _station.GetOwningStation(uid);
            if (station != null)
                _alert.SetLevel((EntityUid) station, alertLevel, true, true, true, false);

            sb.AppendLine(Loc.GetString(loc));
            sb.AppendLine(Loc.GetString("supermatter-seconds-before-delam", ("seconds", sm.DelamTimer)));

            message = sb.ToString();
            global = true;
            sm.DelamAnnounced = true;

            SupermatterAnnouncement(uid, message, global);
            return;
        }

        // We are not taking consistent damage. Engis not needed.
        if (sm.Damage <= sm.DamageArchived)
            return;

        if (sm.Damage >= sm.WarningPoint)
        {
            message = Loc.GetString("supermatter-warning", ("integrity", integrity));
            if (sm.Damage >= sm.EmergencyPoint)
            {
                message = Loc.GetString("supermatter-emergency", ("integrity", integrity));
                global = true;
            }
        }
        SupermatterAnnouncement(uid, message, global);
    }

    /// <summary>
    ///     Help the SM announce something.
    /// </summary>
    /// <param name="global">If true, does the station announcement.</param>
    /// <param name="customSender">If true, sends the announcement from Central Command.</param>
    public void SupermatterAnnouncement(EntityUid uid, string message, bool global = false, string? customSender = null)
    {
        if (global)
        {
            var sender = customSender != null ? customSender : Loc.GetString("supermatter-announcer");
            _chat.DispatchStationAnnouncement(uid, message, sender, colorOverride: Color.Yellow);
            return;
        }
        _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
    }

    /// <summary>
    ///     Returns the integrity rounded to hundreds, e.g. 100.00%
    /// </summary>
    public float GetIntegrity(SupermatterComponent sm)
    {
        var integrity = sm.Damage / sm.DelaminationPoint;
        integrity = (float) Math.Round(100 - integrity * 100, 2);
        integrity = integrity < 0 ? 0 : integrity;
        return integrity;
    }

    /// <summary>
    ///     Decide on how to delaminate.
    /// </summary>
    public DelamType ChooseDelamType(EntityUid uid, SupermatterComponent sm)
    {
        var mix = _atmosphere.GetContainingMixture(uid, true, true);

        if (mix is { })
        {
            // var absorbedGas = mix.Remove(sm.GasEfficiency * mix.TotalMoles);
            var moles = mix.TotalMoles;

            if (moles >= sm.MolePenaltyThreshold)
                return DelamType.Singulo;
        }

        if (sm.Power >= sm.PowerPenaltyThreshold) 
            return DelamType.Tesla;

        // TODO: add resonance cascade when there's crazy conditions, or a destabilizing crystal :godo:

        return DelamType.Explosion;
    }

    /// <summary>
    ///     Handle the end of the station.
    /// </summary>
    private void HandleDelamination(EntityUid uid, SupermatterComponent sm)
    {
        var xform = Transform(uid);

        _delamType = ChooseDelamType(uid, sm);

        if (!sm.Delamming)
        {
            sm.Delamming = true;
            HandleAnnouncements(uid, sm);
        }
        if (sm.Damage < sm.DelaminationPoint && sm.Delamming)
        {
            sm.Delamming = false;
            HandleAnnouncements(uid, sm);
        }

        sm.DelamTimerAccumulator++;

        if (sm.DelamTimer > sm.DelamTimerAccumulator)
            return;

        switch (_delamType)
        {
            case DelamType.Explosion:
            default:
                _explosion.TriggerExplosive(uid);
                break;

            case DelamType.Singulo:
                Spawn(sm.SingularityPrototypeId, xform.Coordinates);
                break;

            case DelamType.Tesla:
                Spawn(sm.TeslaPrototypeId, xform.Coordinates);
                break;

            case DelamType.Cascade:
                Spawn(sm.SupermatterKudzuPrototypeId, xform.Coordinates);
                break;
        }
    }

    private void HandleSoundLoop(EntityUid uid, SupermatterComponent sm)
    {
        var isAggressive = sm.Damage > sm.WarningPoint;
        var isDelamming = sm.Damage > sm.DelaminationPoint;

        if (!isAggressive && !isDelamming)
        {
            sm.AudioStream = _audio.Stop(sm.AudioStream);
            return;
        }

        var smSound = isDelamming ? SuperMatterSound.Delam : SuperMatterSound.Aggressive;

        if (sm.SmSound == smSound)
            return;

        sm.AudioStream = _audio.Stop(sm.AudioStream);
        sm.SmSound = smSound;
    }

    #endregion


    #region Event Handlers

    private void OnCollideEvent(EntityUid uid, SupermatterComponent sm, ref StartCollideEvent args)
    {
        if (!sm.Activated)
            sm.Activated = true;

        var target = args.OtherEntity;
        if (args.OtherBody.BodyType == BodyType.Static
            || HasComp<SupermatterImmuneComponent>(target)
            || _container.IsEntityInContainer(uid))
            return;

        if (TryComp<SupermatterFoodComponent>(target, out var food))
            sm.Power += food.Energy;
        else if (TryComp<ProjectileComponent>(target, out var projectile))
            sm.Power += (float) projectile.Damage.GetTotal();
        else
            sm.Power++;

        sm.MatterPower += HasComp<MobStateComponent>(target) ? 200 : 0;

        if (!HasComp<ProjectileComponent>(target))
        {
            EntityManager.SpawnEntity("Ash", Transform(target).Coordinates);
            _audio.PlayPvs(sm.DustSound, uid);
        }

        EntityManager.QueueDeleteEntity(target);
    }

    private void OnHandInteract(EntityUid uid, SupermatterComponent sm, ref InteractHandEvent args)
    {
        if (!sm.Activated)
            sm.Activated = true;

        var target = args.User;

        if (HasComp<SupermatterImmuneComponent>(target))
            return;

        sm.MatterPower += 200;

        EntityManager.SpawnEntity("Ash", Transform(target).Coordinates);
        _audio.PlayPvs(sm.DustSound, uid);
        EntityManager.QueueDeleteEntity(target);
    }

    private void OnItemInteract(EntityUid uid, SupermatterComponent sm, ref InteractUsingEvent args)
    {
        if (!sm.Activated)
            sm.Activated = true;

        if (sm.SliverRemoved)
            return;

        if (!HasComp<SharpComponent>(args.Used))
            return;

        var dae = new DoAfterArgs(EntityManager, args.User, 30f, new SupermatterDoAfterEvent(), uid)
        {
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            NeedHand = true,
            RequireCanInteract = true,
        };

        _doAfter.TryStartDoAfter(dae);
    }

    private void OnGetSliver(EntityUid uid, SupermatterComponent sm, ref SupermatterDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        // your criminal actions will not go unnoticed
        sm.Damage += sm.DelaminationPoint / 10;
        sm.DamageArchived += sm.DelaminationPoint / 10;

        var integrity = GetIntegrity(sm).ToString("0.00");
        SupermatterAnnouncement(uid, Loc.GetString("supermatter-announcement-cc-tamper", ("integrity", integrity)), true, "Central Command");

        Spawn(sm.SliverPrototypeId, _transform.GetMapCoordinates(args.User));

        if (sm.DelamTimer > 30f)
            sm.DelamTimer -= 10f;
    }

    private void OnExamine(EntityUid uid, SupermatterComponent sm, ref ExaminedEvent args)
    {
        // get all close and personal to it
        if (args.IsInDetailsRange)
        {
            args.PushMarkup(Loc.GetString("supermatter-examine-integrity", ("integrity", GetIntegrity(sm).ToString("0.00"))));
        }
    }

    #endregion
}
