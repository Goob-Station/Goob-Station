// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles slime taming, likely to be expanded in the future.
/// </summary>
public partial class XenobiologySystem
{
    private void InitializeTaming() =>
        SubscribeLocalEvent<SlimeComponent, InteractionSuccessEvent>(OnTame);

    private void OnTame(Entity<SlimeComponent> ent, ref InteractionSuccessEvent args)
    {
        var (slime, comp) = ent;
        var coords = Transform(slime).Coordinates;
        var user = args.User;
        if (comp.Tamer.HasValue)
            return;

        Spawn(ent.Comp.TameEffect, coords);
        comp.Tamer = user;
        Dirty(ent);
    }
}
