using Content.Shared.Inventory.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.SpeciesRestricted;

public sealed class VoxRestrictedSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoxRestrictedComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
    }

    private void OnEquipAttempt(EntityUid uid, VoxRestrictedComponent component, BeingEquippedAttemptEvent args)
    {
        // Get prototype ID of the mob trying to equip
        var meta = MetaData(args.EquipTarget);

        if (meta.EntityPrototype?.ID != "MobVoxraider")
        {
            args.Cancel();
        }
    }
}
