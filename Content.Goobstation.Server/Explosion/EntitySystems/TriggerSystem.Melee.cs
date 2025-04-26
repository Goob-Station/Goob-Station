// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Explosion.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem
{
    private void InitializeMelee()
    {
        SubscribeLocalEvent<TriggerOnMeleeComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<TriggerOnMeleeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count <= 0)
            return;

        _trigger.Trigger(ent, ent);
    }
}
