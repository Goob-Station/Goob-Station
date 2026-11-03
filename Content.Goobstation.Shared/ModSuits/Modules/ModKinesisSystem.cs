using System.Numerics;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModKinesisSystem : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private ThrowingSystem _throwing = default!;

    private const float LeashSlack = 1.5f;
    private const float ThrowDistanceFactor = 0.6f;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModKinesisModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (_timing.ApplyingState || args.Wearer is not { } wearer)
            return;

        var user = EnsureComp<ModKinesisUserComponent>(wearer);
        user.Module = ent;
        Dirty(wearer, user);

        ent.Comp.User = wearer;
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModKinesisModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        Stop(ent);
    }

    [SubscribeLocalEvent]
    private void OnWearerChanged(Entity<ModKinesisModuleComponent> ent, ref ModSuitWearerChangedEvent args)
    {
        Stop(ent);
    }

    private void Stop(Entity<ModKinesisModuleComponent> ent)
    {
        if (_timing.ApplyingState)
            return;

        Release(ent, false);

        if (ent.Comp.User is { } user
            && TryComp<ModKinesisUserComponent>(user, out var userComp)
            && userComp.Module == ent.Owner)
            RemCompDeferred<ModKinesisUserComponent>(user);

        ent.Comp.User = null;
        Dirty(ent);
    }

    [SubscribeNetworkEvent]
    private void OnGrab(ModKinesisGrabEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user
            || !TryGetModule(user, out var module)
            || module.Comp.Held != null
            || !TryGetEntity(msg.Target, out var target)
            || !CanGrab(module, user, target.Value)
            || !_modSuit.TryGetModuleSuit(module, out var suit)
            || !_modSuit.TryUseCharge(suit, module.Comp.GrabPower))
            return;

        if (TryComp<PullableComponent>(target, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(target.Value, pullable, user, true);

        module.Comp.Held = target;
        module.Comp.Cursor = TransformSystem.ToMapCoordinates(GetCoordinates(msg.Cursor));
        Dirty(module);

        var held = EnsureComp<ModKinesisHeldComponent>(target.Value);
        held.Module = module;
        Dirty(target.Value, held);

        if (TryComp<PhysicsComponent>(target, out var body))
            PhysicsSystem.SetBodyStatus(target.Value, body, BodyStatus.InAir);

        module.Comp.HoldStream = _audio.Stop(module.Comp.HoldStream);
        module.Comp.HoldStream = _audio.PlayPvs(module.Comp.HoldSound, target.Value)?.Entity;
    }

    [SubscribeNetworkEvent]
    private void OnCursor(ModKinesisCursorEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user || !TryGetModule(user, out var module))
            return;

        module.Comp.Cursor = TransformSystem.ToMapCoordinates(GetCoordinates(msg.Cursor));
    }

    [SubscribeNetworkEvent]
    private void OnRelease(ModKinesisReleaseEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is { } user && TryGetModule(user, out var module))
            Release(module, true);
    }

    private bool TryGetModule(EntityUid user, out Entity<ModKinesisModuleComponent> module)
    {
        module = default;
        if (!TryComp<ModKinesisUserComponent>(user, out var userComp)
            || userComp.Module is not { } moduleUid
            || !TryComp<ModKinesisModuleComponent>(moduleUid, out var moduleComp)
            || moduleComp.User != user)
            return false;

        module = (moduleUid, moduleComp);
        return true;
    }

    private bool CanGrab(Entity<ModKinesisModuleComponent> module, EntityUid user, EntityUid target)
    {
        if (target == user
            || TerminatingOrDeleted(target)
            || _container.IsEntityInContainer(target)
            || !TryComp<PhysicsComponent>(target, out var body)
            || body.BodyType != BodyType.Dynamic
            || Transform(target).Anchored
            || HasComp<ModKinesisHeldComponent>(target)
            || _whitelist.IsWhitelistPass(module.Comp.Blacklist, target))
            return false;

        return _interaction.InRangeUnobstructed(user, target, module.Comp.Range);
    }

    private void Release(Entity<ModKinesisModuleComponent> ent, bool fling)
    {
        if (ent.Comp.Held is not { } held)
            return;

        ent.Comp.Held = null;
        ent.Comp.Cursor = null;
        ent.Comp.HoldStream = _audio.Stop(ent.Comp.HoldStream);
        Dirty(ent);

        if (TerminatingOrDeleted(held))
            return;

        RemCompDeferred<ModKinesisHeldComponent>(held);

        if (!TryComp<PhysicsComponent>(held, out var body))
            return;

        var velocity = body.LinearVelocity;
        var speed = velocity.Length();
        if (fling && speed >= ent.Comp.ThrowThreshold)
        {
            _throwing.TryThrow(held,
                velocity * ThrowDistanceFactor,
                speed,
                ent.Comp.User,
                pushbackRatio: 0f,
                recoil: false,
                doSpin: true);
            _audio.PlayPvs(ent.Comp.ThrowSound, held);
            return;
        }

        PhysicsSystem.SetBodyStatus(held, body, BodyStatus.OnGround);
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<ModKinesisModuleComponent>();
        while (query.MoveNext(out var uid, out var module))
        {
            if (module.Held is not { } held)
                continue;

            if (module.User is not { } user
                || TerminatingOrDeleted(held)
                || _container.IsEntityInContainer(held)
                || Transform(held).Anchored
                || !TryComp<PhysicsComponent>(held, out var body))
            {
                Release((uid, module), false);
                continue;
            }

            var userPos = TransformSystem.GetMapCoordinates(user);
            var heldPos = TransformSystem.GetMapCoordinates(held);
            if (userPos.MapId != heldPos.MapId
                || (heldPos.Position - userPos.Position).Length() > module.Range + LeashSlack)
            {
                Release((uid, module), false);
                continue;
            }

            var goal = module.Cursor is { } cursor && cursor.MapId == userPos.MapId
                ? cursor.Position
                : heldPos.Position;

            var fromUser = goal - userPos.Position;
            if (fromUser.Length() > module.Range)
                goal = userPos.Position + fromUser.Normalized() * module.Range;

            var maxSpeed = Math.Clamp(module.MassSpeedFactor / MathF.Max(body.Mass, 0.1f), module.MinSpeed, module.MaxSpeed);
            var desired = (goal - heldPos.Position) * module.Gain;
            if (desired.Length() > maxSpeed)
                desired = desired.Normalized() * maxSpeed;

            PhysicsSystem.SetLinearVelocity(held, desired, body: body);
            PhysicsSystem.SetAngularVelocity(held, 0f, body: body);
        }
    }
}
