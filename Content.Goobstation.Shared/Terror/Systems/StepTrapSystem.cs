using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Spider;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Terror;

/// <summary>
/// Generic step trap system. Paralyzes the tripper and raises
/// <see cref="StepTrapTriggeredEvent"/> so other systems can add effects.
/// </summary>
public sealed class StepTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StepTrapComponent, StepTriggerAttemptEvent>(OnAttemptTrigger);
        SubscribeLocalEvent<StepTrapComponent, StepTriggeredOnEvent>(OnStepOn);
    }

    private void OnAttemptTrigger(EntityUid uid, StepTrapComponent comp, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepOn(EntityUid uid, StepTrapComponent comp, ref StepTriggeredOnEvent args)
    {
        if (HasComp<IgnoreSpiderWebComponent>(args.Tripper))
            return;

        _stun.TryAddParalyzeDuration(args.Tripper, comp.SnareTime);
        _audio.PlayPredicted(comp.CaughtSound, args.Tripper, args.Tripper);

        var ev = new StepTrapTriggeredEvent(args.Tripper);
        RaiseLocalEvent(uid, ref ev);
    }
}

/// <summary>
/// Raised on the trap entity when something steps on it and the base stun is applied.
/// Subscribe to this to add effects on top (reagent injection, component application, popups, etc.)
/// </summary>
[ByRefEvent]
public record struct StepTrapTriggeredEvent(EntityUid Tripper);
