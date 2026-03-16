using System.Diagnostics.CodeAnalysis;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public sealed partial class WoundSystem
{
    /// <summary>
    /// Retrieves all wounds associated with a specified entity.
    /// </summary>
    /// <param name="targetEntity">The UID of the target entity.</param>
    /// <param name="targetWoundable">Optional: The WoundableComponent of the target entity.</param>
    /// <returns>An enumerable collection of tuples containing EntityUid and WoundComponent pairs.</returns>
    public IEnumerable<Entity<WoundComponent>> GetAllWounds(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var (_, childWoundable) in GetAllWoundableChildren(targetEntity, targetWoundable))
        {
            if (childWoundable.Wounds == null)
                continue;

            foreach (var woundEntity in childWoundable.Wounds.ContainedEntities)
                yield return (woundEntity, Comp<WoundComponent>(woundEntity));

        }
    }

    /// <summary>
    /// Gets all woundable children of a specified woundable
    /// </summary>
    /// <param name="targetEntity">Owner of the woundable</param>
    /// <param name="targetWoundable"></param>
    /// <returns>Enumerable to the found children</returns>
    public IEnumerable<Entity<WoundableComponent>> GetAllWoundableChildren(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var childEntity in targetWoundable.ChildWoundables)
        {
            if (!TryComp(childEntity, out WoundableComponent? childWoundable))
                continue;

            foreach (var value in GetAllWoundableChildren(childEntity, childWoundable))
                yield return value;
        }

        yield return (targetEntity, targetWoundable);
    }

    /// <summary>
    /// Finds all children of a specified woundable that have a specific component
    /// </summary>
    /// <param name="targetEntity"></param>
    /// <param name="targetWoundable"></param>
    /// <typeparam name="T">the type of the component we want to find</typeparam>
    /// <returns>Enumerable to the found children</returns>
    public IEnumerable<Entity<WoundableComponent, T>> GetAllWoundableChildrenWithComp<T>(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null) where T: Component, new()
    {
        if (!Resolve(targetEntity, ref targetWoundable, false))
            yield break;

        foreach (var childEntity in targetWoundable.ChildWoundables)
        {
            if (!TryComp(childEntity, out WoundableComponent? childWoundable))
                continue;

            foreach (var value in GetAllWoundableChildrenWithComp<T>(childEntity, childWoundable))
                yield return value;
        }

        if (!TryComp(targetEntity, out T? foundComp))
            yield break;

        yield return (targetEntity, targetWoundable, foundComp);
    }

    /// <summary>
    /// Get the wounds on a woundable. Callers must not modify the wound container
    /// during iteration without materializing to a list first.
    /// </summary>
    public IEnumerable<Entity<WoundComponent>> GetWoundableWounds(EntityUid targetEntity,
        WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            yield break;

        foreach (var woundEntity in targetWoundable.Wounds.ContainedEntities)
            yield return (woundEntity, Comp<WoundComponent>(woundEntity));
    }

    /// <summary>
    /// Get the wounds present on a specific woundable, with a component you want
    /// </summary>
    /// <param name="targetEntity">Entity that owns the woundable</param>
    /// <param name="targetWoundable">Woundable component</param>
    /// <returns>An enumerable pointing to one of the found wounds, with the said component</returns>
    public IEnumerable<Entity<WoundComponent, T>> GetWoundableWoundsWithComp<T>(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null) where T : Component, new()
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null || targetWoundable.Wounds.Count == 0)
            yield break;

        foreach (var woundEntity in GetWoundableWounds(targetEntity, targetWoundable))
        {
            if (!TryComp<T>(woundEntity, out var foundComponent))
                continue;

            yield return (woundEntity, woundEntity, foundComponent);
        }
    }

    /// <summary>
    /// Returns you the sum of all wounds on this woundable
    /// </summary>
    /// <param name="targetEntity">The woundable uid</param>
    /// <param name="targetWoundable">The component</param>
    /// <param name="damageGroup">The damage group of said wounds</param>
    /// <param name="healable">Are the wounds supposed to be healable</param>
    /// <returns>The severity sum</returns>
    public FixedPoint2 GetWoundableSeverityPoint(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null,
        string? damageGroup = null,
        bool healable = false,
        bool ignoreBlockers = false)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            return FixedPoint2.Zero;

        var total = FixedPoint2.Zero;
        foreach (var wound in GetWoundableWounds(targetEntity, targetWoundable))
        {
            if (damageGroup != null && (string?) wound.Comp.DamageGroup != damageGroup)
                continue;

            if (healable && !CanHealWound(wound, wound.Comp, ignoreBlockers))
                continue;

            total += wound.Comp.WoundSeverityPoint;
        }

        return total;
    }

    /// <summary>
    /// Returns you the integrity damage the woundable has
    /// </summary>
    /// <param name="targetEntity">The woundable uid</param>
    /// <param name="targetWoundable">The component</param>
    /// <param name="damageGroup">The damage group of wounds that induced the damage</param>
    /// <param name="healable">Is the integrity damage healable</param>
    /// <returns>The integrity damage</returns>
    public FixedPoint2 GetWoundableIntegrityDamage(
        EntityUid targetEntity,
        WoundableComponent? targetWoundable = null,
        string? damageGroup = null,
        bool healable = false,
        bool ignoreBlockers = false)
    {
        if (!Resolve(targetEntity, ref targetWoundable, false)
            || targetWoundable.Wounds == null
            || targetWoundable.Wounds.Count == 0)
            return FixedPoint2.Zero;

        var total = FixedPoint2.Zero;
        foreach (var wound in GetWoundableWounds(targetEntity, targetWoundable))
        {
            if (damageGroup != null && (string?) wound.Comp.DamageGroup != damageGroup)
                continue;

            if (healable && !CanHealWound(wound, wound.Comp, ignoreBlockers))
                continue;

            total += wound.Comp.WoundIntegrityDamage;
        }

        return total;
    }

    /// <summary>
    /// Checks for wounds on an entity that have exceeded their MangleSeverity threshold
    /// </summary>
    public bool HasWoundsExceedingMangleSeverity(EntityUid targetEntity, WoundableComponent? targetWoundable = null)
    {
        if (!Resolve(targetEntity, ref targetWoundable))
            return false;

        foreach (var wound in GetWoundableWounds(targetEntity, targetWoundable))
        {
            if (wound.Comp.MangleSeverity != null && wound.Comp.WoundSeverity >= wound.Comp.MangleSeverity)
                return true;
        }

        return false;
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetWoundableStatesOnBody(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
            result[part] = WoundableSeverity.Severed;

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);

            if (!TryComp<WoundableComponent>(id, out var woundable))
                continue;

            result[target] = woundable.WoundableSeverity;
        }

        return result;
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetDamageableStatesOnBody(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
            result[part] = WoundableSeverity.Severed;

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);

            if (!TryComp<WoundableComponent>(id, out var woundable)
                || !TryComp<DamageableComponent>(id, out var damageable))
                continue;

            var nearestSeverity = WoundableSeverity.Severed;
            var damage = damageable.TotalDamage;

            foreach (var (severity, threshold) in woundable.SortedThresholds!)
            {
                if (damage <= 0)
                {
                    nearestSeverity = WoundableSeverity.Healthy;
                    break;
                }

                if (damage >= woundable.IntegrityCap)
                {
                    nearestSeverity = WoundableSeverity.Mangled;
                    break;
                }

                if (damage > woundable.IntegrityCap - threshold)
                    continue;

                nearestSeverity = severity;
                break;
            }

            result[target] = nearestSeverity;
        }

        return result;
    }

    public Dictionary<TargetBodyPart, WoundableSeverity> GetWoundableStatesOnBodyPainFeels(EntityUid body)
    {
        var result = new Dictionary<TargetBodyPart, WoundableSeverity>();

        foreach (var part in SharedTargetingSystem.GetValidParts())
        {
            result[part] = WoundableSeverity.Severed;
        }

        foreach (var (id, bodyPart) in _body.GetBodyChildren(body))
        {
            var target = _body.GetTargetBodyPart(bodyPart);

            if (!TryComp<WoundableComponent>(id, out var woundable) || !TryComp<NerveComponent>(id, out var nerve))
                continue;

            var damageFeeling = woundable.WoundableIntegrity * nerve.PainFeels;

            var nearestSeverity = woundable.WoundableSeverity;
            foreach (var (severity, value) in woundable.SortedThresholds!)
            {
                if (damageFeeling <= 0)
                {
                    nearestSeverity = WoundableSeverity.Mangled;
                    break;
                }

                if (damageFeeling >= woundable.IntegrityCap)
                {
                    nearestSeverity = WoundableSeverity.Healthy;
                    break;
                }

                if (damageFeeling < value)
                    continue;

                nearestSeverity = severity;
                break;
            }

            result[target] = nearestSeverity;
        }

        return result;
    }

    /// <summary>
    /// Gets the best body part to redirect damage to when the target limb is missing
    /// or has maxed out damage of a given type. Returns null if all parts are maxed.
    /// Priority: torso first, then remaining limbs.
    /// </summary>
    public EntityUid? GetDamageRedirectTarget(
        EntityUid body,
        EntityUid targetPart,
        string damageType,
        BodyComponent? bodyComp = null)
    {
        if (!Resolve(body, ref bodyComp))
            return null;

        if (TryComp<WoundableComponent>(targetPart, out var targetWoundable)
            && !TerminatingOrDeleted(targetPart)
            && targetWoundable.WoundableIntegrity > 0)
            return targetPart;

        EntityUid? fallback = null;

        foreach (var (partId, partComp) in _body.GetBodyChildren(body, bodyComp))
        {
            if (partId == targetPart)
                continue;

            if (!TryComp<WoundableComponent>(partId, out var woundable)
                || woundable.WoundableIntegrity <= 0)
                continue;

            if (partComp.PartType == BodyPartType.Chest)
                return partId;

            fallback ??= partId;
        }

        return fallback;
    }

    /// <summary>
    /// Check if this woundable is root
    /// </summary>
    /// <param name="woundableEntity">Owner of the woundable</param>
    /// <param name="woundable">woundable component</param>
    /// <returns>true if the woundable is the root of the hierarchy</returns>
    public bool IsWoundableRoot(EntityUid woundableEntity, WoundableComponent? woundable = null)
    {
        return Resolve(woundableEntity, ref woundable, false)
            && woundable.RootWoundable == woundableEntity;
    }

    /// <summary>
    /// Validates the wound prototype based on the given prototype ID.
    /// Checks if the specified prototype ID corresponds to a valid EntityPrototype in the collection,
    /// ensuring it contains the necessary WoundComponent.
    /// </summary>
    /// <param name="protoId">The prototype ID to be validated.</param>
    /// <returns>True if the wound prototype is valid, otherwise false.</returns>
    private bool IsWoundPrototypeValid(string protoId)
    {
        return _prototype.TryIndex<EntityPrototype>(protoId, out var woundPrototype)
               && woundPrototype.TryGetComponent<WoundComponent>(out _, _factory);
    }

    private void UpdateWoundableAppearance(EntityUid woundable)
    {
        if (!TryComp<WoundableComponent>(woundable, out var comp) || comp.Wounds == null)
            return;

        var woundList = new List<NetEntity>(comp.Wounds.Count);
        foreach (var woundEntity in comp.Wounds.ContainedEntities)
            woundList.Add(GetNetEntity(woundEntity));

        _appearance.SetData(woundable,
            WoundableVisualizerKeys.Wounds,
            new WoundVisualizerGroupData(woundList));
    }

    private bool AddWound(
        EntityUid target,
        EntityUid wound,
        FixedPoint2 woundSeverity,
        ProtoId<DamageGroupPrototype>? damageGroup,
        WoundableComponent? woundableComponent = null,
        WoundComponent? woundComponent = null)
    {
        if (!_net.IsServer
            || !Resolve(target, ref woundableComponent)
            || !Resolve(wound, ref woundComponent)
            || woundableComponent.Wounds == null
            || woundableComponent.Wounds.Contains(wound)
            || !_timing.IsFirstTimePredicted
            || !woundableComponent.AllowWounds)
            return false;

        _transform.SetParent(wound, target);
        woundComponent.HoldingWoundable = target;
        woundComponent.DamageGroup = damageGroup;

        if (!_container.Insert(wound, woundableComponent.Wounds))
            return false;

        SetWoundSeverity(wound, woundSeverity, woundComponent);

        Dirty(wound, woundComponent);
        Dirty(target, woundableComponent);

        return true;
    }

    private bool RemoveWound(EntityUid woundEntity, WoundComponent? wound = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return false;

        if (!Resolve(woundEntity, ref wound, false)
            || !TryComp(wound.HoldingWoundable, out WoundableComponent? woundable))
            return false;

        UpdateWoundableIntegrity(wound.HoldingWoundable, woundable);
        CheckWoundableSeverityThresholds(wound.HoldingWoundable, woundable);

        // We prevent removal if theres at least one wound holding traumas left.
        foreach (var trauma in _trauma.GetAllWoundTraumas(woundEntity))
        {
            if (Array.IndexOf(Traumas.Systems.TraumaSystem.TraumasBlockingHealing, trauma.Comp.TraumaType) >= 0)
                return false;
        }

        _container.Remove(woundEntity, woundable.Wounds!, false, true);

        return true;
    }

    private void InternalAddWoundableToParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent parentWoundable,
        WoundableComponent childWoundable)
    {
        parentWoundable.ChildWoundables.Add(childEntity);
        childWoundable.ParentWoundable = parentEntity;
        childWoundable.RootWoundable = parentWoundable.RootWoundable;

        FixWoundableRoots(childEntity, childWoundable);

        if (!TryComp<WoundableComponent>(parentWoundable.RootWoundable, out var woundableRoot))
            return;

        var woundableAttached = new WoundableAttachedEvent(parentEntity, parentWoundable);

        RaiseLocalEvent(childEntity, ref woundableAttached);

        foreach (var (woundId, wound) in GetAllWounds(childEntity, childWoundable))
        {
            var ev = new WoundAddedEvent(wound, parentWoundable, woundableRoot);
            RaiseLocalEvent(woundId, ref ev);
        }

        Dirty(childEntity, childWoundable);
    }

    private void InternalRemoveWoundableFromParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent parentWoundable,
        WoundableComponent childWoundable)
    {
        if (TerminatingOrDeleted(childEntity)
            || TerminatingOrDeleted(parentEntity))
            return;

        parentWoundable.ChildWoundables.Remove(childEntity);
        childWoundable.ParentWoundable = null;
        childWoundable.RootWoundable = childEntity;

        FixWoundableRoots(childEntity, childWoundable);

        if (!TryComp<WoundableComponent>(parentWoundable.RootWoundable, out var oldWoundableRoot))
            return;

        var woundableDetached = new WoundableDetachedEvent(parentEntity, parentWoundable);

        RaiseLocalEvent(childEntity, ref woundableDetached);

        foreach (var (woundId, wound) in GetAllWounds(childEntity, childWoundable))
        {
            var ev = new WoundRemovedEvent(wound, childWoundable, oldWoundableRoot);
            RaiseLocalEvent(woundId, ref ev);

            var ev2 = new WoundRemovedEvent(wound, childWoundable, oldWoundableRoot);
            RaiseLocalEvent(childWoundable.RootWoundable, ref ev2);
        }

        Dirty(childEntity, childWoundable);
    }

    /// <summary>
    /// Parents a woundable to another
    /// </summary>
    /// <param name="parentEntity">Owner of the new parent</param>
    /// <param name="childEntity">Owner of the woundable we want to attach</param>
    /// <param name="parentWoundable">The new parent woundable component</param>
    /// <param name="childWoundable">The woundable we are attaching</param>
    /// <returns>true if successful</returns>
    public bool AddWoundableToParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent? parentWoundable = null,
        WoundableComponent? childWoundable = null)
    {
        if (!Resolve(parentEntity, ref parentWoundable, false)
            || !Resolve(childEntity, ref childWoundable, false)
            || childWoundable.ParentWoundable == null)
            return false;

        InternalAddWoundableToParent(parentEntity, childEntity, parentWoundable, childWoundable);
        return true;
    }

    /// <summary>
    /// Removes a woundable from its parent (if present)
    /// </summary>
    /// <param name="parentEntity">Owner of the parent woundable</param>
    /// <param name="childEntity">Owner of the child woundable</param>
    /// <param name="parentWoundable"></param>
    /// <param name="childWoundable"></param>
    /// <returns>true if successful</returns>
    public bool RemoveWoundableFromParent(
        EntityUid parentEntity,
        EntityUid childEntity,
        WoundableComponent? parentWoundable = null,
        WoundableComponent? childWoundable = null)
    {
        if (!Resolve(parentEntity, ref parentWoundable, false)
            || !Resolve(childEntity, ref childWoundable, false)
            || childWoundable.ParentWoundable == null)
            return false;

        InternalRemoveWoundableFromParent(parentEntity, childEntity, parentWoundable, childWoundable);
        return true;
    }

    private void FixWoundableRoots(EntityUid targetEntity, WoundableComponent targetWoundable)
    {
        if (targetWoundable.ChildWoundables.Count == 0)
            return;

        foreach (var (childEntity, childWoundable) in GetAllWoundableChildren(targetEntity, targetWoundable))
        {
            childWoundable.RootWoundable = targetWoundable.RootWoundable;
            Dirty(childEntity, childWoundable);
        }

        Dirty(targetEntity, targetWoundable);
    }

    public bool TryInduceWounds(
        EntityUid uid,
        DamageSpecifier damage,
        out List<Entity<WoundComponent>> woundsInduced,
        WoundableComponent? woundable = null)
    {
        woundsInduced = new List<Entity<WoundComponent>>();
        if (!Resolve(uid, ref woundable))
            return false;

        foreach (var woundToInduce in damage.DamageDict)
        {
            if (!TryInduceWound(uid, woundToInduce.Key, woundToInduce.Value *
                damage.WoundSeverityMultipliers.GetValueOrDefault(woundToInduce.Key, 1), out var woundInduced, woundable))
                return false;

            woundsInduced.Add(woundInduced.Value);
        }

        return true;
    }

    public bool TryInduceWound(
        EntityUid uid,
        string woundId,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundInduced,
        WoundableComponent? woundable = null)
    {
        woundInduced = null;
        if (!Resolve(uid, ref woundable))
            return false;

        if (TryContinueWound(uid, woundId, severity, out woundInduced, woundable))
            return true;

        var wound = TryCreateWound(
            uid,
            woundId,
            severity,
            out woundInduced,
            GetDamageGroupByType(woundId)?.ID,
            woundable);
        return wound;
    }

    /// <summary>
    /// Opens a new wound on a requested woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable (body part).</param>
    /// <param name="woundProtoId">Wound prototype.</param>
    /// <param name="severity">Severity for wound to apply.</param>
    /// <param name="woundCreated">The wound that was created</param>
    /// <param name="damageGroup">Damage group.</param>
    /// <param name="woundable">Woundable component.</param>
    public bool TryCreateWound(
        EntityUid uid,
        string woundProtoId,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundCreated,
        ProtoId<DamageGroupPrototype>? damageGroup,
        WoundableComponent? woundable = null)
    {
        woundCreated = null;

        if (!IsWoundPrototypeValid(woundProtoId)
            || !Resolve(uid, ref woundable))
            return false;

        var wound = Spawn(woundProtoId);
        if (AddWound(uid, wound, severity, damageGroup))
        {
            woundCreated = (wound, Comp<WoundComponent>(wound));
        }
        else
        {
            // The wound failed some important checks, and we cannot let an invalid wound to be spawned!
            if (_net.IsServer && !IsClientSide(wound))
                QueueDel(wound);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Continues wound with specific type, if there's any. Adds severity to it basically.
    /// </summary>
    /// <param name="uid">Woundable entity's UID.</param>
    /// <param name="id">Wound entity's ID.</param>
    /// <param name="severity">Severity to apply.</param>
    /// <param name="woundContinued">The wound the severity was applied to, if any</param>
    /// <param name="woundable">Woundable for wound to add.</param>
    /// <returns>Returns true, if wound was continued.</returns>
    public bool TryContinueWound(
        EntityUid uid,
        string id,
        FixedPoint2 severity,
        [NotNullWhen(true)] out Entity<WoundComponent>? woundContinued,
        WoundableComponent? woundable = null)
    {
        woundContinued = null;
        if (!IsWoundPrototypeValid(id)
            || !Resolve(uid, ref woundable))
            return false;

        var proto = _prototype.Index(id);
        foreach (var wound in GetWoundableWounds(uid, woundable))
        {
            if (proto.ID != wound.Comp.DamageType
                || wound.Comp.IsScar)
                continue;

            ApplyWoundSeverity(wound, severity, wound);
            UpdateWoundableIntegrity(uid, woundable);
            CheckWoundableSeverityThresholds(uid, woundable);
            woundContinued = wound;

            return true;
        }

        return false;
    }
}
