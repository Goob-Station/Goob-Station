// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.MalfunctionAi;
using Content.Shared.Alert.Components;

namespace Content.Goobstation.Client.MalfunctionAi;

/// <summary>
/// Feeds the current processing power to the Malfunction AI's HUD counter alert.
/// (Hacked APC visuals are driven server-side via the APC charge state: hacked APCs
/// reuse the blue emagged screen.)
/// </summary>
public sealed class MalfunctionAiSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfunctionAiComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
    }

    private void OnGetCounterAmount(Entity<MalfunctionAiComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.PowerAlert != args.Alert)
            return;

        args.Amount = ent.Comp.ProcessingPower.Int();
    }
}
