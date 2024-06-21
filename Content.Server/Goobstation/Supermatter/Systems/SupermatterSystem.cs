using System.Linq;
using JetBrains.Annotations;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
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
using Content.Server.Explosion.Components;
using Content.Shared.Supermatter.Components;
using Content.Shared.Supermatter.Systems;

namespace Content.Server.Supermatter.Systems
{
    [UsedImplicitly]
    public sealed class SupermatterSystem : SharedSupermatterSystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly ExplosionSystem _explosion = default!;
        [Dependency] private readonly TransformSystem _xform = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly AmbientSoundSystem _ambient = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        public new DelamType DelamType;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SupermatterComponent, StartCollideEvent>(OnCollideEvent);
            SubscribeLocalEvent<SupermatterComponent, InteractHandEvent>(OnHandInteract);
            SubscribeLocalEvent<SupermatterComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<SupermatterComponent, ComponentRemove>(OnComponentRemove);
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
            var mixture = _atmosphere.GetContainingMixture(uid, true, true);
            mixture?.AdjustMoles(Gas.Oxygen, Atmospherics.OxygenMolesStandard);
            mixture?.AdjustMoles(Gas.Nitrogen, Atmospherics.NitrogenMolesStandard);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_gameTiming.IsFirstTimePredicted)
                return;

            foreach (var (supermatter, xplode, rads) in EntityManager
                         .EntityQuery<SupermatterComponent, ExplosiveComponent, RadiationSourceComponent>())
            {
                var mixture = _atmosphere.GetContainingMixture(supermatter.Owner, true, true);
                HandleOutput(supermatter.Owner, frameTime, supermatter, rads, mixture);
                HandleDamage(supermatter.Owner, frameTime, supermatter, xplode, mixture);
            }
        }

        /// <summary>
        /// Handle outputting based off enery, damage, gas mix and radiation
        /// </summary>
        private void HandleOutput(
            EntityUid uid,
            float frameTime,
            SupermatterComponent sMcomponent,
            RadiationSourceComponent radcomponent,
            GasMixture? mixture = null)
        {
            sMcomponent.AtmosUpdateAccumulator += frameTime;

            if (!(sMcomponent.AtmosUpdateAccumulator > sMcomponent.AtmosUpdateTimer) ||
                mixture is not { })
                return;

            sMcomponent.AtmosUpdateAccumulator -= sMcomponent.AtmosUpdateTimer;

            //Absorbed gas from surrounding area
            var absorbedGas = mixture.Remove(sMcomponent.GasEfficiency * mixture.TotalMoles);
            var absorbedTotalMoles = absorbedGas.TotalMoles;

            if (!(absorbedTotalMoles > 0f))
                return;

            var gasStorage = sMcomponent.GasStorage;
            var gasEffect = sMcomponent.GasDataFields;

            //Lets get the proportions of the gasses in the mix for scaling stuff later
            //They range between 0 and 1
            gasStorage = gasStorage.ToDictionary(
                gas => gas.Key,
                gas => Math.Clamp(absorbedGas.GetMoles(gas.Key) / absorbedTotalMoles, 0, 1)
            );

            //No less then zero, and no greater then one, we use this to do explosions
            //and heat to power transfer
            var gasmixPowerRatio = gasStorage.Sum(gas => gasStorage[gas.Key] * gasEffect[gas.Key].PowerMixRatio);

            //Minimum value of -10, maximum value of 23. Affects plasma and o2 output
            //and the output heat
            var dynamicHeatModifier = gasStorage.Sum(gas => gasStorage[gas.Key] * gasEffect[gas.Key].HeatPenalty);

            //Minimum value of -10, maximum value of 23. Effects plasma and o2 output
            // and the output heat
            var powerTransmissionBonus =
                gasStorage.Sum(gas => gasStorage[gas.Key] * gasEffect[gas.Key].TransmitModifier);

            var h2OBonus = 1 - gasStorage[Gas.WaterVapor] * 0.25f;

            gasmixPowerRatio = Math.Clamp(gasmixPowerRatio, 0, 1);
            dynamicHeatModifier = Math.Max(dynamicHeatModifier, 0.5f);
            powerTransmissionBonus *= h2OBonus;

            //Effects the damage heat does to the crystal
            sMcomponent.DynamicHeatResistance = 1f;

            //more moles of gases are harder to heat than fewer,
            //so let's scale heat damage around them
            sMcomponent.MoleHeatPenaltyThreshold =
                (float) Math.Max(absorbedTotalMoles * sMcomponent.MoleHeatPenalty, 0.25);

            //Ramps up or down in increments of 0.02 up to the proportion of co2
            //Given infinite time, powerloss_dynamic_scaling = co2comp
            //Some value between 0 and 1
            if (absorbedTotalMoles > sMcomponent.PowerlossInhibitionMoleThreshold &&
                gasStorage[Gas.CarbonDioxide] > sMcomponent.PowerlossInhibitionGasThreshold)
            {
                sMcomponent.PowerlossDynamicScaling =
                    Math.Clamp(
                        sMcomponent.PowerlossDynamicScaling + Math.Clamp(
                            gasStorage[Gas.CarbonDioxide] - sMcomponent.PowerlossDynamicScaling, -0.02f, 0.02f), 0f,
                        1f);
            }
            else
            {
                sMcomponent.PowerlossDynamicScaling = Math.Clamp(sMcomponent.PowerlossDynamicScaling - 0.05f, 0f, 1f);
            }

            //Ranges from 0 to 1(1-(value between 0 and 1 * ranges from 1 to 1.5(mol / 500)))
            //We take the mol count, and scale it to be our inhibitor
            var powerlossInhibitor =
                Math.Clamp(
                    1 - sMcomponent.PowerlossDynamicScaling *
                    Math.Clamp(absorbedTotalMoles / sMcomponent.PowerlossInhibitionMoleBoostThreshold, 1f, 1.5f),
                    0f, 1f);

            if (sMcomponent.MatterPower != 0) //We base our removed power off one 10th of the matter_power.
            {
                var removedMatter = Math.Max(sMcomponent.MatterPower / sMcomponent.MatterPowerConversion, 40);
                //Adds at least 40 power
                sMcomponent.Power = Math.Max(sMcomponent.Power + removedMatter, 0);
                //Removes at least 40 matter power
                sMcomponent.MatterPower = Math.Max(sMcomponent.MatterPower - removedMatter, 0);
            }

            //based on gas mix, makes the power more based on heat or less effected by heat
            var tempFactor = gasmixPowerRatio > 0.8 ? 50f : 30f;

            //if there is more pluox and n2 then anything else, we receive no power increase from heat
            sMcomponent.Power =
                Math.Max(
                    absorbedGas.Temperature * tempFactor / Atmospherics.T0C * gasmixPowerRatio + sMcomponent.Power,
                    0);

            //Rad Pulse Calculation
            radcomponent.Intensity = sMcomponent.Power * Math.Max(0, 1f + powerTransmissionBonus / 10f) * 0.003f;

            //Power * 0.55 * a value between 1 and 0.8
            var energy = sMcomponent.Power * sMcomponent.ReactionPowerModifier;

            //Keep in mind we are only adding this temperature to (efficiency)% of the one tile the rock
            //is on. An increase of 4*C @ 25% efficiency here results in an increase of 1*C / (#tilesincore) overall.
            //Power * 0.55 * (some value between 1.5 and 23) / 5

            absorbedGas.Temperature += energy * dynamicHeatModifier * sMcomponent.ThermalReleaseModifier;
            absorbedGas.Temperature = Math.Max(0,
                Math.Min(absorbedGas.Temperature, sMcomponent.HeatThreshold * dynamicHeatModifier));

            //Calculate how much gas to release
            //Varies based on power and gas content

            absorbedGas.AdjustMoles(Gas.Plasma,
                Math.Max(energy * dynamicHeatModifier * sMcomponent.PlasmaReleaseModifier, 0f));

            absorbedGas.AdjustMoles(Gas.Oxygen,
                Math.Max(
                    (energy + absorbedGas.Temperature * dynamicHeatModifier - Atmospherics.T0C) *
                    sMcomponent.OxygenReleaseEfficiencyModifier, 0f));

            _atmosphere.Merge(mixture, absorbedGas);

            var powerReduction = (float) Math.Pow(sMcomponent.Power / 500, 3);

            //After this point power is lowered
            //This wraps around to the begining of the function
            sMcomponent.Power = Math.Max(
                                    sMcomponent.Power - Math.Min(powerReduction * powerlossInhibitor,
                                        sMcomponent.Power * 0.83f * powerlossInhibitor), 0f);
        }

        /// <summary>
        /// Handles environmental damage and dispatching damage warning
        /// </summary>
        private void HandleDamage(
            EntityUid uid,
            float frameTime,
            SupermatterComponent? sMcomponent = null,
            ExplosiveComponent? xplode = null,
            GasMixture? mixture = null)
        {
            if (!Resolve(uid, ref sMcomponent, ref xplode))
            {
                return;
            }

            var xform = Transform(uid);
            var indices = _xform.GetGridOrMapTilePosition(uid, xform);

            sMcomponent.DamageUpdateAccumulator += frameTime;
            sMcomponent.YellAccumulator += frameTime;

            if (!(sMcomponent.DamageUpdateAccumulator > sMcomponent.DamageUpdateTimer))
                return;

            sMcomponent.DamageArchived = sMcomponent.Damage;
            //we're in space or there is no gas to process
            if (!xform.GridUid.HasValue || mixture is not { } || mixture.TotalMoles == 0f)
            {
                sMcomponent.Damage += Math.Max(sMcomponent.Power / 1000 * sMcomponent.DamageIncreaseMultiplier, 0.1f);
            }
            else
            {
                //Absorbed gas from surrounding area
                var absorbedGas = mixture.Remove(sMcomponent.GasEfficiency * mixture.TotalMoles);
                var absorbedTotalMoles = absorbedGas.TotalMoles;

                //Mols start to have a positive effect on damage after 350
                sMcomponent.Damage = (float) Math.Max(
                    sMcomponent.Damage +
                    Math.Max(
                        Math.Clamp(absorbedTotalMoles / 200, 0.5, 1) * absorbedGas.Temperature -
                        (Atmospherics.T0C + sMcomponent.HeatPenaltyThreshold) * sMcomponent.DynamicHeatResistance,
                        0) * sMcomponent.MoleHeatThreshold / 150 * sMcomponent.DamageIncreaseMultiplier, 0);

                //Power only starts affecting damage when it is above 5000
                sMcomponent.Damage =
                    Math.Max(
                        sMcomponent.Damage +
                        Math.Max(sMcomponent.Power - sMcomponent.PowerPenaltyThreshold, 0) / 500 *
                        sMcomponent.DamageIncreaseMultiplier, 0);

                //Molar count only starts affecting damage when it is above 1800
                sMcomponent.Damage =
                    Math.Max(
                        sMcomponent.Damage + Math.Max(absorbedTotalMoles - sMcomponent.MolePenaltyThreshold, 0) / 80 *
                        sMcomponent.DamageIncreaseMultiplier, 0);

                //There might be a way to integrate healing and hurting via heat
                //healing damage
                if (absorbedTotalMoles < sMcomponent.MolePenaltyThreshold)
                {
                    //Only has a net positive effect when the temp is below 313.15, heals up to 2 damage. Psycologists increase this temp min by up to 45
                    sMcomponent.Damage =
                        Math.Max(
                            sMcomponent.Damage +
                            Math.Min(absorbedGas.Temperature - (Atmospherics.T0C + sMcomponent.HeatPenaltyThreshold),
                                0) / 150, 0);
                }

                //if there are space tiles next to SM
                //TODO: change moles out for checking if adjacent tiles exist
                var enumerator = _atmosphere.GetAdjacentTileMixtures(xform.GridUid.Value, indices, false, false);
                while (enumerator.MoveNext(out var ind))
                {
                    if (ind.TotalMoles != 0)
                        continue;

                    var integrity = GetIntegrity(sMcomponent.Damage, sMcomponent.ExplosionPoint);

                    var factor = integrity switch
                    {
                        < 10 => 0.0005f,
                        < 25 => 0.0009f,
                        < 45 => 0.005f,
                        < 75 => 0.002f,
                        _    => 0f
                    };

                    sMcomponent.Damage += Math.Clamp(sMcomponent.Power * factor * sMcomponent.DamageIncreaseMultiplier,
                        0, sMcomponent.MaxSpaceExposureDamage);

                    break;
                }

                sMcomponent.Damage =
                    Math.Min(sMcomponent.DamageArchived + sMcomponent.DamageHardcap * sMcomponent.ExplosionPoint,
                        sMcomponent.Damage);
            }

            HandleSoundLoop(uid, sMcomponent);

            if (sMcomponent.Damage > sMcomponent.ExplosionPoint)
            {
                Delamination(uid, frameTime, sMcomponent, xplode, mixture);
                return;
            }

            if (sMcomponent.Damage > sMcomponent.WarningPoint)
            {
                var integrity = GetIntegrity(sMcomponent.Damage, sMcomponent.ExplosionPoint);
                if (sMcomponent.YellAccumulator >= sMcomponent.YellTimer)
                {
                    if (sMcomponent.Damage > sMcomponent.EmergencyPoint)
                    {
                        _chat.TrySendInGameICMessage(uid,
                            Loc.GetString("supermatter-danger-message", ("integrity", integrity.ToString("0.00"))),
                            InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
                    }
                    else if (sMcomponent.Damage >= sMcomponent.DamageArchived)
                    {
                        _chat.TrySendInGameICMessage(uid,
                            Loc.GetString("supermatter-warning-message", ("integrity", integrity.ToString("0.00"))),
                            InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
                    }
                    else
                    {
                        _chat.TrySendInGameICMessage(uid,
                            Loc.GetString("supermatter-safe-alert", ("integrity", integrity.ToString("0.00"))),
                            InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
                    }

                    sMcomponent.YellAccumulator = 0;
                }
            }

            sMcomponent.DamageUpdateAccumulator -= sMcomponent.DamageUpdateTimer;
        }

        private float GetIntegrity(float damage, float explosionPoint)
        {
            var integrity = damage / explosionPoint;
            integrity = (float) Math.Round(100 - integrity * 100, 2);
            integrity = integrity < 0 ? 0 : integrity;
            return integrity;
        }

        /// <summary>
        /// Runs the logic and timers for Delamination
        /// </summary>
        private void Delamination(
            EntityUid uid,
            float frameTime,
            SupermatterComponent sMcomponent,
            ExplosiveComponent xplode,
            GasMixture? mixture = null)
        {
            var xform = Transform(uid);

            //before we actually start counting down, check to see what delam type we're doing.
            if (!sMcomponent.FinalCountdown)
            {
                //if we're in atmos
                if (mixture is { })
                {
                    //Absorbed gas from surrounding area
                    var absorbedGas = mixture.Remove(sMcomponent.GasEfficiency * mixture.TotalMoles);
                    var absorbedTotalMoles = absorbedGas.TotalMoles;
                    //if the moles on the sm's tile are above MolePenaltyThreshold
                    if (absorbedTotalMoles >= sMcomponent.MolePenaltyThreshold)
                    {
                        DelamType = DelamType.Singulo;
                        _chat.TrySendInGameICMessage(uid, Loc.GetString("supermatter-delamination-overmass"),
                            InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
                    }
                }
                else
                {
                    DelamType = DelamType.Explosion;
                    _chat.TrySendInGameICMessage(uid, Loc.GetString("supermatter-delamination-default"),
                        InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
                }
            }

            sMcomponent.FinalCountdown = true;

            sMcomponent.DelamTimerAccumulator += frameTime;
            sMcomponent.SpeakAccumulator += frameTime;
            var roundSeconds = sMcomponent.FinalCountdownTime - (int) Math.Floor(sMcomponent.DelamTimerAccumulator);

            //we're more than 5 seconds from delam, only yell every 5 seconds.
            if (roundSeconds >= sMcomponent.YellDelam && sMcomponent.SpeakAccumulator >= sMcomponent.YellDelam)
            {
                sMcomponent.SpeakAccumulator -= sMcomponent.YellDelam;
                _chat.TrySendInGameICMessage(uid,
                    Loc.GetString("supermatter-seconds-before-delam", ("Seconds", roundSeconds)),
                    InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
            }
            //less than 5 seconds to delam, count every second.
            else if (roundSeconds < sMcomponent.YellDelam && sMcomponent.SpeakAccumulator >= 1)
            {
                sMcomponent.SpeakAccumulator -= 1;
                _chat.TrySendInGameICMessage(uid,
                    Loc.GetString("supermatter-seconds-before-delam", ("Seconds", roundSeconds)),
                    InGameICChatType.Speak, hideChat: false, checkRadioPrefix: true);
            }

            //TODO: make tesla(?) spawn at SupermatterComponent.PowerPenaltyThreshold and think up other delam types
            //times up, explode or make a singulo
            if (!(sMcomponent.DelamTimerAccumulator >= sMcomponent.FinalCountdownTime))
                return;

            if (DelamType == DelamType.Singulo)
            {
                //spawn a singulo :)
                EntityManager.SpawnEntity("Singularity", xform.Coordinates);
                sMcomponent.AudioStream = _audio.Stop(sMcomponent.AudioStream);
            }
            else
            {
                //explosion!!!!!
                _explosion.TriggerExplosive(
                    uid,
                    explosive: xplode,
                    totalIntensity: sMcomponent.TotalIntensity,
                    radius: sMcomponent.Radius,
                    user: uid
                );

                sMcomponent.AudioStream = _audio.Stop(sMcomponent.AudioStream);
                _ambient.SetAmbience(uid, false);
            }

            sMcomponent.FinalCountdown = false;
        }

        private void HandleSoundLoop(EntityUid uid, SupermatterComponent sMcomponent)
        {
            var isAggressive = sMcomponent.Damage > sMcomponent.WarningPoint;
            var isDelamming = sMcomponent.Damage > sMcomponent.ExplosionPoint;

            if (!isAggressive && !isDelamming)
            {
                sMcomponent.AudioStream = _audio.Stop(sMcomponent.AudioStream);
                return;
            }

            var smSound = isDelamming ? SuperMatterSound.Delam : SuperMatterSound.Aggressive;

            if (sMcomponent.SmSound == smSound)
                return;

            sMcomponent.AudioStream = _audio.Stop(sMcomponent.AudioStream);
            sMcomponent.SmSound = smSound;
        }
        private void OnCollideEvent(EntityUid uid, SupermatterComponent supermatter, ref StartCollideEvent args)
        {
            var target = args.OtherEntity;
            if (args.OtherBody.BodyType == BodyType.Static
                || HasComp<SupermatterImmuneComponent>(target)
                || _container.IsEntityInContainer(uid))
                return;

            if (EntityManager.TryGetComponent<SupermatterFoodComponent>(target, out var supermatterFood))
                supermatter.Power += supermatterFood.Energy;
            else if (EntityManager.TryGetComponent<ProjectileComponent>(target, out var projectile))
                supermatter.Power += (float) projectile.Damage.GetTotal();
            else
                supermatter.Power++;

            supermatter.MatterPower += EntityManager.HasComponent<MobStateComponent>(target) ? 200 : 0;
            if (!EntityManager.HasComponent<ProjectileComponent>(target))
            {
                EntityManager.SpawnEntity("Ash", Transform(target).Coordinates);
                _audio.PlayPvs(supermatter.DustSound, uid);
            }

            EntityManager.QueueDeleteEntity(target);
        }

        private void OnHandInteract(EntityUid uid, SupermatterComponent supermatter, InteractHandEvent args)
        {
            var target = args.User;
            if (HasComp<SupermatterImmuneComponent>(target))
                return;
            supermatter.MatterPower += 200;
            EntityManager.SpawnEntity("Ash", Transform(target).Coordinates);
            _audio.PlayPvs(supermatter.DustSound, uid);
            EntityManager.QueueDeleteEntity(target);
        }
    }
}
