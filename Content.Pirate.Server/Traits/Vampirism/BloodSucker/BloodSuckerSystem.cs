using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Verbs;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Pirate.Shared.Vampirism.Events;
using Content.Pirate.Server.Traits.Vampirism.Components;
using Content.Pirate.Server.Vampire.Systems;
using Content.Goobstation.Common.Religion;
//using Content.Shared.Cocoon;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Shared.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Systems;
using Content.Server.Popups;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Content.Server.Mind;
using Content.Shared.HealthExaminable;
using Content.Shared.Body.Organ;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;
using Content.Shared.Access.Systems;
using Content.Shared.Database;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Atmos.Rotting;
using Content.Server.Nutrition.EntitySystems;
using Content.Pirate.Shared.Vampire.Components;
using Content.Goobstation.Shared.Religion;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Robust.Shared.Audio;


namespace Content.Pirate.Server.Traits.Vampirism.Systems
{
    public sealed class BloodSuckerSystem : EntitySystem
    {
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly MindSystem _mind = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StomachSystem _stomachSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly VampireSystem _vampireSystem = default!;
        [Dependency] private readonly HungerSystem _hunger = default!;
        [Dependency] private readonly RottingSystem _rotting = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodSuckerComponent, GetVerbsEvent<InnateVerb>>(AddSuccVerb);
            SubscribeLocalEvent<BloodSuckedComponent, HealthBeingExaminedEvent>(OnHealthExamined);
            SubscribeLocalEvent<BloodSuckedComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<BloodSuckerComponent, BloodSuckDoAfterEvent>(OnDoAfter);
        }

