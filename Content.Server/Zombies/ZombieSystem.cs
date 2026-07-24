// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.NPC.Prototypes;
using Content.Server.Actions;
using Content.Server.Body.Systems;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Server.Emoting.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Server.Roles;
using Content.Shared.Anomaly.Components;
using Content.Shared.Armor;
using Content.Shared.Bed.Sleep;
using Content.Shared.Cloning.Events;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Roles;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Zombies;
using Content.Shared.Blocking; // Goobstation
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// Shitmed Change
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;

// Language Change
using Content.Server._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Components;
using Content.Shared._EinsteinEngines.Language.Events;

// Goob start - zombie cure
using Content.Shared.Body.Components;
using Content.Server.Temperature.Components;
using Content.Server.Body.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.AnimalHusbandry;
using Content.Goobstation.Common.Traits;
using Content.Shared.Interaction.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Hands.Components;
using Content.Shared.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Shared.CombatMode.Pacification;
using Content.Server.Speech.Components;
using Content.Goobstation.Shared.Sprinting;
using Content.Shared.Prying.Components;
using Content.Shared.Temperature.Components;
using Content.Server.Polymorph.Components;

// Goob end

namespace Content.Server.Zombies
{
    public sealed partial class ZombieSystem : SharedZombieSystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly ActionsSystem _actions = default!;
        [Dependency] private readonly AutoEmoteSystem _autoEmote = default!;
        [Dependency] private readonly EmoteOnDamageSystem _emoteOnDamage = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedRoleSystem _role = default!;
        [Dependency] private readonly LanguageSystem _language = default!;

        public readonly ProtoId<NpcFactionPrototype> Faction = "Zombie";

        public const SlotFlags ProtectiveSlots =
            SlotFlags.FEET |
            SlotFlags.HEAD |
            SlotFlags.EYES |
            SlotFlags.GLOVES |
            SlotFlags.MASK |
            SlotFlags.NECK |
            SlotFlags.INNERCLOTHING |
            SlotFlags.OUTERCLOTHING;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ZombieComponent, EmoteEvent>(OnEmote, before:
                new[] { typeof(VocalSystem), typeof(BodyEmotesSystem) });

            SubscribeLocalEvent<ZombieComponent, MeleeHitEvent>(OnMeleeHit);
            SubscribeLocalEvent<ZombieComponent, MobStateChangedEvent>(OnMobState);
            SubscribeLocalEvent<ZombieComponent, CloningEvent>(OnZombieCloning);
            SubscribeLocalEvent<ZombieComponent, TryingToSleepEvent>(OnSleepAttempt);
            SubscribeLocalEvent<ZombieComponent, GetCharactedDeadIcEvent>(OnGetCharacterDeadIC);
            SubscribeLocalEvent<ZombieComponent, GetCharacterUnrevivableIcEvent>(OnGetCharacterUnrevivableIC);
            SubscribeLocalEvent<ZombieComponent, MindAddedMessage>(OnMindAdded);
            SubscribeLocalEvent<ZombieComponent, MindRemovedMessage>(OnMindRemoved);

            SubscribeLocalEvent<PendingZombieComponent, MapInitEvent>(OnPendingMapInit);
            SubscribeLocalEvent<PendingZombieComponent, BeforeRemoveAnomalyOnDeathEvent>(OnBeforeRemoveAnomalyOnDeath);

            SubscribeLocalEvent<IncurableZombieComponent, MapInitEvent>(OnPendingMapInit);

            SubscribeLocalEvent<ZombifyOnDeathComponent, MobStateChangedEvent>(OnDamageChanged);

