using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnArcaneBarrage(ArcaneBarrageEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (PredictedSpawnItemInHands(ev.Performer, ev.Proto, ev.Action) == null)
            return;

        ev.Handled = true;
    }
}