using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Chemistry;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Radiation.Events;
using Content.Shared.Rejuvenate;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

// Shitmed Change
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Random;

namespace Content.Shared.Damage
{
    public sealed class DamageableSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly INetManager _netMan = default!;
        [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly SharedChemistryGuideDataSystem _chemistryGuideData = default!;

        // Shitmed Dependencies
        [Dependency] private readonly SharedBodySystem _body = default!; // Shitmed Change
        [Dependency] private readonly IRobustRandom _random = default!; // Shitmed Change
        [Dependency] private readonly WoundSystem _wounds = default!;
        [Dependency] private readonly IRobustRandom _LETSGOGAMBLINGEXCLAMATIONMARKEXCLAMATIONMARK = default!;
        [Dependency] private readonly IComponentFactory _factory = default!;
        private EntityQuery<AppearanceComponent> _appearanceQuery;
        private EntityQuery<DamageableComponent> _damageableQuery;
        private EntityQuery<MindContainerComponent> _mindContainerQuery;

        // Shitmed Ent Queries
        private EntityQuery<BodyComponent> _bodyQuery;
        private EntityQuery<ConsciousnessComponent> _consciousnessQuery;
        private EntityQuery<WoundableComponent> _woundableQuery;

        public float UniversalAllDamageModifier { get; private set; } = 1f;
        public float UniversalAllHealModifier { get; private set; } = 1f;
        public float UniversalMeleeDamageModifier { get; private set; } = 1f;
        public float UniversalProjectileDamageModifier { get; private set; } = 1f;
        public float UniversalHitscanDamageModifier { get; private set; } = 1f;
        public float UniversalReagentDamageModifier { get; private set; } = 1f;
        public float UniversalReagentHealModifier { get; private set; } = 1f;
        public float UniversalExplosionDamageModifier { get; private set; } = 1f;
        public float UniversalThrownDamageModifier { get; private set; } = 1f;
        public float UniversalTopicalsHealModifier { get; private set; } = 1f;

        public override void Initialize()
        {
            SubscribeLocalEvent<DamageableComponent, ComponentInit>(DamageableInit);
            SubscribeLocalEvent<DamageableComponent, ComponentHandleState>(DamageableHandleState);
            SubscribeLocalEvent<DamageableComponent, ComponentGetState>(DamageableGetState);
            SubscribeLocalEvent<DamageableComponent, OnIrradiatedEvent>(OnIrradiated);
            SubscribeLocalEvent<DamageableComponent, RejuvenateEvent>(OnRejuvenate);

            _appearanceQuery = GetEntityQuery<AppearanceComponent>();
            _damageableQuery = GetEntityQuery<DamageableComponent>();
            _mindContainerQuery = GetEntityQuery<MindContainerComponent>();

            // Shitmed Queries
            _bodyQuery = GetEntityQuery<BodyComponent>();
            _consciousnessQuery = GetEntityQuery<ConsciousnessComponent>();
            _woundableQuery = GetEntityQuery<WoundableComponent>();

            // Damage modifier CVars are updated and stored here to be queried in other systems.
            // Note that certain modifiers requires reloading the guidebook.
            Subs.CVar(_config, CCVars.PlaytestAllDamageModifier, value =>
            {
                UniversalAllDamageModifier = value;
                _chemistryGuideData.ReloadAllReagentPrototypes();
            }, true);
            Subs.CVar(_config, CCVars.PlaytestAllHealModifier, value =>
            {
                UniversalAllHealModifier = value;
                _chemistryGuideData.ReloadAllReagentPrototypes();
            }, true);
            Subs.CVar(_config, CCVars.PlaytestProjectileDamageModifier, value => UniversalProjectileDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestMeleeDamageModifier, value => UniversalMeleeDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestProjectileDamageModifier, value => UniversalProjectileDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestHitscanDamageModifier, value => UniversalHitscanDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestReagentDamageModifier, value =>
            {
                UniversalReagentDamageModifier = value;
                _chemistryGuideData.ReloadAllReagentPrototypes();
            }, true);
            Subs.CVar(_config, CCVars.PlaytestReagentHealModifier, value =>
            {
                 UniversalReagentHealModifier = value;
                 _chemistryGuideData.ReloadAllReagentPrototypes();
            }, true);
            Subs.CVar(_config, CCVars.PlaytestExplosionDamageModifier, value => UniversalExplosionDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestThrownDamageModifier, value => UniversalThrownDamageModifier = value, true);
            Subs.CVar(_config, CCVars.PlaytestTopicalsHealModifier, value => UniversalTopicalsHealModifier = value, true);
        }

        /// <summary>
        ///     Initialize a damageable component
        /// </summary>
        private void DamageableInit(EntityUid uid, DamageableComponent component, ComponentInit _)
        {
            if (component.DamageContainerID != null &&
                _prototypeManager.TryIndex(component.DamageContainerID, out var damageContainerPrototype)) // Shitmed Change
            {
                // Initialize damage dictionary, using the types and groups from the damage
                // container prototype
                foreach (var type in damageContainerPrototype.SupportedTypes)
                {
                    component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
                }

                foreach (var groupId in damageContainerPrototype.SupportedGroups)
                {
                    var group = _prototypeManager.Index<DamageGroupPrototype>(groupId);
                    foreach (var type in group.DamageTypes)
                    {
                        component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
                    }
                }
            }
            else
            {
                // No DamageContainerPrototype was given. So we will allow the container to support all damage types
                foreach (var type in _prototypeManager.EnumeratePrototypes<DamageTypePrototype>())
                {
                    component.Damage.DamageDict.TryAdd(type.ID, FixedPoint2.Zero);
                }
            }

            component.Damage.GetDamagePerGroup(_prototypeManager, component.DamagePerGroup);
            component.TotalDamage = component.Damage.GetTotal();
        }

        /// <summary>
        ///     Directly sets the damage specifier of a damageable component.
        /// </summary>
        /// <remarks>
        ///     Useful for some unfriendly folk. Also ensures that cached values are updated and that a damage changed
        ///     event is raised.
        /// </remarks>
        public void SetDamage(EntityUid uid, DamageableComponent damageable, DamageSpecifier damage)
        {
            damageable.Damage = damage;
            DamageChanged(uid, damageable);
        }

        /// <summary>
        ///     If the damage in a DamageableComponent was changed, this function should be called.
        /// </summary>
        /// <remarks>
        ///     This updates cached damage information, flags the component as dirty, and raises a damage changed event.
        ///     The damage changed event is used by other systems, such as damage thresholds.
        /// </remarks>
        public void DamageChanged(EntityUid uid,
            DamageableComponent component,
            DamageSpecifier? damageDelta = null,
            bool interruptsDoAfters = true,
            EntityUid? origin = null)
        {
            component.Damage.GetDamagePerGroup(_prototypeManager, component.DamagePerGroup);
            component.TotalDamage = component.Damage.GetTotal();
            Dirty(uid, component);

            if (_appearanceQuery.TryGetComponent(uid, out var appearance) && damageDelta != null)
            {
                var data = new DamageVisualizerGroupData(component.DamagePerGroup.Keys.ToList());
                _appearance.SetData(uid, DamageVisualizerKeys.DamageUpdateGroups, data, appearance);
            }
            RaiseLocalEvent(uid, new DamageChangedEvent(component, damageDelta, interruptsDoAfters, origin));
        }

        /// <summary>
        ///     Applies damage specified via a <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     <see cref="DamageSpecifier"/> is effectively just a dictionary of damage types and damage values. This
        ///     function just applies the container's resistances (unless otherwise specified) and then changes the
        ///     stored damage data. Division of group damage into types is managed by <see cref="DamageSpecifier"/>.
        /// </remarks>
        /// <returns>
        ///     Returns a <see cref="DamageSpecifier"/> with information about the actual damage changes. This will be
        ///     null if the user had no applicable components that can take damage.
        /// </returns>
        public DamageSpecifier? TryChangeDamage(EntityUid? uid,
            DamageSpecifier damage,
            bool ignoreResistances = false,
            bool interruptsDoAfters = true,
            DamageableComponent? damageable = null,
            EntityUid? origin = null,
            bool canBeCancelled = false,
            float partMultiplier = 1.00f,
            TargetBodyPart? targetPart = null,
            bool heavyAttack = false)
        {
            // Shitmed Change Start
            /*if (damage.Empty)
                return damage;*/

            if (!uid.HasValue)
                return null;

            var before = new BeforeDamageChangedEvent(damage, origin, canBeCancelled, targetPart, heavyAttack); // Shitmed Change
            RaiseLocalEvent(uid.Value, ref before);

            if (before.Cancelled)
                return null;

            if (!_damageableQuery.Resolve(uid.Value, ref damageable, false))
                return null;

            if (_woundableQuery.TryComp(uid.Value, out var woundable))
            {
                var damageDict = new Dictionary<string, FixedPoint2>();
                foreach (var (type, severity) in damage.DamageDict)
                {
                    // some wounds like Asphyxiation and Bloodloss aren't supposed to be created.
                    if (!_prototypeManager.TryIndex<EntityPrototype>(type, out var woundPrototype)
                        || !woundPrototype.TryGetComponent<WoundComponent>(out _, _factory))
                        continue;

                    damageDict.Add(type, severity);
                }

                if (damageDict.Count == 0)
                    return null;

                foreach (var (damageType, severity) in damageDict)
                {
                    var actualDamage = severity * partMultiplier;
                    if (!_wounds.TryContinueWound(uid.Value, damageType, actualDamage, woundable))
                        _wounds.TryCreateWound(uid.Value, damageType, actualDamage, GetDamageGroupByType(damageType));
                }

                return null;
            }

            // I added a check for consciousness, because pain is directly hardwired into
            // woundables, pain and other stuff. things having a body and no consciousness comp
            // are impossible to kill... So yeah.
            if (_bodyQuery.HasComp(uid.Value) && _consciousnessQuery.HasComp(uid.Value))
            {
                var target = targetPart ?? _body.GetRandomBodyPart(uid.Value);
                if (origin.HasValue && TryComp<TargetingComponent>(origin.Value, out var targeting))
                    target = _body.GetRandomBodyPart(uid.Value, origin.Value, attackerComp: targeting);

                var (partType, symmetry) = _body.ConvertTargetBodyPart(target);
                var possibleTargets = _body.GetBodyChildrenOfType(uid.Value, partType, symmetry: symmetry).ToList();

                if (possibleTargets.Count == 0)
                    possibleTargets = _body.GetBodyChildren(uid.Value).ToList();

                // No body parts at all?
                if (possibleTargets.Count == 0)
                    return null;

                var chosenTarget = _LETSGOGAMBLINGEXCLAMATIONMARKEXCLAMATIONMARK.PickAndTake(possibleTargets);

                var damageDict = DamageSpecifierToWoundList(
                    uid.Value,
                    origin,
                    target!.Value, // god forgib me
                    damage,
                    damageable,
                    ignoreResistances,
                    partMultiplier);

                var beforeDamage = new BeforeDamageChangedEvent(damage, origin, canBeCancelled);
                RaiseLocalEvent(chosenTarget.Id, ref beforeDamage);

                if (beforeDamage.Cancelled)
                    return null;

                if (damageDict.Count == 0)
                    return null;

                switch (targetPart)
                {
                    // I kinda hate it, but it's needed to allow healing and all-around damage
                    case TargetBodyPart.All when damage.GetTotal() < 0:
                        {
                            if (!_wounds.TryGetWoundableWithMostDamage(
                                uid.Value,
                                out var damageWoundable,
                                GetDamageGroupByType (damageDict.FirstOrDefault().Key)))
                                return null; // No damage at all or a bug. anomalous

                            foreach (var (damageType, severity) in damageDict)
                            {
                                if (!_wounds.TryContinueWound(damageWoundable.Value.Owner, damageType, severity))
                                    _wounds.TryCreateWound(damageWoundable.Value.Owner, damageType, severity, GetDamageGroupByType(damageType));
                            }

                            return damage;
                        }

                    case TargetBodyPart.All when damage.GetTotal() > 0:
                        {
                            var pieces = _body.GetBodyChildren(uid).ToList();
                            var damagePerPiece = DamageSpecifierToWoundList(
                                uid.Value,
                                origin,
                                target.Value,
                                damage / pieces.Count,
                                damageable,
                                ignoreResistances,
                                partMultiplier);
                            var doneDamage = new DamageSpecifier();
                            doneDamage.DamageDict.EnsureCapacity(damage.DamageDict.Capacity);

                            foreach (var bodyPart in pieces)
                            {
                                var beforePartAll = new BeforeDamageChangedEvent(damage, origin, canBeCancelled);
                                RaiseLocalEvent(bodyPart.Id, ref beforePartAll);

                                if (beforePartAll.Cancelled)
                                    continue;

                                foreach (var (damageType, severity) in damagePerPiece)
                                {
                                    // Might cause some bullshit behaviour in the future. let's hope all goes right...
                                    if (doneDamage.DamageDict.TryGetValue(damageType, out var exSeverity))
                                    {
                                        doneDamage.DamageDict[damageType] = exSeverity + severity;
                                    }
                                    else
                                    {
                                        doneDamage.DamageDict[damageType] = severity;
                                    }

                                    if (!_wounds.TryContinueWound(bodyPart.Id, damageType, severity))
                                        _wounds.TryCreateWound(bodyPart.Id, damageType, severity, GetDamageGroupByType(damageType));
                                }
                            }

                            return doneDamage;
                        }

                    default:
                        {
                            var beforePart = new BeforeDamageChangedEvent(damage, origin, canBeCancelled);
                            RaiseLocalEvent(chosenTarget.Id, ref beforePart);

                            if (beforePart.Cancelled)
                                return null;

                            if (damageDict.Count == 0)
                                return null;

                            foreach (var (damageType, severity) in damageDict)
                            {
                                if (!_wounds.TryContinueWound(chosenTarget.Id, damageType, severity))
                                    _wounds.TryCreateWound(chosenTarget.Id, damageType, severity, GetDamageGroupByType(damageType));
                            }

                            return damage;
                        }
                }
            }

            // Shitmed Change End

            // Apply resistances
            if (!ignoreResistances)
            {
                if (damageable.DamageModifierSetId != null &&
                    _prototypeManager.TryIndex(damageable.DamageModifierSetId, out var modifierSet))
                {
                    // TODO DAMAGE PERFORMANCE
                    // use a local private field instead of creating a new dictionary here..
                    // TODO: We need to add a check to see if the given armor covers the targeted part (if any) to modify or not.
                    damage = DamageSpecifier.ApplyModifierSet(damage, modifierSet);
                }

                var ev = new DamageModifyEvent(damage, origin, targetPart); // Shitmed Change
                RaiseLocalEvent(uid.Value, ev);
                damage = ev.Damage;

                if (damage.Empty)
                {
                    return damage;
                }
            }

            damage = ApplyUniversalAllModifiers(damage);

            // TODO DAMAGE PERFORMANCE
            // Consider using a local private field instead of creating a new dictionary here.
            // Would need to check that nothing ever tries to cache the delta.
            var delta = new DamageSpecifier();
            delta.DamageDict.EnsureCapacity(damage.DamageDict.Count);

            var dict = damageable.Damage.DamageDict;
            foreach (var (type, value) in damage.DamageDict)
            {
                // CollectionsMarshal my beloved.
                if (!dict.TryGetValue(type, out var oldValue))
                    continue;

                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + value);
                if (newValue == oldValue)
                    continue;

                dict[type] = newValue;
                delta.DamageDict[type] = newValue - oldValue;
            }

            if (delta.DamageDict.Count > 0)
                DamageChanged(uid.Value, damageable, delta, interruptsDoAfters, origin);

            return delta;
        }

