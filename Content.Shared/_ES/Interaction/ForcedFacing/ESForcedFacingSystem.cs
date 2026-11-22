using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.MouseRotator;
using Content.Shared.Movement.Components;
using Robust.Shared.Timing;

namespace Content.Shared._ES.Interaction.ForcedFacing;

public sealed class ESForcedFacingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private RotateToFaceSystem _rotateToFace = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESForcedFacingUiComponent, BoundUIOpenedEvent>(OnBoundUiOpened);
        SubscribeLocalEvent<ESForcedFacingUiComponent, BoundUIClosedEvent>(OnBoundUiClosed);
        SubscribeLocalEvent<UserInterfaceUserComponent, ESRefreshForcedFacingEvent>(OnRefreshForcedFacingUi);
        SubscribeLocalEvent<DoAfterComponent, ESRefreshForcedFacingEvent>(OnRefreshForcedFacingDoAfter);

        SubscribeLocalEvent<MouseRotatorComponent, ESRefreshNoRotateOnMoveEvent>(OnMouseRotatorRefreshNoRotateOnMove);
        SubscribeLocalEvent<ESForcedFacingComponent, ESRefreshNoRotateOnMoveEvent>(OnRefreshNoRotateOnMove);
        SubscribeLocalEvent<ESForcedFacingComponent, ESRefreshNoRotateOnInteractEvent>(OnRefreshNoRotateOnInteract);

        SubscribeLocalEvent<ESForcedFacingComponent, ComponentShutdown>(OnForcedFacingShutdown);
        SubscribeLocalEvent<ESForcedFacingTargetComponent, ComponentShutdown>(OnForcedFacingTargetShutdown);
    }

    private void OnBoundUiOpened(Entity<ESForcedFacingUiComponent> ent, ref BoundUIOpenedEvent args)
    {
        RefreshForcedFacing(args.Actor);
    }

    private void OnBoundUiClosed(Entity<ESForcedFacingUiComponent> ent, ref BoundUIClosedEvent args)
    {
        RefreshForcedFacing(args.Actor);
    }

    private void OnRefreshForcedFacingUi(Entity<UserInterfaceUserComponent> ent, ref ESRefreshForcedFacingEvent args)
    {
        foreach (var entity in ent.Comp.OpenInterfaces.Keys)
        {
            if (entity != ent.Owner && HasComp<ESForcedFacingUiComponent>(entity))
                args.Targets.Add(entity);
        }
    }

    private void OnRefreshForcedFacingDoAfter(Entity<DoAfterComponent> ent, ref ESRefreshForcedFacingEvent args)
    {
        foreach (var doAfter in ent.Comp.DoAfters.Values)
        {
            if (doAfter.Completed || doAfter.Cancelled)
                continue;

            if (!doAfter.Args.FaceTarget)
                continue;

            if (doAfter.Args.Target is not { } target || target == doAfter.Args.User)
                continue;

            if (TerminatingOrDeleted(target))
                continue;

            args.Targets.Add(target);
        }
    }

    private void OnMouseRotatorRefreshNoRotateOnMove(Entity<MouseRotatorComponent> ent, ref ESRefreshNoRotateOnMoveEvent args)
    {
        args.Enabled = true;
    }

    private void OnRefreshNoRotateOnMove(Entity<ESForcedFacingComponent> ent, ref ESRefreshNoRotateOnMoveEvent args)
    {
        args.Enabled = true;
    }

    private void OnRefreshNoRotateOnInteract(Entity<ESForcedFacingComponent> ent, ref ESRefreshNoRotateOnInteractEvent args)
    {
        args.Enabled = true;
    }

    private void OnForcedFacingShutdown(Entity<ESForcedFacingComponent> ent, ref ComponentShutdown args)
    {
        foreach (var target in ent.Comp.Targets.ToArray())
        {
            StopFacing(ent.AsNullable(), target);
        }
    }

    private void OnForcedFacingTargetShutdown(Entity<ESForcedFacingTargetComponent> ent, ref ComponentShutdown args)
    {
        foreach (var facing in ent.Comp.Facing.ToArray())
        {
            if (!TerminatingOrDeleted(facing))
                RefreshForcedFacing(facing);
        }
    }

    public void RefreshNoRotateOnMove(EntityUid uid)
    {
        var ev = new ESRefreshNoRotateOnMoveEvent();
        RaiseLocalEvent(uid, ref ev);

        if (ev.Enabled)
            EnsureComp<NoRotateOnMoveComponent>(uid);
        else if (!PrototypeHas<NoRotateOnMoveComponent>(uid))
            RemComp<NoRotateOnMoveComponent>(uid);
    }

    public void RefreshNoRotateOnInteract(EntityUid uid)
    {
        var ev = new ESRefreshNoRotateOnInteractEvent();
        RaiseLocalEvent(uid, ref ev);

        if (ev.Enabled)
            EnsureComp<NoRotateOnInteractComponent>(uid);
        else if (!PrototypeHas<NoRotateOnInteractComponent>(uid))
            RemComp<NoRotateOnInteractComponent>(uid);
    }

    public void RefreshForcedFacing(EntityUid uid)
    {
        if (_timing.ApplyingState || TerminatingOrDeleted(uid))
            return;

        var ev = new ESRefreshForcedFacingEvent();
        RaiseLocalEvent(uid, ref ev);

        if (TryComp<ESForcedFacingComponent>(uid, out var comp))
        {
            foreach (var target in comp.Targets.Except(ev.Targets).ToArray())
            {
                StopFacing((uid, comp), target);
            }
            Dirty(uid, comp);
        }

        if (ev.Targets.Count == 0)
        {
            RemComp<ESForcedFacingComponent>(uid);

            RefreshNoRotateOnMove(uid);
            RefreshNoRotateOnInteract(uid);
            return;
        }

        comp ??= EnsureComp<ESForcedFacingComponent>(uid);
        foreach (var target in ev.Targets)
        {
            StartFacing((uid, comp), target);
        }
        Dirty(uid, comp);

        RefreshNoRotateOnMove(uid);
        RefreshNoRotateOnInteract(uid);
    }

    private void StartFacing(Entity<ESForcedFacingComponent> ent, EntityUid target)
    {
        if (ent.Comp.Targets.Contains(target))
            return;

        ent.Comp.Targets.Add(target);

        var facingTarget = EnsureComp<ESForcedFacingTargetComponent>(target);
        facingTarget.Facing.Add(ent);
        Dirty(target, facingTarget);
    }

    private void StopFacing(Entity<ESForcedFacingComponent?> ent, Entity<ESForcedFacingTargetComponent?> target)
    {
        if (!Resolve(ent, ref ent.Comp, false) || !ent.Comp.Targets.Contains(target))
            return;

        ent.Comp.Targets.Remove(target);

        if (TerminatingOrDeleted(target) || !Resolve(target, ref target.Comp, false))
            return;

        target.Comp.Facing.Remove(ent);
        Dirty(target, target.Comp);

        if (target.Comp.Facing.Count == 0)
            RemComp(target, target.Comp);
    }

    private bool PrototypeHas<T>(EntityUid uid) where T : IComponent, new()
    {
        return MetaData(uid).EntityPrototype is { } proto
               && proto.Components.ContainsKey(_factory.GetComponentName<T>());
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ESForcedFacingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PrimaryTarget is not { } target || !Exists(target))
                continue;

            _rotateToFace.TryFaceCoordinates(uid, _transform.GetMapCoordinates(target).Position);
        }
    }
}
