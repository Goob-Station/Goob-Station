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
        SubscribeLocalEvent<JumpComponent, ThrowDoHitEvent>(OnThrowDoHit);
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

    private void OnThrowDoHit(EntityUid uid, JumpComponent component, ThrowDoHitEvent args)
    {
        if (args.Handled)
            return;

        _throwingItem.StopThrow(uid, args.Component);

        if (Transform(args.Target).Anchored)
        {
            _stun.TryParalyze(uid, component.StunTime, true);
            return;
        }

        _stun.TryKnockdown(args.Target, component.StunTime, true);

        args.Handled = true;
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
