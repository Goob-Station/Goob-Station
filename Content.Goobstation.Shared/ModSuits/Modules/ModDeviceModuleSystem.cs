using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModDeviceModuleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    private readonly HashSet<EntityUid> _pendingReturn = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingReturn.Count == 0)
            return;

        foreach (var device in _pendingReturn)
        {
            if (TerminatingOrDeleted(device)
                || !TryComp<ModDeviceItemComponent>(device, out var item)
                || item.Module is not { } module
                || !TryComp<ModDeviceModuleComponent>(module, out var moduleComp))
                continue;

            if (TryComp<ModModuleComponent>(module, out var moduleBase) && moduleBase.Active)
                continue;

            _container.Insert(device, moduleComp.DeviceContainer);
        }

        _pendingReturn.Clear();
    }

    [SubscribeLocalEvent]
    private void OnInit(Entity<ModDeviceModuleComponent> ent, ref ComponentInit args)
    {
        ent.Comp.DeviceContainer = _container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ContainerId);
    }

    [SubscribeLocalEvent]
    private void OnActivateAttempt(Entity<ModDeviceModuleComponent> ent, ref ModModuleActivateAttemptEvent args)
    {
        if (args.User is not { } user || _hands.CountFreeHands(user) > 0)
            return;

        args.Cancelled = true;
        args.Reason = "modsuit-module-hands-full";
    }

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModDeviceModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (args.Wearer is not { } wearer || GetOrSpawnDevice(ent) is not { } device)
            return;

        _pendingReturn.Remove(device);

        if (!_hands.TryPickupAnyHand(wearer, device, checkActionBlocker: false))
            Deactivate(ent);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModDeviceModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (ent.Comp.DeviceEntity is not { } device
            || _pendingReturn.Contains(device)
            || ent.Comp.DeviceContainer.Contains(device)
            || TerminatingOrDeleted(device))
            return;

        _container.Insert(device, ent.Comp.DeviceContainer);
    }

    [SubscribeLocalEvent]
    private void OnDeviceDropped(Entity<ModDeviceItemComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (_timing.ApplyingState
            || ent.Comp.Module is not { } module
            || !TryComp<ModDeviceModuleComponent>(module, out var moduleComp)
            || !TryComp<ModModuleComponent>(module, out var moduleBase)
            || !moduleBase.Active)
            return;

        _pendingReturn.Add(ent);
        Deactivate((module, moduleComp));
    }

    [SubscribeLocalEvent]
    private void OnDeviceShutdown(Entity<ModDeviceItemComponent> ent, ref ComponentShutdown args)
    {
        _pendingReturn.Remove(ent);

        if (ent.Comp.Module is not { } module
            || !TryComp<ModDeviceModuleComponent>(module, out var moduleComp)
            || moduleComp.DeviceEntity != ent.Owner)
            return;

        moduleComp.DeviceEntity = null;
        Dirty(module, moduleComp);
    }

    private EntityUid? GetOrSpawnDevice(Entity<ModDeviceModuleComponent> ent)
    {
        if (ent.Comp.DeviceEntity is { } existing && !TerminatingOrDeleted(existing))
            return existing;

        if (ent.Comp.DeviceContainer.ContainedEntity is { } contained)
            return contained;

        if (!PredictedTrySpawnInContainer(ent.Comp.Device, ent, ent.Comp.ContainerId, out var device))
            return null;

        var item = EnsureComp<ModDeviceItemComponent>(device.Value);
        item.Module = ent;
        Dirty(device.Value, item);

        ent.Comp.DeviceEntity = device;
        Dirty(ent);
        return device;
    }

    private void Deactivate(Entity<ModDeviceModuleComponent> ent)
    {
        if (TryComp<ModModuleComponent>(ent, out var moduleBase)
            && moduleBase.Active
            && _modSuit.TryGetModuleSuit(ent, out var suit))
            _modSuit.SetModuleActive(suit, (ent.Owner, moduleBase), false);
    }
}
