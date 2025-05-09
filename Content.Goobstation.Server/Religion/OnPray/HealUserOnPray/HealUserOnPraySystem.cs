// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Server.OnPray.HealNearOnPray;
using Content.Goidastation.Shared.Religion.Nullrod;
using Content.Shared.Damage;

namespace Content.Goidastation.Server.OnPray.HealUserOnPray;

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
