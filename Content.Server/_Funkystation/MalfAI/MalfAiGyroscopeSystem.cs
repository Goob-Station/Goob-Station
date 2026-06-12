// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Gyroscope ability: lets the Malf AI move its core one tile in the clicked direction,
/// staying grid-aligned and crushing anything in the way.
/// </summary>
public sealed class MalfAiGyroscopeSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiGyroscopeActionEvent>(OnGyroscope);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Drive in-flight gyroscope traversals.
        var query = EntityQueryEnumerator<MalfGyroTraverseComponent>();
        while (query.MoveNext(out var uid, out var traverse))
        {
            var now = _timing.CurTime;
            var t = (float) ((now - traverse.StartTime).TotalSeconds / traverse.DurationSeconds);
            t = Math.Clamp(t, 0f, 1f);

            // Lerp position in map space.
            var pos = Vector2.Lerp(traverse.StartMap.Position, traverse.EndMap.Position, t);
            _xform.SetMapCoordinates(uid, new MapCoordinates(pos, traverse.StartMap.MapId));

            // Lerp rotation.
            var rot = new Angle(MathHelper.Lerp(traverse.StartRotation.Theta, traverse.EndRotation.Theta, t));
            _xform.SetLocalRotation(uid, rot);

            // Contact damage during traversal: thin AABB aligned to the movement axis,
            // blunt damage once per traversal per entity.
            var moveDelta = traverse.EndMap.Position - traverse.StartMap.Position;
            var halfExtents = Math.Abs(moveDelta.X) >= Math.Abs(moveDelta.Y)
                ? new Vector2(traverse.HalfExtentLong, traverse.HalfExtentShort)
                : new Vector2(traverse.HalfExtentShort, traverse.HalfExtentLong);

            var aabb = new Box2(pos - halfExtents, pos + halfExtents);
            var ents = _lookup.GetEntitiesIntersecting(traverse.StartMap.MapId, aabb);

            var dmg = new DamageSpecifier();
            dmg.DamageDict["Blunt"] = traverse.ContactDamage;

            foreach (var ent in ents)
            {
                if (ent == uid)
                    continue;

                // Ignore anchored/static entities (e.g., walls/doors) for contact damage.
                if (TryComp<TransformComponent>(ent, out var entXform) && entXform.Anchored)
                    continue;

                if (!traverse.Damaged.Add(ent))
                    continue;

                _damage.TryChangeDamage(ent, dmg, true);
            }

            if (t >= 1f)
            {
                // Ensure exact final state, re-anchor to the grid and clear the traversal.
                _xform.SetMapCoordinates(uid, traverse.EndMap);
                _xform.SetLocalRotation(uid, traverse.EndRotation);
                _xform.AnchorEntity(uid);
                RemComp<MalfGyroTraverseComponent>(uid);
            }
        }
    }

    private void OnGyroscope(Entity<MalfAiMarkerComponent> ent, ref MalfAiGyroscopeActionEvent args)
    {
        if (args.Handled)
            return;

        // The brain is held inside the core: the core is what physically moves.
        var core = Transform(ent.Owner).ParentUid;
        if (!HasComp<StationAiCoreComponent>(core))
            return;

        // Already traversing.
        if (HasComp<MalfGyroTraverseComponent>(core))
            return;

        // Resolve current map position and clicked position.
        var startMap = _xform.GetMapCoordinates(core);
        var targetMap = _xform.ToMapCoordinates(args.Target);

        // Direction from core to clicked position.
        var clickDirection = targetMap.Position - startMap.Position;
        if (Math.Abs(clickDirection.X) < 0.1f && Math.Abs(clickDirection.Y) < 0.1f)
            return;

        // Project the click direction onto a circle covering all adjacent tiles (incl. diagonals).
        var angle = MathF.Atan2(clickDirection.Y, clickDirection.X);
        const float circleRadius = 1.414f;
        var targetPosition = startMap.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * circleRadius;

        // Snap the destination to the center of the target tile so the core stays grid-aligned.
        MapCoordinates endMap;
        var gridUid = Transform(core).GridUid;
        if (gridUid != null && HasComp<MapGridComponent>(gridUid.Value))
        {
            var gridMatrix = _xform.GetInvWorldMatrix(gridUid.Value);
            var localTargetPos = Vector2.Transform(targetPosition, gridMatrix);

            var centeredLocal = new Vector2(
                MathF.Floor(localTargetPos.X) + 0.5f,
                MathF.Floor(localTargetPos.Y) + 0.5f);
            endMap = _xform.ToMapCoordinates(new EntityCoordinates(gridUid.Value, centeredLocal));
        }
        else
        {
            var centered = new Vector2(
                MathF.Floor(targetPosition.X) + 0.5f,
                MathF.Floor(targetPosition.Y) + 0.5f);
            endMap = new MapCoordinates(centered, startMap.MapId);
        }

        // Obstruction check: walls and closed doors block the traversal, open doors pass.
        if (!_interaction.InRangeUnobstructed(core, endMap, 1.414f))
        {
            _popup.PopupEntity(Loc.GetString("malf-gyro-blocked"), ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        // Rotate 90° during the movement, direction based on the dominant axis.
        var circleDelta = targetPosition - startMap.Position;
        float rotationDir;
        if (Math.Abs(circleDelta.X) >= Math.Abs(circleDelta.Y))
            rotationDir = circleDelta.X < 0 ? +1f : -1f;
        else
            rotationDir = circleDelta.Y > 0 ? +1f : -1f;

        var startRot = Transform(core).LocalRotation;
        var endRot = startRot + Angle.FromDegrees(90f * rotationDir);

        var traverse = EnsureComp<MalfGyroTraverseComponent>(core);
        traverse.StartMap = startMap;
        traverse.EndMap = endMap;
        traverse.StartRotation = startRot;
        traverse.EndRotation = endRot;
        traverse.StartTime = _timing.CurTime;
        traverse.Damaged.Clear();

        // Drive client radial cooldown and server lockout through the Actions system.
        var start = _timing.CurTime;
        _actions.SetCooldown((args.Action.Owner, args.Action.Comp), start, start + TimeSpan.FromSeconds(3));

        _adminLog.Add(LogType.Action, LogImpact.Medium,
            $"Malf AI {ToPrettyString(ent.Owner)} used gyroscope to move core {ToPrettyString(core)}");

        args.Handled = true;
    }
}
