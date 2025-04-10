// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract class SharedVoidCloakSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidCloakHoodComponent, EntParentChangedMessage>(OnEntParentChanged);
        SubscribeLocalEvent<VoidCloakHoodComponent, EntityTerminatingEvent>(OnTerminating);

        SubscribeLocalEvent<VoidCloakComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
    }

    private void OnCheckMagicItem(Entity<VoidCloakComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
    {
        if (!ent.Comp.Transparent)
            args.Args.Handled = true;
    }

    private void OnTerminating(Entity<VoidCloakHoodComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!TryComp(ent, out AttachedClothingComponent? attached))
            return;

        if (TerminatingOrDeleted(attached.AttachedUid))
            return;

        if (!TryComp(attached.AttachedUid, out VoidCloakComponent? comp))
            return;

        MakeCloakVisible(attached.AttachedUid, comp);
    }

    private void OnEntParentChanged(Entity<VoidCloakHoodComponent> ent, ref EntParentChangedMessage args)
    {
        if (!TryComp(ent, out AttachedClothingComponent? attached))
            return;

        if (TerminatingOrDeleted(attached.AttachedUid))
            return;

        if (!TryComp(attached.AttachedUid, out VoidCloakComponent? comp))
            return;

        if (args.Transform.ParentUid == attached.AttachedUid) // If we unequip hood (new parent is cloak)
            MakeCloakVisible(attached.AttachedUid, comp);
        else // If we equip the hood (mew parent is heretic)
            MakeCloakTransparent(attached.AttachedUid, comp);
    }

    private void MakeCloakTransparent(EntityUid cloak, VoidCloakComponent comp)
    {
        comp.Transparent = true;
        _clothing.SetEquippedPrefix(cloak, "transparent-");
        _appearance.SetData(cloak, VoidCloakVisuals.Transparent, true);

        if (_net.IsClient)
            return;

        EnsureComp<StripMenuInvisibleComponent>(cloak);
        UpdatePressureProtection(cloak, false);
    }

    private void MakeCloakVisible(EntityUid cloak, VoidCloakComponent comp)
    {
        comp.Transparent = false;
        _clothing.SetEquippedPrefix(cloak, null);
        _appearance.SetData(cloak, VoidCloakVisuals.Transparent, false);

        if (_net.IsClient)
            return;

        RemCompDeferred<StripMenuInvisibleComponent>(cloak);
        UpdatePressureProtection(cloak, true);
    }

    protected virtual void UpdatePressureProtection(EntityUid cloak, bool enabled)
    {
    }
}