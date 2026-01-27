using Content.Goobstation.Shared.Alert.Events;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Goobstation.Shared.InternalResources.EntitySystems;
public sealed class SharedInternalResourcesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    private readonly TimeSpan _systemUpdateRate = TimeSpan.FromSeconds(1);
    private TimeSpan _systemNextUpdate = TimeSpan.Zero;

    public override void Initialize()
    {
        SubscribeLocalEvent<InternalResourcesComponent, InternalResourcesAmountChangedEvent>(OnInternalResourcesAmountChanged);
        SubscribeLocalEvent<InternalResourcesComponent, InternalResourcesCapacityChangedEvent>(OnInternalResourcesCapacityChanged);
        SubscribeLocalEvent<InternalResourcesComponent, GetValueRelatedAlertValuesEvent>(OnAlertGetValues);
    }

    private void OnInternalResourcesAmountChanged(Entity<InternalResourcesComponent> entity, ref InternalResourcesAmountChangedEvent args)
    {
        UpdateAppearance(entity, args.Data.InternalResourcesType);
    }

    private void OnInternalResourcesCapacityChanged(Entity<InternalResourcesComponent> entity, ref InternalResourcesCapacityChangedEvent args)
    {
        UpdateAppearance(entity, args.Data.InternalResourcesType);
    }

    private void OnAlertGetValues(Entity<InternalResourcesComponent> entity, ref GetValueRelatedAlertValuesEvent args)
    {
        foreach (var type in entity.Comp.CurrentInternalResources)
        {
            if (_protoMan.Index(type.InternalResourcesType).AlertPrototype != args.Alert.ID)
                continue;

            args.CurrentValue = type.CurrentAmount;
            args.MaxValue = type.MaxAmount;

            return;
        }
    }

    /// <summary>
    /// Updates internal resources alert
    /// </summary>
    private void UpdateAppearance(Entity<InternalResourcesComponent> entity, ProtoId<InternalResourcesPrototype> protoId)
    {
        if (!_protoMan.TryIndex(protoId, out var proto))
            return;

        var show = entity.Comp.HasResourceData(proto.ID, out _);

        if (show)
            _alertsSystem.ShowAlert(entity, proto.AlertPrototype);
        else
            _alertsSystem.ClearAlert(entity, proto.AlertPrototype);
    }

    /// <summary>
    /// Updates amount of given resources by float amount with given protoId
    /// </summary>
    public bool TryUpdateResourcesAmount(EntityUid uid, string protoId, float amount, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component) || amount == 0)
            return false;

        if (!component.HasResourceData(protoId, out var data))
            return false;

        return TryUpdateResourcesAmount(uid, data, amount, component);
    }

    /// <summary>
    /// Updates amount of given resources by float amount with given internal resources data
    /// </summary>
    public bool TryUpdateResourcesAmount(EntityUid uid, InternalResourcesData data, float amount, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component) || amount == 0)
            return false;

        if (!component.CurrentInternalResources.Contains(data))
            return false;

        var attemptEv = new InternalResourcesAmountChangeAttemptEvent(uid, data, amount);
        RaiseLocalEvent(uid, ref attemptEv);

        if (attemptEv.Cancelled)
            return false;

        var currentAmount = data.CurrentAmount;
        var newAmount = Math.Clamp(data.CurrentAmount + amount, 0f, data.MaxAmount);

        data.CurrentAmount = newAmount;

        var afterEv = new InternalResourcesAmountChangedEvent(uid, data, currentAmount, newAmount, amount);
        RaiseLocalEvent(uid, afterEv);

        Dirty(uid, component);

        return true;
    }

    /// <summary>
    /// Updates the capacity of a resource by a float amount with a given protoId
    /// Does not SET the capacity - just adds the given value.
    /// </summary>
    public bool TryUpdateResourcesCapacity(EntityUid uid, string protoId, float amount, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || !component.HasResourceData(protoId, out var data))
            return false;

        return TryUpdateResourcesCapacity(uid, data, amount, component);
    }

    /// <summary>
    /// Updates the capacity of a resource by a float amount with a given internal resources data.
    /// Does not SET the capacity - just adds the given value.
    /// </summary>
    public bool TryUpdateResourcesCapacity(EntityUid uid, InternalResourcesData data, float amount, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || !component.CurrentInternalResources.Contains(data))
            return false;

        var currentCapacity = data.MaxAmount;
        var newCapacity = currentCapacity + amount;

        data.MaxAmount = newCapacity;

        var capEv = new InternalResourcesCapacityChangedEvent(uid, data, currentCapacity, newCapacity, amount);
        RaiseLocalEvent(uid, capEv);

        Dirty(uid, component);

        return true;
    }

    /// <summary>
    /// Sets the capacity of a resource using a float amount with a given protoId
    /// </summary>
    public bool TrySetResourcesCapacity(EntityUid uid, string protoId, float capacity, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || !component.HasResourceData(protoId, out var data))
            return false;

        return TrySetResourcesCapacity(uid, data, capacity, component);
    }

    /// <summary>
    /// Sets the capacity of a resource using a float amount with a given internal resources data.
    /// </summary>
    public bool TrySetResourcesCapacity(EntityUid uid, InternalResourcesData data, float capacity, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || !component.CurrentInternalResources.Contains(data))
            return false;

        var currentCapacity = data.MaxAmount;
        var delta = capacity - currentCapacity;

        data.MaxAmount = capacity;

        var capEv = new InternalResourcesCapacityChangedEvent(uid, data, currentCapacity, capacity, delta);
        RaiseLocalEvent(uid, capEv);

        Dirty(uid, component);

        return true;
    }

    /// <summary>
    /// Tries to add internal resources type to entity by protoId.
    /// </summary>
    public bool TryAddInternalResources(EntityUid uid, string protoId, [NotNullWhen(true)] out InternalResourcesData? data)
    {
        data = null;

        if (!_protoMan.TryIndex<InternalResourcesPrototype>(protoId, out var proto))
        {
            Log.Debug($"Failed to add {protoId} internal resource type to entity {ToPrettyString(uid):uid}. Internal resource prototype does not exist.");
            return false;
        }

        EnsureInternalResources(uid, proto, out data);

        return data != null;
    }

    /// <summary>
    /// Tries to remove an internal resource type from an entity with an internal resources component by protoId.
    /// </summary>
    public void TryRemoveInternalResource(EntityUid uid, string protoId, InternalResourcesComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_protoMan.TryIndex<InternalResourcesPrototype>(protoId, out var proto))
        {
            Log.Debug($"Failed to remove {protoId} internal resource type from entity {ToPrettyString(uid):uid}. Internal resource prototype does not exist.");
            return;
        }

        RemoveInternalResource((uid, component), proto);
    }

    /// <summary>
    /// Ensures that entity have InternalResourcesComponent and adds internal resources type to it.
    /// Returns true if entity already had this internal resource type.
    /// </summary>
    public bool EnsureInternalResources(EntityUid uid, InternalResourcesPrototype proto, out InternalResourcesData? data)
    {
        data = null;

        EnsureComp<InternalResourcesComponent>(uid, out var resourcesComp);

        if (resourcesComp.HasResourceData(proto.ID, out data))
            return true;

        _protoMan.TryIndex(proto.ThresholdsProto, out var threshProto);

        var startingAmount = Math.Clamp(proto.BaseStartingAmount, 0f, proto.BaseMaxAmount);
        data = new InternalResourcesData(
            proto.BaseMaxAmount,
            proto.BaseRegenerationRate,
            startingAmount,
            threshProto?.Thresholds,
            proto.ID);

        resourcesComp.CurrentInternalResources.Add(data);
        Dirty(uid, resourcesComp);

        UpdateAppearance((uid, resourcesComp), proto.ID);

        return false;
    }

    /// <summary>
    /// Removes the internal resource type from the entity.
    /// If there are no other internal resources, remove the component aswell.
    /// </summary>
    public void RemoveInternalResource(Entity<InternalResourcesComponent> entity, InternalResourcesPrototype proto)
    {
        if (!entity.Comp.HasResourceData(proto.ID, out var data))
            return;

        entity.Comp.CurrentInternalResources.Remove(data);
        Dirty(entity);

        UpdateAppearance(entity, proto.ID);

        if (entity.Comp.CurrentInternalResources.Count == 0)
            RemComp<InternalResourcesComponent>(entity);
    }

    /// <summary>
    /// Check if user has internal resources type
    /// </summary>
    public bool TryGetResourceType(EntityUid uid, ProtoId<InternalResourcesPrototype> type, [NotNullWhen(true)] out InternalResourcesData? data, InternalResourcesComponent? component = null)
    {
        data = null;

        return Resolve(uid, ref component) && component.HasResourceData(type, out data);
    }

    /// <summary>
    /// Handles internal resources regeneration
    /// </summary>
    public override void Update(float frameTime)
    {
        if (_systemNextUpdate > _gameTiming.CurTime)
            return;

        _systemNextUpdate += _systemUpdateRate;

        var query = EntityQueryEnumerator<InternalResourcesComponent>();
        while (query.MoveNext(out var uid, out var resourcesComp))
        {
            foreach (var resourceData in resourcesComp.CurrentInternalResources)
            {
                var modEv = new InternalResourcesRegenModifierEvent(
                    uid,
                    resourceData,
                    resourceData.RegenerationRate);
                RaiseLocalEvent(uid, ref modEv);

                TryUpdateResourcesAmount(uid, resourceData, modEv.Modifier, resourcesComp);

                if (resourceData.Thresholds == null)
                    continue;

                var thresholdsArray = resourceData.Thresholds.Keys.ToArray();
                foreach (var key in thresholdsArray)
                {
                    var threshold = resourceData.Thresholds[key];
                    // threshold.Item1 is the threshold percentage
                    // threshold.Item2 is the bool for the threshold having been met

                    var scaledAmount = resourceData.MaxAmount * threshold.Item1;

                    if (!threshold.Item2 // threshold needs to not have been met already
                        && resourceData.CurrentAmount <= scaledAmount)
                    {
                        var threshEv = new InternalResourcesThresholdMetEvent(uid, resourceData, key);
                        RaiseLocalEvent(uid, ref threshEv);
                    }

                    threshold.Item2 = resourceData.CurrentAmount <= scaledAmount;

                    resourceData.Thresholds[key] = threshold;
                }
            }
        }
    }
}
