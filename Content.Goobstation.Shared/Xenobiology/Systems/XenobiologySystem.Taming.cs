// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction.Events;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles slime taming, likely to be expanded in the future.
/// </summary>
public partial class XenobiologySystem
{
    private void InitializeTaming() =>
        SubscribeLocalEvent<SlimeComponent, InteractionSuccessEvent>(OnTame);

    private void OnTame(Entity<SlimeComponent> ent, ref InteractionSuccessEvent args)
    {
        if (ent.Comp.Tamer.HasValue
            || _net.IsClient)
            return;

        var (slime, comp) = ent;
        var coords = Transform(slime).Coordinates;
        var user = args.User;

        // Hearts VFX - Slime taming is seperate to core Pettable Component/System
        Spawn(ent.Comp.TameEffect, coords);
        comp.Tamer = user;
        Dirty(ent);
    }
}
