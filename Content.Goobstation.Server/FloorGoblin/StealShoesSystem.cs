// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Goobstation.Shared.FloorGoblin;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed partial class StealShoesSystem : SharedStealShoesSystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly Content.Server.Body.Systems.BodySystem _body = default!;

    protected override void OnDeathServer(EntityUid uid, StealShoesComponent component)
    {
        if (_containers.TryGetContainer(uid, component.ContainerId, out var container))
        {
            var dropCoords = Transform(uid).Coordinates;
            var toDrop = new List<EntityUid>(container.ContainedEntities);
            foreach (var ent in toDrop)
            {
                _containers.Remove(ent, container);
                _xform.SetCoordinates(ent, dropCoords);
                var angle = _random.NextFloat(0f, MathF.Tau);
                var speed = _random.NextFloat(2.5f, 4.5f);
                var vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                if (TryComp<PhysicsComponent>(ent, out var phys))
                    _physics.SetLinearVelocity(ent, vel);
            }
        }

        _body.GibBody(uid);
    }
}
