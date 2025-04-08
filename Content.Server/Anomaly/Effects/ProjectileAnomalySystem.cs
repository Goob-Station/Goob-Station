// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ThunderBear2006 <100388962+ThunderBear2006@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Server.Anomaly.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This handles <see cref="ProjectileAnomalyComponent"/> and the events from <seealso cref="AnomalySystem"/>
/// </summary>
public sealed class ProjectileAnomalySystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectileAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<ProjectileAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
    }

    private void OnPulse(EntityUid uid, ProjectileAnomalyComponent component, ref AnomalyPulseEvent args)
    {
        ShootProjectilesAtEntities(uid, component, args.Severity * args.PowerModifier);
    }

    private void OnSupercritical(EntityUid uid, ProjectileAnomalyComponent component, ref AnomalySupercriticalEvent args)
    {
        ShootProjectilesAtEntities(uid, component, args.PowerModifier);
    }

    private void ShootProjectilesAtEntities(EntityUid uid, ProjectileAnomalyComponent component, float severity)
    {
        var projectileCount = (int) MathF.Round(MathHelper.Lerp(component.MinProjectiles, component.MaxProjectiles, severity));
        var xformQuery = GetEntityQuery<TransformComponent>();
        var mobQuery = GetEntityQuery<MobStateComponent>();
        var xform = xformQuery.GetComponent(uid);

        var inRange = _lookup.GetEntitiesInRange(uid, component.ProjectileRange * severity, LookupFlags.Dynamic).ToList();
        _random.Shuffle(inRange);
        var priority = new List<EntityUid>();
        foreach (var entity in inRange)
        {
            if (mobQuery.HasComponent(entity))
                priority.Add(entity);
        }

        Log.Debug($"shots: {projectileCount}");
        while (projectileCount > 0)
        {
            Log.Debug($"{projectileCount}");
            var target = priority.Any()
                ? _random.PickAndTake(priority)
                : _random.Pick(inRange);

            var targetCoords = xformQuery.GetComponent(target).Coordinates.Offset(_random.NextVector2(0.5f));

            ShootProjectile(
                uid, component,
                xform.Coordinates,
                targetCoords,
                severity);
            projectileCount--;
        }
    }

    private void ShootProjectile(
        EntityUid uid,
        ProjectileAnomalyComponent component,
        EntityCoordinates coords,
        EntityCoordinates targetCoords,
        float severity)
    {
        var mapPos = coords.ToMap(EntityManager, _xform);

        var spawnCoords = _mapManager.TryFindGridAt(mapPos, out var gridUid, out _)
                ? coords.WithEntityId(gridUid, EntityManager)
                : new(_mapManager.GetMapEntityId(mapPos.MapId), mapPos.Position);

        var ent = Spawn(component.ProjectilePrototype, spawnCoords);
        var direction = targetCoords.ToMapPos(EntityManager, _xform) - mapPos.Position;

        if (!TryComp<ProjectileComponent>(ent, out var comp))
            return;

        comp.Damage *= severity;

        _gunSystem.ShootProjectile(ent, direction, Vector2.Zero, uid, uid, component.ProjectileSpeed);
    }
}