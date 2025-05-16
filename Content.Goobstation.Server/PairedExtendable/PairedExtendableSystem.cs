// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.PairedExtendable;
using Content.Server.Emp;
using Content.Shared.Emp;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.PairedExtendable;

public sealed class PairedExtendableSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RightPairedExtendableUserComponent, ToggleRightExtendableEvent>(OnToggleRight);
        SubscribeLocalEvent<LeftPairedExtendableUserComponent, ToggleLeftExtendableEvent>(OnToggleLeft);

        SubscribeLocalEvent<RightPairedExtendableUserComponent, EmpPulseEvent>(OnEmpRight);
        SubscribeLocalEvent<LeftPairedExtendableUserComponent, EmpPulseEvent>(OnEmpLeft);
    }

    private bool ToggleExtendable<T>(EntityUid ent) where T : PairedExtendableUserComponent
    {
        if (!TryComp<T>(ent, out var comp))
            return false;

        if (HasComp<EmpDisabledComponent>(ent) && comp.AffectedByEmp)
        {
            _popup.PopupEntity(Loc.GetString("cyberware-disabled-emp"), ent, ent);
            return false;
        }

        var hand = _hands.EnumerateHands(ent).FirstOrDefault(hand => hand.Location == (typeof(T) == typeof(RightPairedExtendableUserComponent) ? HandLocation.Right : HandLocation.Left));
        if (hand == null)
            return false;

        var activeItem = hand.HeldEntity;
        if (activeItem.HasValue && activeItem == comp.ExtendableUid)
        {
            Del(activeItem);
            comp.ExtendableUid = null;
            _audio.PlayPvs(comp.RetractSound, ent);
            return true;
        }

        var newExtendable = Spawn(comp.ExtendableProto, Transform(ent).Coordinates);
        if (!_hands.TryPickup(ent, newExtendable, hand.Name))
        {
            Del(newExtendable);
            _popup.PopupEntity(Loc.GetString("paired-extendable-hand-busy"), ent, ent);
            return false;
        }

        _audio.PlayPvs(comp.ExtendSound, ent);
        comp.ExtendableUid = newExtendable;
        return true;
    }
    private void OnToggleRight(EntityUid uid, RightPairedExtendableUserComponent component, ToggleRightExtendableEvent args)
    {
        args.Handled = ToggleExtendable<RightPairedExtendableUserComponent>(uid);
    }

    private void OnToggleLeft(EntityUid uid, LeftPairedExtendableUserComponent component, ToggleLeftExtendableEvent args)
    {
        args.Handled = ToggleExtendable<LeftPairedExtendableUserComponent>(uid);
    }

    private void OnEmpRight(EntityUid uid, RightPairedExtendableUserComponent comp, ref EmpPulseEvent args)
    {
        if (!comp.AffectedByEmp)
            return;

        args.Affected = true;
        args.Disabled = true;
    }

    private void OnEmpLeft(EntityUid uid, LeftPairedExtendableUserComponent comp, ref EmpPulseEvent args)
    {
        if (!comp.AffectedByEmp)
            return;

        args.Affected = true;
        args.Disabled = true;
    }
}
