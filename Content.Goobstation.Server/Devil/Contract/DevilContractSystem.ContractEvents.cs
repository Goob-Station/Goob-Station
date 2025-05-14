// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Devil;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
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

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);
        QueueDel(pick.Id);

        Dirty(args.Target, body);
    }

    private void OnLoseLeg(DevilContractLoseLegEvent args)
    {
        if (!TryComp<BodyComponent>(args.Target, out var body))
            return;

        var legs = _bodySystem.GetBodyChildrenOfType(args.Target, BodyPartType.Leg, body).ToList();

        if (legs.Count <= 0)
            return;

        var pick = _random.Pick(legs);

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);

        Dirty(args.Target, body);
    }

    private void OnLoseOrgan(DevilContractLoseOrganEvent args)
    {
        var organs = _bodySystem.GetBodyOrgans(args.Target).ToList();

        if (organs.Count <= 0)
            return;

        var pick = _random.Pick(organs);

        _bodySystem.RemoveOrgan(pick.Id, pick.Component);
        QueueDel(pick.Id);
    }
}