        /// <summary>
        /// Applies the two univeral "All" modifiers, if set.
        /// Individual damage source modifiers are set in their respective code.
        /// </summary>
        /// <param name="damage">The damage to be changed.</param>
        public DamageSpecifier ApplyUniversalAllModifiers(DamageSpecifier damage)
        {
            // Checks for changes first since they're unlikely in normal play.
            if (UniversalAllDamageModifier == 1f && UniversalAllHealModifier == 1f)
                return damage;

            foreach (var (key, value) in damage.DamageDict)
            {
                if (value == 0)
                    continue;

                if (value > 0)
                {
                    damage.DamageDict[key] *= UniversalAllDamageModifier;
                    continue;
                }

                if (value < 0)
                {
                    damage.DamageDict[key] *= UniversalAllHealModifier;
                }
            }

            return damage;
        }

        /// <summary>
        ///     Sets all damage types supported by a <see cref="DamageableComponent"/> to the specified value.
        /// </summary>
        /// <remarks>
        ///     Does nothing If the given damage value is negative.
        /// </remarks>
        public void SetAllDamage(EntityUid uid, DamageableComponent component, FixedPoint2 newValue)
        {
            if (newValue < 0)
            {
                // invalid value
                return;
            }

            foreach (var type in component.Damage.DamageDict.Keys)
            {
                component.Damage.DamageDict[type] = newValue;
            }

            // Setting damage does not count as 'dealing' damage, even if it is set to a larger value, so we pass an
            // empty damage delta.
            DamageChanged(uid, component, new DamageSpecifier());

            // Shitmed Change Start
            if (!HasComp<BodyComponent>(uid))
                return;

            foreach (var (part, _) in _body.GetBodyChildren(uid))
            {
                if (!TryComp(part, out WoundableComponent? woundable))
                    continue;

                foreach (var (type, severity) in component.Damage.DamageDict)
                    if (!_wounds.TryContinueWound(part, type, severity, woundable))
                        _wounds.TryCreateWound(part, type, severity, GetDamageGroupByType(type), woundable);
            }
            // Shitmed Change End
        }

