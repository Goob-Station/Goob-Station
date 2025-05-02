// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.OnPray.HealNearOnPray;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;

namespace Content.Goobstation.Server.OnPray.HealUserOnPray;

public sealed partial class HealUserOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealUserOnPrayComponent, NullrodPrayEvent>(OnPray);
    }
    private void OnPray(EntityUid uid, HealUserOnPrayComponent comp, ref NullrodPrayEvent args)
    {
        _damageable.TryChangeDamage(uid, comp.Damage);
    }
}
