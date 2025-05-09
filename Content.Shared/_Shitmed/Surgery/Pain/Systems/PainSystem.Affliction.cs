using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;

public partial class PainSystem
{
    private void InitAffliction()
    {
        // Pain management hooks.
        SubscribeLocalEvent<PainInflicterComponent, WoundRemovedEvent>(OnPainRemoved);
        SubscribeLocalEvent<PainInflicterComponent, WoundSeverityPointChangedEvent>(OnPainChanged);
    }

    private const string PainModifierIdentifier = "WoundPain";
    private const string PainTraumaticModifierIdentifier = "TraumaticPain";
    private const string PainAdrenalineIdentifier = "PainAdrenaline";

    #region Event Handling

    private void OnPainChanged(Entity<PainInflicterComponent> woundEnt, ref WoundSeverityPointChangedEvent args)
    {
        if (!TryComp<BodyPartComponent>(args.Component.HoldingWoundable, out var bodyPart)
            || bodyPart.Body == null
            || !_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var nerveSys))
            return;

        // bro how
        woundEnt.Comp.RawPain = args.NewSeverity;
        var woundPain = FixedPoint2.Zero;
        var traumaticPain = FixedPoint2.Zero;

        foreach (var (woundId, _) in _wound.GetWoundableWounds(args.Component.HoldingWoundable))
        {
            if (!TryComp<PainInflicterComponent>(woundId, out var painInflicter))
                continue;

            switch (painInflicter.PainType)
            {
                // In case more Pain Types is added for some reasonm
                case PainDamageTypes.WoundPain:
                    woundPain += painInflicter.Pain;
                    break;
                case PainDamageTypes.TraumaticPain:
                    traumaticPain += painInflicter.Pain;
                    break;
                default:
                    woundPain += painInflicter.Pain;
                    break;
            }
        }

        if (!TryAddPainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, woundPain))
            TryChangePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, woundPain);

        if (traumaticPain > 0)
        {
            if (!TryAddPainModifier(
                    nerveSys.Value,
                    args.Component.HoldingWoundable,
                    PainTraumaticModifierIdentifier,
                    traumaticPain,
                    PainDamageTypes.TraumaticPain))
            {
                TryChangePainModifier(
                    nerveSys.Value,
                    args.Component.HoldingWoundable,
                    PainTraumaticModifierIdentifier,
                    traumaticPain);
            }
        }
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
        woundEnt.Comp.RawPain = 0;
        var woundPain = FixedPoint2.Zero;
        var traumaticPain = FixedPoint2.Zero;
        foreach (var (woundId, _) in _wound.GetWoundableWounds(args.Component.HoldingWoundable))
        {
            if (!TryComp<PainInflicterComponent>(woundId, out var painInflicter))
                continue;

            switch (painInflicter.PainType)
            {
                // In case more Pain Types is added for some reasonm
                case PainDamageTypes.WoundPain:
                    woundPain += painInflicter.Pain;
                    break;
                case PainDamageTypes.TraumaticPain:
                    traumaticPain += painInflicter.Pain;
                    break;
                default:
                    woundPain += painInflicter.Pain;
                    break;
            }
        }

        if (woundPain <= 0)
            TryRemovePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier);
        else
            TryChangePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainModifierIdentifier, woundPain);

        if (traumaticPain <= 0)
            TryRemovePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainTraumaticModifierIdentifier);
        else
            TryChangePainModifier(nerveSys.Value, args.Component.HoldingWoundable, PainTraumaticModifierIdentifier, traumaticPain);
    }

    #endregion
}
