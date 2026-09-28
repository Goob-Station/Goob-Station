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

    private void OnMapInit(Entity<FaceHuggerLeapComponent> ent, ref MapInitEvent args) =>
        _actions.AddAction(ent.Owner, ref ent.Comp.LeapActionEntity, ent.Comp.LeapAction);

    private void OnShutdown(Entity<FaceHuggerLeapComponent> ent, ref ComponentShutdown args) =>
        _actions.RemoveAction(ent.Owner, ent.Comp.LeapActionEntity);

    private void OnLeapAction(Entity<FaceHuggerLeapComponent> ent, ref FaceHuggerLeapActionEvent args)
    {
        if (args.Handled
            || _container.IsEntityInContainer(ent.Owner))
            return;

        ent.Comp.IsLeaping = true;

        _throwing.TryThrow(ent.Owner, args.Target, ent.Comp.LeapSpeed, ent.Owner, pushbackRatio: 0f, animated: false);
        _audio.PlayPredicted(ent.Comp.LeapSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }
}
