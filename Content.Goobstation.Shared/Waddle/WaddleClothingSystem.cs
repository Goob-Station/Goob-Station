// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.Waddle;

public sealed class WaddleClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WaddleWhenWornComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<WaddleWhenWornComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(Entity<WaddleWhenWornComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var user = args.Wearer;
        // TODO: refcount
        if (EnsureComp<WaddleAnimationComponent>(user, out var waddle))
            return;

        ent.Comp.AddedWaddle = true;
        Dirty(ent);

        var comp = ent.Comp;
        if (comp.AnimationLength is {} length)
            waddle.AnimationLength = length;
        if (comp.HopIntensity is {} hopIntensity)
            waddle.HopIntensity = hopIntensity;
        if (comp.TumbleIntensity is {} tumbleIntensity)
            waddle.TumbleIntensity = tumbleIntensity;
        if (comp.RunAnimationLengthMultiplier is {} multiplier)
            waddle.RunAnimationLengthMultiplier = multiplier;

        // very unlikely that some waddle clothing doesn't change at least 1 property, don't bother doing change detection meme
        Dirty(user, waddle);
    }

    private void OnGotUnequipped(Entity<WaddleWhenWornComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!ent.Comp.AddedWaddle)
            return;

        // TODO: refcount
        RemComp<WaddleAnimationComponent>(args.Wearer);
        ent.Comp.AddedWaddle = false;
        Dirty(ent);
    }
}
