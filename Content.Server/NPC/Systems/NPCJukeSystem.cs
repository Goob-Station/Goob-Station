// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.NPC.Components;
using Content.Server.NPC.Events;
using Content.Server.Weapons.Melee;
using Content.Shared.NPC;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.NPC.Systems;

public sealed class NPCJukeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<NPCMeleeCombatComponent> _npcMeleeQuery;
    private EntityQuery<NPCRangedCombatComponent> _npcRangedQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _npcMeleeQuery = GetEntityQuery<NPCMeleeCombatComponent>();
        _npcRangedQuery = GetEntityQuery<NPCRangedCombatComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<NPCJukeComponent, NPCSteeringEvent>(OnJukeSteering);
    }

    private void OnJukeSteering(EntityUid uid, NPCJukeComponent component, ref NPCSteeringEvent args)
    {
        if (component.JukeType == JukeType.AdjacentTile)
        {
            if (_npcRangedQuery.TryGetComponent(uid, out var ranged) &&
                ranged.Status == CombatStatus.NotInSight)
            {
                component.TargetTile = null;
                return;
            }

            if (_timing.CurTime < component.NextJuke)
            {
                component.TargetTile = null;
                return;
            }

            if (!TryComp<MapGridComponent>(args.Transform.GridUid, out var grid))
            {
                component.TargetTile = null;
                return;
            }

            var currentTile = _mapSystem.CoordinatesToTile(args.Transform.GridUid.Value, grid, args.Transform.Coordinates);

            if (component.TargetTile == null)
            {
                var targetTile = currentTile;
                var startIndex = _random.Next(8);
                _physicsQuery.TryGetComponent(uid, out var ownerPhysics);
                var collisionLayer = ownerPhysics?.CollisionLayer ?? 0;
                var collisionMask = ownerPhysics?.CollisionMask ?? 0;

                for (var i = 0; i < 8; i++)
                {
                    var index = (startIndex + i) % 8;
                    var neighbor = ((Direction)index).ToIntVec() + currentTile;
                    var valid = true;

                    // TODO: Probably make this a helper on engine maybe
                    var tileBounds = new Box2(neighbor, neighbor + grid.TileSize);
                    tileBounds = tileBounds.Enlarged(-0.1f);

                    foreach (var ent in _lookup.GetEntitiesIntersecting(args.Transform.GridUid.Value, tileBounds))
                    {
                        if (ent == uid ||
                            !_physicsQuery.TryGetComponent(ent, out var physics) ||
                            !physics.CanCollide ||
                            !physics.Hard ||
                            ((physics.CollisionMask & collisionLayer) == 0x0 &&
                            (physics.CollisionLayer & collisionMask) == 0x0))
                        {
                            continue;
                        }

                        valid = false;
                        break;
                    }

                    if (!valid)
                        continue;

                    targetTile = neighbor;
                    break;
                }

                component.TargetTile ??= targetTile;
            }

            var elapsed = _timing.CurTime - component.NextJuke;

            // Finished juke, reset timer.
            if (elapsed.TotalSeconds > component.JukeDuration ||
                currentTile == component.TargetTile)
            {
                component.TargetTile = null;
                component.NextJuke = _timing.CurTime + TimeSpan.FromSeconds(component.JukeDuration);
                return;
            }

            var targetCoords = _mapSystem.GridTileToWorld(args.Transform.GridUid.Value, grid, component.TargetTile.Value);
            var targetDir = (targetCoords.Position - args.WorldPosition);
            targetDir = args.OffsetRotation.RotateVec(targetDir);
            const float weight = 1f;
            var norm = targetDir.Normalized();

            for (var i = 0; i < SharedNPCSteeringSystem.InterestDirections; i++)
            {
                var result = -Vector2.Dot(norm, NPCSteeringSystem.Directions[i]) * weight;

                if (result < 0f)
                    continue;

                args.Steering.Interest[i] = MathF.Max(args.Steering.Interest[i], result);
            }

            args.Steering.CanSeek = false;
        }

        if (component.JukeType == JukeType.Away)
        {
            // TODO: Ranged away juking
            if (_npcMeleeQuery.TryGetComponent(uid, out var melee))
            {
                if (!_melee.TryGetWeapon(uid, out var weaponUid, out var weapon))
                    return;

                if (!HasComp<TransformComponent>(melee.Target))
                    return;

                var cdRemaining = weapon.NextAttack - _timing.CurTime;
                var attackCooldown = TimeSpan.FromSeconds(1f / _melee.GetAttackRate(weaponUid, uid, weapon));

                // Might as well get in range.
                if (cdRemaining < attackCooldown * 0.45f)
                    return;

                // If we get whacky boss mobs might need nearestpos that's more of a PITA
                // so will just use this for now.
                var obstacleDirection = _transform.GetWorldPosition(melee.Target) - args.WorldPosition;

                if (obstacleDirection == Vector2.Zero)
                {
                    obstacleDirection = _random.NextVector2();
                }

                // If they're moving away then pursue anyway.
                // If just hit then always back up a bit.
                if (cdRemaining < attackCooldown * 0.90f &&
                    _physicsQuery.TryGetComponent(melee.Target, out var targetPhysics) &&
                    Vector2.Dot(targetPhysics.LinearVelocity, obstacleDirection) > 0f)
                {
                    return;
                }

                if (cdRemaining < TimeSpan.FromSeconds(1f / _melee.GetAttackRate(weaponUid, uid, weapon)) * 0.45f)
                    return;

                // TODO: Probably add in our bounds and target bounds for ideal distance.
                var idealDistance = weapon.Range * 4f;
                var obstacleDistance = obstacleDirection.Length();

                if (obstacleDistance > idealDistance || obstacleDistance == 0f)
                {
                    // Don't want to get too far.
                    return;
                }

                obstacleDirection = args.OffsetRotation.RotateVec(obstacleDirection);
                var norm = obstacleDirection.Normalized();

                var weight = obstacleDistance <= args.Steering.Radius
                    ? 1f
                    : (idealDistance - obstacleDistance) / idealDistance;

                for (var i = 0; i < SharedNPCSteeringSystem.InterestDirections; i++)
                {
                    var result = -Vector2.Dot(norm, NPCSteeringSystem.Directions[i]) * weight;

                    if (result < 0f)
                        continue;

                    args.Steering.Interest[i] = MathF.Max(args.Steering.Interest[i], result);
                }
            }

            args.Steering.CanSeek = false;
        }
    }
}