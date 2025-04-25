using Content.Shared.Actions;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Dash;

public sealed class DashActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashActionEvent>(OnDashAction);

        SubscribeLocalEvent<DashActionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DashActionComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnDashAction(DashActionEvent args)
    {
        if (args.Handled)
            return;

        if (args.NeedsGravity && TryComp<PhysicsComponent>(args.Performer, out var physics)
                              && physics.BodyStatus == BodyStatus.InAir)
            return;

        var vec = (_transform.ToMapCoordinates(args.Target).Position
                   - _transform.GetMapCoordinates(args.Performer).Position).Normalized();

        if (args.MultiplyByMovementSpeed && TryComp<MovementSpeedModifierComponent>(args.Performer, out var speed))
            vec *= speed.CurrentSprintSpeed;

        _physics.ApplyLinearImpulse(args.Performer, vec * args.Force);

        args.Handled = true;
    }

    private void OnComponentInit(EntityUid uid, DashActionComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnComponentShutdown(EntityUid uid, DashActionComponent comp, ref ComponentShutdown args)
    {
        _actions.RemoveAction(comp.ActionUid);
    }
}
