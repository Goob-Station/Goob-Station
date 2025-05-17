using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gatherable;

/// <summary>
/// Handles interaction with gatherable entities.
/// Actual gathering is done serverside, client just predicts it being deleted.
/// </summary>
public abstract class SharedToolGatherableSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ToolGatherableComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ToolGatherableComponent, GatherDoAfterEvent>(OnDoAfter);
    }

    private void OnInteractUsing(Entity<ToolGatherableComponent> ent, ref InteractUsingEvent args)
    {
        args.Handled = _tool.UseTool(
            tool: args.Used,
            user: args.User,
            target: args.Target,
            doAfterDelay: (float) ent.Comp.GatherTime.TotalSeconds,
            toolQualityNeeded: ent.Comp.ToolQuality,
            doAfterEv: new GatherDoAfterEvent());
    }

    private void OnDoAfter(Entity<ToolGatherableComponent> ent, ref GatherDoAfterEvent args)
    {
        Gather(ent, args.User);
    }

    protected virtual void Gather(Entity<ToolGatherableComponent> ent, EntityUid user)
    {
        // client can definitely predict the audio so do that
        // do not use GatherSoundComponent as well or it will double play
        _audio.PlayPredicted(ent.Comp.Sound, Transform(ent).Coordinates, user);
    }
}

[Serializable, NetSerializable]
public sealed partial class GatherDoAfterEvent : SimpleDoAfterEvent;
