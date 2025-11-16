// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.PairedExtendable;

public sealed class PairedExtendableSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <summary>
    /// Tries to extend or retract an extendable out from the user.
    /// </summary>
    /// <param name="currentExtendable">Current extendable, if any</param>
    /// <param name="newExtendable">New extendable uid</param>
    /// <returns>false if there were to hands, they were busy etc., so an action that triggered this shouldn't be handled</returns>
    public bool ToggleExtendable (EntityUid user, string protoId, HandLocation side, out EntityUid? newExtendable, EntityUid? currentExtendable = null, bool makeUnremovable = true)
    {
        newExtendable = null;
        string? pickedHand = null;
        foreach (var hand in _hands.EnumerateHands(user))
        {
            if (!_hands.TryGetHand(user, hand, out var handPos)
                || handPos.Value.Location != side)
                continue;

            pickedHand = hand;
            break;
        }

        if (pickedHand == null)
            return false;

        if (_hands.GetHeldItem(user, pickedHand) is { } activeItem
            && activeItem == currentExtendable)
        {
            Del(activeItem);
            return true;
        }

        newExtendable = Spawn(protoId, Transform(user).Coordinates);
        if (!_hands.TryPickup(user, newExtendable.Value, pickedHand))
        {
            Del(newExtendable);
            newExtendable = null;
            _popup.PopupEntity(Loc.GetString("paired-extendable-hand-busy"), user, user);
            return false;
        }

        if (makeUnremovable)
            EnsureComp<UnremoveableComponent>(newExtendable.Value);

        return true;
    }
}
