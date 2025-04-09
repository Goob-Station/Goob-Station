// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Lightning;
using Content.Server.Tesla.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// Fires electric arcs at surrounding objects.
/// </summary>
public sealed class LightningArcShooterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightningArcShooterComponent, MapInitEvent>(OnShooterMapInit);
    }

    private void OnShooterMapInit(EntityUid uid, LightningArcShooterComponent component, ref MapInitEvent args)
    {
        component.NextShootTime = _gameTiming.CurTime + TimeSpan.FromSeconds(component.ShootMaxInterval);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightningArcShooterComponent>();
        while (query.MoveNext(out var uid, out var arcShooter))
        {
            if (arcShooter.NextShootTime > _gameTiming.CurTime)
                continue;

            ArcShoot(uid, arcShooter);
            var delay = TimeSpan.FromSeconds(_random.NextFloat(arcShooter.ShootMinInterval, arcShooter.ShootMaxInterval));
            arcShooter.NextShootTime += delay;
        }
    }

    private void ArcShoot(EntityUid uid, LightningArcShooterComponent component)
    {
        var arcs = _random.Next(1, component.MaxLightningArc);
        _lightning.ShootRandomLightnings(uid, component.ShootRange, arcs, component.LightningPrototype, component.ArcDepth);
    }
}