// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Buckle.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Vehicles;

public sealed partial class IgnoreCollisionSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StrapComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<StrapComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnStrapped(EntityUid uid, StrapComponent component, ref StrappedEvent args)
    {
        ProcessCollision(args.Buckle, true);
    }

    private void OnUnstrapped(EntityUid uid, StrapComponent component, ref UnstrappedEvent args)
    {
        ProcessCollision(args.Buckle, false);
    }

    private void ProcessCollision(EntityUid entity, bool strap)
    {
        if (!TryComp<PhysicsComponent>(entity, out var physics) ||
            !TryComp<FixturesComponent>(entity, out var fixtures))
            return;

        if (strap)
        {
            var ignoreComp = EnsureComp<IgnoreCollisionComponent>(entity);
            ignoreComp.OriginalFixtures ??= new();

            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                ignoreComp.OriginalFixtures[id] = (fixture.CollisionLayer, fixture.CollisionMask);
                _physics.SetCollisionLayer(entity, id, fixture, 0, fixtures, physics);
                _physics.SetCollisionMask(entity, id, fixture, 0, fixtures, physics);
            }
            _physics.SetCanCollide(entity, false, body: physics);
        }
        else
        {
            if (!TryComp<IgnoreCollisionComponent>(entity, out var ignoreComp))
                return;

            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                if (!ignoreComp.OriginalFixtures.TryGetValue(id, out var original))
                    continue;

                _physics.SetCollisionLayer(entity, id, fixture, original.Layer, fixtures, physics);
                _physics.SetCollisionMask(entity, id, fixture, original.Mask, fixtures, physics);
            }
            _physics.SetCanCollide(entity, true, body: physics);
            RemComp<IgnoreCollisionComponent>(entity);
        }
    }
}
