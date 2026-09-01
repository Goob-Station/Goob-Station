using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Mind.Components;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

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

    // TODO
    protected virtual void OnMonkeyAscensionRelay(Entity<MindContainerComponent> ent, ref SummonSimiansMaxedOutEvent args) { }
}