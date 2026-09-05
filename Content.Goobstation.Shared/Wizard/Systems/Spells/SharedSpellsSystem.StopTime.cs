using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Physics;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public abstract partial class SharedSpellsSystem
{
    private void OnStopTime(StopTimeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var effect = PredictedSpawnAtPosition(ev.Proto, Transform(ev.Performer).Coordinates);
        var comp = EnsureComp<PreventCollideComponent>(effect); // Just in case
        comp.Uid = ev.Performer;
        Dirty(effect, comp);

        ev.Handled = true;
    }
}