        public Dictionary<string, FixedPoint2> DamageSpecifierToWoundList(
            EntityUid uid,
            EntityUid? origin,
            TargetBodyPart targetPart,
            DamageSpecifier damageSpecifier,
            DamageableComponent damageable,
            bool ignoreResistances = false,
            float partMultiplier = 1.00f)
        {
            var damageDict = new Dictionary<string, FixedPoint2>();

            damageSpecifier = ApplyUniversalAllModifiers(damageSpecifier);

            // some wounds like Asphyxiation and Bloodloss aren't supposed to be created.
            if (!ignoreResistances)
            {
                if (damageable.DamageModifierSetId != null &&
                    _prototypeManager.TryIndex(damageable.DamageModifierSetId, out var modifierSet))
                {
                    // lol bozo
                    var spec = new DamageSpecifier
                    {
                        DamageDict = damageSpecifier.DamageDict,
                    };

                    damageSpecifier = DamageSpecifier.ApplyModifierSet(spec, modifierSet);
                }

                var ev = new DamageModifyEvent(damageSpecifier, origin, targetPart);
                RaiseLocalEvent(uid, ev);
                damageSpecifier = ev.Damage;

                if (damageSpecifier.Empty)
                {
                    return damageDict;
                }
            }

            foreach (var (type, severity) in damageSpecifier.DamageDict)
            {
                if (!_prototypeManager.TryIndex<EntityPrototype>(type, out var woundPrototype)
                    || !woundPrototype.TryGetComponent<WoundComponent>(out _, _factory)
                    || severity <= 0)
                    continue;

                damageDict.Add(type, severity * partMultiplier);
            }

            return damageDict;
        }
        public void SetDamageModifierSetId(EntityUid uid, string? damageModifierSetId, DamageableComponent? comp = null)
        {
            if (!_damageableQuery.Resolve(uid, ref comp))
                return;

            comp.DamageModifierSetId = damageModifierSetId;
            Dirty(uid, comp);
        }

