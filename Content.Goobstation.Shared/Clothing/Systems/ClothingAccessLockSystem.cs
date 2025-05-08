using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.Clothing.Systems;

public sealed class ClothingAccessLockSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingAccessLockComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<ClothingAccessLockComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
    }

    private bool AccessCheck(Entity<ClothingAccessLockComponent> ent, EntityUid user)
    {
        if (!TryComp<AccessReaderComponent>(ent, out var reader))
            return true; // if it doesn't have an access reader comp count it as aa

        return _access.IsAllowed(user, ent, reader);
    }

    private void OnEquipAttempt(Entity<ClothingAccessLockComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (!AccessCheck(ent, args.Equipee) && ent.Comp.RequireEquip)
        {
            args.Reason = "no-access-equip";
            args.Cancel();
        }
    }

    private void OnUnequipAttempt(Entity<ClothingAccessLockComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (!AccessCheck(ent, args.Unequipee) && ent.Comp.RequireUnequip)
        {
            args.Reason = "no-access-unequip";
            args.Cancel();
        }
    }

}
