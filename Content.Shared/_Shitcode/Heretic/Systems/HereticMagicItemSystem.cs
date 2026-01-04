// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Heretic.Systems;

public sealed class HereticMagicItemSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticMagicItemComponent, CheckMagicItemEvent>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, HeldRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, ExaminedEvent>(OnMagicItemExamine);
        SubscribeLocalEvent<HereticMagicItemComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HereticMagicItemComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<HereticMagicItemComponent, GotUnequippedHandEvent>(OnUnequipHand);
    }

    private void OnUnequipHand(Entity<HereticMagicItemComponent> ent, ref GotUnequippedHandEvent args)
    {
        RaiseLostFocusEvent(args.User);
    }

    private void OnUnequip(Entity<HereticMagicItemComponent> ent, ref GotUnequippedEvent args)
    {
        RaiseLostFocusEvent(args.Equipee);
    }

    private void OnShutdown(Entity<HereticMagicItemComponent> ent, ref ComponentShutdown args)
    {
        var parent = Transform(ent).ParentUid;

        if (TerminatingOrDeleted(parent))
            return;

        RaiseLostFocusEvent(parent);
    }

    private void RaiseLostFocusEvent(EntityUid uid)
    {
        var checkEv = new CheckMagicItemEvent();
        RaiseLocalEvent(uid, checkEv);
        if (checkEv.Handled)
            return;

        var ev = new HereticLostFocusEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref CheckMagicItemEvent args)
        => args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref HeldRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;

    private void OnMagicItemExamine(Entity<HereticMagicItemComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<HereticComponent>(args.Examiner))
            return;

        args.PushMarkup(Loc.GetString("heretic-magicitem-examine"));
    }
}
