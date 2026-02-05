using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.SpeciesRestricted;

public sealed class EntityPrototypeRestrictedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntityPrototypeRestrictedComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
    }

    private void OnEquipAttempt(Entity<EntityPrototypeRestrictedComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (!string.Equals(MetaData(args.EquipTarget).EntityPrototype?.ID, ent.Comp.ProtoId.Id, StringComparison.CurrentCultureIgnoreCase))
            args.Cancel();
    }
}
