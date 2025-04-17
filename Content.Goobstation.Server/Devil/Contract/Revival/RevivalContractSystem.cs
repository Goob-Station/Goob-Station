// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.UI;
using Content.Server.Administration.Systems;
using Content.Server.Mind;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Devil.Contract.Revival;
public sealed partial class PendingRevivalContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly DevilContractSystem _contract = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevivalContractComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<RevivalContractComponent, RevivalContractMessage>(OnMessage);
    }

    private void AfterInteract(Entity<RevivalContractComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target is not { Valid: true } target
            || !TryComp<MobStateComponent>(target, out var mobState)
            || mobState.CurrentState != MobState.Dead
            || TryComp<ActorComponent>(target, out var actor))
            return;

        // Non-devils can't offer deals silly.
        if (!HasComp<DevilComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), args.User, PopupType.MediumCaution);
            return;
        }

        // You can't offer two deals at once.
        if (HasComp<PendingRevivalContractComponent>(target))
        {
            var failedPopup = Loc.GetString("revival-contract-use-failed");
            _popupSystem.PopupEntity(failedPopup, args.User, args.User);
            return;
        }

        // Create pending contract
        var pending = EnsureComp<PendingRevivalContractComponent>(target);
        pending.Contractee = target;
        pending.Offerer = args.User;
        pending.Contract = ent;

        // Show confirmation
        var successPopup = Loc.GetString("revival-contract-use-success", ("target", target));
        _popupSystem.PopupEntity(successPopup, args.User, args.User);

        // UI code!!
        ent.Comp.Signer = target;
        ent.Comp.ContractOwner = args.User;

        // WHY DOESNT THIS OPEN PROPERLYYYY!!!
        _userInterface.OpenUi(args.Used, RevivalContractUiKey.Key, actor!.PlayerSession, true);
    }

    private void OnMessage(Entity<RevivalContractComponent> ent, ref RevivalContractMessage args)
    {
        if (args.Accepted)
        {
            TryReviveAndTransferSoul(ent.Comp.Signer);
            _mind.UnVisit(ent.Comp.Signer);
        }

        RemComp<PendingRevivalContractComponent>(ent.Comp.Signer);
    }

    private bool TryReviveAndTransferSoul(EntityUid target)
    {
        if (!TryComp<PendingRevivalContractComponent>(target, out var pending) || TerminatingOrDeleted(target))
            return false;

        if (TryComp<RevivalContractComponent>(pending.Contract, out var contract))
        {
            _rejuvenate.PerformRejuvenate(target);
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-accepted"), target, target);
            _contract.TryTransferSouls(contract.ContractOwner, contract.Signer, 1);
        }

        RemComp<PendingRevivalContractComponent>(target);
        return true;
    }

}
