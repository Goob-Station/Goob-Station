using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;


public abstract partial class SharedSpellsSystem
{
    private void OnSummonMobs(SummonMobsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SummonMobsRelay(ev);

        ev.Handled = true;
    }

    protected virtual void SummonMobsRelay(SummonMobsEvent ev) { }
}