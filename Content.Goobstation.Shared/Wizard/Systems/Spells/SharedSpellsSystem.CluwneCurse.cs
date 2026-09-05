using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Cluwne;
using Content.Shared.StatusEffect;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnCluwneCurse(CluwneCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (TryComp(ev.Target, out StatusEffectsComponent? status))
        {
            _stun.TryUpdateParalyzeDuration(ev.Target, ev.ParalyzeDuration);
            _jitter.DoJitter(ev.Target, ev.StutterDuration, true, status: status);
        }

        EnsureComp<CluwneComponent>(ev.Target);

        ev.Handled = true;
    }
}