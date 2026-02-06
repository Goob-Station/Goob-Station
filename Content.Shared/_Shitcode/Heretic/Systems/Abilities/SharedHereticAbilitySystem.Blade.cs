// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BramvanZijp <56019239+BramvanZijp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Stunnable;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Standing;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stam = default!;

    protected virtual void SubscribeBlade()
    {
        SubscribeLocalEvent<SilverMaelstromComponent, GetClothingStunModifierEvent>(OnBladeStunModify);
        SubscribeLocalEvent<SilverMaelstromComponent, DropHandItemsEvent>(OnBladeDropItems,
            before: new[] { typeof(SharedHandsSystem) });

        SubscribeLocalEvent<EventHereticSacraments>(OnSacraments);
    }

    private void OnSacraments(EventHereticSacraments args)
    {
        if (!TryUseAbility(args.Performer, args))
            return;

        args.Handled = true;

        _statusNew.TryUpdateStatusEffectDuration(args.Performer, args.Status, args.Time);
    }

    private void OnBladeDropItems(Entity<SilverMaelstromComponent> ent, ref DropHandItemsEvent args)
    {
        args.Handled = true;
    }

    private void OnBladeStunModify(Entity<SilverMaelstromComponent> ent, ref GetClothingStunModifierEvent args)
    {
        args.Modifier *= 0.5f;
    }
}