        private string GetDamageGroupByType(string id)
        {
            return (from @group in _prototypeManager.EnumeratePrototypes<DamageGroupPrototype>() where @group.DamageTypes.Contains(id) select @group.ID).FirstOrDefault()!;
        }
        private void DamageableGetState(EntityUid uid, DamageableComponent component, ref ComponentGetState args)
        {
            if (_netMan.IsServer)
            {
                args.State = new DamageableComponentState(component.Damage.DamageDict, component.DamageContainerID, component.DamageModifierSetId, component.HealthBarThreshold);
            }
            else
            {
                // avoid mispredicting damage on newly spawned entities.
                args.State = new DamageableComponentState(component.Damage.DamageDict.ShallowClone(), component.DamageContainerID, component.DamageModifierSetId, component.HealthBarThreshold);
            }
        }

        private void OnIrradiated(EntityUid uid, DamageableComponent component, OnIrradiatedEvent args)
        {
            var damageValue = FixedPoint2.New(args.TotalRads);

            // Radiation should really just be a damage group instead of a list of types.
            DamageSpecifier damage = new();
            foreach (var typeId in component.RadiationDamageTypeIDs)
            {
                damage.DamageDict.Add(typeId, damageValue);
            }

            TryChangeDamage(uid, damage, interruptsDoAfters: false);
        }

