using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.EntityEffects;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SetGrabStage : EntityEffectBase<SetGrabStage>
{
    [DataField]
    public GrabStage Stage;

    [DataField]
    public float EscapeIntentModifier = 1;
};

/// <summary>
/// Used for judo
/// </summary>
public sealed partial class SetGrabStageEntityEffectSystem : EntityEffectSystem<GrabIntentComponent, SetGrabStage>
{
    [Dependency] private GrabIntentSystem _grabIntent = default!;

    protected override void Effect(Entity<GrabIntentComponent> ent, ref EntityEffectEvent<SetGrabStage> args)
    {
        // Are we fucking serious
        if (!TryComp(ent, out PullerComponent? pullerComp))
            return;
        if (pullerComp.Pulling is not { Valid: true } pulledUid)
            return;
        if (!TryComp(pulledUid, out PullableComponent? pullableComp))
            return;
        if (!TryComp(pulledUid, out GrabbableComponent? grabbableComp))
            return;

        _grabIntent.TrySetGrabStages(
            (ent, pullerComp, ent.Comp),
            (pulledUid, pullableComp, grabbableComp),
            args.Effect.Stage,
            args.Effect.EscapeIntentModifier
        );
    }
}
