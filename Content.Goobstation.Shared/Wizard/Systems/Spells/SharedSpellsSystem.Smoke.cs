using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnSmoke(SmokeSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        OnSmokeRelay(ev);

        ev.Handled = true;
    }

    protected virtual void OnSmokeRelay(SmokeSpellEvent ev) { }
}