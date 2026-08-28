using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Terror;

/// <summary>
/// Paralyzes anything that steps on this.
/// </summary>
public sealed class StepTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

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
        if (comp.Blacklist != null && _whitelist.IsValid(comp.Blacklist, args.Tripper))
            return;

        _stun.TryAddParalyzeDuration(args.Tripper, comp.SnareTime);
        _audio.PlayPredicted(comp.CaughtSound, args.Tripper, args.Tripper);

        var ev = new StepTrapTriggeredEvent(args.Tripper);
        RaiseLocalEvent(uid, ref ev);
    }
}

/// <summary>
/// Raised on the trap when something steps on it and the stun is applied.
/// </summary>
[ByRefEvent]
public record struct StepTrapTriggeredEvent(EntityUid Tripper);
