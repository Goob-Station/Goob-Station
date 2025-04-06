using Content.Goobstation.Server.Contract;
using Content.Goobstation.Server.Contract.Revival;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<DevilComponent, CreateContractEvent>(OnContractCreated);
        SubscribeLocalEvent<DevilComponent, CreateRevivalContractEvent>(OnRevivalContractCreated);
        SubscribeLocalEvent<DevilComponent, ShadowJauntEvent>(OnShadowJaunt);
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
        PlayFwooshSound(uid, comp);
    }

    private void OnRevivalContractCreated(EntityUid uid, DevilComponent comp, ref CreateRevivalContractEvent args)
    {
        if (!TryUseAbility(comp, args))
            return;

        var contract = Spawn(_revivalContractPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, contract);

        if (!TryComp<RevivalContractComponent>(contract, out var contractComponent))
            return;

        contractComponent.ContractOwner = args.Performer;
        PlayFwooshSound(uid, comp);
    }

    private void OnShadowJaunt(EntityUid uid, DevilComponent comp, ref ShadowJauntEvent args)
    {
        if (!TryUseAbility(comp, args))
            return;

        Spawn("PolymorphShadowJauntAnimation", Transform(uid).Coordinates);
        _poly.PolymorphEntity(uid, "ShadowJaunt");
    }


}
