// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.CloneProjector;

public abstract class SharedCloneProjectorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolographicCloneComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<HolographicCloneComponent> clone, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || clone.Comp.HostEntity is not { } host)
            return;

        // Stop clones from punching their host.
        // Don't be a shitter.
        foreach (var hitEntity in args.HitEntities)
        {
            if (hitEntity != host)
                continue;

            args.BonusDamage = -args.BaseDamage;
        }
    }

}
