// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.Damage;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

public sealed partial class HealNearOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealNearOnPrayComponent, NullrodPrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, HealNearOnPrayComponent comp, ref NullrodPrayEvent args)
    {
        var userMapCoordinates = _transform.GetMapCoordinates(uid);
        var userCoordinates = _transform.ToCoordinates(userMapCoordinates);

        var query = EntityQueryEnumerator<DamageableComponent>();
        while (query.MoveNext(out var other, out var damageable))
        {
            if (HasComp<WeakToHolyComponent>(other))
                continue;

            var otherMapCoordinates = _transform.GetMapCoordinates(other);
            var otherCoordinates = _transform.ToCoordinates(otherMapCoordinates);

            if (_transform.InRange(userCoordinates, otherCoordinates, comp.Range))
                _damageable.TryChangeDamage(other, comp.Damage);
        }
    }

}
