// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Administration.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Humanoid;

public sealed partial class BaldifySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BaldifyOnHitComponent, MeleeHitEvent>(OnBaldifyMeleeHit);
    }
    private void OnBaldifyMeleeHit(EntityUid uid, BaldifyOnHitComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.Count < 1)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<HumanoidAppearanceComponent>(hit))
                continue;

            EnsureComp<BaldifyComponent>(hit);
        }
    }
}