// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Damage.Components;
using Content.Shared.Damage.Events;

namespace Content.Shared._DV.Damage.Systems;

public sealed partial class BonusStaminaDamageSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BonusStaminaDamageComponent, StaminaMeleeHitEvent>(OnStamHit);
    }

    private void OnStamHit(Entity<BonusStaminaDamageComponent> ent, ref StaminaMeleeHitEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}
