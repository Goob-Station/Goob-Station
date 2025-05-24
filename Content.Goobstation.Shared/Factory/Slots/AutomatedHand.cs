// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// Abstraction over a specific hand of the machine.
/// </summary>
public sealed partial class AutomatedHand : AutomationSlot
{
    /// <summary>
    /// The name of the hand to use
    /// </summary>
    [DataField(required: true)]
    public string HandName = string.Empty;

    private SharedHandsSystem _hands;

    private Hand? _hand;

    [ViewVariables]
    public Hand? Hand
    {
        get
        {
            if (_hand != null)
                return _hand;

            _hands.TryGetHand(Owner, HandName, out _hand);
            return _hand;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        _hands = EntMan.System<SharedHandsSystem>();
    }

    public override bool Insert(EntityUid item)
    {
        return Hand is { } hand
            && base.Insert(item)
            && _hands.TryPickup(Owner, item, hand);
    }

    public override bool CanInsert(EntityUid item)
    {
        return Hand is { } hand
            && base.CanInsert(item)
            && _hands.CanPickupToHand(Owner, item, hand);
    }

    public override EntityUid? GetItem(EntityUid? filter)
    {
        if (Hand?.HeldEntity is not { } item
            || _filter.IsBlocked(filter, item))
            return null;

        return item;
    }
}