        private void OnRejuvenate(EntityUid uid, DamageableComponent component, RejuvenateEvent args)
        {
            if (HasComp<BodyComponent>(uid))
                return;

            TryComp<MobThresholdsComponent>(uid, out var thresholds);
            _mobThreshold.SetAllowRevives(uid, true, thresholds); // do this so that the state changes when we set the damage
            SetAllDamage(uid, component, 0);
            _mobThreshold.SetAllowRevives(uid, false, thresholds);
        }

        private void DamageableHandleState(EntityUid uid, DamageableComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not DamageableComponentState state)
            {
                return;
            }

            component.DamageContainerID = state.DamageContainerId;
            component.DamageModifierSetId = state.ModifierSetId;
            component.HealthBarThreshold = state.HealthBarThreshold;

            // Has the damage actually changed?
            DamageSpecifier newDamage = new() { DamageDict = new(state.DamageDict) };
            var delta = component.Damage - newDamage;
            delta.TrimZeros();

            if (delta.Empty)
                return;

            component.Damage = newDamage;
            DamageChanged(uid, component, delta);
        }
    }

    /// <summary>
    ///     Raised before damage is done, so stuff can cancel it if necessary.
    /// </summary>
    [ByRefEvent]
    public record struct BeforeDamageChangedEvent(
        DamageSpecifier Damage,
        EntityUid? Origin = null,
        bool CanBeCancelled = false, // Shitmed Change
        TargetBodyPart? TargetPart = null, // Shitmed Change
        bool Cancelled = false,
        bool HeavyAttack = false);

    /// <summary>
    ///     Shitmed Change: Raised on parts before damage is done so we can cancel the damage if they evade.
    /// </summary>
    [ByRefEvent]
    public record struct TryChangePartDamageEvent(
        DamageSpecifier Damage,
        EntityUid? Origin = null,
        TargetBodyPart? TargetPart = null,
        bool IgnoreResistances = false,
        bool CanSever = true,
        bool CanEvade = false,
        float PartMultiplier = 1.00f,
        bool Evaded = false,
        bool Cancelled = false);

    /// <summary>
    ///     Raised on an entity when damage is about to be dealt,
    ///     in case anything else needs to modify it other than the base
    ///     damageable component.
    ///
    ///     For example, armor.
    /// </summary>
    public sealed class DamageModifyEvent : EntityEventArgs, IInventoryRelayEvent
    {
        // Whenever locational damage is a thing, this should just check only that bit of armour.
        public SlotFlags TargetSlots { get; } = ~SlotFlags.POCKET;

        public readonly DamageSpecifier OriginalDamage;
        public DamageSpecifier Damage;
        public EntityUid? Origin;
        public readonly TargetBodyPart? TargetPart; // Shitmed Change

        public DamageModifyEvent(DamageSpecifier damage, EntityUid? origin = null, TargetBodyPart? targetPart = null) // Shitmed Change
        {
            OriginalDamage = damage;
            Damage = damage;
            Origin = origin;
            TargetPart = targetPart; // Shitmed Change
        }
    }

    public sealed class DamageChangedEvent : EntityEventArgs
    {
        /// <summary>
        ///     This is the component whose damage was changed.
        /// </summary>
        /// <remarks>
        ///     Given that nearly every component that cares about a change in the damage, needs to know the
        ///     current damage values, directly passing this information prevents a lot of duplicate
        ///     Owner.TryGetComponent() calls.
        /// </remarks>
        public readonly DamageableComponent Damageable;

        /// <summary>
        ///     The amount by which the damage has changed. If the damage was set directly to some number, this will be
        ///     null.
        /// </summary>
        public readonly DamageSpecifier? DamageDelta;

        /// <summary>
        ///     Was any of the damage change dealing damage, or was it all healing?
        /// </summary>
        public readonly bool DamageIncreased;

        /// <summary>
        ///     Does this event interrupt DoAfters?
        ///     Note: As provided in the constructor, this *does not* account for DamageIncreased.
        ///     As written into the event, this *does* account for DamageIncreased.
        /// </summary>
        public readonly bool InterruptsDoAfters;

        /// <summary>
        ///     Contains the entity which caused the change in damage, if any was responsible.
        /// </summary>
        public readonly EntityUid? Origin;
        public DamageChangedEvent(DamageableComponent damageable, DamageSpecifier? damageDelta, bool interruptsDoAfters, EntityUid? origin) // Shitmed Change
        {
            Damageable = damageable;
            DamageDelta = damageDelta;
            Origin = origin;
            if (DamageDelta == null)
                return;

            foreach (var damageChange in DamageDelta.DamageDict.Values)
            {
                if (damageChange > 0)
                {
                    DamageIncreased = true;
                    break;
                }
            }
            InterruptsDoAfters = interruptsDoAfters && DamageIncreased;
        }
    }
}
