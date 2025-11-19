// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System;
using System.Linq;
using System.Numerics;

namespace Content.Goobstation.Server.MobCaller;

public sealed partial class MobCallerSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobCallerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<MobCallerComponent> ent, ref ExaminedEvent args)
    {
        var occluded = false;
        // prevent evil spam examine ddos even though GetSpawnDirections() takes 0.3ms or so worstcase
        if (ent.Comp.LastExamineRaycast + ent.Comp.ExamineRaycastSpacing > _timing.CurTime)
        {
            occluded = ent.Comp.CachedExamineResult;
        }
        else
        {
            occluded = !(GetSpawnDirections((ent, ent.Comp, Transform(ent))).Any());
            ent.Comp.CachedExamineResult = occluded;
            ent.Comp.LastExamineRaycast = _timing.CurTime;
        }
        // tell the user if we're not working due to not being exposed to space
        if (occluded)
            args.PushMarkup(Loc.GetString("mob-caller-occluded"));
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MobCallerComponent>();

        while (query.MoveNext(out var uid, out var caller))
        {
            var xform = Transform(uid);

            if (caller.NeedPower && !_power.IsPowered(uid)
                || caller.NeedAnchored && !xform.Anchored
            )
                continue;

            caller.SpawnAccumulator += TimeSpan.FromSeconds(frameTime);
            if (caller.SpawnAccumulator < caller.SpawnSpacing)
                continue;

            caller.SpawnAccumulator -= caller.SpawnSpacing;

            // prune spawned entities list
            // has to be for-loop and not foreach since we may modify it on the fly
            for (var i = 0; i < caller.SpawnedEntities.Count; i++)
            {
                var mob = caller.SpawnedEntities[i];
                if (TerminatingOrDeleted(mob) || _mobState.IsDead(mob))
                {
                    caller.SpawnedEntities.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            // check happens after we increment accumulator, this is intentional
            if (caller.SpawnedEntities.Count >= caller.MaxAlive)
                continue;

            // choose a direction to spawn the mob in
            var candidates = GetSpawnDirections((uid, caller, xform));
            if (candidates.Count == 0)
                continue;

            // we chose a direction so pick a spawn position
            var chosenDir = _random.Pick(candidates);
            var spawnOffset = chosenDir.ToVec() * _random.NextFloat(caller.MinDistance, caller.MaxDistance);
            var spawnPos = new MapCoordinates(xform.WorldPosition + spawnOffset, xform.MapID);

            // if we would somehow spawn it on a grid, don't
            if (_map.TryFindGridAt(spawnPos, out _, out _))
                continue;

            // spawn the mob and have it follow us
            var spawned = Spawn(caller.SpawnProto, spawnPos);
            caller.SpawnedEntities.Add(spawned);
            _npc.SetBlackboard(spawned, NPCBlackboard.FollowTarget, new EntityCoordinates(uid, Vector2.Zero));
        }
    }

    /// <summary>
    /// Get a list of unoccluded directions.
    /// </summary>
    public List<Angle> GetSpawnDirections(Entity<MobCallerComponent, TransformComponent> ent)
    {
        var candidates = new List<Angle>();
        for (var i = 0; i < ent.Comp1.SpawnDirections; i++)
        {
            var dir = Angle.FromDegrees(360f * (float)i / ent.Comp1.SpawnDirections);
            if (CheckDir(dir))
                candidates.Add(dir);

            bool CheckDir(Angle dir)
            {
                var stepVec = dir.ToVec();

                // raycast to ensure there's continuously space from OcclusionDistance to GridOcclusionDistance
                var gridStepVec = stepVec * ent.Comp1.GridOcclusionFidelity;
                var steps = (int)MathF.Ceiling((ent.Comp1.GridOcclusionDistance - ent.Comp1.OcclusionDistance) / ent.Comp1.GridOcclusionFidelity);
                var checkPos = ent.Comp2.WorldPosition + stepVec * ent.Comp1.OcclusionDistance;
                for (var j = 0; j < steps; j++)
                {
                    // space isn't continuous, discard direction
                    if (_map.TryFindGridAt(new MapCoordinates(checkPos, ent.Comp2.MapID), out _, out _))
                        return false;

                    checkPos += gridStepVec;
                }

                // now also check that there's no obstructions in that direction before the continuous space
                var ray = new CollisionRay(ent.Comp2.WorldPosition, stepVec, (int)ent.Comp1.OcclusionMask);
                var rayCastResults = _physics.IntersectRay(ent.Comp2.MapID, ray, ent.Comp1.OcclusionDistance, ent);

                return !rayCastResults.Any();
            }
        }

        return candidates;
    }
}
