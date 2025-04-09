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
            if (!uid.HasValue || !_damageableQuery.Resolve(uid.Value, ref damageable, false))
                return null;

            if (damage.Empty)
                return damage;

            var before = new BeforeDamageChangedEvent(damage, origin, canBeCancelled, targetPart, heavyAttack); // Shitmed Change
            RaiseLocalEvent(uid.Value, ref before);

            if (before.Cancelled)
                return null;

            // For entities with a body, route damage through body parts and then sum it up
            if (_bodyQuery.HasComp(uid.Value))
            {
                var appliedDamage = ApplyDamageToBodyParts(uid.Value, damage, origin, ignoreResistances,
                    interruptsDoAfters, targetPart, partMultiplier);

                return appliedDamage;
            }
            // For entities without a body, apply damage directly
            return ApplyDamageToEntity(uid.Value, damage, ignoreResistances, interruptsDoAfters, origin, damageable);
        }

        /// <summary>
        /// Applies damage to an entity with body parts, targeting specific parts as needed.
        /// </summary>
        private DamageSpecifier? ApplyDamageToBodyParts(
            EntityUid uid,
            DamageSpecifier damage,
            EntityUid? origin,
            bool ignoreResistances,
            bool interruptsDoAfters,
            TargetBodyPart? targetPart,
            float partMultiplier)
        {
            // Apply universal modifiers
            damage = ApplyUniversalAllModifiers(damage);
            DamageSpecifier? totalAppliedDamage = null;

            // This cursed shitcode lets us know if the target part is a power of 2
            // therefore having multiple parts targeted.
            if (targetPart != null
                && targetPart != 0 && (targetPart & (targetPart - 1)) != 0)
            {
                // Extract only the body parts that are targeted in the bitmask
                var targetedBodyParts = new List<(EntityUid Id, BodyPartComponent Component)>();

                // Get only the primitive flags (powers of 2) - these are the actual individual body parts
                var primitiveFlags = Enum.GetValues<TargetBodyPart>()
                    .Where(flag => flag != 0 && (flag & (flag - 1)) == 0) // Power of 2 check
                    .ToList();

                foreach (var flag in primitiveFlags)
                {
                    // Check if this specific flag is set in our targetPart bitmask
                    if (targetPart.Value.HasFlag(flag))
                    {
                        var query = _body.ConvertTargetBodyPart(flag);
                        var parts = _body.GetBodyChildrenOfType(uid, query.Type,
                            symmetry: query.Symmetry).ToList();

                        if (parts.Count > 0)
                            targetedBodyParts.AddRange(parts);
                    }
                }

                // If we couldn't find any of the targeted parts, fall back to all body parts
                if (targetedBodyParts.Count == 0)
                    targetedBodyParts = _body.GetBodyChildren(uid).ToList();
                var bodyParts = _body.GetBodyChildren(uid).ToList();
                if (bodyParts.Count == 0)
                    return null;

                var damagePerPart = damage / bodyParts.Count;
                var appliedDamage = new DamageSpecifier();

                foreach (var (partId, _) in bodyParts)
                {
                    if (!_damageableQuery.TryComp(partId, out var partDamageable))
                        continue;

                    // Apply damage to this part
                    var partDamageResult = TryChangeDamage(partId, damagePerPart, ignoreResistances,
                        interruptsDoAfters, partDamageable, origin);

                    if (partDamageResult != null && !partDamageResult.Empty)
                    {
                        // Accumulate total damage
                        foreach (var (type, value) in partDamageResult.DamageDict)
                        {
                            if (appliedDamage.DamageDict.TryGetValue(type, out var existing))
                                appliedDamage.DamageDict[type] = existing + value;
                            else
                                appliedDamage.DamageDict[type] = value;
                        }
                    }
                }

                totalAppliedDamage = appliedDamage;
            }
            else
            {
                // Target a specific body part
                TargetBodyPart? target;

                if (targetPart != null)
                    target = _body.GetRandomBodyPart(uid, targetPart: targetPart.Value);
                else if (origin.HasValue && TryComp<TargetingComponent>(origin.Value, out var targeting))
                    target = _body.GetRandomBodyPart(uid, origin.Value, attackerComp: targeting);
                else
                    target = _body.GetRandomBodyPart(uid);

                var (partType, symmetry) = _body.ConvertTargetBodyPart(target);
                var possibleTargets = _body.GetBodyChildrenOfType(uid, partType, symmetry: symmetry).ToList();

                if (possibleTargets.Count == 0)
                    possibleTargets = _body.GetBodyChildren(uid).ToList();

                // No body parts at all?
                if (possibleTargets.Count == 0)
                    return null;

                var chosenTarget = _LETSGOGAMBLINGEXCLAMATIONMARKEXCLAMATIONMARK.PickAndTake(possibleTargets);

                if (!_damageableQuery.TryComp(chosenTarget.Id, out var partDamageable))
                    return null;

                // Apply part multiplier if needed
                var adjustedDamage = partMultiplier != 1.0f ? damage * partMultiplier : damage;

                var appliedDamage = TryChangeDamage(chosenTarget.Id, adjustedDamage, ignoreResistances,
                    interruptsDoAfters, partDamageable, origin);

                if (appliedDamage != null && !appliedDamage.Empty)
                    totalAppliedDamage = appliedDamage;
            }

            return ApplyDamageToEntity(uid, totalAppliedDamage, ignoreResistances: true, interruptsDoAfters: interruptsDoAfters, origin: origin);
        }

        /// <summary>
        /// Applies damage directly to an entity without routing through body parts.
        /// </summary>
        private DamageSpecifier? ApplyDamageToEntity(
            EntityUid uid,
            DamageSpecifier? damage,
            bool ignoreResistances,
            bool interruptsDoAfters,
            EntityUid? origin,
            DamageableComponent? damageable = null)
        {
            if (!Resolve(uid, ref damageable) || damage == null)
                return null;

            // Apply resistances
            if (!ignoreResistances)
            {
                if (damageable.DamageModifierSetId != null &&
                    _prototypeManager.TryIndex(damageable.DamageModifierSetId, out var modifierSet))
                {
                    damage = DamageSpecifier.ApplyModifierSet(damage, modifierSet);
                }

                if (TryComp(uid, out BodyPartComponent? bodyPart))
                {
                    TargetBodyPart? target = _body.GetTargetBodyPart((uid, bodyPart));
                    if (bodyPart.Body != null)
                    {
                        // First raise the event on the parent to apply any parent modifiers
                        var parentEv = new DamageModifyEvent(damage, origin, target);
                        RaiseLocalEvent(bodyPart.Body.Value, parentEv);
                        damage = parentEv.Damage;
                    }

                    // Then raise on the part itself for any part-specific modifiers
                    var ev = new DamageModifyEvent(damage, origin, target);
                    RaiseLocalEvent(uid, ev);
                    damage = ev.Damage;
                }
                else
                {
                    // Not a body part, just apply modifiers normally
                    var ev = new DamageModifyEvent(damage, origin);
                    RaiseLocalEvent(uid, ev);
                    damage = ev.Damage;
                }

                if (damage.Empty)
                {
                    return damage;
                }
            }

            damage = ApplyUniversalAllModifiers(damage);

            var delta = new DamageSpecifier();
            delta.DamageDict.EnsureCapacity(damage.DamageDict.Count);

            var dict = damageable.Damage.DamageDict;
            foreach (var (type, value) in damage.DamageDict)
            {
                if (!dict.TryGetValue(type, out var oldValue))
                    continue;

                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + value);
                if (newValue == oldValue)
                    continue;

                dict[type] = newValue;
                delta.DamageDict[type] = newValue - oldValue;
            }

            if (delta.DamageDict.Count > 0)
                DamageChanged(uid, damageable, delta, interruptsDoAfters, origin);

            return delta;
        }

        /// <summary>
        ///     Applies the two univeral "All" modifiers, if set.
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
            // invalid value
            if (newValue < 0)
                return;

            // If entity has a body, set damage on all body parts
            if (_bodyQuery.HasComp(uid))
            {
                foreach (var (part, _) in _body.GetBodyChildren(uid))
                {
                    if (!_damageableQuery.TryComp(part, out var partDamageable))
                        continue;

                    // I LOVE RECURSION!!!
                    SetAllDamage(part, partDamageable, newValue);
                }
            }

            foreach (var type in component.Damage.DamageDict.Keys)
                component.Damage.DamageDict[type] = newValue;

            // Update cached values
            component.Damage.GetDamagePerGroup(_prototypeManager, component.DamagePerGroup);
            component.TotalDamage = component.Damage.GetTotal();

            // Setting damage does not count as 'dealing' damage
            DamageChanged(uid, component, new DamageSpecifier());

            if (_woundableQuery.TryComp(uid, out var woundable))
            {
                _wounds.UpdateWoundableIntegrity(uid, woundable);

                // Create wounds if damage was applied
                if (newValue > 0 && woundable.AllowWounds)
                    foreach (var (type, value) in component.Damage.DamageDict)
                        if (!_wounds.TryContinueWound(uid, type, value, woundable))
                            _wounds.TryCreateWound(uid, type, value, GetDamageGroupByType(type), woundable);
            }
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
