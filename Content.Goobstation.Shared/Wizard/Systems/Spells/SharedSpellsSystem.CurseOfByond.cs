
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

// Holy shit
public abstract partial class SharedSpellsSystem
{
    private void OnPredictionToggle(PredictionToggleSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (HasComp<CurseOfByondComponent>(ev.Target))
            RemComp<CurseOfByondComponent>(ev.Target);
        else
            EnsureComp<CurseOfByondComponent>(ev.Target);

        ev.Handled = true;
    }
}