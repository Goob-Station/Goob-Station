// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.MantisBlades;
using Content.Server.Emp;
using Content.Shared.Emp;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.MantisBlades;

public sealed class MantisBladesSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RightMantisBladeUserComponent, ToggleRightMantisBladeEvent>(OnToggleRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, ToggleLeftMantisBladeEvent>(OnToggleLeft);

        SubscribeLocalEvent<RightMantisBladeUserComponent, EmpPulseEvent>(OnEmpRight);
        SubscribeLocalEvent<LeftMantisBladeUserComponent, EmpPulseEvent>(OnEmpLeft);
    }

    private bool ToggleBlade<T>(EntityUid ent) where T : Component, IMantisBladeUserComponent
    {
        if (!TryComp<T>(ent, out var comp))
            return false;

        if (HasComp<EmpDisabledComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("cyberware-disabled-emp"), ent, ent);
            return false;
        }

        var hand = _hands.EnumerateHands(ent).FirstOrDefault(hand => hand.Location == (typeof(T) == typeof(RightMantisBladeUserComponent) ? HandLocation.Right : HandLocation.Left));
        if (hand == null)
            return false;

        var activeItem = hand.HeldEntity;
        if (activeItem.HasValue && activeItem == comp.BladeUid)
        {
            Del(activeItem);
            comp.BladeUid = null;
            _audio.PlayPvs(comp.RetractSound, ent);
            return true;
        }

        var newBlade = Spawn(comp.BladeProto, Transform(ent).Coordinates);
        if (!_hands.TryPickup(ent, newBlade, hand.Name))
        {
            Del(newBlade);
            _popup.PopupEntity(Loc.GetString("mantis-blade-hand-busy"), ent, ent);
            return false;
        }

        _audio.PlayPvs(comp.ExtendSound, ent);
        comp.BladeUid = newBlade;
        return true;
    }
    private void OnToggleRight(EntityUid uid, RightMantisBladeUserComponent component, ToggleRightMantisBladeEvent args)
    {
        args.Handled = ToggleBlade<RightMantisBladeUserComponent>(uid);
    }

    private void OnToggleLeft(EntityUid uid, LeftMantisBladeUserComponent component, ToggleLeftMantisBladeEvent args)
    {
        args.Handled = ToggleBlade<LeftMantisBladeUserComponent>(uid);
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
}
