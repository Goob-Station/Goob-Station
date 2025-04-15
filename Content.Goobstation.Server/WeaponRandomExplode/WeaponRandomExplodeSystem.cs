// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.WeaponRandomExplode
{
    public sealed class WeaponRandomExplodeSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WeaponRandomExplodeComponent, ShotAttemptedEvent>(OnShot);
        }

        private void OnShot(EntityUid uid, WeaponRandomExplodeComponent component, ShotAttemptedEvent args)
        {
            if (component.explosionChance <= 0)
                return;

            TryComp<BatteryComponent>(uid, out var battery);
            if (battery == null || battery.CurrentCharge <= 0)
                return;

            if (_random.Prob(component.explosionChance))
            {
                var intensity = 1;
                if (component.multiplyByCharge > 0)
                {
                    intensity = Convert.ToInt32(component.multiplyByCharge * (battery.CurrentCharge / 100));
                }

                _explosionSystem.QueueExplosion(
                    (EntityUid) uid,
                    typeId: "Default",
                    totalIntensity: intensity,
                    slope: 5,
                    maxTileIntensity: 10);
                QueueDel(uid);
            }
        }
    }
}