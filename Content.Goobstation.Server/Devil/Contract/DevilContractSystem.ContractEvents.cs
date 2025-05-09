// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Devil;
using Content.Server.Body.Components;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Devil.Contract;

public sealed partial class DevilContractSystem
{
    private void InitializeSpecialActions()
    {
        SubscribeLocalEvent<DevilContractSoulOwnershipEvent>(OnSoulOwnership);
        SubscribeLocalEvent<DevilContractLoseHandEvent>(OnLoseHand);
        SubscribeLocalEvent<DevilContractLoseLegEvent>(OnLoseLeg);
        SubscribeLocalEvent<DevilContractLoseOrganEvent>(OnLoseOrgan);
        SubscribeLocalEvent<DevilContractChanceEvent>(OnChance);
    }
    private void OnSoulOwnership(DevilContractSoulOwnershipEvent args)
    {
        if (args.Contract?.ContractOwner is not { } contractOwner)
            return;

        TryTransferSouls(contractOwner, args.Target, 1);
    }

    private void OnLoseHand(DevilContractLoseHandEvent args)
    {
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        var hands = _bodySystem.GetBodyChildrenOfType(args.Target, BodyPartType.Hand, body).ToList();

        if (hands.Count <= 0)
            return;

        var pick = _random.Pick(hands);

        var ev = new AmputateAttemptEvent(pick.Id);
        RaiseLocalEvent(pick.Id, ref ev);

        Dirty(args.Target, body);
        _sawmill.Debug($"Removed part {ToPrettyString(pick.Id)} from {ToPrettyString(args.Target)}");
    }

    private void OnLoseLeg(DevilContractLoseLegEvent args)
    {
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        var legs = _bodySystem.GetBodyChildrenOfType(args.Target, BodyPartType.Leg, body).ToList();

        if (legs.Count <= 0)
            return;

        var pick = _random.Pick(legs);

        var ev = new AmputateAttemptEvent(pick.Id);
        RaiseLocalEvent(pick.Id, ref ev);

        Dirty(args.Target, body);
        _sawmill.Debug($"Removed part {ToPrettyString(pick.Id)} from {ToPrettyString(args.Target)}");
    }

    private void OnLoseOrgan(DevilContractLoseOrganEvent args)
    {
        // don't remove the brain, as funny as that is.
        var eligibleOrgans = _bodySystem.GetBodyOrgans(args.Target)
            .Where(o => !HasComp<BrainComponent>(o.Id))
            .ToList();

        if (eligibleOrgans.Count <= 0)
            return;

        var pick = _random.Pick(eligibleOrgans);

        _bodySystem.RemoveOrgan(pick.Id, pick.Component);
        _sawmill.Debug($"Removed part {ToPrettyString(pick.Id)} from {ToPrettyString(args.Target)}");
        QueueDel(pick.Id);
    }

    // LETS GO GAMBLING!!!!!
    private void OnChance(DevilContractChanceEvent args)
    {
        AddRandomClause(args.Target);
    }
}
