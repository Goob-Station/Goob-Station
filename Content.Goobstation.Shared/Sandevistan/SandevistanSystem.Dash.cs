using System.Numerics;
using Content.Goobstation.Shared.Counter;
using Content.Goobstation.Shared.ForcedDirectionRotate;
using Content.Goobstation.Shared.Projectiles;
using Content.Shared.Movement.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// A scripted dash attack.
/// </summary>
public sealed partial class SandevistanSystem
{
    private void InitializeDash()
    {
        SubscribeLocalEvent<SandevistanUserComponent, CounterTriggeredEvent>(OnCountered);
        SubscribeLocalEvent<SandevistanUserComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
    }

    private void UpdateDash()
    {
        var query = EntityQueryEnumerator<SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.DashActive)
                continue;

            if (_netManager.IsClient && uid != _player.LocalEntity)
                continue;

            UpdateDashAttack(uid, comp);
        }
    }

    private void OnCountered(Entity<SandevistanUserComponent> ent, ref CounterTriggeredEvent args) =>
        StartDash(ent, args.Attacker);

    private void OnUpdateCanMove(Entity<SandevistanUserComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.DashActive)
            args.Cancel();
    }

    private void StartDash(Entity<SandevistanUserComponent> ent, EntityUid target)
    {
        if (ent.Comp.DashActive
            || target == ent.Owner
            || TerminatingOrDeleted(target)
            || !_mobState.IsAlive(target))
            return;

        if (!ent.Comp.Active && !Enable(ent))
            return;

        var userPos = _transform.GetWorldPosition(ent.Owner);
        var targetPos = _transform.GetWorldPosition(target);
        if ((targetPos - userPos).Length() > ent.Comp.DashAttackMaxRange)
            return;

        if (!TryFindDashPath(ent, userPos, targetPos, out _))
            return;

        ent.Comp.DashActive = true;
        ent.Comp.DashTarget = target;
        ent.Comp.DashPhase = SandevistanDashPhase.Windup;
        ent.Comp.DashWindupEndTime = _timing.CurTime + ent.Comp.DashAttackWindupDuration;
        ent.Comp.DashLegStart = userPos;
        ent.Comp.DashZigzagsDone = -1;
        ent.Comp.DashStuckHops = 0;
        ent.Comp.DashFinalApproach = false;
        Dirty(ent);
        _actionBlocker.UpdateCanMove(ent);

        PlayToggleSound(ent, ent.Comp.StartSound);

        var rotate = EnsureComp<ForcedDirectionRotateComponent>(ent);
        rotate.FaceTarget = target;
        Dirty(ent.Owner, rotate);

        var immunity = EnsureComp<ProjectileImmunityComponent>(ent);
        immunity.DodgeEffect = ent.Comp.DashDodgeEffect;
        Dirty(ent.Owner, immunity);
    }

    private void UpdateDashAttack(EntityUid uid, SandevistanUserComponent sande)
    {
        if (!sande.Active
            || TerminatingOrDeleted(sande.DashTarget)
            || !_mobState.IsAlive(sande.DashTarget) && sande.DashPhase != SandevistanDashPhase.Trampling)
        {
            EndDash(uid, sande);
            return;
        }

        var userPos = _transform.GetWorldPosition(uid);
        var targetPos = _transform.GetWorldPosition(sande.DashTarget);

        switch (sande.DashPhase)
        {
            case SandevistanDashPhase.Windup:
                {
                    PinUser(uid, sande.DashLegStart, targetPos - sande.DashLegStart);

                    if (_timing.CurTime >= sande.DashWindupEndTime)
                    {
                        sande.DashPhase = SandevistanDashPhase.Dashing;
                        Dirty(uid, sande);
                    }

                    return;
                }
            case SandevistanDashPhase.Dashing:
                {
                    var hopElapsed = _timing.CurTime - sande.DashLegStartTime;
                    var hopTotal = sande.DashAttackDashDuration + (sande.DashFinalApproach ? TimeSpan.Zero : sande.DashAttackLingerDuration);

                    if (sande.DashZigzagsDone < 0 || hopElapsed >= hopTotal)
                    {
                        if (sande.DashZigzagsDone >= 0)
                        {
                            if ((userPos - sande.DashLegStart).Length() < 0.5f)
                                sande.DashStuckHops++;
                            else
                                sande.DashStuckHops = 0;

                            if (sande.DashStuckHops >= 2)
                            {
                                EndDash(uid, sande);
                                return;
                            }
                        }

                        if (sande.DashFinalApproach)
                        {
                            if (!HasCircleRoom((uid, sande), targetPos, sande.DashAttackCircleRadius))
                            {
                                BeginTrample(uid, sande);
                                return;
                            }

                            sande.DashPhase = SandevistanDashPhase.Circling;
                            sande.DashCircleEndTime = _timing.CurTime + sande.DashAttackCircleDuration;
                            sande.DashCircleAngle = (userPos - targetPos).ToWorldAngle().Theta;
                            Dirty(uid, sande);
                            return;
                        }

                        sande.DashZigzagsDone++;

                        // If the target SOMEHOW runs faster than the sande, don't chase forever.
                        if (sande.DashZigzagsDone > sande.DashAttackMaxHops)
                        {
                            EndDash(uid, sande);
                            return;
                        }

                        if (!TryFindDashPath((uid, sande), userPos, targetPos, out var path))
                        {
                            EndDash(uid, sande);
                            return;
                        }

                        // if they have a clear LOS and the target is close enough, start the circle phase.
                        if (path.Count == 1 && (targetPos - userPos).Length() <= sande.DashAttackCircleEntryRange)
                            sande.DashFinalApproach = true;

                        StartDashLeg(uid, sande, userPos, sande.DashFinalApproach ? targetPos : path[0]);
                        return;
                    }

                    var dashSecs = (float) sande.DashAttackDashDuration.TotalSeconds;
                    var t = dashSecs > 0.001f ? MathF.Min(1f, (float) hopElapsed.TotalSeconds / dashSecs) : 1f;
                    var frac = 1f - MathF.Pow(1f - t, 3f);
                    var newPos = Vector2.Lerp(sande.DashLegStart, sande.DashWaypoint, frac);

                    PinUser(uid, newPos, targetPos - newPos);
                    return;
                }
            case SandevistanDashPhase.Circling:
                {
                    if (_timing.CurTime >= sande.DashCircleEndTime)
                    {
                        BeginTrample(uid, sande);
                        return;
                    }

                    var circleSecs = MathF.Max(0.01f, (float) sande.DashAttackCircleDuration.TotalSeconds);
                    var remaining = (float) (sande.DashCircleEndTime - _timing.CurTime).TotalSeconds;
                    var progress = Math.Clamp(1f - remaining / circleSecs, 0f, 1f);
                    var eased = 1f - MathF.Pow(1f - progress, 3f);
                    var sweep = MathF.Tau * sande.DashAttackCircleRevolutions * eased;
                    var offset = new Angle(sande.DashCircleAngle + sweep).RotateVec(new Vector2(sande.DashAttackCircleRadius, 0f));
                    var circlePos = ClampToWalls((uid, sande), targetPos, targetPos + offset);

                    PinUser(uid, circlePos, targetPos - circlePos);
                    return;
                }
            case SandevistanDashPhase.Trampling:
                {
                    UpdateTrample(uid, sande, targetPos);
                    return;
                }
        }
    }

    /// <summary>
    /// Removes velocity.
    /// </summary>
    private void PinUser(EntityUid uid, Vector2 pos, Vector2 facing)
    {
        _transform.SetWorldPosition(uid, pos);
        _physics.SetLinearVelocity(uid, Vector2.Zero);

        if (facing.LengthSquared() > 0.0001f)
            _transform.SetWorldRotation(uid, facing.ToWorldAngle());
    }

    /// <summary>
    /// Sets up the next zig-zag hop.
    /// </summary>
    private void StartDashLeg(EntityUid uid, SandevistanUserComponent sande, Vector2 fromPos, Vector2 aimPos)
    {
        sande.DashLegStart = fromPos;
        sande.DashLegStartTime = _timing.CurTime;

        var toTarget = aimPos - fromPos;
        var dist = toTarget.Length();

        if (sande.DashFinalApproach || dist < sande.DashAttackCircleEntryRange)
            sande.DashWaypoint = ClampToWalls((uid, sande), fromPos, aimPos);
        else
        {
            var dir = toTarget / dist;
            var advance = MathF.Min(sande.DashAttackHopDistance, dist);
            var sideSign = sande.DashZigzagsDone % 2 == 0 ? 1f : -1f;
            var wobble = 0.7f + 0.6f * MathF.Abs(MathF.Sin(sande.DashZigzagsDone * 2.1f));
            var lateral = MathF.Min(sande.DashAttackZigzagWidth, dist * 0.4f) * wobble;
            var perpendicular = new Vector2(-dir.Y, dir.X);
            sande.DashWaypoint = PickHopWaypoint(uid, sande, fromPos, dir * advance, perpendicular * (sideSign * lateral));
        }

        PlayToggleSound((uid, sande), sande.StartSound);
        Dirty(uid, sande);
    }

    /// <summary>
    /// Picks the waypoint for a zig-zag hop.
    /// </summary>
    private Vector2 PickHopWaypoint(EntityUid uid, SandevistanUserComponent sande, Vector2 fromPos, Vector2 forward, Vector2 swing)
    {
        var best = fromPos;
        var bestLength = 0f;

        foreach (var scale in sande.DashAttackHopLateralScales)
        {
            var candidate = fromPos + forward + swing * scale;
            var clamped = ClampToWalls((uid, sande), fromPos, candidate);
            var actual = (clamped - fromPos).Length();

            if (actual >= (candidate - fromPos).Length() * 0.9f)
                return clamped;

            if (actual > bestLength)
            {
                bestLength = actual;
                best = clamped;
            }
        }

        return best;
    }

    /// <summary>
    /// Stops the user from going through a wall.
    /// </summary>
    private Vector2 ClampToWalls(Entity<SandevistanUserComponent> ent, Vector2 from, Vector2 to)
    {
        var delta = to - from;
        var dist = delta.Length();
        if (dist < 0.01f)
            return to;

        var dir = delta / dist;
        var ray = new CollisionRay(from, dir, (int) ent.Comp.DashObstacleMask);

        foreach (var result in _physics.IntersectRay(Transform(ent).MapID, ray, dist, ent, returnOnFirstHit: true))
            return from + dir * MathF.Max(0f, result.Distance - 0.35f);

        return to;
    }

    /// <summary>
    /// True when the straight from-to line crosses no walls.
    /// </summary>
    private bool IsPathClear(Entity<SandevistanUserComponent> ent, Vector2 from, Vector2 to) =>
        ClampToWalls(ent, from, to) == to;

    /// <summary>
    /// Whether there's enough open space around the target to run the circling phase.
    /// </summary>
    private bool HasCircleRoom(Entity<SandevistanUserComponent> ent, Vector2 targetPos, float radius)
    {
        var samples = 8;
        var minReach = radius * 0.7f;
        var blocked = 0;

        for (var i = 0; i < samples; i++)
        {
            var angle = new Angle(MathF.Tau * i / samples);
            var point = targetPos + angle.RotateVec(new Vector2(radius, 0f));

            if ((ClampToWalls(ent, targetPos, point) - targetPos).LengthSquared() < minReach * minReach)
                blocked++;
        }

        return blocked <= samples / 4;
    }

    /// <summary>
    /// Starts the attack phase.
    /// </summary>
    private void BeginTrample(EntityUid uid, SandevistanUserComponent sande)
    {
        var target = sande.DashTarget;
        var userPos = _transform.GetWorldPosition(uid);
        var targetPos = _transform.GetWorldPosition(target);

        sande.DashPhase = SandevistanDashPhase.Trampling;
        sande.DashTramplePassesDone = 0;

        // TODO: Sandevistan, Uncomment once the cinematic system is merged.

        // var total = (sande.DashAttackTramplePause + sande.DashAttackTrampleDashDuration) * sande.DashAttackTrampleCount
        //     + sande.DashAttackTrampleHoldDuration;
        // var cinematic = _cinematic.StartCinematic(uid, sande.DashCinematic);
        // cinematic.PullsOtherCameras = true;
        // cinematic.CameraPull = sande.DashCinematicCameraPull;
        // cinematic.EndTime = _timing.CurTime + total + TimeSpan.FromSeconds(1.2);
        // Dirty(uid, cinematic);

        var toTarget = targetPos - userPos;
        var dir = toTarget.LengthSquared() < 0.01f ? new Vector2(0f, -1f) : Vector2.Normalize(toTarget);
        StartTramplePass(uid, sande, targetPos, dir);
    }

    /// <summary>
    /// Lines up the next pass.
    /// </summary>
    private void StartTramplePass(EntityUid uid, SandevistanUserComponent sande, Vector2 targetPos, Vector2 dir)
    {
        var half = sande.DashAttackTrampleDistance / 2f;
        var bestStart = targetPos;
        var bestEnd = targetPos;
        var bestScore = float.MinValue;

        for (var i = 0; i < sande.DashAttackTrampleLineSamples; i++)
        {
            var candidate = new Angle(MathF.PI * i / sande.DashAttackTrampleLineSamples).RotateVec(dir);
            var back = ClampToWalls((uid, sande), targetPos, targetPos - candidate * half);
            var front = ClampToWalls((uid, sande), targetPos, targetPos + candidate * half);
            var backRoom = (back - targetPos).Length();
            var frontRoom = (front - targetPos).Length();

            var start = backRoom >= frontRoom ? back : front;
            var end = backRoom >= frontRoom ? front : back;
            var length = backRoom + frontRoom;

            var lineDir = length > 0.01f ? Vector2.Normalize(end - start) : candidate;
            var score = length + Vector2.Dot(lineDir, dir) * 0.5f;

            if (score > bestScore)
            {
                bestScore = score;
                bestStart = start;
                bestEnd = end;
            }

            if (i == 0 && length >= sande.DashAttackTrampleDistance * 0.95f)
                break;
        }

        var total = (bestEnd - bestStart).Length();
        sande.DashLegStart = bestStart;
        sande.DashWaypoint = bestEnd;
        sande.DashLegStartTime = _timing.CurTime;
        sande.DashTrampleDir = total > 0.01f ? Vector2.Normalize(bestEnd - bestStart) : dir;
        sande.DashTrampleHitFrac = total > 0.01f
            ? Math.Clamp((targetPos - bestStart).Length() / total, 0f, 1f)
            : 0f;
        sande.DashTrampleStruck = false;

        PinUser(uid, bestStart, targetPos - bestStart);

        PlayToggleSound((uid, sande), sande.StartSound);
        Dirty(uid, sande);
    }

    private void UpdateTrample(EntityUid uid, SandevistanUserComponent sande, Vector2 targetPos)
    {
        if (sande.DashTramplePassesDone >= sande.DashAttackTrampleCount)
        {
            if (_timing.CurTime >= sande.DashTrampleEndTime)
            {
                EndDash(uid, sande);
                return;
            }

            PinUser(uid, sande.DashWaypoint, targetPos - sande.DashWaypoint);
            return;
        }

        var elapsed = _timing.CurTime - sande.DashLegStartTime;
        if (elapsed < sande.DashAttackTramplePause)
        {
            PinUser(uid, sande.DashLegStart, targetPos - sande.DashLegStart);
            return;
        }

        var lineLength = (sande.DashWaypoint - sande.DashLegStart).Length();
        var dashSecs = MathF.Max(0.05f,
            (float) sande.DashAttackTrampleDashDuration.TotalSeconds
            * MathF.Min(1f, lineLength / MathF.Max(0.01f, sande.DashAttackTrampleDistance)));
        var t = Math.Clamp((float) (elapsed - sande.DashAttackTramplePause).TotalSeconds / dashSecs, 0f, 1f);
        var newPos = Vector2.Lerp(sande.DashLegStart, sande.DashWaypoint, t);

        PinUser(uid, newPos, sande.DashTrampleDir);

        if (!sande.DashTrampleStruck && (t >= sande.DashTrampleHitFrac || (targetPos - newPos).Length() < 0.8f))
            LandTramplePass(uid, sande);

        if (t < 1f)
            return;

        sande.DashTramplePassesDone++;
        Dirty(uid, sande);

        if (sande.DashTramplePassesDone >= sande.DashAttackTrampleCount)
        {
            sande.DashTrampleEndTime = _timing.CurTime + sande.DashAttackTrampleHoldDuration;
            return;
        }

        var side = sande.DashTramplePassesDone % 2 == 0 ? 1f : -1f;
        var swing = MathF.PI * (0.55f + 0.1f * (sande.DashTramplePassesDone % 3)) * side;
        StartTramplePass(uid, sande, targetPos, new Angle(swing).RotateVec(sande.DashTrampleDir));
    }

    private void LandTramplePass(EntityUid uid, SandevistanUserComponent sande)
    {
        sande.DashTrampleStruck = true;

        var target = sande.DashTarget;
        var dir = sande.DashTrampleDir;

        _recoil.KickCamera(target, dir * sande.DashAttackTrampleCameraKick);
        _recoil.KickCamera(uid, -dir * sande.DashAttackTrampleCameraKick * 0.5f);

        Trample(uid, sande, target, dir * sande.SlowfieldTrampleMinSpeed);

        Dirty(uid, sande);
    }

    /// <summary>
    /// Ends the dash attack.
    /// </summary>
    private void EndDash(EntityUid uid, SandevistanUserComponent sande, bool disable = true)
    {
        if (!sande.DashActive)
            return;

        // TODO: Sandevistan, Uncomment once the cinematic system is merged.

        // if (sande.DashPhase == SandevistanDashPhase.Trampling
        //     && sande.DashTramplePassesDone < sande.DashAttackTrampleCount)
        //     _cinematic.StopCinematic(uid);

        sande.DashActive = false;
        sande.DashFinalApproach = false;
        sande.DashTrampleStruck = false;
        RemCompDeferred<ForcedDirectionRotateComponent>(uid);
        RemCompDeferred<ProjectileImmunityComponent>(uid);
        Dirty(uid, sande);
        _actionBlocker.UpdateCanMove(uid);

        if (disable)
            Disable(uid, sande);
    }

    /// <summary>
    /// Plans the route the dash will follow.
    /// </summary>
    private bool TryFindDashPath(Entity<SandevistanUserComponent> ent, Vector2 from, Vector2 to, out List<Vector2> path)
    {
        path = new List<Vector2>();

        if (IsPathClear(ent, from, to))
        {
            path.Add(to);
            return true;
        }

        var xform = Transform(ent);
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        var start = _map.TileIndicesFor(gridUid, grid, new MapCoordinates(from, xform.MapID));
        var goal = _map.TileIndicesFor(gridUid, grid, new MapCoordinates(to, xform.MapID));

        if (FindGridPath(ent.Comp, gridUid, grid, start, goal) is not { } tilePath)
            return false;

        var points = new List<Vector2>(tilePath.Count);
        foreach (var tile in tilePath)
            points.Add(_map.GridTileToWorldPos(gridUid, grid, tile));
        points[^1] = to;

        StringPullPath(ent, from, points, path);
        return true;
    }

    /// <summary>
    /// Reduces a chain of path points down to just its corners.
    /// </summary>
    private void StringPullPath(Entity<SandevistanUserComponent> ent, Vector2 from, List<Vector2> points, List<Vector2> corners)
    {
        var position = from;
        var index = 0;

        while (index < points.Count)
        {
            var furthestVisible = index;
            for (var i = points.Count - 1; i > index; i--)
            {
                if (!IsPathClear(ent, position, points[i]))
                    continue;

                furthestVisible = i;
                break;
            }

            corners.Add(points[furthestVisible]);
            position = points[furthestVisible];
            index = furthestVisible + 1;
        }
    }

    private static readonly Vector2i[] PathNeighbors =
    [
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(1, -1), new(-1, 1), new(-1, -1),
    ];

    private List<Vector2i>? FindGridPath(SandevistanUserComponent sande, EntityUid gridUid, MapGridComponent grid, Vector2i start, Vector2i goal)
    {
        if (start == goal)
            return new List<Vector2i> { start };

        var mask = (int) sande.DashObstacleMask;
        var blockedCache = new Dictionary<Vector2i, bool>();
        var cameFrom = new Dictionary<Vector2i, Vector2i>();
        var costs = new Dictionary<Vector2i, float> { [start] = 0f };
        var closed = new HashSet<Vector2i>();
        var open = new List<Vector2i> { start };
        var scores = new Dictionary<Vector2i, float> { [start] = 0f };
        var expansions = 0;

        while (open.Count > 0)
        {
            var bestIndex = 0;
            for (var i = 1; i < open.Count; i++)
            {
                if (scores[open[i]] < scores[open[bestIndex]])
                    bestIndex = i;
            }

            var tile = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (!closed.Add(tile))
                continue;

            if (tile == goal)
            {
                var result = new List<Vector2i> { goal };
                while (cameFrom.TryGetValue(result[^1], out var previous))
                    result.Add(previous);
                result.Reverse();
                return result;
            }

            if (++expansions > sande.MaxPathExpansions)
                return null;

            foreach (var offset in PathNeighbors)
            {
                var next = tile + offset;
                if (next != goal)
                {
                    if (IsTileBlocked(gridUid, grid, next, mask, blockedCache))
                        continue;

                    if (offset.X != 0 && offset.Y != 0 && (IsTileBlocked(gridUid, grid, tile + new Vector2i(offset.X, 0), mask, blockedCache)
                            || IsTileBlocked(gridUid, grid, tile + new Vector2i(0, offset.Y), mask, blockedCache)))
                        continue;
                }

                var cost = costs[tile] + (offset.X != 0 && offset.Y != 0 ? 1.41421f : 1f);
                if (costs.TryGetValue(next, out var existing) && existing <= cost)
                    continue;

                costs[next] = cost;
                cameFrom[next] = tile;

                var dx = (float) (goal.X - next.X);
                var dy = (float) (goal.Y - next.Y);
                scores[next] = cost + MathF.Sqrt(dx * dx + dy * dy);
                open.Add(next);
            }
        }

        return null;
    }

    private bool IsTileBlocked(EntityUid gridUid, MapGridComponent grid, Vector2i tile, int mask, Dictionary<Vector2i, bool> cache)
    {
        if (cache.TryGetValue(tile, out var cached))
            return cached;

        var blocked = false;
        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, tile);
        while (!blocked && anchored.MoveNext(out var ent))
        {
            if (!TryComp<PhysicsComponent>(ent, out var body)
                || !body.CanCollide
                || !TryComp<FixturesComponent>(ent, out var fixtures))
                continue;

            foreach (var fixture in fixtures.Fixtures.Values)
                if (fixture.Hard && (fixture.CollisionLayer & mask) != 0)
                {
                    blocked = true;
                    break;
                }
        }

        cache[tile] = blocked;
        return blocked;
    }
}
