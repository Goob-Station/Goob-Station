// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.UI;
using Content.Server.Administration.Systems;
using Content.Server.Mind;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Server.GameObjects;

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
        SubscribeLocalEvent<PendingRevivalContractComponent, RevivalContractMessage>(OnMessage);
    }

    private void AfterInteract(Entity<RevivalContractComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target is not { Valid: true } target
            || !TryComp<MobStateComponent>(target, out var mobState)
            || mobState.CurrentState != MobState.Dead)
            return;

        // Non-devils can't offer deals silly.
        if (!HasComp<DevilComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), args.User, PopupType.MediumCaution);
            return;
        }

        // Make sure the mind actually exists
        if (!_mind.TryGetMind(target, out var mindId, out var mindComp) || mindComp.CurrentEntity is not { } ghost)
        {
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-no-mind"), args.User, args.User);
            return;
        }

        // You can't offer two deals at once.
        if (HasComp<PendingRevivalContractComponent>(ghost))
        {
            var failedPopup = Loc.GetString("revival-contract-use-failed");
            _popupSystem.PopupEntity(failedPopup, args.User, args.User);
            return;
        }

        // Create pending contract
        var pending = EnsureComp<PendingRevivalContractComponent>(ghost);
        pending.Contractee = target;
        pending.Offerer = args.User;
        pending.Contract = ent;
        pending.MindId = mindId;

        // Show confirmation
        var successPopup = Loc.GetString("revival-contract-use-success", ("target", target));
        _popupSystem.PopupEntity(successPopup, args.User, args.User);

        ent.Comp.Signer = target;
        ent.Comp.ContractOwner = args.User;

        TryOpenUi(ghost);
    }

    private bool TryOpenUi(EntityUid target)
    {
        if (!_userInterface.HasUi(target, RevivalContractUiKey.Key))
            return false;

        if (_mind.TryGetMind(target, out _, out var mindComp) && mindComp.Session is { } session)
            _userInterface.OpenUi(target, RevivalContractUiKey.Key, session);

        return true;
    }

    private void OnMessage(Entity<PendingRevivalContractComponent> ent, ref RevivalContractMessage args)
    {
        if (args.Accepted && ent.Comp.Contractee is { } contractee)
        {
            TryReviveAndTransferSoul(contractee, ent.Comp);
            _mind.UnVisit(ent.Comp.MindId);
        }

        RemComp<PendingRevivalContractComponent>(args.Actor);
    }

    private bool TryReviveAndTransferSoul(EntityUid target, PendingRevivalContractComponent pending)
    {
        if (TerminatingOrDeleted(target))
            return false;

        if (TryComp<RevivalContractComponent>(pending.Contract, out var contract) && contract is { ContractOwner: { Valid: true } contractOwner, Signer: { } signer })
        {
            _rejuvenate.PerformRejuvenate(target);
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-accepted"), target, target);
            _contract.TryTransferSouls(contractOwner, signer, 1);
        }

        RemComp(target, pending);
        return true;
    }

}
