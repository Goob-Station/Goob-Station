// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Explosion.Components;
using Content.Server._Shitmed.ItemSwitch;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Explosion.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed class ExplodeOnMeleeHitSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExplodeOnMeleeHitComponent, MeleeHitEvent>(OnHit, before: [typeof(ItemSwitchSystem)]);
    }

    private void OnHit(Entity<ExplodeOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        if (!TryComp(ent, out ExplosiveComponent? explosive))
            return;

        foreach (var hit in args.HitEntities)
        {
            _explosions.QueueExplosion(hit,
                explosive.ExplosionType,
                explosive.TotalIntensity,
                explosive.IntensitySlope,
                explosive.MaxIntensity,
                explosive.TileBreakScale,
                explosive.MaxTileBreak,
                explosive.CanCreateVacuum,
                args.User);
        }
    }
}
