using Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;
using Content.Shared.Actions;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Server.Hydroponics.Mutations.MassMushroomOrganism;

public abstract partial class MassMushroomOrganismSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeMassMushroomOrganismHost();

        SubscribeLocalEvent<MassMushroomOrganismComponent, GotEquippedEvent>(OnGotEquipped);
    }

    private void OnGotEquipped(EntityUid uid, MassMushroomOrganismComponent component, GotEquippedEvent args)
    {
        var owner = args.Equipee;

        component.Host = owner;
        if (!TryComp<MassMushroomOrganismHostComponent>(owner, out var host))
            return;
        host.AttachedMushroomOrganism = uid;
        Dirty(uid, component);
        Dirty(owner, host);
    }
}
