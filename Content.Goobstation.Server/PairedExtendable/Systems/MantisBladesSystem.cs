// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PairedExtendable.MantisBlades;
using Content.Server.Emp;
using Content.Shared.Actions;
using Content.Shared.Emp;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.PairedExtendable.Systems;

public sealed class MantisBladesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PairedExtendableSystem _pairedExtendable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentInit>(OnInitRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentInit>(OnInitLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, EmpPulseEvent>(OnEmpRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, EmpPulseEvent>(OnEmpLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, ToggleRightMantisBladeEvent>(OnToggleRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ToggleLeftMantisBladeEvent>(OnToggleLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, ComponentShutdown>(OnShutdownRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ComponentShutdown>(OnShutdownLeft);
    }
    private void OnInitRight(EntityUid uid, RightMantisBladeUserComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnInitLeft(EntityUid uid, LeftMantisBladeUserComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnEmpRight(EntityUid uid, RightMantisBladeUserComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
    }

    private void OnEmpLeft(EntityUid uid, LeftMantisBladeUserComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
    }

    private void OnToggleRight(Entity<RightMantisBladeUserComponent> ent, ref ToggleRightMantisBladeEvent args)
    {
        if (HasComp<EmpDisabledComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("mantis-blade-disabled-emp"), ent, ent);
            return;
        }

        args.Handled = _pairedExtendable.ToggleExtendable<RightMantisBladeUserComponent>(ent);
    }

    private void OnToggleLeft(Entity<LeftMantisBladeUserComponent> ent, ref ToggleLeftMantisBladeEvent args)
    {
        if (HasComp<EmpDisabledComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("mantis-blade-disabled-emp"), ent, ent);
            return;
        }

        args.Handled = _pairedExtendable.ToggleExtendable<LeftMantisBladeUserComponent>(ent);
    }

    private void OnShutdownRight(EntityUid uid, RightMantisBladeUserComponent comp, ref ComponentShutdown args)
    {
        Del(comp.ExtendableUid);
        _actions.RemoveAction(comp.ActionUid);
    }

    private void OnShutdownLeft(EntityUid uid, LeftMantisBladeUserComponent comp, ref ComponentShutdown args)
    {
        Del(comp.ExtendableUid);
        _actions.RemoveAction(comp.ActionUid);
    }
}
