using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

/// <summary>
/// TODO: finish moving goob wiz spells then remove Goob after deleting SpellsSystem
/// </summary>
public abstract partial class SharedSpellsSystem
{
    private void OnSummonSimians(SummonSimiansEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SpawnMonkeysRelay(ev);

        ev.Handled = true;
    }

    // TODO: predict (has random guh)
    protected virtual void SpawnMonkeysRelay(SummonSimiansEvent ev) { }
}