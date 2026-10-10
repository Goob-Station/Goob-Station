using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech;
using Content.Shared.Mech.Components;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed class MechAirTankModuleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechAirTankModuleComponent, MechEquipmentInsertedEvent>(OnInserted);
        SubscribeLocalEvent<MechAirTankModuleComponent, MechEquipmentRemovedEvent>(OnRemoved);
    }

    private void OnInserted(Entity<MechAirTankModuleComponent> module, ref MechEquipmentInsertedEvent args)
    {
        if (!TryComp<MechComponent>(args.Mech, out var mech))
            return;

        module.Comp.PreviousAirtight = mech.Airtight;
        mech.Airtight = true;
        Dirty(args.Mech, mech);
        Dirty(module);
    }

    private void OnRemoved(Entity<MechAirTankModuleComponent> module, ref MechEquipmentRemovedEvent args)
    {
        if (TryComp<MechComponent>(args.Mech, out var mech) && module.Comp.PreviousAirtight is { } previous)
        {
            mech.Airtight = previous;
            Dirty(args.Mech, mech);
        }

        module.Comp.PreviousAirtight = null;
        Dirty(module);
    }
}
