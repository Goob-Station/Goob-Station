using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;
using Content.Shared.Spider;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class StickyWebTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorWebComponent, StepTriggeredOnEvent>(OnStepOn);
        SubscribeLocalEvent<TerrorWebComponent, StepTriggerAttemptEvent>(OnAttemptTrigger);
    }

    private void OnAttemptTrigger(EntityUid uid, TerrorWebComponent comp, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepOn(EntityUid uid, TerrorWebComponent comp, ref StepTriggeredOnEvent args)
    {
        //Spooder does not get stunned
        if (HasComp<IgnoreSpiderWebComponent>(args.Tripper))
            return;

        _stun.TryParalyze(args.Tripper, comp.SnareTime, true);

        _audio.PlayPredicted(comp.CaughtSound, args.Tripper, args.Tripper);
        _popup.PopupCoordinates("The sticky web ensnares you!", Transform(uid).Coordinates, args.Tripper);
    }
}
