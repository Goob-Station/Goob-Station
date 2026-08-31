using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Lavaland.Movement;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnTileToggle(TileToggleSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (HasComp<HierophantBeatComponent>(ev.Target))
            RemComp<HierophantBeatComponent>(ev.Target);
        else
            EnsureComp<HierophantBeatComponent>(ev.Target);

        ev.Handled = true;
    }
}