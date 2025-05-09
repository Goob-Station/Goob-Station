// SPDX-FileCopyrightText: 2025 Coenx-flex <coengmurray@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Popups;

namespace Content.Shared.AbilitySuppression;

public sealed partial class MagicSuppressionSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicSuppressionComponent, InventoryRelayedEvent<CheckMagicSuppressionEvent>>(OnCheckSuppression);
        SubscribeLocalEvent<MagicSuppressionComponent, ExaminedEvent>(OnMagicSuppressionExamine);

    }

    public bool TryMagicSuppressed(EntityUid ent)
    {
        // Check if wearing an item that does it
        var sup = new CheckMagicSuppressionEvent();
        RaiseLocalEvent(ent, sup);

        if (sup.Cancelled)
        {
            _popup.PopupClient(Loc.GetString("suppression-ability-block", ("name", Name(sup.Blocker))), ent, ent, PopupType.Medium);
            return true;
        }

        return false;
    }

    private void OnCheckSuppression(Entity<MagicSuppressionComponent> ent, ref InventoryRelayedEvent<CheckMagicSuppressionEvent> args)
    {
        args.Args.Blocker = ent;
        args.Args.Cancelled = true;
    }

    private void OnMagicSuppressionExamine(Entity<MagicSuppressionComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("suppression-item-examine"));
    }
}