        private void AddSuccVerb(EntityUid uid, BloodSuckerComponent component, GetVerbsEvent<InnateVerb> args)
        {

            var victim = args.Target;
            var ignoreClothes = false;

            if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || args.User == victim || !args.CanAccess)
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartSuccDoAfter(uid, victim, component, bloodstream, !ignoreClothes); // start doafter
                },
                Text = Loc.GetString("action-name-suck-blood"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnHealthExamined(EntityUid uid, BloodSuckedComponent component, HealthBeingExaminedEvent args)
        {
            args.Message.PushNewline();
            args.Message.AddMarkup(Loc.GetString("bloodsucked-health-examine", ("target", uid)));
        }

        private void OnDamageChanged(EntityUid uid, BloodSuckedComponent component, DamageChangedEvent args)
        {
            if (args.DamageIncreased)
                return;

            if (_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var brute) && args.Damageable.Damage.TryGetDamageInGroup(brute, out var bruteTotal)
                && _prototypeManager.TryIndex<DamageGroupPrototype>("Airloss", out var airloss) && args.Damageable.Damage.TryGetDamageInGroup(airloss, out var airlossTotal))
                if (bruteTotal == 0 && airlossTotal == 0)
                    RemComp<BloodSuckedComponent>(uid);
        }

        private void OnDoAfter(EntityUid uid, BloodSuckerComponent component, BloodSuckDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
                return;

            var success = TrySucc(uid, args.Args.Target.Value);
            args.Handled = success;
            if (success)
                args.Repeat = true;
        }
        public void StartSuccDoAfter(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodSuckerComponent = null, BloodstreamComponent? stream = null, bool doChecks = true)
        {
            if (!Resolve(bloodsucker, ref bloodSuckerComponent) || !Resolve(victim, ref stream))
                return;

            if (doChecks)
            {
                if (!_interactionSystem.InRangeUnobstructed(bloodsucker, victim))
                    return;

                // Block if the bloodsucker cant suck
                var ingestAttempt = new IngestionAttemptEvent(IngestionSystem.DefaultFlags);
                RaiseLocalEvent(bloodsucker, ref ingestAttempt);
                if (ingestAttempt.Cancelled)
                    return;
                // Block if the victim is wearing pressure-protecting headgear, because that would be only thing that really hard to bite through
                if (_inventorySystem.TryGetSlotEntity(victim, "head", out var head) && HasComp<PressureProtectionComponent>(head))
                {
                    _popups.PopupEntity(Loc.GetString("bloodsucker-fail-mouth-blocked", ("target", victim)), victim, bloodsucker, PopupType.Medium);
                    return;
                }
                if (_rotting.IsRotten(victim))
                {
                    _popups.PopupEntity(Loc.GetString("vampire-blooddrink-rotted"), victim, bloodsucker, PopupType.Medium);
                    return;
                }
                // is bloodsucker have mouth free?
                if (IsMouthBlocked(bloodsucker))
                {
                    _popups.PopupEntity(Loc.GetString("vampire-mouth-covered"), bloodsucker, bloodsucker);
                    return;
                }
                // Warn if the victim is another vampire
                if (HasComp<VampireComponent>(victim) || HasComp<VampirismComponent>(victim))
                {
                    _popups.PopupEntity(Loc.GetString("bloodsucker-victim-is-vampire"), victim, bloodsucker, PopupType.MediumCaution);
                }
            }
            // Antag vampires get an early heads-up that a faith-protected victim will give no power.
            // Drinking is still allowed, they just won't feed on the blood (see TryGainVampirePower).
            if (TryComp<VampireComponent>(bloodsucker, out var vamp)
                && !vamp.FullPower
                && _vampireSystem.IsProtectedByFaith(victim))
            {
                _popups.PopupEntity(Loc.GetString("vampire-target-protected-by-faith"), bloodsucker, bloodsucker, PopupType.MediumCaution);
            }

            if (stream.BloodReagent != "Blood")
                _popups.PopupEntity(Loc.GetString("bloodsucker-not-blood", ("target", victim)), victim, bloodsucker, PopupType.Medium);
            else if (_solutionSystem.PercentFull(victim) != 0)
                _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood", ("target", victim)), victim, bloodsucker, PopupType.Medium);
            else
                _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start", ("target", victim)), victim, bloodsucker, PopupType.Medium);

            _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start-victim", ("sucker", bloodsucker)), victim, victim, PopupType.LargeCaution);

            var args = new DoAfterArgs(EntityManager, bloodsucker, bloodSuckerComponent.Delay, new BloodSuckDoAfterEvent(), bloodsucker, target: victim)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                MovementThreshold = 0.01f,
                DistanceThreshold = 2f,
                NeedHand = false
            };

            _doAfter.TryStartDoAfter(args);
        }

        public bool TrySucc(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodsuckerComp = null)
        {
            // Is bloodsucker a bloodsucker?
            if (!Resolve(bloodsucker, ref bloodsuckerComp))
                return false;

            // Check for IPCs/silicons
            if (HasComp<SiliconComponent>(victim))
            {
                _popups.PopupEntity(Loc.GetString("vampire-drink-target-not-viable"), bloodsucker, bloodsucker, PopupType.MediumCaution);
                return false;
            }

            // Does victim have a bloodstream?
            if (!TryComp<BloodstreamComponent>(victim, out var bloodstream))
                return false;

            // No blood left, yikes.
            if (_bloodstreamSystem.GetBloodLevelPercentage((victim, bloodstream)) == 0.0f)
            {
                _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood", ("target", victim)), victim, bloodsucker, PopupType.Medium);
                return false;
            }

            // Does bloodsucker have a stomach?
            List<Entity<StomachComponent, OrganComponent>>? stomachList;
            if (!_bodySystem.TryGetBodyOrganEntityComps<StomachComponent>(bloodsucker, out stomachList)
                || stomachList == null || stomachList.Count == 0)
            {
                return false;
            }

            if (!_solutionSystem.TryGetSolution(stomachList[0].Comp2.Owner, StomachSystem.DefaultSolutionName, out var stomachSolution))
                return false;

            // Are we too full?

            if (_solutionSystem.PercentFull(bloodsucker) >= 1)
            {
                _popups.PopupEntity(Loc.GetString("drink-component-try-use-drink-had-enough"), bloodsucker, bloodsucker, PopupType.MediumCaution);
                return false;
            }

            _adminLogger.Add(LogType.MeleeHit, LogImpact.Medium, $"{ToPrettyString(bloodsucker):player} sucked blood from {ToPrettyString(victim):target}");

            // All good, succ time.
            _audio.PlayPvs("/Audio/Items/drink.ogg", bloodsucker);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked-victim", ("sucker", bloodsucker)), victim, victim, PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked", ("target", victim)), bloodsucker, bloodsucker, PopupType.Medium);
            EnsureComp<BloodSuckedComponent>(victim);

            // Make everything actually ingest.
            if (bloodstream.BloodSolution == null)
                return false;

            var temp = _solutionSystem.SplitSolution(bloodstream.BloodSolution.Value, bloodsuckerComp.UnitsToSucc);
            _stomachSystem.TryTransferSolution(stomachList[0].Comp2.Owner, temp, stomachList[0].Comp1);

            // Add a little pierce
            DamageSpecifier damage = new();
            damage.DamageDict.Add("Piercing", 1); // Slowly accumulate enough to gib after like half an hour

            _damageableSystem.TryChangeDamage(victim, damage, true, true);

            //I'm not porting the nocturine gland, this code is deprecated, and will be reworked at a later date.
            //if (bloodsuckerComp.InjectWhenSucc && _solutionSystem.TryGetInjectableSolution(victim, out var injectable))
            //{
            //    _solutionSystem.TryAddReagent(victim, injectable, bloodsuckerComp.InjectReagent, bloodsuckerComp.UnitsToInject, out var acceptedQuantity);
            //}

            // Antag vampires additionally feed on the blood to power their abilities.
            if (TryComp<VampireComponent>(bloodsucker, out var vamp))
                TryGainVampirePower(bloodsucker, vamp, victim, temp.Volume.Float());

            return true;
        }

        /// <summary>
        /// Antag-only follow up to a successful bite. Runs the additional checks the old
        /// VampireComponent drinking system had and, if they pass, feeds the vampire's power.
        /// On a failed check the vampire still drank normally but is warned and gains nothing.
        /// </summary>
        private void TryGainVampirePower(EntityUid uid, VampireComponent comp, EntityUid target, float drunkAmount)
        {
            if (drunkAmount <= 0f)
                return;

            // Drinking another vampire's blood grants no power.
            if (HasComp<VampireComponent>(target) || HasComp<VampirismComponent>(target))
            {
                _popups.PopupEntity(Loc.GetString("bloodsucker-victim-is-vampire"), uid, uid, PopupType.MediumCaution);
                return;
            }

            // Holy people give no power unless we are at full power.
            if (_vampireSystem.IsProtectedByFaith(target) && !comp.FullPower)
            {
                _popups.PopupEntity(Loc.GetString("vampire-target-protected-by-faith"), uid, uid, PopupType.MediumCaution);
                return;
            }

            // Only so much power can be wrung out of a single victim.
            var drunkFromTarget = comp.BloodDrunkFromTargets.GetValueOrDefault(target, 0);
            if (drunkFromTarget >= comp.MaxBloodPerTarget)
            {
                _popups.PopupEntity(Loc.GetString("vampire-drink-target-hard-max", ("amount", comp.MaxBloodPerTarget)), uid, uid, PopupType.MediumCaution);
                return;
            }

            // Silicons and (optionally) the dead are not a usable source of power.
            if (HasComp<SiliconComponent>(target)
                || !TryComp<MobStateComponent>(target, out var mobState)
                || (mobState.CurrentState == MobState.Dead && comp.DeadEfficiency == 0f))
            {
                _popups.PopupEntity(Loc.GetString("vampire-drink-target-not-viable"), uid, uid, PopupType.MediumCaution);
                return;
            }

            // How much of the drawn blood is actually usable as power.
            var targetIsHumanoid = HasComp<HumanoidAppearanceComponent>(target);
            var efficiency = targetIsHumanoid ? comp.HumanoidEfficiency : comp.NonHumanoidEfficiency;
            if (mobState.CurrentState == MobState.Dead)
                efficiency *= comp.DeadEfficiency;
            if (TryComp<PerishableComponent>(target, out var rot))
                efficiency *= GetRotEfficiency(comp, rot.Stage);

            if (efficiency <= 0f)
            {
                _popups.PopupEntity(Loc.GetString("vampire-drink-target-rot"), uid, uid, PopupType.MediumCaution);
                return;
            }

            var bloodGained = MathF.Min(drunkAmount * efficiency * 2, comp.MaxBloodPerTarget - drunkFromTarget);
            if (bloodGained <= 0f)
                return;

            // Damage, sound and other side effects of biting are handled by the shared TrySucc flow.
            // Here antags only convert the drawn blood into usable power.
            _vampireSystem.AddBlood(uid, comp, bloodGained, target, countTotalBlood: targetIsHumanoid);
        }

        private static float GetRotEfficiency(VampireComponent comp, int stage) => stage switch
        {
            0 => comp.Rot0Efficiency,
            1 => comp.Rot1Efficiency,
            2 => comp.Rot2Efficiency,
            3 => comp.Rot3Efficiency,
            _ => comp.Rot4Efficiency,
        };

        private bool IsMouthBlocked(EntityUid uid)
        {
            if (!HasComp<InventoryComponent>(uid))
                return false;

            var slots = new[] { "mask", "head" };
            foreach (var slot in slots)
                if (_inventorySystem.TryGetSlotEntity(uid, slot, out var ent) &&
                    TryComp<IngestionBlockerComponent>(ent.Value, out var blocker) &&
                    blocker.Enabled)

                    return true;

            return false;
        }
    }
}
