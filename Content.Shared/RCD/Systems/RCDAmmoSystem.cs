// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Robust.Shared.Timing;

namespace Content.Shared.RCD.Systems;

public sealed class RCDAmmoSystem : EntitySystem
{
    [Dependency] private readonly SharedChargesSystem _sharedCharges = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RCDAmmoComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RCDAmmoComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnExamine(EntityUid uid, RCDAmmoComponent comp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var examineMessage = Loc.GetString("rcd-ammo-component-on-examine", ("charges", comp.Charges));
        args.PushText(examineMessage);
    }

    private void OnAfterInteract(EntityUid uid, RCDAmmoComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || !_timing.IsFirstTimePredicted)
            return;

        if (args.Target is not { Valid: true } target ||
            !HasComp<RCDComponent>(target) ||
            !TryComp<LimitedChargesComponent>(target, out var charges))
            return;

        var current = _sharedCharges.GetCurrentCharges((target, charges));
        var user = args.User;
        args.Handled = true;
        var count = Math.Min(charges.MaxCharges - current, comp.Charges);
        if (count <= 0)
        {
            _popup.PopupClient(Loc.GetString("rcd-ammo-component-after-interact-full"), target, user);
            return;
        }

        _popup.PopupClient(Loc.GetString("rcd-ammo-component-after-interact-refilled"), target, user);
        _sharedCharges.AddCharges(target, count);
        comp.Charges -= count;
        Dirty(uid, comp);

        // prevent having useless ammo with 0 charges
        if (comp.Charges <= 0)
            PredictedQueueDel(uid); // Goobstation - Fix prediction errors
    }
}
