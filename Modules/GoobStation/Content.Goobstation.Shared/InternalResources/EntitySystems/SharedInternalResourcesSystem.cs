using Content.Goobstation.Shared.Alert.Events;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;

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
        SubscribeLocalEvent<InternalResourcesComponent, GetValueRelatedAlertValuesEvent>(OnAlertGetValues);
    }

    private void OnInternalResourcesAmountChanged(Entity<InternalResourcesComponent> entity, ref InternalResourcesAmountChangedEvent args)
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

        _alertsSystem.ShowAlert(entity, proto.AlertPrototype);
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
    /// Ensures that entity have InternalResourcesComponent and adds internal resources type to it.
    /// Returns true if entity already had this internal resource type.
    /// </summary>
    public bool EnsureInternalResources(EntityUid uid, InternalResourcesPrototype proto, out InternalResourcesData? data)
    {
        data = null;

        EnsureComp<InternalResourcesComponent>(uid, out var resourcesComp);

        if (resourcesComp.HasResourceData(proto.ID, out data))
            return true;

        var startingAmount = Math.Clamp(proto.BaseStartingAmount, 0f, proto.BaseMaxAmount);
        data = new InternalResourcesData(proto.BaseMaxAmount, proto.BaseRegenerationRate, startingAmount, proto.ID);

        resourcesComp.CurrentInternalResources.Add(data);
        Dirty(uid, resourcesComp);

        UpdateAppearance((uid, resourcesComp), proto.ID);

        return false;
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
                TryUpdateResourcesAmount(uid, resourceData, resourceData.RegenerationRate, resourcesComp);
        }
    }
}
