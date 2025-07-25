// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the mutation shop ability.
/// Opens the mutation shop
/// </summary>
public sealed class SharedWerewolfMutationShopSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfMutationShopComponent, OpenMutationShopEvent>(OnOpenMutationShop);
    }

    private void OnOpenMutationShop(Entity<WerewolfMutationShopComponent> entity, ref OpenMutationShopEvent args)
    {

    }
}
