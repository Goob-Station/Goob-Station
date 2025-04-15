// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._Goobstation.Weapons.FoldingWeapon;

public sealed class FoldingWeaponSystem : EntitySystem
{
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoldingWeaponComponent, WieldAttemptEvent>(OnBeforeWield);
        SubscribeLocalEvent<FoldingWeaponComponent, ItemToggledEvent>(OnToggle);
        SubscribeLocalEvent<FoldingWeaponComponent, AttemptShootEvent>(OnShootAttempt);
    }

    private void OnShootAttempt(Entity<FoldingWeaponComponent> ent, ref AttemptShootEvent args)
    {
        if (!CanShoot(ent))
            args.Cancelled = true;
    }

    private void OnToggle(Entity<FoldingWeaponComponent> ent, ref ItemToggledEvent args)
    {
        if (args.User != null && TryComp(ent, out WieldableComponent? wieldable))
            _wieldable.TryUnwield(ent, wieldable, args.User.Value, true);

        var prefix = args.Activated ? "unfolded" : "folded";

        _item.SetHeldPrefix(ent, prefix);
        _clothing.SetEquippedPrefix(ent, prefix);
    }

    private void OnBeforeWield(Entity<FoldingWeaponComponent> ent, ref WieldAttemptEvent args)
    {
        if (!CanShoot(ent))
            args.Cancel();
    }

    private bool CanShoot(EntityUid ent)
    {
        return !TryComp(ent, out ItemToggleComponent? toggle) || toggle.Activated;
    }
}