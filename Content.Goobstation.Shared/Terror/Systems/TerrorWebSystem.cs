using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Drunk;
using Content.Shared.Popups;
using Content.Shared.Spider;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class StickyWebTrapSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

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

        if (HasComp<InfestedWebComponent>(uid))
        {
            _popup.PopupPredicted("The sticky web ensnares you! Spiderlings begin to crawl all over you!", args.Tripper, args.Tripper, PopupType.MediumCaution);
            EnsureComp<InfestedComponent>(args.Tripper);
        }
        else if (TryComp<PoisonWebComponent>(uid, out var poison))
        {
            _popup.PopupPredicted("The sticky web ensnares you! You don't feel too good...", args.Tripper, args.Tripper, PopupType.MediumCaution);
            Inject(args.Tripper, poison.ReagentId, poison.ReagentAmount);
        }
        else if (TryComp<SlimyWebComponent>(uid, out var slimy))
        {
            _popup.PopupPredicted("The sticky web ensnares you! You start to feel woozy...", args.Tripper, args.Tripper, PopupType.MediumCaution);
            Inject(args.Tripper, slimy.ReagentId, slimy.AlcoholAmount);
        }
        else
            _popup.PopupPredicted("The sticky web ensnares you!", args.Tripper, args.Tripper, PopupType.MediumCaution);
    }

    private void Inject(EntityUid target, ProtoId<ReagentPrototype> reagent, FixedPoint2 amount)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return;

        var solution = new Solution();
        solution.AddReagent(reagent, amount);

        _bloodstream.TryAddToChemicals((target, bloodstream), solution);
    }


}
