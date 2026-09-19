using Content.Goobstation.Common.Body.Components;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Body;
using Content.Goobstation.Shared.GrabIntent;
using Content.Server.Body.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared.Body.Components;

namespace Content.Server.Body.Systems;

public sealed partial class RespiratorSystem
{
    private EntityQuery<BodyComponent> _bodyQuery;

    // Can breathe check for grab or if they need air
    private bool GoobCanBreathe(EntityUid uid, RespiratorComponent respirator)
    {
        var airEv = new CheckNeedsAirEvent();
        RaiseLocalEvent(uid, ref airEv);

        if (HasComp<BreathingImmunityComponent>(uid) || HasComp<SpecialBreathingImmunityComponent>(uid))
            return true;

        // DeltaV: Cosmic Cult - One line change but a refactor would be better. this is kinda cringe.
        // Makes cultists gasp and respirate but not asphyxiate in space.
        if (TryComp<CosmicCultComponent>(uid, out var cultComponent)
            && !cultComponent.Respiration
            && !_mobState.IsIncapacitated(uid))
            return true;

        if (airEv.Cancelled)
            return true;

        if (respirator.Saturation < respirator.SuffocationThreshold)
            return false;
        if (TryComp<GrabbableComponent>(uid, out var grabbable)
            && grabbable.GrabStage == GrabStage.Suffocate)
            return false;

        return !HasComp<KravMagaBlockedBreathingComponent>(uid);
    }

    private float ShitmedGetSaturationLossMultiplier(Entity<RespiratorComponent> ent)
    {
        var organs = _bodySystem.GetBodyOrganEntityComps<LungComponent>(ent.Owner);
        var multiplier = -1f;
        foreach (var (_, lung, _) in organs)
        {
            multiplier *= lung.SaturationLoss * ent.Comp.SaturationLoss; // Goob Edit - In a DeltaV Edit :o
        }
        return multiplier;
    }

    private void ShitmedTakeSuffocationConsciousnessModifier(Entity<RespiratorComponent> ent)
    {
        if (_consciousness.TryGetNerveSystem(ent, out var nerveSys))
        {
            if (!_consciousness.TryGetConsciousnessModifier(ent, nerveSys.Value, out var modifier, "Suffocation"))
            {
                _consciousness.AddConsciousnessModifier(
                    ent,
                    nerveSys.Value,
                    -ent.Comp.Damage.GetTotal(),
                    identifier: "Suffocation",
                    type: ConsciousnessModType.Pain);
            }
            else
            {
                _consciousness.SetConsciousnessModifier(
                    ent,
                    nerveSys.Value,
                    modifier.Value.Change - ent.Comp.Damage.GetTotal(),
                    identifier: "Suffocation",
                    type: ConsciousnessModType.Pain);
            }
        }
    }

    private void ShitmedStopSuffocationConsciousnessModifier(Entity<BodyComponent> ent)
    {
        if (!TryComp(ent, out RespiratorComponent? respirator))
            return;

        if (_consciousness.TryGetNerveSystem(ent, out var nerveSys)
            && _consciousness.TryGetConsciousnessModifier(ent, nerveSys.Value, out var modifier, "Suffocation"))
        {
            if (modifier.Value.Change < respirator.DamageRecovery.GetTotal())
            {
                _consciousness.RemoveConsciousnessModifier(ent, nerveSys.Value, "Suffocation");
            }
            else
            {
                _consciousness.SetConsciousnessModifier(
                    ent,
                    nerveSys.Value,
                    modifier.Value.Change + respirator.DamageRecovery.GetTotal(),
                    identifier: "Suffocation",
                    type: ConsciousnessModType.Pain);
            }
        }
    }
}