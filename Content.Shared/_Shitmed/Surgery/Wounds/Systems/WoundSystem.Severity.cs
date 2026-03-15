using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Targeting.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public sealed partial class WoundSystem
{
    /// <summary>
    /// Sets severity of a wound.
    /// </summary>
    /// <param name="uid">UID of the wound.</param>
    /// <param name="severity">Severity to set.</param>
    /// <param name="wound">Wound to which severity is applied.</param>
    public void SetWoundSeverity(EntityUid uid,
        FixedPoint2 severity,
        WoundComponent? wound = null,
        WoundableComponent? woundable = null)
    {
        if (!Resolve(uid, ref wound)
            || !Resolve(wound.HoldingWoundable, ref woundable))
            return;

        var old = wound.WoundSeverityPoint;

        var upperLimit = wound.WoundSeverityPoint + woundable.WoundableIntegrity;
        wound.WoundSeverityPoint =
        FixedPoint2.Clamp(ApplySeverityModifiers(wound.HoldingWoundable, severity), 0, upperLimit);

        if (wound.WoundSeverityPoint != old)
        {
            var ev = new WoundSeverityPointChangedEvent(wound, old, wound.WoundSeverityPoint);
            RaiseLocalEvent(uid, ref ev);
        }

        CheckSeverityThresholds(uid, wound.HoldingWoundable, wound, woundable);
        Dirty(uid, wound);

        UpdateWoundableIntegrity(wound.HoldingWoundable);
        CheckWoundableSeverityThresholds(wound.HoldingWoundable);
    }

    /// <summary>
    /// Applies severity to a wound. Does NOT call UpdateWoundableIntegrity or
    /// CheckWoundableSeverityThresholds, callers must do so themselves after
    /// all wound changes are complete.
    /// </summary>
    public void ApplyWoundSeverity(
        EntityUid uid,
        FixedPoint2 severity,
        WoundComponent? wound = null,
        WoundableComponent? woundable = null)
    {
        if (!Resolve(uid, ref wound)
            || !Resolve(wound.HoldingWoundable, ref woundable))
            return;

        var old = wound.WoundSeverityPoint;
        var rawValue = severity > 0
            ? old + ApplySeverityModifiers(wound.HoldingWoundable, severity)
            : old + severity;

        var upperLimit = wound.WoundSeverityPoint + woundable.WoundableIntegrity;
        wound.WoundSeverityPoint = FixedPoint2.Clamp(rawValue, 0, upperLimit);
        Dirty(uid, wound);
        if (wound.WoundSeverityPoint != old || rawValue > wound.WoundSeverityPoint)
        {
            FixedPoint2? overflow = rawValue > wound.WoundSeverityPoint ? rawValue - wound.WoundSeverityPoint : null;
            var ev = new WoundSeverityPointChangedEvent(wound, old, wound.WoundSeverityPoint, overflow);
            RaiseLocalEvent(uid, ref ev);
        }

        if (severity > 0
            && wound.MangleSeverity != null
            && HasWoundsExceedingMangleSeverity(wound.HoldingWoundable))
            _trauma.ApplyMangledTraumas(wound.HoldingWoundable, uid, severity, woundable);

        CheckSeverityThresholds(uid, wound.HoldingWoundable, wound, woundable);
    }

    public FixedPoint2 ApplySeverityModifiers(
        EntityUid woundable,
        FixedPoint2 severity,
        WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component))
            return severity;

        if (component.SeverityMultipliers.Count == 0)
            return severity;

        var sum = 0f;
        foreach (var multiplier in component.SeverityMultipliers)
            sum += (float) multiplier.Value.Change;

        return severity * (sum / component.SeverityMultipliers.Count);
    }

    /// <summary>
    /// Applies severity multiplier to a wound.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="owner">UID of the multiplier owner.</param>
    /// <param name="change">The severity multiplier itself</param>
    /// <param name="identifier">A string to defy this multiplier from others.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryAddWoundableSeverityMultiplier(
        EntityUid uid,
        EntityUid owner,
        FixedPoint2 change,
        string identifier,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        if (!component.SeverityMultipliers.TryAdd(owner, new WoundableSeverityMultiplier(change, identifier)))
            return false;

        foreach (var wound in component.Wounds.ContainedEntities)
            CheckSeverityThresholds(wound, uid, woundableComp: component);

        UpdateWoundableIntegrity(uid, component);
        CheckWoundableSeverityThresholds(uid, component);

        return true;
    }

    /// <summary>
    /// Removes a multiplier from a woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="identifier">Identifier of the said multiplier.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryRemoveWoundableSeverityMultiplier(
        EntityUid uid,
        string identifier,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        if (!TryFindMultiplierByIdentifier(component.SeverityMultipliers, identifier, out var foundKey))
            return false;

        if (!component.SeverityMultipliers.Remove(foundKey, out _))
            return false;

        foreach (var wound in component.Wounds.ContainedEntities)
            CheckSeverityThresholds(wound, uid, woundableComp: component);

        UpdateWoundableIntegrity(uid, component);
        CheckWoundableSeverityThresholds(uid, component);

        return true;
    }

    /// <summary>
    /// Changes a multiplier's change in a specified woundable.
    /// </summary>
    /// <param name="uid">UID of the woundable.</param>
    /// <param name="identifier">Identifier of the said multiplier.</param>
    /// <param name="change">The new multiplier fixed point.</param>
    /// <param name="component">Woundable to which severity multiplier is applied.</param>
    public bool TryChangeWoundableSeverityMultiplier(
        EntityUid uid,
        string identifier,
        FixedPoint2 change,
        WoundableComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Wounds == null
            || !_net.IsServer)
            return false;

        if (!TryFindMultiplierByIdentifier(component.SeverityMultipliers, identifier, out var foundKey))
            return false;

        component.SeverityMultipliers.Remove(foundKey, out var multiplierValue);
        multiplierValue.Change = change;
        component.SeverityMultipliers.Add(foundKey, multiplierValue);

        foreach (var wound in component.Wounds.ContainedEntities)
            CheckSeverityThresholds(wound, uid, woundableComp: component);

        UpdateWoundableIntegrity(uid, component);
        CheckWoundableSeverityThresholds(uid, component);
        return true;
    }

    private bool TryFindMultiplierByIdentifier(
        Dictionary<EntityUid, WoundableSeverityMultiplier> multipliers,
        string identifier,
        out EntityUid key)
    {
        foreach (var (k, v) in multipliers)
        {
            if (v.Identifier == identifier)
            {
                key = k;
                return true;
            }
        }
        key = default;
        return false;
    }

    private void CheckSeverityThresholds(EntityUid wound,
        EntityUid woundable,
        WoundComponent? component = null,
        WoundableComponent? woundableComp = null)
    {
        if (!Resolve(wound, ref component, false)
            || !Resolve(woundable, ref woundableComp)
            || !_net.IsServer)
            return;

        var nearestSeverity = component.WoundSeverity;
        foreach (var (severity, value) in WoundThresholds)
        {
            var scaledThreshold = value * (woundableComp.IntegrityCap / 100);
            if (component.WoundSeverityPoint < scaledThreshold)
                continue;

            if (severity == WoundSeverity.Healed && component.WoundSeverityPoint > 0)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity == component.WoundSeverity)
            return;

        var ev = new WoundSeverityChangedEvent(component.WoundSeverity, nearestSeverity);
        RaiseLocalEvent(wound, ref ev);
        component.WoundSeverity = nearestSeverity;

        if (!TerminatingOrDeleted(component.HoldingWoundable))
            UpdateWoundableAppearance(component.HoldingWoundable);
    }

    /// <summary>
    /// Checks if the current integrity crosses any severity thresholds and updates accordingly
    /// </summary>
    private void CheckWoundableSeverityThresholds(EntityUid woundable, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component, false))
            return;

        var nearestSeverity = component.WoundableSeverity;
        foreach (var (severity, value) in component.SortedThresholds!)
        {
            if (component.WoundableIntegrity >= component.IntegrityCap)
            {
                nearestSeverity = WoundableSeverity.Healthy;
                break;
            }

            if (component.WoundableIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity == component.WoundableSeverity)
            return;

        var ev = new WoundableSeverityChangedEvent(component.WoundableSeverity, nearestSeverity);
        RaiseLocalEvent(woundable, ref ev);
        component.WoundableSeverity = nearestSeverity;

        Dirty(woundable, component);

        var bodyPart = Comp<BodyPartComponent>(woundable);
        if (bodyPart.Body == null)
            return;

        if (!TryComp<TargetingComponent>(bodyPart.Body.Value, out var targeting))
            return;

        targeting.BodyStatus = GetWoundableStatesOnBodyPainFeels(bodyPart.Body.Value);
        Dirty(bodyPart.Body.Value, targeting);

        if (_net.IsServer)
            RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(bodyPart.Body.Value)), bodyPart.Body.Value);

        UpdateWoundableAppearance(woundable);
    }

    /// <summary>
    /// Updates the woundable integrity based on the current damage
    /// </summary>
    public void UpdateWoundableIntegrity(EntityUid uid, WoundableComponent? component = null, DamageableComponent? damageable = null)
    {
        if (!Resolve(uid, ref component, false)
            || !Resolve(uid, ref damageable, false)
            || component.Wounds == null)
            return;

        var damage = FixedPoint2.Zero;
        foreach (var woundEntity in component.Wounds.ContainedEntities)
        {
            var woundComp = Comp<WoundComponent>(woundEntity);
            if (!woundComp.IsScar)
                damage += woundComp.WoundIntegrityDamage;
        }

        var newIntegrity = FixedPoint2.Clamp(component.IntegrityCap - damage, 0, component.IntegrityCap);
        if (newIntegrity == component.WoundableIntegrity)
            return;

        var ev = new WoundableIntegrityChangedEvent(component.WoundableIntegrity, newIntegrity);
        RaiseLocalEvent(uid, ref ev);
        var bodySeverity = FixedPoint2.Zero;
        var bodyPart = Comp<BodyPartComponent>(uid);

        if (bodyPart.Body.HasValue)
        {
            var rootPart = Comp<BodyComponent>(bodyPart.Body.Value)?.RootContainer?.ContainedEntity;
            if (rootPart.HasValue)
            {
                foreach (var child in GetAllWoundableChildren(rootPart.Value))
                    bodySeverity += GetWoundableIntegrityDamage(child, child);
            }

            var ev1 = new WoundableIntegrityChangedOnBodyEvent(
                (uid, component),
                bodySeverity - (component.WoundableIntegrity - newIntegrity),
                bodySeverity);
            RaiseLocalEvent(bodyPart.Body.Value, ref ev1);
        }
        component.WoundableIntegrity = newIntegrity;
        Dirty(uid, component);
    }
}
