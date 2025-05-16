// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.PairedExtendable;

public sealed class SharedPairedExtendableSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RightPairedExtendableUserComponent, ComponentInit>(OnInitRight);
        SubscribeLocalEvent<LeftPairedExtendableUserComponent, ComponentInit>(OnInitLeft);

        SubscribeLocalEvent<RightPairedExtendableUserComponent, ComponentShutdown>(OnShutdownRight);
        SubscribeLocalEvent<LeftPairedExtendableUserComponent, ComponentShutdown>(OnShutdownLeft);
    }

    private void OnInitRight(EntityUid uid, RightPairedExtendableUserComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnInitLeft(EntityUid uid, LeftPairedExtendableUserComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnShutdownRight(EntityUid uid, RightPairedExtendableUserComponent comp, ref ComponentShutdown args)
    {
        Del(comp.ExtendableUid);
        _actions.RemoveAction(comp.ActionUid);
    }

    private void OnShutdownLeft(EntityUid uid, LeftPairedExtendableUserComponent comp, ref ComponentShutdown args)
    {
        Del(comp.ExtendableUid);
        _actions.RemoveAction(comp.ActionUid);
    }
}
