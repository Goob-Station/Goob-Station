using Content.Shared.Actions;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.Dash;

public sealed class DashActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashActionEvent>(OnDashAction);

        SubscribeLocalEvent<DashActionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DashActionComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnDashAction(DashActionEvent args)
    {
        if (!TryComp<PhysicsComponent>(args.Performer, out var physics) || physics.BodyStatus == BodyStatus.InAir)
            return;

        _throwing.TryThrow(args.Performer, args.Target, args.ThrowSpeed, default, default, default, default, default, false);

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
