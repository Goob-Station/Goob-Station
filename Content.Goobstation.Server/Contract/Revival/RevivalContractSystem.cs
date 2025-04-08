// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil;
using Content.Server.Administration.Systems;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Contract.Revival;
public sealed partial class PendingRevivalContractSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly DevilContractSystem _contract = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevivalContractComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<PendingRevivalContractComponent, GetVerbsEvent<InnateVerb>>(AddRevivalVerbs);
    }

    private void AfterInteract(EntityUid uid, RevivalContractComponent comp, AfterInteractEvent args)
    {
        // Block of shitcode.
        if (!TryComp<MobStateComponent>(args.Target, out var mobState) ||
            mobState.CurrentState != MobState.Dead ||
            !HasComp<RevivalContractComponent>(args.Used)
            || args.Target == null || args.Handled) // Todo: Give this an actor comp check when done testing.
            return;

        // Non-devils can't offer deals silly.
        if (!HasComp<DevilComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("devil-sign-invalid-user"), args.User, PopupType.MediumCaution);
            return;
        }

        // You can't offer two deals at once.
        if (HasComp<PendingRevivalContractComponent>((EntityUid)args.Target))
        {
            var failedPopup = Loc.GetString("revival-contract-use-failed");
            _popupSystem.PopupEntity(failedPopup, uid);
            return;
        }

        // Create pending contract
        var pending = AddComp<PendingRevivalContractComponent>((EntityUid)args.Target);
        pending.Contractee = uid;
        pending.Offerer = args.User;
        pending.ExpiryTime = _timing.CurTime + TimeSpan.FromSeconds(45);

        // Show confirmation
        var sucessPopup = Loc.GetString("revival-contract-use-success", ("target", args.Target));
        _popupSystem.PopupEntity(sucessPopup, uid);


        // Show instructions
        var prompt = Loc.GetString("revival-contract-prompt", ("offerer", args.User));
        _popupSystem.PopupEntity(prompt, (EntityUid)args.Target, (EntityUid)args.Target);
        args.Handled = true;

    }

    private void AddRevivalVerbs(EntityUid target, PendingRevivalContractComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        // Add verbs
        InnateVerb acceptVerb = new()
        {
            Act = () => HandleContractResponse(args.Target, true),
            Text = Loc.GetString("revival-contract-prompt-accept"),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "cheat-death"),
        };

        InnateVerb rejectVerb = new()
        {
            Act = () => HandleContractResponse(args.Target, false),
            Text = Loc.GetString("revival-contract-prompt-reject"),
            Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Actions/devil.rsi"), "heart-broken"),
        };

        args.Verbs.Add(acceptVerb);
        args.Verbs.Add(rejectVerb);
    }
    private void HandleContractResponse(EntityUid target, bool accepted)
    {
        if (!TryComp<PendingRevivalContractComponent>(target, out var pending))
            return;

        if (accepted)
        {
            if (TryComp<RevivalContractComponent>(pending.Contractee, out var contract))
            {
                contract.Signer = target;
                _rejuvenate.PerformRejuvenate(target);
                _popupSystem.PopupEntity(Loc.GetString("revival-contract-accepted"), target);

               _contract.TryTransferSouls(contract.ContractOwner, contract.Signer, 1);
            }
        }
        else
        {
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-rejected"), target);
        }

        RemComp<PendingRevivalContractComponent>(target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<PendingRevivalContractComponent>();

        while (query.MoveNext(out var uid, out var pending))
        {
            if (_timing.CurTime <= pending.ExpiryTime)
                continue;
            _popupSystem.PopupEntity(Loc.GetString("revival-contract-expired"), uid);
            RemComp<PendingRevivalContractComponent>(uid);

        }
    }


}
