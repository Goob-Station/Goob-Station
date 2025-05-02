// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Server.Devil.Contract.Revival;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Contract;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem
{
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<DevilComponent, CreateContractEvent>(OnContractCreated);
        SubscribeLocalEvent<DevilComponent, CreateRevivalContractEvent>(OnRevivalContractCreated);
        SubscribeLocalEvent<DevilComponent, ShadowJauntEvent>(OnShadowJaunt);
        SubscribeLocalEvent<DevilComponent, DevilPossessionEvent>(OnPossess);
    }

    private void OnContractCreated(EntityUid uid, DevilComponent comp, ref CreateContractEvent args)
    {
        if (!TryUseAbility(args))
            return;

        var contract = Spawn(comp.ContractPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, contract);

        if (!TryComp<DevilContractComponent>(contract, out var contractComponent))
            return;

        contractComponent.ContractOwner = args.Performer;
        PlayFwooshSound(uid, comp);
        DoContractFlavor(uid, Name(uid));
    }

    private void OnRevivalContractCreated(EntityUid uid, DevilComponent comp, ref CreateRevivalContractEvent args)
    {
        if (!TryUseAbility(args))
            return;

        var contract = Spawn(comp.RevivalContractPrototype, Transform(uid).Coordinates);
        _hands.TryPickupAnyHand(uid, contract);

        if (!TryComp<RevivalContractComponent>(contract, out var contractComponent))
            return;

        contractComponent.ContractOwner = args.Performer;
        PlayFwooshSound(uid, comp);
        DoContractFlavor(uid, Name(uid));
    }

    private void OnShadowJaunt(EntityUid uid, DevilComponent comp, ref ShadowJauntEvent args)
    {
        if (!TryUseAbility(args))
            return;

        Spawn(comp.JauntAnimationProto, Transform(uid).Coordinates);
        Spawn(comp.PentagramEffectProto, Transform(uid).Coordinates);
        _poly.PolymorphEntity(uid, comp.JauntEntityProto);
    }

    private void OnPossess(EntityUid uid, DevilComponent comp, ref DevilPossessionEvent args)
    {
        if (args.Target == default || !TryUseAbility(args))
        {
            var message = Loc.GetString("invalid-possession-target");
            _popup.PopupEntity(message, uid, uid);
            return;
        }

        if (comp.PowerLevel != DevilPowerLevel.None)
            comp.PossessionDuration *= (int)comp.PowerLevel;

        if (_possession.TryPossessTarget(args.Target, args.Performer, comp.PossessionDuration, true))
        {
            Spawn(comp.JauntAnimationProto, Transform(args.Performer).Coordinates);
            Spawn(comp.PentagramEffectProto, Transform(args.Performer).Coordinates);
            _poly.PolymorphEntity(args.Performer, GetJauntEntity(comp));
        }
    }
}
