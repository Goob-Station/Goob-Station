using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModDeviceModuleSystem : EntitySystem
{
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

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
        if (args.Wearer is not { } wearer)
            return;

        var device = PredictedSpawnAttachedTo(ent.Comp.Device, Transform(wearer).Coordinates);
        var item = EnsureComp<ModDeviceItemComponent>(device);
        item.Module = ent;
        Dirty(device, item);

        ent.Comp.DeviceEntity = device;
        Dirty(ent);

        if (!_hands.TryPickupAnyHand(wearer, device, checkActionBlocker: false))
            Retract(ent, true);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModDeviceModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        Retract(ent, false);
    }

    [SubscribeLocalEvent]
    private void OnDeviceDropped(Entity<ModDeviceItemComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (ent.Comp.Module is not { } module)
            return;

        ent.Comp.Module = null;
        Dirty(ent);
        PredictedQueueDel(ent.Owner);

        if (TryComp<ModDeviceModuleComponent>(module, out var moduleComp))
        {
            moduleComp.DeviceEntity = null;
            Dirty(module, moduleComp);
        }

        if (TryComp<ModModuleComponent>(module, out var moduleBase)
            && moduleBase.Active
            && _modSuit.TryGetModuleSuit(module, out var suit))
            _modSuit.SetModuleActive(suit, (module, moduleBase), false);
    }

    private void Retract(Entity<ModDeviceModuleComponent> ent, bool deactivate)
    {
        if (ent.Comp.DeviceEntity is not { } device)
            return;

        ent.Comp.DeviceEntity = null;
        Dirty(ent);

        if (TryComp<ModDeviceItemComponent>(device, out var item))
        {
            item.Module = null;
            Dirty(device, item);
        }

        PredictedQueueDel(device);

        if (deactivate
            && TryComp<ModModuleComponent>(ent, out var moduleBase)
            && moduleBase.Active
            && _modSuit.TryGetModuleSuit(ent, out var suit))
            _modSuit.SetModuleActive(suit, (ent.Owner, moduleBase), false);
    }
}
