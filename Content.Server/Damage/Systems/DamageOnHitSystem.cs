// SPDX-FileCopyrightText: 2023 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using System.Linq;

namespace Content.Server.Damage.Systems;

public sealed class DamageOnHitSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageOnHitComponent, MeleeHitEvent>(DamageItem);
    }
    // Looks for a hit, then damages the held item an appropriate amount.
    private void DamageItem(EntityUid uid, DamageOnHitComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.Any()) {
            _damageableSystem.TryChangeDamage(uid, component.Damage, component.IgnoreResistances);
        }
    }
}