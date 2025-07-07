// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Prying.Components;
using Content.Shared.Prying.Components;

namespace Content.Shared._DV.Prying.Systems;

public sealed partial class PlayerToolModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerToolModifierComponent, GetPryTimeModifierEvent>(OnPry);
    }

    private void OnPry(Entity<PlayerToolModifierComponent> ent, ref GetPryTimeModifierEvent args)
    {
        args.PryTimeModifier *= ent.Comp.PryTimeMultiplier;
    }
}
