using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.StepTrap;
using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Handles terror-specific web effects when a step trap is triggered:
/// infested, poison, slimy, or plain sticky web popups and reagent injection.
/// </summary>
public sealed class TerrorWebSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorWebComponent, StepTrapTriggeredEvent>(OnTriggered);
    }

    private void OnTriggered(EntityUid uid, TerrorWebComponent _, ref StepTrapTriggeredEvent ev)
    {
        if (HasComp<InfestedWebComponent>(uid))
        {
            _popup.PopupPredicted(
                Loc.GetString("sticky-web-infested"),
                ev.Tripper, ev.Tripper,
                PopupType.MediumCaution);
            EnsureComp<InfestedComponent>(ev.Tripper);
        }
        else if (TryComp<PoisonWebComponent>(uid, out var poison))
        {
            _popup.PopupPredicted(
                Loc.GetString("sticky-web-poison"),
                ev.Tripper, ev.Tripper,
                PopupType.MediumCaution);
            Inject(ev.Tripper, poison.ReagentId, poison.ReagentAmount);
        }
        else if (TryComp<SlimyWebComponent>(uid, out var slimy))
        {
            _popup.PopupPredicted(
                Loc.GetString("sticky-web-slimy"),
                ev.Tripper, ev.Tripper,
                PopupType.MediumCaution);
            Inject(ev.Tripper, slimy.ReagentId, slimy.AlcoholAmount);
        }
        else
        {
            _popup.PopupPredicted(
                Loc.GetString("sticky-web-generic"),
                ev.Tripper, ev.Tripper,
                PopupType.MediumCaution);
        }
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
