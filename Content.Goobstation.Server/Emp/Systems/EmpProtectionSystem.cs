using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using Content.Server.Emp;

namespace Content.Goobstation.Server.Emp;

public sealed class EmpProtectionSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slot = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EmpProtectionComponent, EmpAttemptEvent>(OnEmpAttempt);

        SubscribeLocalEvent<EmpContainerProtectionComponent, ItemSlotInsertAttemptEvent>(OnInserted);
        SubscribeLocalEvent<EmpContainerProtectionComponent, EntRemovedFromContainerMessage>(OnEjected);
        SubscribeLocalEvent<EmpContainerProtectionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EmpContainerProtectionComponent, MapInitEvent>(OnInit);
    }

    private void OnEmpAttempt(EntityUid uid, EmpProtectionComponent component, EmpAttemptEvent args)
        => args.Cancel();

    private void OnInserted(EntityUid uid, EmpContainerProtectionComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        EnsureComp<EmpProtectionComponent>(args.Item);
        component.BatteryUid = args.Item;
    }

    private void OnEjected(EntityUid uid, EmpContainerProtectionComponent component, ref EntRemovedFromContainerMessage args)
    {
        RemComp<EmpProtectionComponent>(args.Entity);
        component.BatteryUid = null;
    }
    private void OnShutdown(EntityUid uid, EmpContainerProtectionComponent component, ComponentShutdown args)
    {
        if (component.BatteryUid == null)
            return;

        RemComp<EmpProtectionComponent>(component.BatteryUid.Value);
    }

    private void OnInit(EntityUid uid, EmpContainerProtectionComponent component, MapInitEvent args)
    {
        var battery = _slot.GetItemOrNull(uid, component.ContainerId);
        if (battery == null)
            return;

        EnsureComp<EmpProtectionComponent>(battery.Value);
    }
}
