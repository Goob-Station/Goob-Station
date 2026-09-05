using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Lavaland.Movement;
using Content.Shared._vg.TileMovement;

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

        // hierophant's beat gives you a speed bonus
        var isBeneficial = HasComp<WizardComponent>(ev.Target) || HasComp<ApprenticeComponent>(ev.Target);

        if (HasComp<HierophantBeatComponent>(ev.Target) || HasComp<TileMovementComponent>(ev.Target))
        {
            RemComp<HierophantBeatComponent>(ev.Target);
            RemComp<TileMovementComponent>(ev.Target);
        }
        else if (isBeneficial)
            AddComp<HierophantBeatComponent>(ev.Target);
        else
            AddComp<TileMovementComponent>(ev.Target);

        ev.Handled = true;
    }
}