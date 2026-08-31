using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnTeslaBlast(TeslaBlastEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (TryComp(ev.Performer, out CastingTeslaBlastComponent? casting))
        {
            _teslaBlast.CancelDoAfter(ev.Performer, casting);

            ev.Handled = true;
            return;
        }

        _teslaBlast.StartCharging(ev);
    }
}