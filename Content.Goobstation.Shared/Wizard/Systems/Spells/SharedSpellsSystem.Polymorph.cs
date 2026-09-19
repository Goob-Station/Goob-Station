using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnPolymorph(PolymorphSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        ev.Handled = PolymorphRelay(ev);
    }

    protected virtual bool PolymorphRelay(PolymorphSpellEvent ev) { return false; }
}