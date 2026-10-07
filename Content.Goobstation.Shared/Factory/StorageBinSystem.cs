using Content.Goobstation.Shared.Factory.Filters;
using Content.Shared.DeviceLinking;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Factory;

public sealed partial class StorageBinSystem : EntitySystem
{
    [Dependency] private AutomationFilterSystem _filter = default!;
    [Dependency] private SharedDeviceLinkSystem _device = default!;

    public const string ContainerId = "storagebase";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StorageBinComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<StorageBinComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<StorageBinComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }

    private void OnInsertAttempt(Entity<StorageBinComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container.ID != ContainerId)
            return;

        if (_filter.IsBlocked(_filter.GetSlot(ent), args.EntityUid))
            args.Cancel();
    }

    private void OnEntInserted(Entity<StorageBinComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ContainerId)
            return;

        _device.InvokePort(ent, ent.Comp.InsertedPort);
    }

    private void OnEntRemoved(Entity<StorageBinComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ContainerId)
            return;

        _device.InvokePort(ent, ent.Comp.RemovedPort);
    }
}
