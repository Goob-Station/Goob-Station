// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.UI;
using Content.Goobstation.Shared.Devil;
using Content.Server.Administration.Systems;
using Content.Server.EUI;
using Content.Server.Mind;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil.Contract.Revival;
public sealed partial class PendingRevivalContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly DevilContractSystem _contract = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly PendingRevivalContractSystem _revivalContract = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevivalContractComponent, AfterInteractEvent>(AfterInteract);
    }

    private void AfterInteract(EntityUid uid, RevivalContractComponent comp, AfterInteractEvent args)
    {
        // Seperated into two checks for readabilities’ sake.
        if (!TryComp<MobStateComponent>(args.Target, out var mobState) || mobState.CurrentState != MobState.Dead || !HasComp<ActorComponent>(args.Target))
            return;

        if (args.Target == null || args.Handled)
            return;

        // Non-devils can't offer deals silly.
        if (!HasComp<DevilComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), args.User, PopupType.MediumCaution);
            return;
        }

        // You can't offer two deals at once.
        if (HasComp<PendingRevivalContractComponent>(args.Target))
        {
            var failedPopup = Loc.GetString("revival-contract-use-failed");
            _popupSystem.PopupEntity(failedPopup, uid);
            return;
        }

        // Create pending contract
        var pending = AddComp<PendingRevivalContractComponent>((EntityUid)args.Target);
        pending.Contractee = uid;
        pending.Offerer = args.User;

        // Show confirmation
        var sucessPopup = Loc.GetString("revival-contract-use-success", ("target", args.Target));
        _popupSystem.PopupEntity(sucessPopup, uid);

        // UI code!!
        if (_mindSystem.TryGetMind((EntityUid)args.Target, out _, out var mind) && mind.Session is { } playerSession)
            _euiManager.OpenEui(new RevivalContractEui(mind, _mindSystem, _revivalContract), playerSession);

    }

    public void TryReviveAndTransferSoul(EntityUid? target)
    {
        if (!TryComp<PendingRevivalContractComponent>(target, out var pending))
            return;

        if (TryComp<RevivalContractComponent>(pending.Contractee, out var contract) && contract.ContractOwner != null)
        {
            contract.Signer = target;
            _rejuvenate.PerformRejuvenate((EntityUid)target);
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-accepted"), (EntityUid)target);
            _contract.TryTransferSouls((EntityUid)contract.ContractOwner, (EntityUid)contract.Signer, 1);
        }

        RemComp<PendingRevivalContractComponent>((EntityUid)target);
    }

}
