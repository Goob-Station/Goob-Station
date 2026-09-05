using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locRathenGut = "spell-rathen-gut-popup";

    private void OnRathen(RathenEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var mapPos = _xform.GetMapCoordinates(ev.Performer);
        var stunTime = ev.StunTime;

        var targets = _lookup.GetEntitiesInRange<FartComponent>(mapPos, ev.MaxRange, LookupFlags.Dynamic);

        foreach (var ent in targets)
        {
            if (ent.Owner == ev.Performer)
                continue;

            if (_divineIntervention.TouchSpellDenied(ent))
                continue;

            if (!TryComp<BodyComponent>(ent, out var body)
                || _mobState.IsDead(ent))
                continue;

            _stun.KnockdownOrStun(ent, stunTime, true);

            if (!ent.Comp.SuperFarted)
            {
                ent.Comp.FartInhale = true;
                _chat.TryEmoteWithChat(ent, "FartSuper", ignoreActionBlocker: true, forceEmote: true);
            }
            else
            {
                _popup.PopupPredicted(
                Loc.GetString(_locRathenGut),
                ent,
                ent,
                PopupType.LargeCaution);

                _damageable.TryChangeDamage(ent,
                    ev.SuperFartDamage,
                    true,
                    origin: ev.Performer);

                if (TryComp<BloodstreamComponent>(ent, out var bloodstream)
                    && bloodstream.BloodSolution is not null
                    && _solutionContainer.ResolveSolution(ent.Owner, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution))
                {
                    var toSpill = _solutionContainer.SplitSolution(bloodstream.BloodSolution.Value, 15);
                    _puddle.TrySpillAt(ent, toSpill, out _);
                }

                foreach (var limbType in new[] { BodyPartType.Arm, BodyPartType.Leg })
                    foreach (var (partId, _) in _body.GetBodyChildrenOfType(ent, limbType, body))
                    {
                        if (_random.Prob(ev.LimbTearChance)
                            && TryComp<WoundableComponent>(partId, out var woundable)
                            && woundable.ParentWoundable.HasValue)
                            _wound.AmputateWoundable(woundable.ParentWoundable.Value, partId, woundable);
                    }
            }
        }

        ev.Handled = true;
    }
}