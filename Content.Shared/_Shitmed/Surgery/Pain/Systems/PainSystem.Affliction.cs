using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

public partial class PainSystem
{
    private void InitAffliction()
    {
        // Pain management hooks.
        SubscribeLocalEvent<PainInflicterComponent, WoundRemovedEvent>(OnPainRemoved);
        SubscribeLocalEvent<PainInflicterComponent, WoundSeverityPointChangedEvent>(OnPainChanged);
    }

    private const string PainModifierIdentifier = "Pain";

    #region Event Handling

    private void OnPainChanged(Entity<PainInflicterComponent> woundEnt, ref WoundSeverityPointChangedEvent args)
    {
        if (_timing.ApplyingState
            || !TryComp<BodyPartComponent>(args.Component.HoldingWoundable, out var bodyPart)
            || bodyPart.Body == null
            || !_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var nerveSys))
            return;

        // bro how
        woundEnt.Comp.Pain = FixedPoint2.Clamp(args.NewSeverity * woundEnt.Comp.PainMultiplier, 0f, 100f);
        var allPain = (FixedPoint2) 0;

        foreach (var (woundId, _) in _wound.GetWoundableWounds(args.Component.HoldingWoundable))
        {
            if (!TryComp<PainInflicterComponent>(woundId, out var painInflicter))
                continue;

            allPain += painInflicter.Pain;
        }

        if (!TryAddPainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, allPain))
            TryChangePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, allPain);
    }

    private void OnPainRemoved(Entity<PainInflicterComponent> woundEnt, ref WoundRemovedEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Component.HoldingWoundable, out var bodyPart)
            || bodyPart.Body == null)
            return;

        var rootPart = Comp<BodyComponent>(bodyPart.Body.Value).RootContainer.ContainedEntity;

        if (!rootPart.HasValue
            || !_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var nerveSys))
            return;

        // bro how
        var allPain = (FixedPoint2) 0;
        foreach (var (woundId, _) in _wound.GetWoundableWounds(args.Component.HoldingWoundable))
        {
            if (!TryComp<PainInflicterComponent>(woundId, out var painInflicter))
                continue;

            allPain += painInflicter.Pain;
        }

        if (allPain <= 0)
            TryRemovePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier);
        else
            TryChangePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, allPain);
    }

    #endregion
}