            // Goob Edit - Prevent Zombies Speaking/Understanding Languages
            SubscribeLocalEvent<ZombieComponent, DetermineEntityLanguagesEvent>(OnLanguageApply);
            SubscribeLocalEvent<ZombieComponent, ComponentShutdown>(OnShutdown);
            // more goob something something unzombify this shit needs cleanup
            SubscribeLocalEvent<ZombieComponent, EntityUnZombifiedEvent>(OnUnZombifyEvent);
        }

        private void OnBeforeRemoveAnomalyOnDeath(Entity<PendingZombieComponent> ent, ref BeforeRemoveAnomalyOnDeathEvent args)
        {
            // Pending zombies (e.g. infected non-zombies) do not remove their hosted anomaly on death.
            // Current zombies DO remove the anomaly on death.
            args.Cancelled = true;
        }

        private void OnPendingMapInit(EntityUid uid, IncurableZombieComponent component, MapInitEvent args)
        {
            _actions.AddAction(uid, ref component.Action, component.ZombifySelfActionPrototype);
            _faction.AddFaction(uid, Faction);

            if (HasComp<ZombieComponent>(uid) || HasComp<ZombieImmuneComponent>(uid))
                return;

            EnsureComp<PendingZombieComponent>(uid, out PendingZombieComponent pendingComp);

            pendingComp.GracePeriod = _random.Next(pendingComp.MinInitialInfectedGrace, pendingComp.MaxInitialInfectedGrace);
        }

        private void OnPendingMapInit(EntityUid uid, PendingZombieComponent component, MapInitEvent args)
        {
            if (_mobState.IsDead(uid))
            {
                ZombifyEntity(uid);
                return;
            }

            component.NextTick = _timing.CurTime + TimeSpan.FromSeconds(1f);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            var curTime = _timing.CurTime;

            // Hurt the living infected
            var query = EntityQueryEnumerator<PendingZombieComponent, DamageableComponent, MobStateComponent>();
            while (query.MoveNext(out var uid, out var comp, out var damage, out var mobState))
            {
                // Process only once per second
                if (comp.NextTick > curTime)
                    continue;

                comp.NextTick = curTime + TimeSpan.FromSeconds(1f);

                comp.GracePeriod -= TimeSpan.FromSeconds(1f);
                if (comp.GracePeriod > TimeSpan.Zero)
                    continue;

                if (_random.Prob(comp.InfectionWarningChance))
                    _popup.PopupEntity(Loc.GetString(_random.Pick(comp.InfectionWarnings)), uid, uid);

                var multiplier = _mobState.IsCritical(uid, mobState)
                    ? comp.CritDamageMultiplier
                    : 1f;

                _damageable.TryChangeDamage(uid,
                    comp.Damage * multiplier,
                    true,
                    false,
                    damage,
                    targetPart: TargetBodyPart.All, // Shitmed Change
                    splitDamage: SplitDamageBehavior.SplitEnsureAll); // Shitmed Change
            }

            // Heal the zombified
            var zombQuery = EntityQueryEnumerator<ZombieComponent, DamageableComponent, MobStateComponent>();
            while (zombQuery.MoveNext(out var uid, out var comp, out var damage, out var mobState))
            {
                // Process only once per second
                if (comp.NextTick + TimeSpan.FromSeconds(1) > curTime)
                    continue;

                comp.NextTick = curTime;

                if (_mobState.IsDead(uid, mobState))
                    continue;

                var multiplier = _mobState.IsCritical(uid, mobState)
                    ? comp.PassiveHealingCritMultiplier
                    : 1f;

                // Gradual healing for living zombies.
                _damageable.TryChangeDamage(uid,
                    comp.PassiveHealing * multiplier,
                    true,
                    false,
                    damage,
                    ignoreBlockers: true, // Shitmed Change
                    targetPart: TargetBodyPart.All, // Shitmed Change
                    splitDamage: SplitDamageBehavior.SplitEnsureAll); // Shitmed Change
            }
        }

        private void OnSleepAttempt(EntityUid uid, ZombieComponent component, ref TryingToSleepEvent args)
        {
            args.Cancelled = true;
        }

        private void OnGetCharacterDeadIC(EntityUid uid, ZombieComponent component, ref GetCharactedDeadIcEvent args)
        {
            args.Dead = true;
        }

        private void OnGetCharacterUnrevivableIC(EntityUid uid, ZombieComponent component, ref GetCharacterUnrevivableIcEvent args)
        {
            args.Unrevivable = true;
        }

        private void OnStartup(EntityUid uid, ZombieComponent component, ComponentStartup args)
        {
            if (component.EmoteSoundsId == null
                || TerminatingOrDeleted(uid)) // Goob Change
                return;

            // Goobstation Change Start
            var comp = EnsureComp<LanguageSpeakerComponent>(uid); // Ensure they can speak language before adding language.
            if (!string.IsNullOrEmpty(component.ForcedLanguage)) // Should never be false, but security either way.
                comp.CurrentLanguage = component.ForcedLanguage;
            _language.UpdateEntityLanguages(uid);
            // Goobstation Change End
        }

        private void OnEmote(EntityUid uid, ZombieComponent component, ref EmoteEvent args)
        {
            // always play zombie emote sounds and ignore others
            if (args.Handled)
                return;

            _protoManager.Resolve(component.EmoteSoundsId, out var sounds);

            args.Handled = _chat.TryPlayEmoteSound(uid, sounds, args.Emote);
        }

        private void OnMobState(EntityUid uid, ZombieComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Alive)
            {
                // Groaning when damaged
                EnsureComp<EmoteOnDamageComponent>(uid);
                _emoteOnDamage.AddEmote(uid, "Scream");

                // Random groaning
                EnsureComp<AutoEmoteComponent>(uid);
                _autoEmote.AddEmote(uid, "ZombieGroan");
            }
            else
            {
                // Stop groaning when damaged
                _emoteOnDamage.RemoveEmote(uid, "Scream");

                // Stop random groaning
                _autoEmote.RemoveEmote(uid, "ZombieGroan");
            }
        }

        private bool IsUserBlocking(BlockingUserComponent? component) // Goobstation
        {
            return (TryComp<BlockingComponent>(component?.BlockingItem, out var blockComp) && blockComp.IsBlocking);
        }

        private float GetZombieInfectionChance(EntityUid uid, ZombieComponent zombieComponent)
        {
            var chance = zombieComponent.BaseZombieInfectionChance;

            var armorEv = new CoefficientQueryEvent(ProtectiveSlots);
            RaiseLocalEvent(uid, armorEv);
            foreach (var resistanceEffectiveness in zombieComponent.ResistanceEffectiveness.DamageDict)
            {
                if (armorEv.DamageModifiers.Coefficients.TryGetValue(resistanceEffectiveness.Key, out var coefficient))
                {
                    // Scale the coefficient by the resistance effectiveness, very descriptive I know
                    // For example. With 30% slash resist (0.7 coeff), but only a 60% resistance effectiveness for slash,
                    // you'll end up with 1 - (0.3 * 0.6) = 0.82 coefficient, or a 18% resistance
                    var adjustedCoefficient = 1 - ((1 - coefficient) * resistanceEffectiveness.Value.Float());
                    chance *= adjustedCoefficient;
                }
            }

            var zombificationResistanceEv = new ZombificationResistanceQueryEvent(ProtectiveSlots);
            RaiseLocalEvent(uid, zombificationResistanceEv);
            chance *= zombificationResistanceEv.TotalCoefficient;

            return MathF.Max(chance, zombieComponent.MinZombieInfectionChance);
        }

        private void OnMeleeHit(Entity<ZombieComponent> entity, ref MeleeHitEvent args)
        {
            if (!args.IsHit)
                return;

            var cannotSpread = HasComp<NonSpreaderZombieComponent>(args.User);

            foreach (var uid in args.HitEntities)
            {
                if (args.User == uid)
                    continue;

                if (!TryComp<MobStateComponent>(uid, out var mobState))
                    continue;

                if (HasComp<ZombieComponent>(uid) || HasComp<IncurableZombieComponent>(uid))
                {
                    // Don't infect, don't deal damage, do not heal from bites, don't pass go!
                    args.Handled = true;
                    continue;
                }

                if (_mobState.IsAlive(uid, mobState))
                {
                    _damageable.TryChangeDamage(args.User, entity.Comp.HealingOnBite, true, false);

                    // If we cannot infect the living target, the zed will just heal itself.
                    if (HasComp<ZombieImmuneComponent>(uid) || cannotSpread ||
                        _random.Prob(GetZombieInfectionChance(uid, entity.Comp)))
                        continue;


                    if (TryComp<BlockingUserComponent>(entity, out var blockingUser) &&
                        IsUserBlocking(
                            blockingUser)) // Goobstation edit - prevents infection if user is actively blocking
                        return;

                    EnsureComp<PendingZombieComponent>(uid);
                    EnsureComp<ZombifyOnDeathComponent>(uid);
                }
                else
                {
                    if (HasComp<ZombieImmuneComponent>(uid) || cannotSpread)
                        continue;

                    // If the target is dead and can be infected, infect.
                    ZombifyEntity(uid);
                    args.Handled = true;
                }
            }
        }

        private void OverrideComp<T>(EntityUid target, EntityUid source) where T : IComponent // Goob, for below function
        {
            if (!TryComp(source, out T? toCopy))
            {
                RemComp<T>(target);
                return;
            }

            CopyComp<T>(source, target, toCopy);
        }

        /// <summary>
        ///     This is the function to call if you want to unzombify an entity.
        /// </summary>
        /// <param name="source">the entity having the ZombieComponent</param>
        /// <param name="target">the entity you want to unzombify (different from source in case of cloning, for example)</param>
        /// <param name="zombiecomp"></param>
        /// <remarks>
        ///     this currently only restore the skin/eye color from before zombified
        ///     TODO: completely rethink how zombies are done to allow reversal.
        /// </remarks>
        public bool UnZombify(EntityUid source, EntityUid target, ZombieComponent? zombiecomp)
        {
            if (!Resolve(source, ref zombiecomp))
                return false;

            foreach (var (layer, info) in zombiecomp.BeforeZombifiedCustomBaseLayers)
            {
                _humanoidAppearance.SetBaseLayerColor(target, layer, info.Color);
                _humanoidAppearance.SetBaseLayerId(target, layer, info.Id);
            }
            if (TryComp<HumanoidAppearanceComponent>(target, out var appcomp))
            {
                appcomp.EyeColor = zombiecomp.BeforeZombifiedEyeColor;
            }
            _humanoidAppearance.SetSkinColor(target, zombiecomp.BeforeZombifiedSkinColor, false);
            _bloodstream.ChangeBloodReagents(target, zombiecomp.BeforeZombifiedBloodReagents);

            return true;
        }

        private void OnZombieCloning(Entity<ZombieComponent> ent, ref CloningEvent args)
        {
            // Goob - trolled, just use cure
            //UnZombify(ent.Owner, args.CloneUid, ent.Comp);
        }

        // Make sure players that enter a zombie (for example via a ghost role or the mind swap spell) count as an antagonist.
        private void OnMindAdded(Entity<ZombieComponent> ent, ref MindAddedMessage args)
        {
            if (!_role.MindHasRole<ZombieRoleComponent>(args.Mind))
                _role.MindAddRole(args.Mind, "MindRoleZombie", mind: args.Mind.Comp);
        }

        // Remove the role when getting cloned, getting gibbed and borged, or leaving the body via any other method.
        private void OnMindRemoved(Entity<ZombieComponent> ent, ref MindRemovedMessage args)
        {
            _role.MindRemoveRole<ZombieRoleComponent>((args.Mind.Owner, args.Mind.Comp));
        }

        #region Goob Changes

        /// <summary>
        /// Tries to cure the entity of zombification by reverting its polymorph
        /// </summary>
        /// <param name="ent">Entity to cure.</param>
        /// <param name="currentUid">Entity to use now, differs if succeeded.</param>
        /// <returns></returns>
        private bool TryCureZombie(Entity<ZombieComponent> ent, out EntityUid currentUid)
        {
            if (TryComp(ent, out PolymorphedEntityComponent? comp)
                && _polymorph.Revert((ent, comp)) is { } uid)
                currentUid = uid;
            else
                currentUid = ent.Owner;
            return currentUid != ent.Owner;
        }

        private void OnUnZombifyEvent(Entity<ZombieComponent> ent, ref EntityUnZombifiedEvent args)
        {
            bool success = TryCureZombie(ent, out EntityUid currentUid);
            _popup.PopupEntity(
                Loc.GetString($"zombie-cure-{(success ? "success" : "failed")}"),
                currentUid,
                PopupType.Medium
            );

            // we want to make sure this is added to the reverted ent
            if (args.Inoculate)
                EnsureComp<ZombieImmuneComponent>(currentUid);
        }

        /// <summary>
        ///     This forces the languages to reset and apply only the current language for the entity based on Zombie Component.
        /// </summary>
        private void OnLanguageApply(Entity<ZombieComponent> ent, ref DetermineEntityLanguagesEvent args)
        {
            if (ent.Comp.LifeStage is ComponentLifeStage.Removing
                or ComponentLifeStage.Stopping
                or ComponentLifeStage.Stopped)
                return;

            // Clear the languages and then apply the forced language.
            args.SpokenLanguages.Clear();
            args.UnderstoodLanguages.Clear();
            args.SpokenLanguages.Add(ent.Comp.ForcedLanguage);
            args.UnderstoodLanguages.Add(ent.Comp.ForcedLanguage);
        }

        // When comp is removed, reset languages.
        private void OnShutdown(Entity<ZombieComponent> ent, ref ComponentShutdown args)
        {
            if (TerminatingOrDeleted(ent))
                return;

            _language.UpdateEntityLanguages(ent.Owner); // This uses ent.Owner because UpdateEntityLanguages checks for <LanguageSpeakerComponent>.
        }

        #endregion
    }
}
