// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.PairedExtendable;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.PairedExtendable;

public sealed class PairedExtendableSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public bool ToggleExtendable<T>(EntityUid ent) where T : PairedExtendableUserComponent
    {
        if (!TryComp<T>(ent, out var comp))
            return false;

        Hand? ourHand = null;
        foreach (var hand in _hands.EnumerateHands(ent))
        {
            if (hand.Location == (comp is RightPairedExtendableUserComponent ? HandLocation.Right : HandLocation.Left))
            {
                ourHand = hand;
                break;
            }
        }

        if (ourHand == null)
            return false;

        var activeItem = ourHand.HeldEntity;

        if (activeItem.HasValue
        && activeItem == comp.ExtendableUid)
        {
            Del(activeItem);
            comp.ExtendableUid = null;
            _audio.PlayPvs(comp.RetractSound, ent);
            return true;
        }

        var newExtendable = Spawn(comp.ExtendableProto, Transform(ent).Coordinates);
        if (!_hands.TryPickup(ent, newExtendable, ourHand.Name))
        {
            Del(newExtendable);
            _popup.PopupEntity(Loc.GetString("paired-extendable-hand-busy"), ent, ent);
            return false;
        }

        _audio.PlayPvs(comp.ExtendSound, ent);
        comp.ExtendableUid = newExtendable;
        if (comp.MakeUnremovable)
            EnsureComp<UnremoveableComponent>(newExtendable);

        return true;
    }
}
