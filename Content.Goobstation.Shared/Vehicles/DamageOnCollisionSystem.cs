// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Vehicles.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Vehicles.Systems;

public sealed partial class VehicleDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DamageOnCollisionComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, DamageOnCollisionComponent component, ref StartCollideEvent args)
    {
        // if (args.OurFixtureId != "Hard" || args.OtherFixtureId != "Hard")
        //     return;

        if (component.LastHit != null && _gameTiming.CurTime < component.LastHit + TimeSpan.FromSeconds(component.DamageCooldown))
            return;

        var speed = args.OurBody.LinearVelocity.Length();
        if (speed < component.MinImpactSpeed)
            return;

        var otherUid = args.OtherEntity;
        if (HasComp<VehicleComponent>(otherUid) || otherUid == uid)
            return;

        _damageable.TryChangeDamage(otherUid, component.Damage);
        component.LastHit = _gameTiming.CurTime;

        if (component.Sound != null)
            _audio.PlayPredicted(component.Sound, uid, null);
    }
}
