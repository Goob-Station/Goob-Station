using Content.Shared.Actions;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Jump;

public sealed class JumpSystem : EntitySystem
{
    [Dependency] private readonly ThrownItemSystem _throwingItem = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<JumpComponent, ComponentStartup>(OnJumpStartup);
        SubscribeLocalEvent<JumpComponent, ComponentShutdown>(OnJumpShutdown);
        SubscribeLocalEvent<JumpComponent, JumpActionEvent>(OnJump);
        SubscribeLocalEvent<JumpComponent, StopThrowEvent>(OnStopThrow);
        SubscribeLocalEvent<JumpComponent, ThrowAttemptEvent>(OnThrowDoHit);
    }

    private void OnJumpStartup(EntityUid uid, JumpComponent component, ComponentStartup args) =>
        _actions.AddAction(uid, ref component.JumpActionEntity, component.JumpAction);

    private void OnJumpShutdown(EntityUid uid, JumpComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.JumpActionEntity);

    private void OnJump(EntityUid uid, JumpComponent component, JumpActionEvent args)
    {
        if (args.Handled || _container.IsEntityInContainer(uid))
            return;

        _throwing.TryThrow(uid, args.Target, component.JumpSpeed, uid, 10F);

        _audio.PlayPvs(component.JumpSound, uid, component.JumpSound?.Params);

        _appearance.SetData(uid, JumpVisuals.Jumping, true);

        args.Handled = true;
    }

    private void OnStopThrow(EntityUid uid, JumpComponent component, StopThrowEvent args) =>
        _appearance.SetData(uid, JumpVisuals.Jumping, false);

    private void OnThrowDoHit(EntityUid uid, JumpComponent component, ref ThrowAttemptEvent args)
    {

        if (args.Cancelled
            || !TryComp<ThrownItemComponent>(args.ItemUid, out var thrownComp)
            || args.TargetUid == null)
            return;

        _throwingItem.StopThrow(uid, thrownComp);

        if (Transform(args.TargetUid.Value).Anchored)
        {
            _stun.TryUpdateParalyzeDuration(uid, component.StunTime);
            return;
        }

        _stun.TryKnockdown(args.TargetUid.Value, component.StunTime, true);

        args.Cancel();
    }
}

[Serializable, NetSerializable]
public enum JumpVisuals : byte
{
    Jumping
}

public enum JumpLayers : byte
{
    Jumping
}
