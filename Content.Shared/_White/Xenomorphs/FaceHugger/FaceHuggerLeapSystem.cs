using Content.Shared.Actions;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._White.Xenomorphs.FaceHugger;

/// <summary>
/// Handles the leap action for sentient facehuggers
/// </summary>
public sealed class SharedFaceHuggerLeapSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggerLeapComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FaceHuggerLeapComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FaceHuggerLeapComponent, FaceHuggerLeapActionEvent>(OnLeapAction);
    }

    private void OnMapInit(EntityUid uid, FaceHuggerLeapComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.LeapActionEntity, component.LeapAction);

    private void OnShutdown(EntityUid uid, FaceHuggerLeapComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.LeapActionEntity);

    private void OnLeapAction(EntityUid uid, FaceHuggerLeapComponent component, FaceHuggerLeapActionEvent args)
    {
        if (args.Handled
            || _container.IsEntityInContainer(uid))
            return;

        component.IsLeaping = true;

        _throwing.TryThrow(uid, args.Target, component.LeapSpeed, uid, pushbackRatio: 0f, animated: false);
        _audio.PlayPredicted(component.LeapSound, uid, uid);

        args.Handled = true;
    }
}
