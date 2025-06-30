// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Damage;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Clothing.Systems
{
    public sealed class DamageOverTimeSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly DamageableSystem _damageSys = default!;

        public override void Update(float frameTime)
        {
            var currentTime = _timing.CurTime;
            var query = EntityQueryEnumerator<DamageOverTimeComponent>();
            while (query.MoveNext(out var uid, out var component))
            {
                if (currentTime < component.NextTickTime)
                    continue;
                component.NextTickTime = currentTime + component.Interval;
                _damageSys.TryChangeDamage(uid, component.Damage, ignoreResistances: component.IgnoreResistances);
            }
        }
    }
}