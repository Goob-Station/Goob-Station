// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.MalfAI;
using Content.Server.Damage.Systems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Components;

namespace Content.Server.MalfAI;

public sealed class MalfAiGyroscopeSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly Content.Server.Silicons.StationAi.StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to the world-targeted action event.
        SubscribeLocalEvent<MalfAiGyroscopeActionEvent>(OnGyroscope);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Drive in-flight gyroscope traversals.
        var query = EntityQueryEnumerator<MalfGyroTraverseComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            // Fetch the live component instance to ensure Damaged set updates persist across frames.
            var traverse = Comp<MalfGyroTraverseComponent>(uid);

            var now = _timing.CurTime;
            var t = (float) ((now - traverse.StartTime).TotalSeconds / traverse.DurationSeconds);
            t = Math.Clamp(t, 0f, 1f);

            // Lerp position in map space.
            var pos = Vector2.Lerp(traverse.StartMap.Position, traverse.EndMap.Position, t);
            _xform.SetMapCoordinates(uid, new MapCoordinates(pos, traverse.StartMap.MapId));

            // Lerp rotation.
            var startR = traverse.StartRotation.Theta;
            var endR = traverse.EndRotation.Theta;
            var rot = new Angle(MathHelper.Lerp(startR, endR, t));
            _xform.SetLocalRotation(uid, rot);

            // Contact damage during traversal: blunt damage to any entity intersecting the core, once per traversal.
            // Build a thin AABB aligned to the movement axis so we only affect the two tiles the core traverses.
            var moveDelta = traverse.EndMap.Position - traverse.StartMap.Position;

            // Decide which axis is dominant; elongate along that axis, keep thin across it.
            var halfLong = traverse.HalfExtentLong;
            var halfShort = traverse.HalfExtentShort;

            Vector2 halfExtents;
            if (Math.Abs(moveDelta.X) >= Math.Abs(moveDelta.Y))
                halfExtents = new Vector2(halfLong, halfShort); // horizontal move
            else
                halfExtents = new Vector2(halfShort, halfLong); // vertical move

            var min = pos - halfExtents;
            var max = pos + halfExtents;
            var aabb = new Box2(min, max);

            var ents = _lookup.GetEntitiesIntersecting(traverse.StartMap.MapId, aabb);

            var dmg = new DamageSpecifier();
            dmg.DamageDict["Blunt"] = traverse.ContactDamage;

            foreach (var ent in ents)
            {
                if (ent == uid)
                    continue;

                // Ignore anchored/static entities (e.g., walls/doors) for contact damage.
                if (TryComp<TransformComponent>(ent, out var xform) && xform.Anchored)
                    continue;

                if (traverse.Damaged.Contains(ent))
                    continue;

                // Apply damage once per traversal per entity.
                _damage.TryChangeDamage(ent, dmg, true);
                traverse.Damaged.Add(ent);
            }

            if (t >= 1f)
            {
                // Ensure exact final state and clear the traversal.
                _xform.SetMapCoordinates(uid, traverse.EndMap);
                _xform.SetLocalRotation(uid, traverse.EndRotation);
                RemComp<MalfGyroTraverseComponent>(uid);
            }
        }
    }

    private void OnGyroscope(MalfAiGyroscopeActionEvent ev)
    {
        // Resolve the actual AI core entity to move.
        var core = ev.Performer;

        // 1) If the performer is the core, we're good.
        if (!HasComp<StationAiCoreComponent>(core))
        {
            // 2) Try find a core whose RemoteEntity equals the performer (remote-controlled AI).
            var query = EntityManager.EntityQueryEnumerator<StationAiCoreComponent>();
            while (query.MoveNext(out var ent, out var comp))
            {
                if (comp.RemoteEntity == core)
                {
                    core = ent;
                    break;
                }
            }

            // 3) If still not a core, walk up the transform parent chain to find a parent core
            //    (covers the case where the performer is inserted into the core).
            if (!HasComp<StationAiCoreComponent>(core))
            {
                var current = core;
                for (var i = 0; i < 8; i++)
                {
                    var parent = Transform(current).ParentUid;

                    // Stop if parent is invalid/non-existent.
                    if (!EntityManager.EntityExists(parent))
                        break;

                    if (HasComp<StationAiCoreComponent>(parent))
                    {
                        core = parent;
                        break;
                    }

                    current = parent;
                }
            }

            // If we couldn't resolve a core, bail out safely.
            if (!HasComp<StationAiCoreComponent>(core))
                return;
        }

        // Resolve current map position and clicked position.
        var startMap = _xform.GetMapCoordinates(core);
        var targetMap = ev.Target.ToMap(EntityManager, _xform);

        // Calculate direction vector from core to clicked position
        var clickDirection = targetMap.Position - startMap.Position;

        // If clicking on the same tile, do nothing
        if (Math.Abs(clickDirection.X) < 0.1f && Math.Abs(clickDirection.Y) < 0.1f)
            return;

        // Calculate the angle from core to clicked position
        var angle = MathF.Atan2(clickDirection.Y, clickDirection.X);

        // Define circle radius - use sqrt(2) ≈ 1.414 to cover all adjacent tiles including diagonals
        var circleRadius = 1.414f;

        // Find the closest point on the circle in the direction of the click
        var circleX = MathF.Cos(angle) * circleRadius;
        var circleY = MathF.Sin(angle) * circleRadius;

        // The target position is the core position plus the circle point
        var targetPosition = startMap.Position + new Vector2(circleX, circleY);

        // Grid-anchor the destination to the center of the target tile
        MapCoordinates endMap;
        var gridUid = Transform(core).GridUid;
        if (gridUid != null && EntityManager.TryGetComponent<MapGridComponent>(gridUid.Value, out var grid))
        {
            // Convert world position to grid local coordinates
            var gridMatrix = _xform.GetInvWorldMatrix(gridUid.Value);
            var localTargetPos = Vector2.Transform(targetPosition, gridMatrix);

            var tileX = (int) MathF.Floor(localTargetPos.X);
            var tileY = (int) MathF.Floor(localTargetPos.Y);
            var centeredLocal = new Vector2(tileX + 0.5f, tileY + 0.5f);
            var centeredCoords = new EntityCoordinates(gridUid.Value, centeredLocal);
            endMap = centeredCoords.ToMap(EntityManager, _xform);
        }
        else
        {
            var centered = new Vector2(
                MathF.Floor(targetPosition.X) + 0.5f,
                MathF.Floor(targetPosition.Y) + 0.5f);
            endMap = new MapCoordinates(centered, startMap.MapId);
        }

        // Obstruction check: block traversal if a wall/closed door is between start and end.
        // Use the standard unobstructed interaction check so open doors pass and walls/closed doors block.
        // This check now uses the calculated endMap position after lengthdir calculation.
        // Using 1.414f range to match the circle radius that covers all adjacent tiles.
        var unobstructed = _interaction.InRangeUnobstructed(core, endMap, 1.414f);

        if (!unobstructed)
        {
            // Find the AI entity that triggered this action to get the proper eye for popup positioning
            var aiEntity = ev.Performer;
            var popupTarget = GetAiEyeForPopup(aiEntity) ?? core;
            _popup.PopupEntity(Loc.GetString("malf-gyro-blocked"), popupTarget, aiEntity, Content.Shared.Popups.PopupType.SmallCaution);
            return;
        }

        // Prepare traversal: 0.3 seconds and rotate 90° during movement based on direction.
        var startRot = Transform(core).LocalRotation;

        // Determine rotation direction based on circle movement:
        // - Horizontal: left (circleX < 0) => CCW (+90), right => CW (-90)
        // - Vertical: up (circleY > 0) => CCW (+90), down => CW (-90)
        var rotationDir = 0f;
        if (Math.Abs(circleX) >= Math.Abs(circleY))
            rotationDir = circleX < 0 ? +1f : -1f;
        else
            rotationDir = circleY > 0 ? +1f : -1f;

        var endRot = startRot + Angle.FromDegrees(90f * rotationDir);

        var traverse = EnsureComp<MalfGyroTraverseComponent>(core);
        traverse.StartMap = startMap;
        traverse.EndMap = endMap;
        traverse.StartRotation = startRot;
        traverse.EndRotation = endRot;
        traverse.StartTime = _timing.CurTime;
        traverse.DurationSeconds = 0.3f;

        // Drive client radial cooldown and server lockout through the Actions system.
        var start = _timing.CurTime;
        var end = start + TimeSpan.FromSeconds(3);
        _actions.SetCooldown(ev.Action.Owner, start, end);

        // Mark handled so ActionsUseDelay will also be respected by the system.
        ev.Handled = true;
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
