using Content.Shared._White.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Jump;

public sealed class LeapSystem : EntitySystem
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
        SubscribeLocalEvent<LeapComponent, ComponentStartup>(OnJumpStartup);
        SubscribeLocalEvent<LeapComponent, ComponentShutdown>(OnJumpShutdown);
        SubscribeLocalEvent<LeapComponent, LeapActionEvent>(OnJump);
        SubscribeLocalEvent<LeapComponent, StopThrowEvent>(OnStopThrow);
        SubscribeLocalEvent<LeapComponent, ThrowAttemptEvent>(OnThrowDoHit);
    }

    private void OnJumpStartup(Entity<LeapComponent> ent, ref ComponentStartup args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.JumpActionEntity, ent.Comp.JumpAction);

    private void OnJumpShutdown(Entity<LeapComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.JumpActionEntity);

    private void OnJump(Entity<LeapComponent> ent, ref LeapActionEvent args)
    {
        if (args.Handled || _container.IsEntityInContainer(ent.Owner))
            return;

        _throwing.TryThrow(ent.Owner, args.Target, ent.Comp.JumpSpeed, ent.Owner, 10F);

        _audio.PlayPvs(ent.Comp.JumpSound, ent.Owner, ent.Comp.JumpSound?.Params);

        _appearance.SetData(ent.Owner, JumpVisuals.Jumping, true);

        args.Handled = true;
    }

    private void OnStopThrow(Entity<LeapComponent> ent, ref StopThrowEvent args)
    {
        _appearance.SetData(ent.Owner, JumpVisuals.Jumping, false);
    }

    private void OnThrowDoHit(Entity<LeapComponent> ent, ref ThrowAttemptEvent args)
    {
        if (args.Cancelled
            || !TryComp<ThrownItemComponent>(args.ItemUid, out var thrownComp)
            || args.TargetUid is not { } target)
            return;

        _throwingItem.StopThrow(ent.Owner, thrownComp);

        if (Transform(target).Anchored)
        {
            _stun.TryUpdateParalyzeDuration(ent.Owner, ent.Comp.StunTime);
            return;
        }

        _stun.TryKnockdown(target, ent.Comp.StunTime, true);

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
