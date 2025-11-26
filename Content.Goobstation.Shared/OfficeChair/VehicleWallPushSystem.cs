// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Vehicles;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Item;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Goobstation.Shared.OfficeChair;

public sealed partial class VehicleWallPushSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly INetConfigurationManager _config = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VehicleWallPushComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<VehicleWallPushComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<VehicleWallPushComponent, VehicleWallPushActionEvent>(OnKick);
    }

    private void OnStrapped(EntityUid uid, VehicleWallPushComponent strap, ref StrappedEvent args)
    {
        if (!TryComp(uid, out VehicleWallPushComponent? comp))
            return;

        if (comp.KickAction == null)
            _actions.AddAction(args.Buckle.Owner, ref comp.KickAction, comp.ActionProto, uid);
    }

    private void OnUnstrapped(EntityUid uid, VehicleWallPushComponent strap, ref UnstrappedEvent args)
    {
        if (!TryComp(uid, out VehicleWallPushComponent? comp))
            return;

        if (comp.KickAction != null)
            _actions.RemoveAction(args.Buckle.Owner, comp.KickAction);

        comp.KickAction = null;
    }

    private void OnKick(EntityUid uid, VehicleWallPushComponent comp, ref VehicleWallPushActionEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp(uid, out VehicleComponent? vehicle) || vehicle.Driver != args.Performer)
            return;
        if (!TryComp(uid, out PhysicsComponent? physics))
            return;

        var from = _xform.GetMapCoordinates(uid);
        var to = _xform.ToMapCoordinates(args.Target);
        if (from.MapId != to.MapId)
            return;

        var aim = to.Position - from.Position;
        var aimLen = aim.Length();
        if (aimLen == 0)
            return;

        var dir = aim / aimLen;
        var ray = new CollisionRay(from.Position, dir, VehicleWallPushComponent.KickMask);

        if (_physics.IntersectRayWithPredicate(to.MapId, ray, comp.MaxDistance, x => x == vehicle.Driver || x == uid).FirstOrNull() is not { HitEntity: { } blocker })
            return;

        _audio.PlayPredicted(comp.RollSound, args.Performer, args.Performer);

        if (HasComp<PhysicsComponent>(blocker))
        {
            var shoveRange = _config.GetCVar(GoobCVars.ShoveRange);
            var shoveSpeed = _config.GetCVar(GoobCVars.ShoveSpeed);
            var shoveMass = _config.GetCVar(GoobCVars.ShoveMassFactor);

            var userPos = from.Position;
            var targetPos = _xform.GetMapCoordinates(blocker).Position;
            var delta = targetPos - userPos;

            if (delta.LengthSquared() > 0f)
            {
                var force = shoveRange * _contests.MassContest(args.Performer, blocker, rangeFactor: shoveMass);
                var pushVec = Vector2.Normalize(delta) * force;
                _throwing.TryThrow(blocker, pushVec, force * shoveSpeed, args.Performer, animated: true, playSound: false);
            }
        }

        var addVel = -dir * comp.KickSpeed;
        _physics.SetLinearVelocity(uid, physics.LinearVelocity + addVel);
        args.Handled = true;
    }

}
