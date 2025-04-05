using Content.Goobstation.Server.Contract;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<DevilComponent, OpenSoulStoreEvent>(OnStoreOpened);
        SubscribeLocalEvent<DevilComponent, CreateContractEvent>(OnContractCreated);
        SubscribeLocalEvent<DevilComponent, ShadowJauntEvent>(OnShadowJaunt);
    }

    private void OnStoreOpened(EntityUid uid, DevilComponent comp, ref OpenSoulStoreEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnContractCreated(EntityUid uid, DevilComponent comp, ref CreateContractEvent args)
    {
        if (!TryUseAbility(comp, args))
            return;

        var contract = Spawn(_contractPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, contract);

        if (!TryComp<DevilContractComponent>(contract, out var contractComponent))
            return;

        contractComponent.ContractOwner = args.Performer;

        // Add a firey sound effect here
    }

    private void OnShadowJaunt(EntityUid uid, DevilComponent comp, ref ShadowJauntEvent args)
    {
        if (!TryUseAbility(comp, args))
            return;

        _poly.PolymorphEntity(uid, "ShadowJaunt");
    }


}
