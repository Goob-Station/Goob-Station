// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Actions;
using Content.Shared.Actions;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;

namespace Content.Client.Charges;

public sealed class ChargesSystem : SharedChargesSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    private Dictionary<EntityUid, int> _lastCharges = new();
    private Dictionary<EntityUid, int> _tempLastCharges = new();

    public override void Update(float frameTime)
    {
        // Technically this should probably be in frameupdate but no one will ever notice a tick of delay on this.
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        // Update recharging actions. Server doesn't actually care about this and it's a waste of performance, actions are immediate.
        var query = AllEntityQuery<AutoRechargeComponent, LimitedChargesComponent>();

        while (query.MoveNext(out var uid, out var recharge, out var charges))
        {
            if (_actions.GetAction(uid, false) is not {} action)
                continue;

            var current = GetCurrentCharges((uid, charges, recharge));

            if (!_lastCharges.TryGetValue(uid, out var last) || current != last)
            {
                _actions.UpdateAction(action);
            }

            _tempLastCharges[uid] = current;
        }

        _lastCharges.Clear();

        foreach (var (uid, value) in _tempLastCharges)
        {
            _lastCharges[uid] = value;
        }

        _tempLastCharges.Clear();
    }
}
